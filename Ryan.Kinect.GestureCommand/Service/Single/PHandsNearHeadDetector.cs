using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    class PHandsNearHeadDetector  : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandsNearHead;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHandsNearHeadDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            //if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
            //    return;

            Vector3? head = skeleton.Joints[JointType.Head].Position.ToVector3();
            Vector3? rightHand = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? leftHand = skeleton.Joints[JointType.HandLeft].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                //if (joint.TrackingState != JointTrackingState.Tracked)
                //    continue;

                switch (joint.JointType)
                {
                    case JointType.Head:
                        head = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHand = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        leftHand = joint.Position.ToVector3();
                        break;
                }
            }*/
             
            //RaisePostureDetected(check(head, leftHand, rightHand,1));

            if (check(head, leftHand, rightHand))
            {
                RaisePostureDetected(Name.ToString());
                
                return;
            }
            

            Reset();
        }

        private string check(Vector3? head, Vector3? leftHand, Vector3? rightHand, int i)
        {

            if (!leftHand.HasValue || !head.HasValue || !rightHand.HasValue)
                return "no value";
            if (Math.Abs(leftHand.Value.Y - head.Value.Y) > 0.05)
                return "1";

            if (Math.Abs(rightHand.Value.Y - head.Value.Y) > 0.05)
                return "2";

            if (rightHand.Value.X < head.Value.X) 
                return "3";

            if (head.Value.X < leftHand.Value.X)
                return "4";

            if ((rightHand.Value.X - head.Value.X) > 0.1)
                return "5";

            if ((head.Value.X - leftHand.Value.X) > 0.1)
                return "6";


            
            return "都過了";
        }

        private bool check(Vector3? head, Vector3? leftHand, Vector3? rightHand)
        {

            if (!leftHand.HasValue || !head.HasValue || !rightHand.HasValue)
                return false;

            if (Math.Abs(leftHand.Value.Y - head.Value.Y) > 0.05 || Math.Abs(rightHand.Value.Y - head.Value.Y) > 0.05 ||
                    (rightHand.Value.X < head.Value.X) || head.Value.X < leftHand.Value.X  ||
                    (rightHand.Value.X - head.Value.X) > 0.13 || (head.Value.X - leftHand.Value.X) > 0.13)
                    return false;
            
            return true;
        }
    }
}
