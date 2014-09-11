using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PRibDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PRib;

        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PRibDetector()
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

      
                //RaisePostureDetected(check(spine, handLeft, handRight,0));
            

            if (check(spine, handLeft, handRight))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            

            Reset();
        }

        private string check(Vector3? spine, Vector3? handLeft, Vector3? handRight, int i)
        {

            if (!handRight.HasValue || !handLeft.HasValue || !spine.HasValue )
                return "無值";

            if (! (handLeft.Value.X < handRight.Value.X))
            {
                return "右手X沒比左手大";
            }
            
            if(!(Math.Abs(spine.Value.X - handLeft.Value.X) < 0.2) )
            {
                return "中心點X減左手沒小於0.085";
            }

    if(!(Math.Abs(handRight.Value.X - spine.Value.X) < 0.2 ) )
            {
                return "4";
            }

    if(!(Math.Abs(handLeft.Value.Y - spine.Value.Y) < 0.2) )
            {
                return "5";
            }
    if(!(Math.Abs(handRight.Value.Y - spine.Value.Y) < 0.2) )
            {
                return "6";
            }
    if(!(Math.Abs(handLeft.Value.Z - spine.Value.Z) < 0.3 ) )
            {
                return "7";
            }
    if(!(Math.Abs(handRight.Value.Z - spine.Value.Z) < 0.3) )
            {
                return "8";
            }
    return "";
        }

        private bool check(Vector3? spine, Vector3? handLeft, Vector3? handRight)
        {

            if (!handRight.HasValue || !handLeft.HasValue || !spine.HasValue )
                return false;

            if ( (handLeft.Value.X < handRight.Value.X) && 
                Math.Abs(spine.Value.X - handLeft.Value.X) < 0.2 && Math.Abs(handRight.Value.X - spine.Value.X) < 0.2 && 
                Math.Abs(handLeft.Value.Y - spine.Value.Y) < 0.2 && Math.Abs(handRight.Value.Y - spine.Value.Y) < 0.2 &&
                Math.Abs(handLeft.Value.Z - spine.Value.Z) < 0.3 && Math.Abs(handRight.Value.Z - spine.Value.Z) < 0.3)
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
