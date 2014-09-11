using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using log4net;

namespace Ryan.Kinect.GestureCommand.Service
{
    /// <summary>
    /// 抽象物件。擴充Kinect.Toolbox，可以手勢和姿勢組合辨識
    /// </summary>
    public abstract class CombinedGesturePostureDetector : GestureDetector
    {
        private static ILog log = LogManager.GetLogger(typeof(CombinedGesturePostureDetector));

        List<Object> gesturePostureDetectors = new List<object>();
        //List<GestureDetector> gestureDetectors = new List<GestureDetector>();

        public string Name
        {
            get;
            private set;
        }

        public CombinedGesturePostureDetector(string name, double epsilon = 1000)
        {
            Epsilon = epsilon;
            Name = name;
        }

        public double Epsilon
        {
            get;
            private set;
        }

        public int GesturePostureDetectorsCount
        {
            get
            {
                return gesturePostureDetectors.Count;
            }
        }

        
        public void Add(Object detector)
        {

            try
            {
                GestureDetector gestureDetector = (GestureDetector)detector;
                gestureDetector.OnGestureDetected += gesturePostureDetector_OnGesturePostureDetected;

            }
            catch (InvalidCastException )
            {
                try
                {
                    PostureDetector postureDetector = (PostureDetector)detector;
                    postureDetector.PostureDetected += gesturePostureDetector_OnGesturePostureDetected;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    log.Fatal(ex);
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                log.Fatal(ex);
                throw ex;
            }

            gesturePostureDetectors.Add(detector);
        }

        public void Remove(Object detector)
        {
            try
            {
                GestureDetector gestureDetector = (GestureDetector)detector;
                gestureDetector.OnGestureDetected -= gesturePostureDetector_OnGesturePostureDetected;

            }
            catch (InvalidCastException)
            {
                try
                {
                    PostureDetector postureDetector = (PostureDetector)detector;
                    postureDetector.PostureDetected -= gesturePostureDetector_OnGesturePostureDetected;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    log.Fatal(ex);
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                log.Fatal(ex);
                throw ex;
            }

            gesturePostureDetectors.Remove(detector);

        }


        void gesturePostureDetector_OnGesturePostureDetected(string gesture)
        {
            CheckGestures(gesture);
        }

        protected abstract void CheckGestures(string gesture);

        protected override void LookForGesture()
        {
            // Do nothing
        }
    }
}
