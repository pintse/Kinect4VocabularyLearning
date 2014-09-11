using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.DAO;

namespace Ryan.ObjectRecognition.Factory
{
    /// <summary>
    /// 物件辨識模組產生資料存取物件之工廠
    /// </summary>
    public class DAOFactory 
    {
        private static DAOFactory _DAOFactory = new DAOFactory();

        private static ObjectMainDAO _ObjectMainDAO = ObjectMainDAO.getInstance();
        private static IObjectPictureDAO _IObjectPictureDAO = ObjectPictureDAO.getInstance();
        private static IObjectSURFDAO _IObjectSURFDAO = ObjectSURFDAO.getInstance() ;
        private static IObjectColorDAO _IObjectColorDAO = ObjectColorDAO.getInstance();
        

        private DAOFactory() { }

        public static DAOFactory getInstance()
        {
            return _DAOFactory;
        }

        public  IObjectSURFDAO getObjectSURFDAOInstance()
        {
            return _IObjectSURFDAO;
        }

        public IObjectColorDAO getObjectColorDAOInstance()
        {
            return _IObjectColorDAO;
        }

        public IObjectPictureDAO getObjectPictureDAOInstance()
        {
            return _IObjectPictureDAO;
        }

        public ObjectMainDAO getObjectMainDAOInstance()
        {
            return _ObjectMainDAO;
        }
    }
}
