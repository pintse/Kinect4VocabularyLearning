using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.Service;
using Ryan.Kinect.GestureCommand.VO;
using Ryan.Kinect.Toolkit;

namespace _2KinectDividedCard
{
    partial class RecognitionWindow
    {

        void LoadAllGestureDetectors()
        {
            foreach (var gesture in GlobalData.GesturePostureSettings)
            {
                try
                {
                    if (gesture.Type == "g" && gesture.Algorithm == "t")
                    {
                        LoadTemplatedGestureDetector(gesture.ID);
                    }
                    else if (gesture.Type == "g" && gesture.Algorithm == "a")
                    {
                        initialAlgorithmicGestureDetector(gesture.ID, gesture.Detector, gesture.Epsilon);
                    }
                }
                catch (Exception ex)
                {
                    log.Fatal(gesture.ID+"::"+ex);
                    throw ex;
                }
            }
        }

        void LoadAlllCombinedGesturePostureDetector()
        {
            foreach (var gesture in GlobalData.GesturePostureSettings)
            {
                try
                {
                    if (gesture.Type == "c" && gesture.Algorithm == "c")
                    {
                        LoadCombinedGesturePostureDetector(gesture.ID, gesture.Combinations, gesture.Epsilon);
                    }
                }
                catch (Exception ex)
                {
                    log.Fatal(gesture.ID + "::" + ex);
                    throw ex;
                }
            }
            
        }

        /// <summary>
        /// 對需要辨識的手勢偵測器，紀錄代表手勢的某關節點的軌跡
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="gestures"></param>
        void addJointPositionOnlyMust(Skeleton skeleton, List<GlobalData.GestureTypes> gestures)
        {
            foreach (var rec in gestures)
            {
                GesturePostureVO gesture = GlobalData.GesturePostureSettings.Find(delegate(GesturePostureVO vo) { return vo.ID == rec; });
                if (gesture.Type == "g")
                {
                    Joint joint = skeleton.Joints[gesture.GestureJoint];

                    if (joint.TrackingState != JointTrackingState.Tracked)
                        continue;

                    GestureDetectorList[gesture.ID].Add(  joint.Position, this.Kinect);
                }
                else if (gesture.Type == "c")
                {
                    addJointPositionOnlyMust(skeleton, gesture.Combinations);
                }
            }
        }

        void cleanTaskRecognitionActions()
        {
            foreach (var gesture in this.TaskRecognitionActions)
            {
                try
                {
                    this.TaskRecognitions.Find(delegate(string data) { return data == gesture.ToString(); });
                }
                catch (System.ArgumentNullException ane)
                {
                    log.Debug(ane);
                    TaskRecognitionActions.Remove(gesture);
                    cleanTaskRecognitionActions();
                    break;
                }
            }
        }
        
        /// <summary>
        /// 紀錄代表手勢的某關節點的軌跡
        /// </summary>
        /// <param name="skeleton"></param>
        [Obsolete("",false)]
        void AddJointPosition(Skeleton skeleton)
        {
            foreach (Joint joint in skeleton.Joints)  //Ryan:每個frame開始比對手勢（某一關節點的路徑）的程式入口
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                //處理UI控制，以右手當滑鼠
                if (joint.JointType == JointType.HandRight)
                {
                    MouseController.Current.SetHandPosition(Kinect, joint, skeleton);
                }

                foreach (var gesture in GlobalData.GesturePostureSettings)
                {
                    if (gesture.Type == "g")
                    {

                        if (joint.JointType == gesture.GestureJoint)
                        {
                            GestureDetectorList[gesture.ID].Add(joint.Position, this.Kinect);  //Ryan: add函式裡包含處理比對手勢
                        }
                    }
                }               
            }
        }

        void LoadTemplatedGestureDetector(GlobalData.GestureTypes gesture)
        {
            using (Stream recordStream = File.Open(GlobalData.GesturesTemplatePath + gesture.ToString() + ".save", FileMode.OpenOrCreate))
            {
                GestureDetectorList.Add(gesture, new TemplatedGestureDetector(gesture.ToString(), recordStream));
                GestureDetectorList[gesture].DisplayCanvas = gesturesCanvas;
                GestureDetectorList[gesture].OnGestureDetected += OnGestureDetected;
                
                ////以下寫法用以上寫法取代
                //circleGestureRecognizer = new TemplatedGestureDetector(GlobalData.GestureTypes.GCircle.ToString(), recordStream);
                ////circleGestureRecognizer.DisplayCanvas = gesturesCanvas;
                //circleGestureRecognizer.OnGestureDetected += OnGestureDetected;
                //Console.WriteLine("new circleGestureRecognizer");
                
                //MouseController.Current.ClickGestureDetector = circleGestureRecognizer;
            }
        }

        void initialAlgorithmicGestureDetector(GlobalData.GestureTypes gesture, string detector, int windowSize)
        {
            Type type = Activator.CreateInstance("Ryan.Kinect.GestureCommand", "Ryan.Kinect.GestureCommand.Service.Single." + detector).Unwrap().GetType();
            object[] arg = { gesture.ToString(), windowSize };
            Type[] ctorArgs = { typeof(string) , typeof(int) };
            ConstructorInfo ctor = type.GetConstructor(ctorArgs);
            GestureDetector gestureDetector = (GestureDetector)ctor.Invoke(arg);
            
            GestureDetectorList.Add(gesture, gestureDetector);
            GestureDetectorList[gesture].OnGestureDetected += OnGestureDetected;

        }
 

        void LoadCombinedGesturePostureDetector(GlobalData.GestureTypes gesture, List< GlobalData.GestureTypes > combinations, int epsilon)
        {
            CombinedGestureDetectorList.Add(gesture, new ParallelCombinedGesturePostureDetector(gesture.ToString(), epsilon));  //TODO 延遲時間，考慮抽參數
            CombinedGestureDetectorList[gesture].OnGestureDetected += OnGestureDetected;

            foreach (var combination in combinations)
            {
                if (GlobalData.GesturePostureSettings.Find(delegate(GesturePostureVO vo) { return vo.ID == combination; }).Type == "p")
                {
                    CombinedGestureDetectorList[gesture].Add(PostureDetectorList[combination]);
                }
                else if (GlobalData.GesturePostureSettings.Find(delegate(GesturePostureVO vo) { return vo.ID == combination; }).Type == "g")
                {
                    CombinedGestureDetectorList[gesture].Add(GestureDetectorList[combination]);
                }
                else
                {
                    string errMessage = "GesturePostureVO.Type 設定值錯誤！！！";
                    log.Fatal(errMessage);
                    throw new Exception(errMessage);
                }
            }

            /*
            ObjectRecognitionDetector = new ParallelCombinedGesturePostureDetector(GlobalData.GestureTypes.CObjectRecognition.ToString(), 3500);
            ObjectRecognitionDetector.OnGestureDetected += OnGestureDetected;
            ObjectRecognitionDetector.Add( FrontHandDetectorI);
            ObjectRecognitionDetector.Add( GestureDetectorList[GlobalData.GestureTypes.GCircle] );
            */

            Console.WriteLine("new " + gesture);
        }

        private void recordGesture_Click(object sender, RoutedEventArgs e)
        {
            if (((TemplatedGestureDetector)GestureDetectorList[GlobalData.GestureTypes.GCircle]).IsRecordingPath)
            {
                ((TemplatedGestureDetector)GestureDetectorList[GlobalData.GestureTypes.GCircle]).EndRecordTemplate();
                //recordGesture.Content = "Record Gesture"; //UI
                return;
            }
            Console.WriteLine("Ryan::MainWindow.recordGesture_Click(object sender, RoutedEventArgs e)");
            ((TemplatedGestureDetector)GestureDetectorList[GlobalData.GestureTypes.GCircle]).StartRecordTemplate();
            //recordGesture.Content = "Stop Recording"; //UI
        }

        void OnGestureDetected(string gesture)
        {
            if (gesture == null)
                return;

            string showMeg = gesture;
            //int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));  //UI
            //detectedGestures.SelectedIndex = pos;  //UI

            //TODO 物件辨識偵測器，add節點和姿勢，尚未實做
            if (GlobalData.GestureTypes.CObjectRecognition.ToString() == gesture)
            {
                GestureCommand = GlobalData.GestureTypes.CObjectRecognition;
                textCommands.Text = "物件辨識中...";
            }

            //this.TaskRecognitionActions.Remove((GlobalData.GestureTypes)Enum.Parse(typeof(GlobalData.GestureTypes), gesture, true));
            this.TaskRecognitions.Remove(gesture);
            textBlockGestureResult.Text = showMeg;

            checkFinishRecognition();
            
        }

        void CloseGestureDetector()
        {
            if ( !GestureDetectorList.ContainsKey(GlobalData.GestureTypes.GCircle)  || ((TemplatedGestureDetector)GestureDetectorList[GlobalData.GestureTypes.GCircle]) == null)
                return;

            using (Stream recordStream = File.Create(GlobalData.GesturesTemplatePath + GlobalData.GestureTypes.GCircle + ".save"))
            {
                ((TemplatedGestureDetector)GestureDetectorList[GlobalData.GestureTypes.GCircle]).SaveState(recordStream);
            }
            GestureDetectorList[GlobalData.GestureTypes.GCircle].OnGestureDetected -= OnGestureDetected;
        }
    }
}
