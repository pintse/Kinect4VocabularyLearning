using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class DrinkDetector : PostureDetector
    {
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public DrinkDetector() : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? headPosition = null;
            Vector3? leftHandPosition = null;
            Vector3? rightHandPosition = null;

            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.Head:
                        headPosition = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        leftHandPosition = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHandPosition = joint.Position.ToVector3();
                        break;
                }
            }


            if (check(headPosition, leftHandPosition))
            {
                RaisePostureDetected("Drink");
                return;
            }
            
            
            Reset();
        }

        private bool check(Vector3? headPosition, Vector3? handPosition)
        {
            if (!handPosition.HasValue || !headPosition.HasValue)
                return false;

            if (Math.Abs(handPosition.Value.X - headPosition.Value.X) > 0.05)
                return false;

            if (Math.Abs(handPosition.Value.Y - headPosition.Value.Y) > 0.03)
                return false;

            if (Math.Abs(handPosition.Value.Z - headPosition.Value.Z) > 0.4)
                return false;

            return true;
        }
    }
}
