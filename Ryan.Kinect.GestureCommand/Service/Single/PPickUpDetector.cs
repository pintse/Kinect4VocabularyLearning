using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PPickUpDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PPickUp;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PPickUpDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            //if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                //return;

            Vector3? spine = skeleton.Joints[JointType.Spine].Position.ToVector3();
            Vector3? head = skeleton.Joints[JointType.Head].Position.ToVector3();
            Vector3? rightHand = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? rightKnee = skeleton.Joints[JointType.KneeRight].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.Spine:
                        spine = joint.Position.ToVector3();
                        break;
                    case JointType.Head:
                        head = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHand = joint.Position.ToVector3();
                        break;
                    case JointType.KneeRight:
                        rightKnee = joint.Position.ToVector3();
                        break;
                }
            }*/


            if (check(spine, head, rightHand, rightKnee))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            

            Reset();
        }

        private bool check(Vector3? spine, Vector3? head, Vector3? rightHand, Vector3? rightKnee)
        {

            if (!rightHand.HasValue || !head.HasValue || !spine.HasValue || !rightKnee.HasValue)
                return false;

            if (spine.Value.Z - head.Value.Z > 0.02 && Math.Abs(rightHand.Value.Y - rightKnee.Value.Y) < 0.2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
