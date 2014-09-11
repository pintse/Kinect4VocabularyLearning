using System.IO;
using System.Windows;
using Kinect.Toolbox;
using System;
using Ryan.Kinect.GestureCommand.Service.Single;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;
using System.Collections.Generic;

namespace _2KinectDividedCard
{
    partial class RecognitionWindowXX
    {
        void LoadAllPostureDetectors()
        {
            foreach (var posture in GlobalData.GesturePostureSettings)
            {
                try
                {
                    if (posture.Type == "p" && posture.Algorithm == "a")
                    {
                        initialAlgorithmicPostureDetector(posture.ID, posture.Detector);
                    }
                }
                catch (Exception ex)
                {
                    log.Fatal(posture.ID + "::" + ex);
                    throw ex;
                }
            }

            //initalDrinkPostureDetector();
            
        }

        /// <summary>
        /// 對需要辨識的姿勢偵測器，擷取目前骨架提供姿勢辨識
        /// </summary>
        /// <param name="skeleton"></param>
        void trackPosturesOnlyMust(Skeleton skeleton, List<GlobalData.GestureTypes> postures)
        {
            foreach (var rec in postures)
            {
                GesturePostureVO posture = GlobalData.GesturePostureSettings.Find(delegate(GesturePostureVO vo) { return vo.ID == rec; });

                if (posture.Type == "p")
                {
                    PostureDetectorList[posture.ID].TrackPostures(skeleton);
                }
                else if (posture.Type == "c")
                {
                    trackPosturesOnlyMust(skeleton, posture.Combinations);
                }
            }
        }

        /// <summary>
        /// 擷取目前骨架提供姿勢辨識
        /// </summary>
        /// <param name="skeleton"></param>
        void TrackPostures(Skeleton skeleton)
        {

            foreach (var posture in PostureDetectorList)
            {
                posture.Value.TrackPostures(skeleton);
            }

            /*
            //DrinkI.TrackPostures(skeleton);
            PostureDetectorList[GlobalData.GestureTypes.PFrontHand].TrackPostures(skeleton);
            //templatePostureDetector.TrackPostures(skeletons[i]); 
            */

            if (recordNextFrameForPosture)
            {
                templatePostureDetector.AddTemplate(skeleton);
                recordNextFrameForPosture = false;
                Console.WriteLine("Ryan::MainWindow.ProcessFrame(ReplaySkeletonFrame frame)::recordNextFrameForPosture=" + recordNextFrameForPosture + ", time::" + DateTime.Now.ToString());
            }
        }

        void LoadLetterTPostureDetector()
        {
            /* 目前沒用到
            using (Stream recordStream = File.Open(letterT_KBPath, FileMode.OpenOrCreate))
            {
                templatePostureDetector = new TemplatedPostureDetector("T", recordStream);
                templatePostureDetector.PostureDetected += templatePostureDetector_PostureDetected;
            }*/
        }

        /*
        void initalDrinkPostureDetector()
        {
            DrinkI = new DrinkDetector();  //TODO 這個處理應該要移到後段GestureCommand裡去處理
            DrinkI.PostureDetected += OnAlgorithmicPostureDetected;  //TODO 這個處理應該要移到後段GestureCommand裡去處理
        }*/

        void initialAlgorithmicPostureDetector(GlobalData.GestureTypes posture, string detector)
        {

            var x = Activator.CreateInstance("Ryan.Kinect.GestureCommand", "Ryan.Kinect.GestureCommand.Service.Single." + detector);
            PostureDetector postureDetector = (PostureDetector)x.Unwrap();

            PostureDetectorList.Add(posture, postureDetector);
            PostureDetectorList[posture].PostureDetected += OnAlgorithmicPostureDetected;

            ////以下作法被上面取代
            //FrontHandDetectorI = new FrontHandDetector();  
            //FrontHandDetectorI.PostureDetected += OnAlgorithmicPostureDetected;  
        }

        

        void ClosePostureDetector()
        {
            /* 目前沒用到
            if (templatePostureDetector == null)
                return;

            using (Stream recordStream = File.Create(letterT_KBPath))
            {
                templatePostureDetector.SaveState(recordStream);
            }
            templatePostureDetector.PostureDetected -= templatePostureDetector_PostureDetected;
            Console.WriteLine("Ryan::MainWindow.Postures.ClosePostureDetector()::撤銷事件註冊");
             */ 
        }

        void templatePostureDetector_PostureDetected(string posture)
        {
            Console.WriteLine("Ryan::MainWindow.Postures.templatePostureDetector_PostureDetected(string posture)");
            MessageBox.Show("Give me a......." + posture);
            Console.WriteLine("Ryan::MainWindow.Postures.templatePostureDetector_PostureDetected(string posture)::recordNextFrameForPosture=" + recordNextFrameForPosture);
        }

        /// <summary>
        /// Ryan create
        /// </summary>
        /// <param name="posture"></param>
        void OnAlgorithmicPostureDetected(string posture)
        {
            //int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));

            //detectedGestures.SelectedIndex = pos;

            //this.TaskRecognitionActions.Remove((GlobalData.GestureTypes)Enum.Parse(typeof(GlobalData.GestureTypes), posture, true));
            this.TaskRecognitions.Remove(posture);
            this.textBlockGestureResult.Text = posture;

            checkFinishRecognition();
        }

        private void recordT_Click(object sender, RoutedEventArgs e)
        {
            recordNextFrameForPosture = true;
            Console.WriteLine("Ryan::MainWindow.Postures.recordT_Click(object sender, RoutedEventArgs e)::recordNextFrameForPosture=" + recordNextFrameForPosture + ", time::" + DateTime.Now.ToString());
        }
    }
}
