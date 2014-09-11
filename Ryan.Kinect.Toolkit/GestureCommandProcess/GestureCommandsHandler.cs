using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.Kinect.GestureCommand;
using Ryan.Kinect.Toolkit.ImageProcess;
using System.Drawing;
using System.Threading;
using log4net;
using Microsoft.Kinect;

namespace Ryan.Kinect.Toolkit.GestureCommands
{
    /// <summary>
    /// Presentation Gesture Handler
    /// </summary>
    public class GestureCommandsHandler
    {
        private static ILog log = LogManager.GetLogger(typeof(GestureCommandsHandler));

        private static GestureCommandsHandler _GestureCommandsHandler = new GestureCommandsHandler();


        private GestureCommandsHandler()
        {
        }

        public static GestureCommandsHandler getInstance()
        {
            return _GestureCommandsHandler;
        }

        public void prepareRelatedData()
        {
            GestureCommandRecognitionFacade gcrf = GestureCommandRecognitionFacade.getInstance();
            
            gcrf.prepareRelatedData();
        }


        /*private string getAllPosition(Skeleton fSkeleton)
        {
            //Joint jpl = fSkeletons[i].Joints[JointType.HandLeft];
            //LeftHandMessage.Text = "追蹤到左手 X=" + jpl.Position.X + ", Y=" + jpl.Position.Y + ", Z=" + jpl.Position.Z;
            StringBuilder sb = new StringBuilder();

            sb.Append(".AnkleLeft.X:" + fSkeleton.Joints[JointType.AnkleLeft].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.AnkleLeft].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.AnkleLeft].Position.Z);
            sb.Append(".AnkleRight.X:" + fSkeleton.Joints[JointType.AnkleRight].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.AnkleRight].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.AnkleRight].Position.Z);
            sb.Append(".ElbowLeft.X:" + fSkeleton.Joints[JointType.ElbowLeft].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.ElbowLeft].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.ElbowLeft].Position.Z);
            sb.Append(".ElbowRight.X:" + fSkeleton.Joints[JointType.ElbowRight].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.ElbowRight].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.ElbowRight].Position.Z);
            sb.Append(".FootLeft.X:" + fSkeleton.Joints[JointType.FootLeft].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.FootLeft].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.FootLeft].Position.Z);
            sb.Append(".FootRight.X:" + fSkeleton.Joints[JointType.FootRight].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.FootRight].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.FootRight].Position.Z);
            sb.Append(".HandLeft.X:" + fSkeleton.Joints[JointType.HandLeft].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.HandLeft].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.HandLeft].Position.Z);
            sb.Append(".HandRight.X:" + fSkeleton.Joints[JointType.HandRight].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.HandRight].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.HandRight].Position.Z);
            sb.Append(".Head.X:" + fSkeleton.Joints[JointType.Head].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.Head].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.Head].Position.Z);
            sb.Append(".HipCenter.X:" + fSkeleton.Joints[JointType.HipCenter].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.HipCenter].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.HipCenter].Position.Z);
            sb.Append(".HipLeft.X:" + fSkeleton.Joints[JointType.HipLeft].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.HipLeft].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.HipLeft].Position.Z);
            sb.Append(".HipRight.X:" + fSkeleton.Joints[JointType.HipRight].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.HipRight].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.HipRight].Position.Z);
            sb.Append(".KneeLeft.X:" + fSkeleton.Joints[JointType.KneeLeft].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.KneeLeft].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.KneeLeft].Position.Z);
            sb.Append(".KneeRight.X:" + fSkeleton.Joints[JointType.KneeRight].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.KneeRight].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.KneeRight].Position.Z);
            sb.Append(".ShoulderCenter.X:" + fSkeleton.Joints[JointType.ShoulderCenter].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.ShoulderCenter].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.ShoulderCenter].Position.Z);
            sb.Append(".ShoulderLeft.X:" + fSkeleton.Joints[JointType.ShoulderLeft].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.ShoulderLeft].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.ShoulderLeft].Position.Z);
            sb.Append(".ShoulderRight.X:" + fSkeleton.Joints[JointType.ShoulderRight].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.ShoulderRight].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.ShoulderRight].Position.Z);
            sb.Append(".Spine.X:" + fSkeleton.Joints[JointType.Spine].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.Spine].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.Spine].Position.Z);
            sb.Append(".WristLeft.X:" + fSkeleton.Joints[JointType.WristLeft].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.WristLeft].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.WristLeft].Position.Z);
            sb.Append(".WristRight.X:" + fSkeleton.Joints[JointType.WristRight].Position.X);
            sb.Append(":" + fSkeleton.Joints[JointType.WristRight].Position.Y);
            sb.Append(":" + fSkeleton.Joints[JointType.WristRight].Position.Z);

            return sb.ToString();
        }*/
    }


}
