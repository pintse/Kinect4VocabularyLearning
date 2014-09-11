using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinect.Toolbox
{
    /// <summary>
    /// Ryan: With the ParallelCombinedGestureDetector and the SerialCombinedGestureDetector, you will be able to compose gestures (in a parallel or serial way) in order to create really complex gestures.
    /// </summary>
    public class ParallelCombinedGestureDetector : CombinedGestureDetector
    {
        DateTime? firstDetectedGestureTime;
        List<string> detectedGesturesName = new List<string>();

        public ParallelCombinedGestureDetector(double epsilon = 1000)
            : base(epsilon)
        {
        }

        protected override void CheckGestures(string gesture)
        {
            Console.WriteLine("Ryan::ParallelCombinedGestureDetector.CheckGestures(string gesture)");  //Ryan:當手畫圈圈時，畫完判斷出圈圈的同時差不多也印出這一行
            if (!firstDetectedGestureTime.HasValue || detectedGesturesName.Contains(gesture) || DateTime.Now.Subtract(firstDetectedGestureTime.Value).TotalMilliseconds > Epsilon)
            {
                firstDetectedGestureTime = DateTime.Now;
                detectedGesturesName.Clear();
            }

            detectedGesturesName.Add(gesture);

            if (detectedGesturesName.Count == GestureDetectorsCount)
            {
                RaiseGestureDetected(string.Join("&", detectedGesturesName));
                firstDetectedGestureTime = null;
            }
        }
    }
}
