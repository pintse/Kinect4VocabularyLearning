using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PPopeyeDetector  : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PPopeye;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PPopeyeDetector()
            : base(0)
        {
            Epsilon = 0.1f;
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? elbowRight = skeleton.Joints[JointType.ElbowRight].Position.ToVector3();
            Vector3? handRight = skeleton.Joints[JointType.HandRight].Position.ToVector3();
            Vector3? shoulderRight = skeleton.Joints[JointType.ShoulderRight].Position.ToVector3();
            Vector3? elbowLeft = skeleton.Joints[JointType.ElbowLeft].Position.ToVector3();
            Vector3? handLeft = skeleton.Joints[JointType.HandLeft].Position.ToVector3();
            Vector3? shoulderLeft = skeleton.Joints[JointType.ShoulderLeft].Position.ToVector3();

            /*
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.ElbowRight:
                        elbowRight = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        handRight = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderRight:
                        shoulderRight = joint.Position.ToVector3();
                        break;
                    case JointType.ElbowLeft:
                        elbowLeft = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        handLeft = joint.Position.ToVector3();
                        break;
                    case JointType.ShoulderLeft:
                        shoulderLeft = joint.Position.ToVector3();
                        break;
                }
            }*/


            if (check(elbowRight, handRight, shoulderRight))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }
            else if (check(elbowLeft, handLeft, shoulderLeft))
            {
                RaisePostureDetected( Name.ToString());
                return;
            }
            

            Reset();
        }

        private bool check(Vector3? elbow, Vector3? hand, Vector3? shoulder)
        {
            if (!hand.HasValue || !elbow.HasValue || !shoulder.HasValue)
                return false;

            //Console.WriteLine("PPopeyeDetector::" + elbow.Value.Y + "," + hand.Value.Y + "," + shoulder.Value.Y);

            if (Math.Abs(elbow.Value.Y - shoulder.Value.Y) > 0.05 || (hand.Value.Y - elbow.Value.Y < 0.2 ))
                return false;

            return true;
        }
    }
}

