using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PHandsOutstretchedDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandsOutstretched;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHandsOutstretchedDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? handRight = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? shoulderRight = skeleton.Joints[JointType.ShoulderRight].Position.ToVector3();
            Vector3? handLeft = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            Vector3? shoulderLeft = skeleton.Joints[JointType.ShoulderLeft].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.HandRight:
                        handRight = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderRight:
                        shoulderRight = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        handLeft = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderLeft:
                        shoulderLeft = joint.Position.ToVector3();
                        break;
                }
            }*/

            if (check(handLeft, shoulderLeft, handRight, shoulderRight))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            

            Reset();
        }

        private bool check(Vector3? handLeft, Vector3? shoulderLeft, Vector3? handRight, Vector3? shoulderRight)
        {
            try
            {
                if (!handLeft.HasValue || !shoulderLeft.HasValue || !handRight.HasValue || !shoulderRight.HasValue)
                {
                    //Console.WriteLine("return false::" + hand.HasValue + "," + knee.HasValue +" , "+ shoulder.HasValue +" , "+ hipCenter.HasValue);
                    return false;
                }

                //Console.WriteLine("PHandsOnKneeAndHeadLeanForwardDetector::" + hipCenter.Value.Z + " , " + shoulder.Value.Z + " , " + hand.Value.X + " , " + knee.Value.X + " , " +
                    //hand.Value.Y + " , " + knee.Value.Y + " , " + hand.Value.Z + " , " + knee.Value.Z);
                float chkValue1 = 0.3f;
                float chkValue2 = 1.1f;
                //float chkValue3 = 0.2f;
                //float chkValue4 = 0.1f;

                //PHandsOnKneeAndHeadLeanForwardDetector.Coordinate4Test = "" + (hipCenter.Value.Z - shoulder.Value.Z) + "<" + chkValue1 + ", \n " + Math.Abs(hand.Value.X - knee.Value.X) + ">" + chkValue2 +
                //    " , \n " + Math.Abs(hand.Value.Y - knee.Value.Y) + ">" + chkValue3 + " , \n " + Math.Abs(hand.Value.Z - knee.Value.Z) + ">" + chkValue4;

                //|| (shoulderLeft.Value.Z - handLeft.Value.Z) < chkValue1 || (shoulderRight.Value.Z - handRight.Value.Z) < chkValue1
                if ((handRight.Value.X - handLeft.Value.X) < chkValue2 )
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
