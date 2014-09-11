using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.SURF;
using System.Drawing;
using Ryan.ObjectRecognition.VO;
using Ryan.ObjectRecognition.DAO;
using log4net;
using System.Threading.Tasks;
using Ryan.Common;

namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// The class is not published openly
    /// 顏色辨識處理
    /// </summary>
    public class ColorRecongitionProcessor : IRecongitionProcessor
    {
        private const int _MajorColorCriticalPoint = 10;

        private static volatile IRecongitionProcessor _Myself;
        private static readonly object ticket = new object();

        private static ILog log = LogManager.GetLogger(typeof(ColorRecongitionProcessor));

        private ClassifiedColor _ClassifiedColor;
        private IObjectColorDAO _ObjectColorDAO;

        #region 物件生成

        private ColorRecongitionProcessor(ClassifiedColor classifiedColor, IObjectColorDAO objectColorDAO)
        {
            _ClassifiedColor = classifiedColor;
            _ObjectColorDAO = objectColorDAO;

        }

        public static IRecongitionProcessor getInstance(ClassifiedColor classifiedColor, IObjectColorDAO objectColorDAO)
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new ColorRecongitionProcessor(classifiedColor, objectColorDAO);

                    }
                }

            }

            return _Myself;
        }

        #endregion

        public Dictionary<string, CongruousObjectVO> getBestCongruousObjects(Bitmap realBitamp, Dictionary<string, CongruousObjectVO> candidateObjects)
        {
            //"The function is not published openly
            return candidateObjects;
        }


        #region 建新照片特徵資料

        public void buildNewImageFeaturePersistence(ObjectPictureVO objectPictureVO)
        {
            //"The function is not published openly
        }

        #endregion
    }
}
