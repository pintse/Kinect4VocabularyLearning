using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PTwistDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PTwist;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PTwistDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            //if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                //return;

            Vector3? hipRight = skeleton.Joints[JointType.HipRight].Position.ToVector3();
            Vector3? hipLeft = skeleton.Joints[JointType.HipLeft].Position.ToVector3();
            Vector3? shoulderRight = skeleton.Joints[JointType.ShoulderRight].Position.ToVector3();
            Vector3? shoulderLeft = skeleton.Joints[JointType.ShoulderLeft].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                //if (joint.TrackingState != JointTrackingState.Tracked)
                 //   continue;

                switch (joint.JointType)
                {
                    case JointType.HipLeft:
                        hipLeft = joint.Position.ToVector3();
                        break;
                    case JointType.HipRight:
                        hipRight = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderLeft:
                        shoulderLeft = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderRight:
                        shoulderRight = joint.Position.ToVector3();
                        break;
                    
                }
            }*/

      
                //RaisePostureDetected(check(spine, handLeft, handRight,0));
            

            if (check(shoulderLeft, shoulderRight, hipLeft, hipRight))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            

            Reset();
        }

        

        private bool check(Vector3? shoulderLeft, Vector3? shoulderRight, Vector3? hipLeft, Vector3? hipRight)
        {

            if (!shoulderRight.HasValue || !shoulderLeft.HasValue || !hipLeft.HasValue || !hipRight.HasValue)
                return false;

            if (Math.Abs(hipRight.Value.Z - hipLeft.Value.Z) < 0.1 &&
                Math.Abs(shoulderRight.Value.Z - shoulderLeft.Value.Z) > 0.2)
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
