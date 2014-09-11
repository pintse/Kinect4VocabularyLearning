using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PHandRightAboveDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandRightAbove;

        public float Epsilon { get; set; }
        public float MaxRange { get; set; }

        public PHandRightAboveDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            //Vector3? spine = skeleton.Joints[JointType.Spine].Position.ToVector3();
            Vector3? rightHandPosition = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? leftHandPosition = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            Vector3? leftElbow = skeleton.Joints[JointType.ElbowLeft].Position.ToVector3();
            Vector3? rightElbow = skeleton.Joints[JointType.ElbowRight].Position.ToVector3();

            
            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                if (spine != null && rightHandPosition != null)
                    break;

                switch (joint.JointType)
                {
                    case JointType.Spine:
                        spine = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHandPosition = joint.Position.ToVector3();
                        break;
                }
            }*/



            if (check(leftHandPosition, rightHandPosition, leftElbow, rightElbow))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
           

            Reset();
        }

        private bool check(Vector3? handLeft, Vector3? handRight, Vector3? elbowLeft, Vector3? elbowRight)
        {
            if (!handRight.HasValue || !handLeft.HasValue || !elbowLeft.HasValue || !elbowRight.HasValue)
                return false;

            if (handRight.Value.X > handLeft.Value.X || handRight.Value.Y < handLeft.Value.Y ||
                handRight.Value.Y < elbowLeft.Value.Y || (handRight.Value.Y - handLeft.Value.Y) < 0.1)
                return false;

            return true;
        }
    }
}
