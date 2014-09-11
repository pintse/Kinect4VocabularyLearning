using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PHandRightOnLeftDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandRightOnLeft;

        public float Epsilon { get; set; }
        public float MaxRange { get; set; }

        public PHandRightOnLeftDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? spine = skeleton.Joints[JointType.Spine].Position.ToVector3();
            Vector3? rightHandPosition = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            
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

           

            if (check(spine, rightHandPosition))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
           

            Reset();
        }

        private bool check(Vector3? spine, Vector3? handPosition)
        {
            if (!handPosition.HasValue || !spine.HasValue)
                return false;

            if (spine.Value.X < handPosition.Value.X || spine.Value.X - handPosition.Value.X < 0.3)
                return false;

            return true;
        }
    }
}
