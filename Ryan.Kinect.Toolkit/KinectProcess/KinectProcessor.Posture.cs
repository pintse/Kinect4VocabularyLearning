using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.Toolkit.KinectProcess
{
    /// <summary>
    /// Presentation Kinect 串流相關處理 -- Posture偵測處理
    /// </summary>
    partial class KinectProcessor
    {
        public void LoadAllPostureDetectors()
        {
            foreach (var posture in GlobalData.GesturePostureSettings)
            {
                try
                {
                    addGestureCommands(posture);

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
        public void trackPosturesOnlyMust(Skeleton skeleton, List<GlobalData.GestureTypes> postures)
        {
            foreach (var rec in postures)
            {
                GesturePostureVO posture = GlobalData.GesturePostureSettings.Find(delegate(GesturePostureVO vo) { return vo.ID == rec; });

                if (posture.Type == "p")
                {
                    PostureDetectorList[posture.ID].TrackPostures(skeleton);
                }
                else if (posture.Type == "c" || posture.Type == "s")
                {
                    trackPosturesOnlyMust(skeleton, posture.Combinations);
                }
            }
        }


        void initialAlgorithmicPostureDetector(GlobalData.GestureTypes posture, string detector)
        {

            var x = Activator.CreateInstance(GlobalData.ModuleName, detector);
            PostureDetector postureDetector = (PostureDetector)x.Unwrap();

            PostureDetectorList.Add(posture, postureDetector);
            PostureDetectorList[posture].PostureDetected += OnAlgorithmicPostureDetected;

        }

        /// <summary>
        /// Ryan create
        /// </summary>
        /// <param name="posture"></param>
        void OnAlgorithmicPostureDetected(string posture)
        {
            //int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));

            //detectedGestures.SelectedIndex = pos;

            if (posture == null)
                return;

            this.TaskRecognitions.Remove(posture);
            
            this.ActionRecognitionResults = posture;
            ((MyWindow)this.UI).gestureFinish(posture);
            //checkFinishRecognition(gesture);        
            
            log.Debug("remove p:" + posture + "-->TaskRecognitions.Count::" + this.TaskRecognitions.Count);

            //this.TextBlockRecognitionResults[0].Text = posture;
            this.ActionRecognitionResults = posture; //Debug用

            //checkFinishRecognition(posture);

        }

    }
}
