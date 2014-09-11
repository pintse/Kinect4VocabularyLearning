using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PRightHandHighAboutHipDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PRightHandHighAboutHip;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PRightHandHighAboutHipDetector()
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
                }
            }*/


            if (check(hipCenter, rightHand))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
           
            Reset();
        }

        private bool check(Vector3? hipCenter, Vector3? handPosition)
        {
            
            if (!handPosition.HasValue || !hipCenter.HasValue)
                return false;

            if (hipCenter.Value.Z - handPosition.Value.Z < 0.3 || Math.Abs(hipCenter.Value.Y - handPosition.Value.Y) > 0.1)
                return false;

            return true;
        }
    }
}
