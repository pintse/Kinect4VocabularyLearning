using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PHandsOnKneeAndHeadLeanForwardDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandsOnKneeAndHeadLeanForward;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHandsOnKneeAndHeadLeanForwardDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? kneeRight = skeleton.Joints[JointType.KneeRight].Position.ToVector3();
            Vector3? handRight = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? shoulderRight = skeleton.Joints[JointType.ShoulderRight].Position.ToVector3();
            Vector3? kneeLeft = skeleton.Joints[JointType.KneeLeft].Position.ToVector3();
            Vector3? handLeft = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            Vector3? shoulderLeft = skeleton.Joints[JointType.ShoulderLeft].Position.ToVector3();
            Vector3? hipCenter = skeleton.Joints[JointType.HipCenter].Position.ToVector3();
            Vector3? head = skeleton.Joints[JointType.Head].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.KneeRight:
                        kneeRight = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        handRight = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderRight:
                        shoulderRight = joint.Position.ToVector3();
                        break;
                    case JointType.KneeLeft:
                        kneeLeft = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        handLeft = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderLeft:
                        shoulderLeft = joint.Position.ToVector3();
                        break;
                    case JointType.HipCenter:
                        hipCenter = joint.Position.ToVector3();
                        break;
                }
            }*/

            if (check(kneeRight, handRight, head, hipCenter) && check(kneeLeft, handLeft, head, hipCenter))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            

            Reset();
        }

        private bool check(Vector3? knee, Vector3? hand, Vector3? head, Vector3? hipCenter)
        {
            try
            {
                if (!hand.HasValue || !knee.HasValue || !head.HasValue || !hipCenter.HasValue)
                {
                    //Console.WriteLine("return false::" + hand.HasValue + "," + knee.HasValue +" , "+ shoulder.HasValue +" , "+ hipCenter.HasValue);
                    return false;
                }

                //Console.WriteLine("PHandsOnKneeAndHeadLeanForwardDetector::" + hipCenter.Value.Z + " , " + shoulder.Value.Z + " , " + hand.Value.X + " , " + knee.Value.X + " , " +
                    //hand.Value.Y + " , " + knee.Value.Y + " , " + hand.Value.Z + " , " + knee.Value.Z);
                float chkValue1 = 0.15f;
                float chkValue2 = 0.18f;
                float chkValue3 = 0.2f;
                float chkValue4 = 0.18f;

                //PHandsOnKneeAndHeadLeanForwardDetector.Coordinate4Test = "" + (hipCenter.Value.Z - shoulder.Value.Z) + "<" + chkValue1 + ", \n " + Math.Abs(hand.Value.X - knee.Value.X) + ">" + chkValue2 +
                //    " , \n " + Math.Abs(hand.Value.Y - knee.Value.Y) + ">" + chkValue3 + " , \n " + Math.Abs(hand.Value.Z - knee.Value.Z) + ">" + chkValue4;

                if ((hipCenter.Value.Z - head.Value.Z) < chkValue1 || Math.Abs(hand.Value.X - knee.Value.X) > chkValue2 || Math.Abs(hand.Value.Y - knee.Value.Y) > chkValue3 || Math.Abs(hand.Value.Z - knee.Value.Z) > chkValue4)
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
    }
}
