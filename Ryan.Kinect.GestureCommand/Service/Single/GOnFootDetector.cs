using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using log4net;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class GOnFootDetector : GestureDetector  //Ryan:Algorithmic search作法
    {
        private static ILog log = LogManager.GetLogger(typeof(GOnFootDetector));

        public float MoveMinimalLength { get; set; }
        public float MoveMaximalWidth { get; set; }
        public int MoveMininalDuration { get; set; }
        public int MoveMaximalDuration { get; set; }

        readonly string GestureName; 

        public GOnFootDetector(int windowSize = 60)
            : base(windowSize)
        {
            MoveMinimalLength = 0.07f;
            //MoveMaximalWidth = 0.15f;
            MoveMininalDuration = 50;
            MoveMaximalDuration = 1500;
        }

        public GOnFootDetector(string gestureName, int windowSize = 60)
            : this(windowSize)
        {
            this.GestureName = gestureName;
        }

        public GOnFootDetector()
            : this(60) 
        {
        }       

        protected bool ScanPositions( Func<Vector3, Vector3, bool> directionFunction,
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    //log.Debug("重設start:" + index);
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
            
                if (ScanPositions((p1, p2) => p2.Y - p1.Y > 0.006f, // Progression to right
                    (p1, p2) => Math.Abs(p2.Y - p1.Y) > MoveMinimalLength, // Length
                    MoveMininalDuration, MoveMaximalDuration)) // Duration
                {
                    RaiseGestureDetected(this.GestureName);
                    return;
                }

        }
    }
}
