using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PHeadDownAndHandOnHeadDetector  : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHeadDownAndHandOnHead;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHeadDownAndHandOnHeadDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? head = skeleton.Joints[JointType.Head].Position.ToVector3();
            Vector3? handRight = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? handLeft = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            Vector3? shoulderCenter = skeleton.Joints[JointType.ShoulderCenter].Position.ToVector3();
            Vector3? spine = skeleton.Joints[JointType.Spine].Position.ToVector3();
            
            
            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.Head:
                        head = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        handRight = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        handLeft = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderCenter:
                        shoulderCenter = joint.Position.ToVector3();
                        break;
                }
            }*/

            if (check(head, handRight, spine) || check(head, handLeft, spine))
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

        private bool check(Vector3? head, Vector3? hand, Vector3? spine)
        {

            if (!hand.HasValue || !head.HasValue || !spine.HasValue)
                {
                    //Console.WriteLine("return false::" + hand.HasValue + "," + knee.HasValue +" , "+ shoulder.HasValue +" , "+ hipCenter.HasValue);
                    return false;
                }

                //Console.WriteLine("PHandsOnKneeAndHeadLeanForwardDetector::" + hipCenter.Value.Z + " , " + shoulder.Value.Z + " , " + hand.Value.X + " , " + knee.Value.X + " , " +
                    //hand.Value.Y + " , " + knee.Value.Y + " , " + hand.Value.Z + " , " + knee.Value.Z);
                float chkValue1 = 0.09f;
                float chkValue2 = 0.3f;
                float chkValue3 = 0.7f;
                float chkValue4 = 0.1f;

                /*PostureDetector.Coordinate4Test = "<" + chkValue1 + "::" + (shoulderCenter.Value.Z - head.Value.Z)+" \n"+
                    ">" + chkValue2 + "::" + Math.Abs(hand.Value.X - head.Value.X)+" \n"+
                    ">" + chkValue3 + "::" + Math.Abs(hand.Value.Y - head.Value.Y)+" \n"+
                    ">" + chkValue4 + "::" + Math.Abs(hand.Value.Z - head.Value.Z)+" \n"; */

                if ((spine.Value.Z - head.Value.Z) < chkValue1 || Math.Abs(hand.Value.X - head.Value.X) > chkValue2 || 
                    Math.Abs(hand.Value.Y - head.Value.Y) > chkValue3 || Math.Abs(hand.Value.Z - head.Value.Z) > chkValue4)
                    return false;
            
            return true;
        }
    }
}