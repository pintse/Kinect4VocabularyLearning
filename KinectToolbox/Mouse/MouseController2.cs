using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using log4net;
using Microsoft.Kinect;

namespace Kinect.Toolbox
{
    public class MouseController2
    {
        private static ILog log = LogManager.GetLogger(typeof(MouseController2));

        static MouseController2 current;
        public static MouseController2 Current
        {
            get
            {
                if (current == null)
                {
                    current = new MouseController2();
                }

                return current;
            }
        }

        Vector2? lastKnownPosition;
        float previousDepth;

        // Filters
        Vector2 savedFilteredJointPosition;
        Vector2 savedTrend;
        Vector2 savedBasePosition;
        int frameCount;

        // Impostors
        Canvas impostorCanvas;
        Visual rootVisual;
        MouseImpostor impostor;

        /// <summary>
        /// 目前游標是否吸附在控制項上
        /// </summary>
        bool isMagnetized;
        /// <summary>
        /// 游標開始靜止的時間
        /// </summary>
        DateTime magnetizationStartDate;
        /// <summary>
        /// 前一個吸附的控制項
        /// </summary>
        FrameworkElement previousMagnetizedElement;

        // Gesture detector for click
        GestureDetector clickGestureDetector;
        bool clickGestureDetected;
        public GestureDetector ClickGestureDetector
        {
            get
            {
                return clickGestureDetector;
            }
            set
            {
                if (value != null)
                {
                    value.OnGestureDetected += (obj) =>
                        {
                            clickGestureDetected = true;
                        };
                }

                clickGestureDetector = value;
            }
        }

        /// <summary>
        /// 是否關閉以使用手勢（例如：畫一個圈圈）表示按下滑鼠左鍵
        /// 需要另外設定該手勢的Detector物件
        /// </summary>
        public bool DisableGestureClick
        {
            get;
            set;
        }

        public Canvas ImpostorCanvas
        {
            set
            {
                if (value == null)
                {
                    if (impostorCanvas != null)
                        impostorCanvas.Children.Remove(impostor);

                    if (impostor != null)  //我增加if判斷式
                        impostor.OnProgressionCompleted -= impostor_OnProgressionCompleted;

                    impostor = null;
                    rootVisual = null;
                    return;
                }

                impostorCanvas = value;
                rootVisual = impostorCanvas.Parent as Visual;
                impostor = new MouseImpostor();

                impostor.OnProgressionCompleted += impostor_OnProgressionCompleted;

                value.Children.Add(impostor);
            }
        }

        /// <summary>
        /// 當觸發此事件處理函式，將會使用Automation assembly（參照專案中的UIAutomationProvider.dll）。此命名空間含有模擬「標準控制項的使用」之類別，例如：IToggleProvider界面能夠模擬Checkbox的勾選和取消勾選
        /// </summary>
        void impostor_OnProgressionCompleted()
        {
            if (previousMagnetizedElement != null)
            {
                var peer = UIElementAutomationPeer.CreatePeerForElement(previousMagnetizedElement);

                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

                if (invokeProv == null)
                {
                    var toggleProv = peer.GetPattern(PatternInterface.Toggle) as IToggleProvider;

                    toggleProv.Toggle();
                }
                else
                    invokeProv.Invoke();

                previousMagnetizedElement = null;
                isMagnetized = false;
            }
        }

        /// <summary>
        /// 磁性範圍，吸附游標的最小距離
        /// </summary>
        public float MagneticRange
        {
            get;
            set;
        }

        /// <summary>
        /// 具磁性的控制項之集合
        /// </summary>
        public List<FrameworkElement> MagneticsControl
        {
            get;
            private set;
        }

        // Filter parameters
        public float TrendSmoothingFactor
        {
            get;
            set;
        }

        public float JitterRadius
        {
            get;
            set;
        }

        public float DataSmoothingFactor
        {
            get;
            set;
        }

        public float PredictionFactor
        {
            get;
            set;
        }

        public float GlobalSmooth
        {
            get;
            set;
        }

        MouseController2()
        {
            TrendSmoothingFactor = 0.25f;
            JitterRadius = 0.05f;
            DataSmoothingFactor = 0.5f;
            PredictionFactor = 0.5f;

            GlobalSmooth = 0.9f;

            MagneticsControl = new List<FrameworkElement>();

            MagneticRange = 25.0f;
        }

        Vector2 FilterJointPosition(KinectSensor sensor, Joint joint)
        {
            //log.Debug(sensor.DeviceConnectionId+":"+sensor.UniqueKinectId);
            Vector2 filteredJointPosition;
            Vector2 differenceVector;
            Vector2 currentTrend;
            float distance;

            Vector2 baseJointPosition = Tools.Convert(sensor, joint.Position);
            Vector2 prevFilteredJointPosition = savedFilteredJointPosition;
            Vector2 previousTrend = savedTrend;
            Vector2 previousBaseJointPosition = savedBasePosition;

            // Checking frames count
            switch (frameCount)
            {
                case 0:
                    filteredJointPosition = baseJointPosition;
                    currentTrend = Vector2.Zero;
                    break;
                case 1:
                    filteredJointPosition = (baseJointPosition + previousBaseJointPosition) * 0.5f;
                    differenceVector = filteredJointPosition - prevFilteredJointPosition;
                    currentTrend = differenceVector * TrendSmoothingFactor + previousTrend * (1.0f - TrendSmoothingFactor);
                    break;
                default:
                    // Jitter filter
                    differenceVector = baseJointPosition - prevFilteredJointPosition;
                    distance = Math.Abs(differenceVector.Length);

                    if (distance <= JitterRadius)
                    {
                        filteredJointPosition = baseJointPosition * (distance / JitterRadius) + prevFilteredJointPosition * (1.0f - (distance / JitterRadius));
                    }
                    else
                    {
                        filteredJointPosition = baseJointPosition;
                    }

                    // Double exponential smoothing filter
                    filteredJointPosition = filteredJointPosition * (1.0f - DataSmoothingFactor) + (prevFilteredJointPosition + previousTrend) * DataSmoothingFactor;

                    differenceVector = filteredJointPosition - prevFilteredJointPosition;
                    currentTrend = differenceVector * TrendSmoothingFactor + previousTrend * (1.0f - TrendSmoothingFactor);
                    break;
            }

            // Compute potential new position
            Vector2 potentialNewPosition = filteredJointPosition + currentTrend * PredictionFactor;

            // Cache current value
            savedBasePosition = baseJointPosition;
            savedFilteredJointPosition = filteredJointPosition;
            savedTrend = currentTrend;
            frameCount++;

            return potentialNewPosition;
        }

        public void SetHandPosition(KinectSensor sensor, Joint joint, Skeleton skeleton)
        {
            
            Vector2 vector2 = FilterJointPosition(sensor, joint);

            if (!lastKnownPosition.HasValue)
            {
                lastKnownPosition = vector2;
                previousDepth = joint.Position.Z;
                return;
            }

            bool isClicked = false;

            if (DisableGestureClick)
            {
            }
            else
            {
                if (ClickGestureDetector == null)
                    isClicked = Math.Abs(joint.Position.Z - previousDepth) > 0.05f;
                else
                    isClicked = clickGestureDetected;
            }

            //追蹤每個角落的位置與每個角落至游標的相對距離。如過控制項在游標附近（意味著距離值小於MagneticRange屬性），則將游標移至控制項的中心且更新替代項的進度值（ProgressBar），表示點按進行中
            if (impostor != null)
            {
                // Still magnetized ? 
                if ((vector2 - lastKnownPosition.Value).Length > 0.1f)
                {
                    impostor.Progression = 0;
                    isMagnetized = false;
                    previousMagnetizedElement = null;
                }

                // Looking for nearest magnetic control
                float minDistance = float.MaxValue;
                FrameworkElement nearestElement = null;
                var impostorPosition = new Vector2((float)(vector2.X * impostorCanvas.ActualWidth), (float)(vector2.Y * impostorCanvas.ActualHeight));

                foreach (FrameworkElement element in MagneticsControl)
                {
                    // Getting the four corners
                    var position = element.TransformToAncestor(rootVisual).Transform(new Point(0, 0));
                    var p1 = new Vector2((float)position.X, (float)position.Y);
                    var p2 = new Vector2((float)(position.X + element.ActualWidth), (float)position.Y);
                    var p3 = new Vector2((float)(position.X + element.ActualWidth), (float)(position.Y + element.ActualHeight));
                    var p4 = new Vector2((float)position.X, (float)(position.Y + element.ActualHeight));

                    // Minimal distance
                    float previousMinDistance = minDistance;
                    minDistance = Math.Min(minDistance, (impostorPosition - p1).Length);
                    minDistance = Math.Min(minDistance, (impostorPosition - p2).Length);
                    minDistance = Math.Min(minDistance, (impostorPosition - p3).Length);
                    minDistance = Math.Min(minDistance, (impostorPosition - p4).Length);

                    if (minDistance != previousMinDistance)
                    {
                        nearestElement = element;
                    }
                }

                // If a control is at a sufficient distance
                if (minDistance < MagneticRange || isMagnetized)
                {
                    // Magnetic control found
                    var position = nearestElement.TransformToAncestor(rootVisual).Transform(new Point(0, 0));

                    Canvas.SetLeft(impostor, position.X + nearestElement.ActualWidth / 2 - impostor.ActualWidth / 2);
                    Canvas.SetTop(impostor, position.Y + nearestElement.ActualHeight / 2);
                    lastKnownPosition = vector2;

                    if (!isMagnetized || previousMagnetizedElement != nearestElement)
                    {
                        isMagnetized = true;
                        magnetizationStartDate = DateTime.Now;
                    }
                    else
                    {
                        impostor.Progression = (int)(((DateTime.Now - magnetizationStartDate).TotalMilliseconds * 100) / 2000.0);
                    }
                }
                else
                {
                    Canvas.SetLeft(impostor, impostorPosition.X - impostor.ActualWidth / 2);
                    Canvas.SetTop(impostor, impostorPosition.Y);
                }

                if (!isMagnetized)
                    lastKnownPosition = vector2;

                previousMagnetizedElement = nearestElement;
            }
            else
            {
                //我Mark下面一行，沒有設定要用手勢控制時，就停止操控滑鼠位置
                //MouseInterop.ControlMouse((int)((vector2.X - lastKnownPosition.Value.X) * Screen.PrimaryScreen.Bounds.Width * GlobalSmooth), (int)((vector2.Y - lastKnownPosition.Value.Y) * Screen.PrimaryScreen.Bounds.Height * GlobalSmooth), isClicked);
                lastKnownPosition = vector2;
            }


            previousDepth = joint.Position.Z;

            clickGestureDetected = false;
        }
    }
}
