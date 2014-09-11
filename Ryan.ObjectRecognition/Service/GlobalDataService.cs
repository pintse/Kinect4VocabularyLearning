using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.DAO;
using Ryan.ObjectRecognition.VO;
using log4net;

namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// ObjectRecognition全域資料處理
    /// </summary>
    public class GlobalDataService
    {
        private static ILog log = LogManager.GetLogger(typeof(GlobalDataService));
        private static GlobalDataService _Myself = new GlobalDataService();

        private GlobalDataService() { }

        public static GlobalDataService getInstance()
        {
            return _Myself;
        }

        public void prepareRelatedData(List<IObjectFeatureDAO> objectFeatureDAOs, ObjectMainDAO objectMainDAO, IObjectPictureDAO objectPictureDAO)
        {
            try{
                if (GlobalData.getInstance() != null && GlobalData.getInstance().CandidateObjects != null)
                {
                    return;
                }

            }catch(Exception ex){  
                //第一次Call會出現Exception，這時不用停止程式
                Console.WriteLine(ex);
                log.Info(ex);
            }

            try
            {

                Dictionary<string, CongruousObjectVO> candidateObjects = objectMainDAO.retrieveObjectsMainSource();

                foreach (IObjectFeatureDAO objectFeatureDAO in objectFeatureDAOs)
                {
                    candidateObjects = objectFeatureDAO.fillFeatures2Objects(candidateObjects);
                }

                GlobalData.getInstance(candidateObjects);  //第一次getInstance要使用這個建構式
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                log.Fatal(ex);
                throw ex;
            }

        }
        
    }
}
