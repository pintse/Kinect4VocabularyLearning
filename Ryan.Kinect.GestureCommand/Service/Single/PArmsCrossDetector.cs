using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PArmsCrossDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PArmsCross;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PArmsCrossDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? hipCenter = skeleton.Joints[JointType.HipCenter].Position.ToVector3();
            Vector3? rightHand = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? leftHand = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            Vector3? rightElbow = skeleton.Joints[JointType.ElbowRight].Position.ToVector3();
            Vector3? leftElbow = skeleton.Joints[JointType.ElbowLeft].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.HipCenter:
                        hipCenter = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHand = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        leftHand = joint.Position.ToVector3();
                        break;
                    case JointType.ElbowRight:
                        rightElbow = joint.Position.ToVector3();
                        break;
                    case JointType.ElbowLeft:
                        leftElbow = joint.Position.ToVector3();
                        break;
                }
            }*/


            if (check(hipCenter, leftHand, leftElbow, rightHand, rightElbow))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            else
            {
            }

            Reset();
        }

        private bool check(Vector3? hipCenter, Vector3? leftHand, Vector3? leftElbow, Vector3? rightHand, Vector3? rightElbow)
        {

            if (!leftHand.HasValue || !hipCenter.HasValue || !leftElbow.HasValue || !rightHand.HasValue || !rightElbow.HasValue)
                return false;

            /*PostureDetector.Coordinate4Test = Math.Abs(hipCenter.Value.Y - leftHand.Value.Y) + "\n" +
                Math.Abs(rightElbow.Value.Y - leftHand.Value.Y) + "\n" +
                Math.Abs(leftElbow.Value.Y - rightHand.Value.Y) + "\n" +
                Math.Abs(rightElbow.Value.Z - leftHand.Value.Z) + "\n" +
                Math.Abs(leftElbow.Value.Z - rightHand.Value.Z);
            */
            if (Math.Abs(hipCenter.Value.Y - leftHand.Value.Y) > 0.15 || rightHand.Value.X > leftHand.Value.X ||
                Math.Abs(rightElbow.Value.Y - leftHand.Value.Y) > 0.15 || Math.Abs(leftElbow.Value.Y - rightHand.Value.Y) > 0.15 ||
                Math.Abs(rightElbow.Value.Z - leftHand.Value.Z) > 0.25 || Math.Abs(leftElbow.Value.Z - rightHand.Value.Z) > 0.25)
                return false;

            return true;
        }
    }
}
