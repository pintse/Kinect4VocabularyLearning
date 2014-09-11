using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ryan.ObjectRecognition.SURF;

namespace Ryan.ObjectRecognition.VO
{
    /// <summary>
    /// 物件圖片Value Object
    /// </summary>
    public class ObjectPictureVO
    {
        /// <summary>
        /// 物件編號（檔名）
        /// </summary>
        string _ObjectId;

        public string ObjectId
        {
            get { return _ObjectId; }
            set { _ObjectId = value; }
        }
        
        
        /// <summary>
        /// Key:物件名稱
        /// </summary>
        string _ObjectName;

        public string ObjectName
        {
            get { return _ObjectName; }
            set { _ObjectName = value; }
        }


        string _ObjectPictureId;

        /// <summary>
        /// Key:物件圖片流水號
        /// </summary>
        public string ObjectPictureId
        {
            get { return _ObjectPictureId; }
            set { _ObjectPictureId = value; }
        }

        /// <summary>
        /// 檔案路徑補充資訊
        /// </summary>
        string _ExtendPath;

        public string ExtendPath
        {
            get { return _ExtendPath; }
            set { _ExtendPath = value; }
        }

        /// <summary>
        /// 物件圖片
        /// </summary>
        Bitmap _ObjectBitmap;

        public Bitmap ObjectBitmap
        {
            get { return _ObjectBitmap; }
            set { _ObjectBitmap = value; }
        }


        /// <summary>
        /// 顏色資訊
        /// </summary>
        Dictionary<string, int> _ColorsDistribution = new Dictionary<string, int>();

        public Dictionary<string, int> ColorsDistribution
        {
            get { return _ColorsDistribution; }
            set { _ColorsDistribution = value; }
        }

        /// <summary>
        /// 特徵點資訊
        /// </summary>
        List<IPoint> _FeaturePoints = new List<IPoint>() ;

        public List<IPoint> FeaturePoints
        {
            get { return _FeaturePoints; }
            set { _FeaturePoints = value; }
        }


        public void clear()
        {
            ObjectBitmap = null;
            ColorsDistribution = null;
            for (int i = 0; i < FeaturePoints.Count; i++)
            {
                FeaturePoints[i].descriptor = null;
            }
            FeaturePoints.Clear();
        }
    }
}
