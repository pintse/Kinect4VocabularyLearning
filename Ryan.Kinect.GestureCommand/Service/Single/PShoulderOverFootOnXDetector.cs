using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class PShoulderOverFootOnXDetector : PostureDetector
    {
        private GlobalData.GestureTypes Name = GlobalData.GestureTypes.PShoulderOverFootOnX;
        public float Epsilon {get;set;}
        public float MaxRange { get; set; }

        public PShoulderOverFootOnXDetector()
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
            Vector3? shoulderCenter = skeleton.Joints[JointType.ShoulderCenter].Position.ToVector3();

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
                    case JointType.ShoulderCenter:
                        shoulderCenter = joint.Position.ToVector3();
                        break;
                }
            }*/

            if (check( kneeLeft, shoulderRight, kneeRight, shoulderLeft))
            {
                RaisePostureDetected(Name.ToString());
                return;
            }

            Reset();
        }

        private bool check(Vector3? kneeLeft, Vector3? shoulderRight, Vector3? kneeRight, Vector3? shoulderLeft)
        {
            try
            {
                if ( !kneeLeft.HasValue || !shoulderRight.HasValue || !kneeRight.HasValue || !shoulderLeft.HasValue)
                {
                    //Console.WriteLine("return false::" + hand.HasValue + "," + knee.HasValue +" , "+ shoulder.HasValue +" , "+ hipCenter.HasValue);
                    return false;
                }

                //PostureDetector.Coordinate4Test = "shoulderLeft.Value.X:" + shoulderLeft.Value.X+" \n"+
                //    "shoulderRight.Value.X:" + shoulderRight.Value.X;

                //越往右，X值越大，
                if ((shoulderLeft.Value.X > kneeRight.Value.X)   || ( kneeLeft.Value.X > shoulderRight.Value.X))
                    return true;

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
