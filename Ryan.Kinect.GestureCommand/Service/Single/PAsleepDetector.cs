using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PAsleepDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PAsleep;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PAsleepDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            //if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                //return;

            Vector3? shoulderCenter = skeleton.Joints[JointType.ShoulderCenter].Position.ToVector3();
            Vector3? leftHandPosition = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            Vector3? rightHandPosition = skeleton.Joints[JointType.HandRight].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.ShoulderCenter:
                        shoulderCenter = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        leftHandPosition = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHandPosition = joint.Position.ToVector3();
                        break;  
                }
            }*/

            /*if (leftHandPosition.HasValue && headPosition.HasValue)
            {
                Console.WriteLine(skeleton.TrackingId+":" + (leftHandPosition.Value.X - headPosition.Value.X) + ", " + leftHandPosition.Value.X + " , " + headPosition.Value.X);
                Console.WriteLine(skeleton.TrackingId+":" + (leftHandPosition.Value.Y - headPosition.Value.Y) + ", " + leftHandPosition.Value.Y + " , " + headPosition.Value.Y);
                Console.WriteLine(skeleton.TrackingId+":" + (leftHandPosition.Value.Z - headPosition.Value.Z) + ", " + leftHandPosition.Value.Z + " , " + headPosition.Value.Z);
            }*/


            if (check(shoulderCenter, leftHandPosition, rightHandPosition))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            else
            {
                /*if (leftHandPosition.HasValue && headPosition.HasValue)
                {
                    Console.WriteLine("@@@@@@@");
                    Console.WriteLine(skeleton.TrackingId + ":" + (leftHandPosition.Value.X - headPosition.Value.X) + ", " + leftHandPosition.Value.X + " , " + headPosition.Value.X);
                    Console.WriteLine(skeleton.TrackingId + ":" + (leftHandPosition.Value.Y - headPosition.Value.Y) + ", " + leftHandPosition.Value.Y + " , " + headPosition.Value.Y);
                    Console.WriteLine(skeleton.TrackingId + ":" + (leftHandPosition.Value.Z - headPosition.Value.Z) + ", " + leftHandPosition.Value.Z + " , " + headPosition.Value.Z);
                }*/
            }

            Reset();
        }

        private bool check(Vector3? shoulderCenter, Vector3? leftHandPosition, Vector3? rightHandPosition)
        {

            if (!rightHandPosition.HasValue || !leftHandPosition.HasValue || !shoulderCenter.HasValue)
                return false;

            if (shoulderCenter.Value.X > leftHandPosition.Value.X || shoulderCenter.Value.X > rightHandPosition.Value.X
                || leftHandPosition.Value.X - shoulderCenter.Value.X > 0.17 || rightHandPosition.Value.X - shoulderCenter.Value.X > 0.17
                || Math.Abs(shoulderCenter.Value.Y - leftHandPosition.Value.Y) > 0.17 || Math.Abs(shoulderCenter.Value.Y - rightHandPosition.Value.Y) > 0.17)
                return false;

            return true;
        }
    }
}
