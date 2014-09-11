using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using log4net;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class GPullHandDetector : GestureDetector  //Ryan:Algorithmic search作法
    {
        private static ILog log = LogManager.GetLogger(typeof(GPullHandDetector));

        public float SwipeMinimalLength { get; set; }
        public float SwipeMaximalHeight { get; set; }
        public int SwipeMininalDuration { get; set; }
        public int SwipeMaximalDuration { get; set; }

        readonly string GestureName; 

        public GPullHandDetector(int windowSize = 20)
            : base(windowSize)
        {
            SwipeMinimalLength = 0.2f;
            SwipeMaximalHeight = 0.3f;
            SwipeMininalDuration = 100;
            SwipeMaximalDuration = 150;
        }

        public GPullHandDetector(string gestureName, int windowSize = 20)
            : this(windowSize)
        {
            this.GestureName = gestureName;
        }

        public GPullHandDetector()
            : this(20) 
        {
        }

        protected bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction,
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        //protected bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, 
          //  Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!heightFunction(Entries[0].Position, Entries[index].Position) || !directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    //PostureDetector.Coordinate4Test = "heightFunction:" + heightFunction(Entries[0].Position, Entries[index].Position)+"\n"+
                    //    "directionFunction:" + directionFunction(Entries[index].Position, Entries[index + 1].Position)+"\n"+
                    //    "Entries.Clear()"+index;
                    
                    Entries.Clear();
                    return false;
                    //start = index;

                }

                //PostureDetector.Coordinate4Test = "heightFunction:" + heightFunction(Entries[0].Position, Entries[index].Position) + "\n" +
                //        "directionFunction:" + directionFunction(Entries[index].Position, Entries[index + 1].Position) + "\n" + index;

                if (lengthFunction(Entries[index].Position, Entries[start].Position))
                {
                    //PostureDetector.Coordinate4Test = PostureDetector.Coordinate4Test +"長度OK";
                    double totalMilliseconds = (Entries[index].Time - Entries[start].Time).TotalMilliseconds;
                    //if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    if (totalMilliseconds <= maxTime)
                    {
                        return true;
                    }
                }
            }

            return false;


        }

        protected override void LookForGesture()  //Ryan:Algorithmic search作法
        {
            // Push
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height //設定heightFunction的定義Func<Vector3, Vector3, bool>，第一個Vector3為p1,第二個Vector3為p2，bool為『Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight』的運算結果，將這樣的定義當作參數傳入ScanPositions中，ScanPositions內使用這個『有運算定義』的參數給予p1,p2的值，然後得到運算後的結果
                (p1, p2) => p2.Z - p1.Z > 0.02f, // Progression to push
                (p1, p2) => Math.Abs(p2.Z - p1.Z) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                RaiseGestureDetected(this.GestureName);
                //PostureDetector.Coordinate4Test = PostureDetector.Coordinate4Test+"\n"+GestureName;
                return;
            }


        }
    }
}
