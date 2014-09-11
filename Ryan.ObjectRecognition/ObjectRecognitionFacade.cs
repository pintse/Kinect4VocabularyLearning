using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ryan.ObjectRecognition.DAO;
using Ryan.ObjectRecognition.Factory;
using Ryan.ObjectRecognition.Service;
using log4net;
using Ryan.ObjectRecognition.VO;

namespace Ryan.ObjectRecognition
{
    /// <summary>
    /// 1.此物件為One instance
    /// 2.整個物件辨識功能的入口介面
    /// 3.若要多行緒執行，由外部呼叫者自行操控執行緒，本介面與其後端物件不處理多執行緒事務
    /// </summary>
    public class ObjectRecognitionFacade
    {
        private static DAOFactory _DAOFactory = DAOFactory.getInstance();
        private static ServiceFactory _ServiceFactory = ServiceFactory.getInstance(_DAOFactory);

        private static ObjectRecognitionFacade _ObjectRecognitionFacade = new ObjectRecognitionFacade();

        private static ILog log = LogManager.GetLogger(typeof(ObjectRecognitionFacade));

        private ObjectRecognitionFacade()
        {
            Console.WriteLine("載入" + AppDomain.CurrentDomain.BaseDirectory.ToString() + "log4netconfig.xml");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory.ToString() + "log4netconfig.xml"));
        }

        public static ObjectRecognitionFacade getInstance()
        {
            return _ObjectRecognitionFacade;
        }

        public string recognizeObjects(Bitmap objectBimap)
        {
            return _ServiceFactory.getRecongitionHandler().recognizeObject(objectBimap);

        }

        
        public void buildNewPicturesData()
        {
            try
            {
                log.Info("ObjectRecognitionFacade.buildNewPicturesData...");
                _ServiceFactory.getRecongitionHandler().processNewPictures(_DAOFactory.getObjectPictureDAOInstance(), _DAOFactory.getObjectMainDAOInstance());
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

            }
        }


        public void prepareRelatedData()
        {
            Console.WriteLine("prepareRelatedData start...");
            log.Info("prepareRelatedData start...");

            try
            {
                _ServiceFactory.getGlobalDataService().prepareRelatedData( _ServiceFactory.getRecongitionCollection().ObjectFeatureDAOs, _DAOFactory.getObjectMainDAOInstance(), _DAOFactory.getObjectPictureDAOInstance());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                log.Fatal(ex);
                throw ex;
            }

            Console.WriteLine("prepareRelatedData end...");
            log.Info("prepareRelatedData end...");

        }

        #region 測試區
        public void testDAO()
        {

            DAOFactory daof = DAOFactory.getInstance();
            IObjectSURFDAO picDAO = daof.getObjectSURFDAOInstance();
            //((ObjectSURFDAO)picDAO).testDB();

        }

        private void test()
        {
            log.Info("GlobalData.getInstance().CandidateObjects.Count::" + GlobalData.getInstance().CandidateObjects.Count);

            CongruousObjectVO congruousObjectVO = null;
            Dictionary<string, CongruousObjectVO> realtimeObjects = GlobalData.getInstance().CandidateObjects;

            foreach (KeyValuePair<string, CongruousObjectVO> kvp in realtimeObjects)
            {

                log.Info(kvp.Key);
                log.Info(kvp.Value.ObjectPicture.ObjectName);
                log.Info(kvp.Value.ObjectPicture.ObjectPictureId);

                log.Info("物件即時辨識開始...");
                congruousObjectVO = _ServiceFactory.getRecongitionHandler().recognizeObjectCongruous(kvp.Value.ObjectPicture.ObjectBitmap);

                if (congruousObjectVO.ObjectPicture != null)
                {
                    log.Info("＃＃＃物件即時辨識結果::ObjectId::" + congruousObjectVO.ObjectPicture.ObjectId + ", ObjectPictureId::" + congruousObjectVO.ObjectPicture.ObjectPictureId +
                        ", ObjectName::" + congruousObjectVO.ObjectPicture.ObjectName);

                    log.Info(", Color::" + ((congruousObjectVO.RecognitionScoreSet.ContainsKey("Color")) ? congruousObjectVO.RecognitionScoreSet["Color"] + "" : "") +
                        ", SURF::" + ((congruousObjectVO.RecognitionScoreSet.ContainsKey("SURF")) ? congruousObjectVO.RecognitionScoreSet["SURF"] + "" : ""));
                }
                else
                {
                    log.Info("物件即時辨識結果無符合...");
                }

            }
        }

        #endregion
    }
}
