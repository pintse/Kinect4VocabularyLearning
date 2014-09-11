using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using log4net;

namespace Ryan.Kinect.GestureCommand.Service
{
    /// <summary>
    /// 偵測多個手勢和姿勢同時進行
    /// </summary>
    public class ParallelCombinedGesturePostureDetector : CombinedGesturePostureDetector
    {
        private static ILog log = LogManager.GetLogger(typeof(ParallelCombinedGesturePostureDetector));

        DateTime? firstDetectedGestureTime;
        List<string> detectedGesturesName = new List<string>();

        public ParallelCombinedGesturePostureDetector(string name, double epsilon = 1000)
            : base(name, epsilon)
        {
        }

        protected override void CheckGestures(string gesture)
        {
            try
            {

                //Console.WriteLine("ParallelCombinedGesturePostureDetector:" + Name);

                if (!firstDetectedGestureTime.HasValue || DateTime.Now.Subtract(firstDetectedGestureTime.Value).TotalMilliseconds > Epsilon)
                {

                    firstDetectedGestureTime = DateTime.Now;
                    detectedGesturesName.Clear();
                }

                if (detectedGesturesName.Contains(gesture))
                    return;

                detectedGesturesName.Add(gesture);                

                if (detectedGesturesName.Count == GesturePostureDetectorsCount)
                {
                    log.Debug(Name+"::"+string.Join("&", detectedGesturesName));
                    //RaiseGestureDetected(string.Join("&", detectedGesturesName));
                    RaiseGestureDetected(Name);
                    firstDetectedGestureTime = null;
                    PostureDetector.Coordinate4Test = Name;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                Console.WriteLine(ex);
                throw ex;
            }
        }
    }
}
