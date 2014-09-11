using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Kinect;
using Ryan.Kinect.Toolkit.VO;

namespace Ryan.Kinect.Toolkit.VO
{
    /// <summary>
    /// 使用者Value Object
    /// </summary>
    public class Player
    {
        public Player()
        {
            Dictionary<JointType, JointInfo> jointInfos = new Dictionary<JointType, JointInfo>();
            jointInfos.Add(JointType.HandLeft, new JointInfo(JointType.HandLeft));
            jointInfos.Add(JointType.HandRight, new JointInfo(JointType.HandRight));
            this.jointInfos = jointInfos;

            Dictionary<JointType, String> holdObjects = new Dictionary<JointType, String>();
            holdObjects.Add(JointType.HandLeft, "");
            holdObjects.Add(JointType.HandRight, "");
            this.holdObjects = holdObjects;

            Dictionary<String, String> action = new Dictionary<string, string>();
            this.action = action;
        }

        /// <summary>
        /// Track Id
        /// </summary>
        public int playerId { get; set; }
        public string playerName { get; set; }
        public SolidColorBrush color { get; set; }
        public int frameIdx { get; set; }
        public SkeletonTrackingState trackingState { get; set; }
        /// <summary>
        /// 資料庫裡設定的使用者帳號ID
        /// </summary>
        public string userID { get; set; }
        public string groupID { get; set; }
        /// <summary>
        /// 關節點資訊，預設僅初始化HandLeft，HandRight
        /// </summary>
        public Dictionary<JointType, JointInfo> jointInfos { get; set; }

        /// <summary>
        /// 手持物件紀錄
        /// </summary>
        public Dictionary<JointType, String> holdObjects { get; set; }

        /// <summary>
        /// 做的行為。<手勢行為代碼, 手勢行為名稱>
        /// </summary>
        public Dictionary<String, String> action { get; set; }
    }
}
