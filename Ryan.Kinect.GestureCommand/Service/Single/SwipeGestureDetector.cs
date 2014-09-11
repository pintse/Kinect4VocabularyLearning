using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class SwipeGestureDetector : GestureDetector  //Ryan:Algorithmic search作法
    {
        public float SwipeMinimalLength { get; set; }
        public float SwipeMaximalHeight { get; set; }
        public int SwipeMininalDuration { get; set; }
        public int SwipeMaximalDuration { get; set; }

        DateTime? firstDetectedGestureTime;
        private double Epsilon = 500;
        private bool SwipeToRightFlag = false, SwipeToLeftFlag = false;

        readonly string GestureName; 

        public SwipeGestureDetector(int windowSize = 20)
            : base(windowSize)
        {
            SwipeMinimalLength = 0.3f;
            SwipeMaximalHeight = 0.3f;
            SwipeMininalDuration = 250;
            SwipeMaximalDuration = 2500;
        }

        public SwipeGestureDetector(string gestureName, int windowSize = 20)
            : this(windowSize)
        {
            this.GestureName = gestureName;
        }

        public SwipeGestureDetector(): this(20) 
        {
        }

        protected bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction,
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!heightFunction(Entries[0].Position, Entries[index].Position) || !directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    start = index;
                }

                if (lengthFunction(Entries[index].Position, Entries[start].Position))
                {
                    double totalMilliseconds = (Entries[index].Time - Entries[start].Time).TotalMilliseconds;
                    if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void LookForGesture()  //Ryan:Algorithmic search作法
        {
            // Swipe to right
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height //設定heightFunction的定義Func<Vector3, Vector3, bool>，第一個Vector3為p1,第二個Vector3為p2，bool為『Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight』的運算結果，將這樣的定義當作參數傳入ScanPositions中，ScanPositions內使用這個『有運算定義』的參數給予p1,p2的值，然後得到運算後的結果
                (p1, p2) => p2.X - p1.X > -0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                if (!firstDetectedGestureTime.HasValue || DateTime.Now.Subtract(firstDetectedGestureTime.Value).TotalMilliseconds > Epsilon)
                {
                    firstDetectedGestureTime = DateTime.Now;
                    this.SwipeToLeftFlag = false;
                }

                this.SwipeToRightFlag = true;

                //RaiseGestureDetected("SwipeToRight"+"::"+this.GestureName);
                //return;
            }

            // Swipe to left
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  // Height
                (p1, p2) => p2.X - p1.X < 0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration))// Duration
            {
                if (!firstDetectedGestureTime.HasValue || DateTime.Now.Subtract(firstDetectedGestureTime.Value).TotalMilliseconds > Epsilon)
                {
                    firstDetectedGestureTime = DateTime.Now;
                    this.SwipeToRightFlag = false;
                }
                this.SwipeToLeftFlag = true;

                //RaiseGestureDetected("SwipeToLeft" + "::" + this.GestureName);
                //return;

            }

            if (this.SwipeToLeftFlag && this.SwipeToRightFlag)
            {
                firstDetectedGestureTime = null;
                this.SwipeToLeftFlag = false;
                this.SwipeToRightFlag = false;
                RaiseGestureDetected(this.GestureName);
                return;
            }

        }
    }
}