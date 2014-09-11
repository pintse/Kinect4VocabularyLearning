using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PHandRightBetweenHeadAndShoulderDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandRightBetweenHeadAndShoulder;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHandRightBetweenHeadAndShoulderDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? head = skeleton.Joints[JointType.Head].Position.ToVector3();
            Vector3? rightHand = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? shoulderCenter = skeleton.Joints[JointType.ShoulderCenter].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.Head:
                        head = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHand = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderCenter:
                        shoulderCenter = joint.Position.ToVector3();
                        break;
                }
            }*/


            if (check(head, rightHand, shoulderCenter))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            else
            {
            }

            Reset();
        }

        private bool check(Vector3? head, Vector3? rightHand, Vector3? shoulderCenter)
        {

            if (!rightHand.HasValue || !head.HasValue || !shoulderCenter.HasValue )
                return false;

            /*PostureDetector.Coordinate4Test = "" + (rightHand.Value.Y > head.Value.Y) + "\n" +
                (rightHand.Value.Y < shoulderCenter.Value.Y) + "\n" +
                (head.Value.Z - rightHand.Value.Z) + ":" + (head.Value.Z - rightHand.Value.Z > 0.3) + "\n" +
                (Math.Abs(rightHand.Value.X - shoulderCenter.Value.X))+":"+(Math.Abs(rightHand.Value.X - shoulderCenter.Value.X) > 0.17);
            */
            if ((rightHand.Value.Y > head.Value.Y) || (rightHand.Value.Y < shoulderCenter.Value.Y) ||
                (head.Value.Z - rightHand.Value.Z > 0.3) || 
                (Math.Abs(rightHand.Value.X - shoulderCenter.Value.X ) > 0.07))
                return false;

            return true;
        }
    }
}