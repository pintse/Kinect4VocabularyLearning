using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PFrontHandRightDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PFrontHandRight;

        public float Epsilon { get; set; }
        public float MaxRange { get; set; }

        public PFrontHandRightDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? shoulderCenter = skeleton.Joints[JointType.ShoulderCenter].Position.ToVector3();
            Vector3? rightHandPosition = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            //Vector3? rightHandPosition = null;

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


            if (check(shoulderCenter, rightHandPosition))
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

        private bool check(Vector3? shoulderCenter, Vector3? handPosition)
        {
            //Console.WriteLine("手伸：" + Math.Abs(handPosition.Value.Z - shoulderCenter.Value.Z));
            if (!handPosition.HasValue || !shoulderCenter.HasValue)
                return false;

            if (shoulderCenter.Value.Z - handPosition.Value.Z < 0.45 || Math.Abs(shoulderCenter.Value.Y - handPosition.Value.Y) > 0.2)
                return false;

            return true;
        }
    }
}
