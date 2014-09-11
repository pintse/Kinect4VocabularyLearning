using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.Service;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.Toolkit.KinectProcess
{
    /// <summary>
    /// Presentation Kinect 串流相關處理 -- Gesture偵測處理
    /// </summary>
    partial class KinectProcessor
    {
        public void LoadAllGestureDetectors()
        {
            foreach (var gesture in GlobalData.GesturePostureSettings)
            {
                try
                {
                    addGestureCommands(gesture);

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
                    log.Fatal(gesture.ID + "::" + ex);
                    throw ex;
                }
            }
        }

        void addGestureCommands( GesturePostureVO  gesture)
        {
            if (gesture.CommandFlg == "Y")
                GestureCommands.Add(gesture.ID);
        }

        public void LoadAlllCombinedGesturePostureDetector()
        {
            foreach (var gesture in GlobalData.GesturePostureSettings)
            {
                try
                {
                    addGestureCommands(gesture);

                    if (gesture.Type == "c" && gesture.Algorithm == "c")
                    {
                        LoadCombinedGesturePostureDetector(gesture.ID, gesture.Combinations, gesture.Epsilon);
                    }
                    else if (gesture.Type == "s" && gesture.Algorithm == "s")
                    {
                        LoadSerialCombinedGesturePostureDetector(gesture.ID, gesture.Combinations, gesture.Epsilon);
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
        public void addJointPositionOnlyMust(Skeleton skeleton, List<GlobalData.GestureTypes> gestures)
        {
            foreach (var rec in gestures)
            {
                GesturePostureVO gesture = GlobalData.GesturePostureSettings.Find(delegate(GesturePostureVO vo) { return vo.ID == rec; });
                if (gesture.Type == "g")
                {
                    Joint joint = skeleton.Joints[gesture.GestureJoint];

                    if (joint.TrackingState != JointTrackingState.Tracked)
                        continue;

                    GestureDetectorList[gesture.ID].Add(joint.Position, this.Kinect);
                }
                else if (gesture.Type == "c" || gesture.Type == "s")
                {
                    addJointPositionOnlyMust(skeleton, gesture.Combinations);
                }
            }            
        }

        public void generateRemidCommands()
        {
            if (this.GestureRemindCommands != null)
            {
                this.GestureRemindCommands.Clear();
                this.GestureRemindCommands = null;
            }
            this.GestureRemindCommands = new List<GlobalData.GestureTypes>(this.GestureCommands);
            foreach (var gesture in this.GestureCommands)
            {
                bool exists = this.TaskRecognitionActions.Exists(delegate(GlobalData.GestureTypes data) { return data == gesture; });
                if (exists)
                {
                    this.GestureRemindCommands.Remove(gesture);
                }
            }
        }
        
       

        void LoadTemplatedGestureDetector(GlobalData.GestureTypes gesture)
        {
            try
            {
                using (Stream recordStream = File.Open(GlobalData.GesturesTemplatePath + gesture.ToString() + ".save", FileMode.OpenOrCreate))
                {
                    GestureDetectorList.Add(gesture, new TemplatedGestureDetector(gesture.ToString(), recordStream));
                    //GestureDetectorList[gesture].DisplayCanvas = gesturesCanvas;
                    GestureDetectorList[gesture].OnGestureDetected += OnGestureDetected;

                }
            }
            catch (IOException ioex)
            {
                log.Info(ioex);
                LoadTemplatedGestureDetector(gesture);
                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }

        void initialAlgorithmicGestureDetector(GlobalData.GestureTypes gesture, string detector, int windowSize)
        {
            Type type = Activator.CreateInstance(GlobalData.ModuleName, detector).Unwrap().GetType();
            object[] arg = { gesture.ToString(), windowSize };
            Type[] ctorArgs = { typeof(string), typeof(int) };
            ConstructorInfo ctor = type.GetConstructor(ctorArgs);
            GestureDetector gestureDetector = (GestureDetector)ctor.Invoke(arg);

            GestureDetectorList.Add(gesture, gestureDetector);
            GestureDetectorList[gesture].OnGestureDetected += OnGestureDetected;

        }


        void LoadCombinedGesturePostureDetector(GlobalData.GestureTypes gesture, List<GlobalData.GestureTypes> combinations, int epsilon)
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

            Console.WriteLine("new " + gesture);
        }

        void LoadSerialCombinedGesturePostureDetector(GlobalData.GestureTypes gesture, List<GlobalData.GestureTypes> combinations, int epsilon)
        {
            CombinedGestureDetectorList.Add(gesture, new SerialCombinedGesturePostureDetector(gesture.ToString(), epsilon));  //TODO 延遲時間，考慮抽參數
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

            Console.WriteLine("new " + gesture);
        }

        void OnGestureDetected(string gesture)
        {
            if (gesture == null)
                return;

            string showMeg = gesture;
            //int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));  //UI
            //detectedGestures.SelectedIndex = pos;  //UI

            if (GlobalData.GestureTypes.GCircle.ToString() == gesture)
            {
                GestureCommand = GlobalData.GestureTypes.GCircle;
            }
            else
            {
 
                this.TaskRecognitions.Remove(gesture);
                
                this.ActionRecognitionResults = showMeg;
                ((MyWindow)this.UI).gestureFinish(gesture);
 
                
                log.Debug("remove g:" + gesture + "-->TaskRecognitions.Count::" + this.TaskRecognitions.Count);
            }

            

        }

        public void CloseGestureDetector()
        {
            if (!GestureDetectorList.ContainsKey(GlobalData.GestureTypes.GCircle) || ((TemplatedGestureDetector)GestureDetectorList[GlobalData.GestureTypes.GCircle]) == null)
                return;

            using (Stream recordStream = File.Create(GlobalData.GesturesTemplatePath + GlobalData.GestureTypes.GCircle + ".save"))
            {
                ((TemplatedGestureDetector)GestureDetectorList[GlobalData.GestureTypes.GCircle]).SaveState(recordStream);
            }
            GestureDetectorList[GlobalData.GestureTypes.GCircle].OnGestureDetected -= OnGestureDetected;
        }
    }
}
