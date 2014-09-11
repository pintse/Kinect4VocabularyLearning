using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.SURF;
using Ryan.ObjectRecognition.VO;
using Ryan.ObjectRecognition.DAO;
using System.Drawing;
using log4net;
using System.Threading.Tasks;


namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// SURF特徵點辨識處理
    /// </summary>
    public class SURFRecongitionProcessor : IRecongitionProcessor
    {
        private static volatile IRecongitionProcessor _Myself;
        private static readonly object ticket = new object();
        private static ILog log = LogManager.GetLogger(typeof(SURFRecongitionProcessor));

        private const float FLT_MAX = 3.402823466e+38F; /* max value */
        private IObjectSURFDAO _ObjectSURFDAO;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="objectSURFDAO">物件圖片DAO</param>
        private SURFRecongitionProcessor(IObjectSURFDAO objectSURFDAO)
        {
            _ObjectSURFDAO = objectSURFDAO;
        }

        public static IRecongitionProcessor getInstance(IObjectSURFDAO objectSURFDAO)
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new SURFRecongitionProcessor(objectSURFDAO);

                    }
                }

            }

            return _Myself;
        }

        public Dictionary<string, CongruousObjectVO> getBestCongruousObjects(Bitmap realBitamp, Dictionary<string, CongruousObjectVO> candidateObjects)
        {
            try
            {
                log.Info("@@@計算即時影像特徵點開始");
                List<IPoint> realipts = getFeaturePoints(realBitamp);
                log.Info("###計算即時影像特徵點結束");

                Dictionary<string, CongruousObjectVO> CongruousObjectVOs = new Dictionary<string, CongruousObjectVO>();

                int congruousAmount = 0;
                int max = 0;
                string maxObjectPicture = "";

                foreach (KeyValuePair<string, CongruousObjectVO> kvp in candidateObjects)
                {

                    compareFeature(realipts, ref CongruousObjectVOs, ref congruousAmount, ref max, ref maxObjectPicture, kvp);

                }

                return CongruousObjectVOs;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }

        private void compareFeature(List<IPoint> realipts, ref Dictionary<string, CongruousObjectVO> CongruousObjectVOs, ref int congruousAmount, ref int max, ref string maxObjectPicture, KeyValuePair<string, CongruousObjectVO> kvp)
        {
            //log.Info("%%%比對特徵點開始");
            congruousAmount = CompareFeaturePoints(realipts, kvp.Value.ObjectPicture.FeaturePoints);
            
            //log.Info("$$$比對特徵點結束");

            if (congruousAmount > max)
            {
                max = congruousAmount;
                maxObjectPicture = kvp.Key;
            }

            CongruousObjectVO cVo = new CongruousObjectVO();
            cVo.ObjectPicture = kvp.Value.ObjectPicture;
            //cVo.RecognitionScoreSet = kvp.Value.RecognitionScoreSet;
            cVo.RecognitionScoreSet = new Dictionary<string, double>(kvp.Value.RecognitionScoreSet);
            cVo.RecognitionScoreSet.Add("SURF", congruousAmount);
            CongruousObjectVOs.Add(cVo.ObjectPicture.ObjectId + GlobalData.IMAGE_NANE_DELIMITER + cVo.ObjectPicture.ObjectPictureId, cVo);
        }


        private List<IPoint> getFeaturePoints(Bitmap bitmap)
        {
            try
            {
                IntegralImage iimg = IntegralImage.FromImage(bitmap);
                List<IPoint> ipts = FastHessian.getIpoints(0.0002f, 5, 2, iimg);
                SurfDescriptor.DecribeInterestPoints(ipts, false, false, iimg);

                return ipts;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }

        }
        
        private int CompareFeaturePoints(List<IPoint> realipts, List<IPoint> dataipts)
        {
            //log.Info("CompareFeaturePoints::" + realipts.Count + ", " + dataipts.Count);
            int count = 0;
            double dist;
            double d1, d2;

            try
            {
                foreach (var realipt in realipts)
                {
                    d1 = d2 = FLT_MAX;

                    for (int j = 0; j < dataipts.Count; j++)
                    {
                        dist = GetDistance(realipt, dataipts[j]);

                        if (dist < d1) // if this feature matches better than current best
                        {
                            d2 = d1;
                            d1 = dist;
                        }
                        else if (dist < d2) // this feature matches better than second best
                        {
                            d2 = dist;
                        }
                    }//j

                    if (d1 / d2 < 0.77) //匹配
                    {
                        count += 1;
                    }
                }

                log.Debug("CompareFeaturePoints end...");
                return count;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }
        
        /*
        private int CompareFeaturePoints4Parallel(List<IPoint> realipts, List<IPoint> dataipts)
        {
            //log.Info("CompareFeaturePoints::" + realipts.Count + ", " + dataipts.Count);
            int count = 0;
            double dist;
            double d1, d2;

            try
            {
                Parallel.ForEach(realipts, (realipt, loopState) =>
                {
                    d1 = d2 = FLT_MAX;

                    //Parallel.ForEach(dataipts, (dataipt, loopState2) =>
                    for (int j = 0; j < dataipts.Count; j++)
                    {
                        dist = GetDistance(realipt, dataipts[j]);

                        if (dist < d1) // if this feature matches better than current best
                        {
                            d2 = d1;
                            d1 = dist;
                        }
                        else if (dist < d2) // this feature matches better than second best
                        {
                            d2 = dist;
                        }
                    }//j
                    //});

                    if (d1 / d2 < 0.77) //匹配
                    {
                        count += 1;
                    }
                }); //Parallel
                log.Debug("CompareFeaturePoints end...");
                return count;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }
        */

        private static double GetDistance(IPoint ip1, IPoint ip2)
        {
            float sum = 0.0f;
            for (int i = 0; i < 64; ++i)
            {
                sum += (ip1.descriptor[i] - ip2.descriptor[i]) * (ip1.descriptor[i] - ip2.descriptor[i]);
            }
            return Math.Sqrt(sum);
        }


        #region 建新照片特徵資料

        public void buildNewImageFeaturePersistence(ObjectPictureVO objectPictureVO)
        {
            try
            {
                log.Debug(objectPictureVO.ObjectId + "," + objectPictureVO.ObjectPictureId + " >> 開始SURF特徵點運算");
                objectPictureVO.FeaturePoints = getFeaturePoints(objectPictureVO.ObjectBitmap);
                log.Debug(objectPictureVO.ObjectId + "," + objectPictureVO.ObjectPictureId + " >> 結束SURF特徵點運算，開始將結果寫入DB");
                _ObjectSURFDAO.saveSURFIPoints(objectPictureVO);
                log.Debug(objectPictureVO.ObjectId + "," + objectPictureVO.ObjectPictureId + " >> 完成SURF特徵點寫入DB");
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
        }

        #endregion
    }//class
}//namespace

