using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kinect.Toolbox;

namespace Ryan.Kinect.GestureCommand.VO
{
    /// <summary>
    /// 手勢辨識模組全域資料
    /// </summary>
    public class GlobalData
    {
        //20140911
        /*
        private static Dictionary<String, GestureDetector> _GesturesAlgorithmic;

        internal static Dictionary<String, GestureDetector> GesturesAlgorithmic
        {
            get { return GlobalData._GesturesAlgorithmic; }
            set { GlobalData._GesturesAlgorithmic = value; }
        }
        private static Dictionary<String, GestureDetector> _GesturesTemplate;

        internal static Dictionary<String, GestureDetector> GesturesTemplate
        {
            get { return GlobalData._GesturesTemplate; }
            set { GlobalData._GesturesTemplate = value; }
        }
        private static Dictionary<String, PostureDetector> _PosturesAlgorithmic;

        internal static Dictionary<String, PostureDetector> PosturesAlgorithmic
        {
            get { return GlobalData._PosturesAlgorithmic; }
            set { GlobalData._PosturesAlgorithmic = value; }
        }
        private static Dictionary<String, PostureDetector> _PosturesTemplate;

        internal static Dictionary<String, PostureDetector> PosturesTemplate
        {
            get { return GlobalData._PosturesTemplate; }
            set { GlobalData._PosturesTemplate = value; }
        } */

        private GlobalData()
        {
        }

        public static List<GesturePostureVO> GesturePostureSettings
        {
            get;
            internal set;
        }

        public enum GestureTypes
        {
            Init, GCircle, PFrontHandLeft, PFrontHandRight, CObjectRecognition, PPopeye, PHandsOnKneeAndHeadLeanForward, PHeadDownAndHandOnHead, GSwipeHandLeft,
            GSwipeHandRight, CFrontSwipeHands, CHi,
            PHandNearAnkle,
            PHandsOutstretched,
            PRightHandHighAboutHip,
            GPushHandLeft, GPushHandRight, CPushHands,
            PHandOnHeadAndHandHighAboutShoulder, GPullHandRight,
            GHandLeftMoveToLeft, GHandRightMoveToRight, CHandstMoveToExtended,
            PShoulderOverFootOnX,
            PHandLeftHorizontalNearShoulder, GHandRightUp, CHandRightLRiseOverHandLeft,
            PArmsCross, CArmsCrossSway,
            PHandRightBetweenHeadAndShoulder, GHandRightMoveToLeft, CHandRightMoveToLeftNearMouth,
            PHandsOverHead, GShoulderMoveToAdvance, GShoulderMoveToDown, CSwoop,
            PAsleep,
            PPickUp,
            GLeapJump,
            PImpossible,
            PRib,
            PTwist,
            CSwim,
            GHandRightDown,
            GHandLeftDown,
            GOnFootRight,
            GOnFootLeft,
            COnFoot,
            PHandRightHigherElbow,
            CDitch,
            PHandsNearHead,
            GHeadShake,
            CInsane,
            GApproachMoveToAdvance,
            PHandRightOnRight,
            PHandRightOnLeft,
            PHandRightInFront,
            PHandRightInBack,
            PHandRightAbove,
            PHandRightBelow,
            GPushHandRightDitch,
            PHandsOverElbows
        }

        public static String GesturesTemplatePath = Path.Combine(Environment.CurrentDirectory, @"data\GesturesTemplate\");
        public static String PosturesTemplatePath = Path.Combine(Environment.CurrentDirectory, @"data\PosturesTemplate\");

        public static string ModuleName = "Ryan.Kinect.GestureCommand";
        public static string DetectorPackageName = "Ryan.Kinect.GestureCommand.Service.Single";


    }
}