using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Ryan.Kinect.GestureCommand.Service
{
    /// <summary>
    /// 偵測多個手勢和姿勢依序進行
    /// </summary>
    public class SerialCombinedGesturePostureDetector : CombinedGesturePostureDetector
    {
        private static ILog log = LogManager.GetLogger(typeof(ParallelCombinedGesturePostureDetector));

        DateTime? previousGestureTime;
        List<string> detectedGesturesName = new List<string>();

        public SerialCombinedGesturePostureDetector(string name, double epsilon = 2000)
            : base(name, epsilon)
        {
        }

        protected override void CheckGestures(string gesture)
        {            
            var currentTime = DateTime.Now;

            if (!previousGestureTime.HasValue || detectedGesturesName.Contains(gesture) || currentTime.Subtract(previousGestureTime.Value).TotalMilliseconds > Epsilon)
            {
                detectedGesturesName.Clear();
            }

            previousGestureTime = currentTime;

            detectedGesturesName.Add(gesture);

            if (detectedGesturesName.Count == GesturePostureDetectorsCount)
            {
                log.Debug(string.Join(">", detectedGesturesName));
                RaiseGestureDetected(Name);
                previousGestureTime = null;
            }
        }
    }
}
