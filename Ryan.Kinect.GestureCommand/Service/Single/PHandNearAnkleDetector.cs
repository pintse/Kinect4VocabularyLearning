using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PHandNearAnkleDetector : PostureDetector
    {
        public static string Coordinate4Test;
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandNearAnkle;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHandNearAnkleDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? ankleRight = skeleton.Joints[JointType.AnkleRight].Position.ToVector3();
            Vector3? handRight = skeleton.Joints[JointType.HandRight].Position.ToVector3();

            Vector3? ankleLeft = skeleton.Joints[JointType.AnkleLeft].Position.ToVector3();
            Vector3? handLeft = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            
            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.AnkleRight:
                        ankleRight = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        handRight = joint.Position.ToVector3();
                        break;
                    case JointType.AnkleLeft:
                        ankleLeft = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        handLeft = joint.Position.ToVector3();
                        break;
                    
                }
            }*/

            if (check(ankleRight, handRight) || check(ankleLeft, handLeft) || check(ankleRight, handLeft) || check(ankleLeft, handRight))
            {
                RaisePostureDetected(Name.ToString());
                
                return;
            }
            else
            {
                /*if (leftHandPosition.HasValue && headPosition.HasValue)
                {
                    Console.WriteLine("@@@@@@@");
                    Console.WriteLine(skeleton.TrackingId + ":" + (leftHandPosition.Value.X - headPosition.Value.X) + ", " + leftHandPosition.Value.X + " , " + headPosition.Value.X);
                    Console.WriteLine(skeleton.TrackingId + ":" + (leftHandPosition.Value.Y - headPosition.Value.Y) + ", " + leftHandPosition.Value.Y + " , " + headPosition.Value.Y);
                    Console.WriteLine(skeleton.TrackingId + ":" + (leftHandPosition.Value.Z - headPosition.Value.Z) + ", " + leftHandPosition.Value.Z + " , " + headPosition.Value.Z);
                }*/
            }

            Reset();
        }

        private bool check(Vector3? ankle, Vector3? hand)
        {
            try
            {
                if (!hand.HasValue || !ankle.HasValue )
                {
                    //Console.WriteLine("return false::" + hand.HasValue + "," + knee.HasValue +" , "+ shoulder.HasValue +" , "+ hipCenter.HasValue);
                    return false;
                }

                //Console.WriteLine("PHandsOnKneeAndHeadLeanForwardDetector::" + hipCenter.Value.Z + " , " + shoulder.Value.Z + " , " + hand.Value.X + " , " + knee.Value.X + " , " +
                    //hand.Value.Y + " , " + knee.Value.Y + " , " + hand.Value.Z + " , " + knee.Value.Z);
                float chkValue1 = 0.15f;
                float chkValue2 = 0.18f;
                float chkValue3 = 0.2f;
                float chkValue4 = 0.1f;

                //PHandsOnKneeAndHeadLeanForwardDetector.Coordinate4Test = "" + (hipCenter.Value.Z - shoulder.Value.Z) + "<" + chkValue1 + ", \n " + Math.Abs(hand.Value.X - knee.Value.X) + ">" + chkValue2 +
                //    " , \n " + Math.Abs(hand.Value.Y - knee.Value.Y) + ">" + chkValue3 + " , \n " + Math.Abs(hand.Value.Z - knee.Value.Z) + ">" + chkValue4;

                if ( Math.Abs(hand.Value.X - ankle.Value.X) > chkValue2 || Math.Abs(hand.Value.Y - ankle.Value.Y) > chkValue3 || Math.Abs(hand.Value.Z - ankle.Value.Z) > chkValue4)
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