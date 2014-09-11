using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PImpossibleDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PImpossible;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PImpossibleDetector()
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
            Vector3? handRight = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? handLeft = skeleton.Joints[JointType.HandLeft].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                //if (joint.TrackingState != JointTrackingState.Tracked)
                 //   continue;

                switch (joint.JointType)
                {
                    case JointType.Spine:
                        spine = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        handLeft = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        handRight = joint.Position.ToVector3();
                        break;
                    
                }
            }*/


            if (check(spine, handLeft, handRight))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }

            Reset();
        }

        private bool check(Vector3? spine, Vector3? handLeft, Vector3? handRight)
        {

            if (!handRight.HasValue || !handLeft.HasValue || !spine.HasValue )
                return false;

            if (handLeft.Value.X > handRight.Value.X && handLeft.Value.Y > spine.Value.Y && handRight.Value.Y > spine.Value.Y )
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
