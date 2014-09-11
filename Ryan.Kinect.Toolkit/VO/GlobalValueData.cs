using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using Ryan.Common.VO;
using log4net;
using System.IO;
using Ryan.Kinect.Toolkit.VO;

namespace Ryan.Kinect.Toolkit.VO
{
    /// <summary>
    /// Toolkit 模組全域變數
    /// </summary>
    public static class GlobalValueData
    {
        private static ILog log = LogManager.GetLogger(typeof(GlobalValueData));

        public static List<Player> Players = new List<Player>();

        public static DBSettingVO DBSettingVO { get; set; }
        private static KinectTypes kinectType;
        public static KinectTypes KinectType
        {
            get
            {
                return kinectType;
            }
            set
            {
                kinectType = value;
                GlobalValueData.Messages.Add("Kinect Type=" + value);
            }
        }

        public static string ErrorWavPath = Path.Combine(Environment.CurrentDirectory, @"Data\Audio\error.wav");

        public static List<string> Messages = new List<string>();

        public static Dictionary<string, Queue <string>> GestureCommandMessage = new Dictionary< string, Queue<string>>();

        public enum KinectTypes { Kinect4Windows = 1 , Kinect4Xbox = 2 }

    }
}
