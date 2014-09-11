using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PHandLeftHorizontalNearShoulderDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandLeftHorizontalNearShoulder;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHandLeftHorizontalNearShoulderDetector()
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
            Vector3? leftHand = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            Vector3? leftElbow = skeleton.Joints[JointType.ElbowLeft].Position.ToVector3();
            Vector3? rightHand = skeleton.Joints[JointType.HandRight].Position.ToVector3();

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
                        leftHand = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHand = joint.Position.ToVector3();
                        break;
                    case JointType.ElbowLeft:
                        leftElbow =  joint.Position.ToVector3();
                        break;   
                }
            }*/


            if (check(shoulderCenter, leftHand, leftElbow, rightHand))
            {
                RaisePostureDetected( Name.ToString());
                return;
            }
            else
            {
            }

            Reset();
        }

        private bool check(Vector3? shoulderCenter, Vector3? handLeft, Vector3? elbow, Vector3? handRight)
        {

            if (!handLeft.HasValue || !shoulderCenter.HasValue || !elbow.HasValue || !handRight.HasValue)
                return false;

            if (Math.Abs(elbow.Value.Y - handLeft.Value.Y) > 0.2 || shoulderCenter.Value.Y < handLeft.Value.Y ||
                shoulderCenter.Value.Y - handLeft.Value.Y > 0.2 || handLeft.Value.Y < handRight.Value.Y)
                return false;

            return true;
        }
    }
}
