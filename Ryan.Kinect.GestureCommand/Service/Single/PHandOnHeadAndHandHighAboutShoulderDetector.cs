using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PHandOnHeadAndHandHighAboutShoulderDetector  : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PHandOnHeadAndHandHighAboutShoulder;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PHandOnHeadAndHandHighAboutShoulderDetector()
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

            if (check(head, handRight, shoulderCenter, handLeft) || check(head, handLeft, shoulderCenter, handRight))
            {
                RaisePostureDetected(Name.ToString());
                
                return;
            }
            

            Reset();
        }

        private bool check(Vector3? head, Vector3? hand1, Vector3? shoulderCenter, Vector3? hand2)
        {

            if (!hand1.HasValue || !hand2.HasValue || !head.HasValue || !shoulderCenter.HasValue)
                {
                    //Console.WriteLine("return false::" + hand.HasValue + "," + knee.HasValue +" , "+ shoulder.HasValue +" , "+ hipCenter.HasValue);
                    return false;
                }

                //Console.WriteLine("PHandsOnKneeAndHeadLeanForwardDetector::" + hipCenter.Value.Z + " , " + shoulder.Value.Z + " , " + hand.Value.X + " , " + knee.Value.X + " , " +
                    //hand.Value.Y + " , " + knee.Value.Y + " , " + hand.Value.Z + " , " + knee.Value.Z);
                float chkValue1 = 0.2f;
                float chkValue2 = 0.3f;
                float chkValue3 = 0.3f;
                float chkValue4 = 0.2f;

                /*PostureDetector.Coordinate4Test = "shoulderCenter - hand2.Z:"+(Math.Abs(shoulderCenter.Value.Z - hand2.Value.Z) < chkValue1)+" \n"+
                    "shoulderCenter - hand2.X:"+(Math.Abs(shoulderCenter.Value.X - hand2.Value.X) > chkValue2)+ "\n"+
                    "shoulderCenter - hand2.Y:"+(Math.Abs(shoulderCenter.Value.Y - hand2.Value.Y) > chkValue3)+" \n"+
                    "hand1 - head.X:"+(Math.Abs(hand1.Value.X - head.Value.X) > chkValue2)+" \n"+
                    "hand1 - head.Y:" + (Math.Abs(hand1.Value.Y - head.Value.Y) > chkValue3) + " \n" +
                    "hand1 - head.Z:"+(Math.Abs(hand1.Value.Z - head.Value.Z) > chkValue4)+" \n";*/

                if (Math.Abs(shoulderCenter.Value.Z - hand2.Value.Z) < chkValue1 ||
                    Math.Abs(shoulderCenter.Value.X - hand2.Value.X) > chkValue2 ||
                    Math.Abs(shoulderCenter.Value.Y - hand2.Value.Y) > chkValue3 ||
                    Math.Abs(hand1.Value.X - head.Value.X) > chkValue2 || 
                    Math.Abs(hand1.Value.Y - head.Value.Y) > chkValue3 || 
                    Math.Abs(hand1.Value.Z - head.Value.Z) > chkValue4)
                    return false;
            
            return true;
        }
    }
}