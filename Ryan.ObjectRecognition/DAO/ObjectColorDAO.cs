using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Ryan.ObjectRecognition.VO;
using MySql.Data.MySqlClient;
using log4net;
using Ryan.Common;

namespace Ryan.ObjectRecognition.DAO
{
    /// <summary>
    /// The class is not published openly
    /// </summary>
    public class ObjectColorDAO : Ryan.Common.DAO.BaseDAO, IObjectColorDAO, IObjectFeatureDAO
    {

        private static ObjectColorDAO _Myself = new ObjectColorDAO();
        private static ILog log = LogManager.GetLogger(typeof(ObjectColorDAO));

        private ObjectColorDAO()
        {
        }

        public static ObjectColorDAO getInstance()
        {
            return _Myself;
        }

        public void saveColor(ObjectPictureVO objectVO)
        {
           
        }

        public List<ObjectPictureVO> getObjectsColorsDistribution()
        {
            throw new SoftwareException("The function is not published openly");
        }


        public Dictionary<string, CongruousObjectVO> fillFeatures2Objects(Dictionary<string, CongruousObjectVO> candidateObjects)
        {
            //"The function is not published openly;
            return candidateObjects;
        }
    }
}
