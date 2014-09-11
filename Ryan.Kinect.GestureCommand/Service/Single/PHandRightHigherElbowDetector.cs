using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PHandRightHigherElbowDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandRightHigherElbow;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHandRightHigherElbowDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? elbowRight = skeleton.Joints[JointType.ElbowRight].Position.ToVector3();
            Vector3? handRight = skeleton.Joints[JointType.HandRight].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                //if (joint.TrackingState != JointTrackingState.Tracked)
                //    continue;

                switch (joint.JointType)
                {
                    case JointType.ElbowRight:
                        elbowRight = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        handRight = joint.Position.ToVector3();
                        break;
           
                }
            }*/

            if (check(elbowRight, handRight))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            

            Reset();
        }

        private bool check(Vector3? elbow, Vector3? hand)
        {
            if (!hand.HasValue || !elbow.HasValue )
                return false;

            //Console.WriteLine("PPopeyeDetector::" + elbow.Value.Y + "," + hand.Value.Y + "," + shoulder.Value.Y);

            if (hand.Value.Y < elbow.Value.Y || Math.Abs(hand.Value.Y - elbow.Value.Y) < 0.2 )
                return false;

            return true;
        }
    }
}
