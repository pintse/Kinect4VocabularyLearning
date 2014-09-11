using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.VO;
using MySql.Data.MySqlClient;
using log4net;

namespace Ryan.ObjectRecognition.DAO
{
    /// <summary>
    /// 物件資料主檔資料存取
    /// </summary>
    public class ObjectMainDAO : Ryan.Common.DAO.BaseDAO
    {
        private static ObjectMainDAO _Myself = new ObjectMainDAO();
        private static ILog log = LogManager.GetLogger(typeof(ObjectMainDAO));

        private ObjectMainDAO() { }

        public static ObjectMainDAO getInstance()
        {
            return _Myself;
        }


        public Dictionary<string, CongruousObjectVO> retrieveObjectsMainSource()
        {
            Dictionary<string, CongruousObjectVO> candidateObjects = new Dictionary<string, CongruousObjectVO>();

            string command = "select a.OBJECT_ID, a.OBJECT_NAME, b.PICTURE_ID " +
                            "from object_list a join object_pic_list b on a.OBJECT_ID = b.OBJECT_ID where a.ENABLE='Y'";
            MySqlConnection newConnection = getNewConnection();
            newConnection.Open();

            MySqlCommand cmd = new MySqlCommand(command, newConnection);
            //Data ResultSet
            using (MySqlDataReader data_reader = cmd.ExecuteReader())
            {
                if (data_reader.HasRows)
                {
                    while (data_reader.Read())
                    {
                        ObjectPictureVO pictureVO = new ObjectPictureVO();
                        pictureVO.ObjectId = data_reader.GetString("OBJECT_ID");
                        pictureVO.ObjectName = data_reader.GetString("OBJECT_NAME");
                        pictureVO.ObjectPictureId = data_reader.GetString("PICTURE_ID");

                        CongruousObjectVO dataVO = new CongruousObjectVO();
                        dataVO.ObjectPicture = pictureVO;

                        candidateObjects.Add(pictureVO.ObjectId + GlobalData.IMAGE_NANE_DELIMITER + pictureVO.ObjectPictureId, dataVO);

                    }
                }
            }
            newConnection.Close();
            newConnection.Dispose();
            newConnection = null;

            return candidateObjects;
        }


        public void createNewImageMainFile(ObjectPictureVO objectPictureVO)
        {
            lock(this)
            {
                MySqlConnection newConnection = getNewConnection();
                newConnection.Open();

                try
                {

                    String command = "";
                    MySqlCommand cmd = null;

                    if (objectPictureVO.ObjectId.IndexOf("N") == 0)
                    {
                        int objectId = queryObjectIdMax() + 1;
                        objectPictureVO.ObjectId = Convert.ToString(objectId);
                        command = "INSERT INTO object_list(OBJECT_ID,OBJECT_NAME, ENABLE) VALUE (" + objectId + ", \"尚未設定物件名稱\" , \"Y\" )";
                        cmd = new MySqlCommand(command, newConnection);
                        cmd.ExecuteNonQuery();
                    }

                    int pictureId = queryPictureIdMax(Convert.ToInt32(objectPictureVO.ObjectId)) + 1;
                    objectPictureVO.ObjectPictureId = Convert.ToString(pictureId);

                    command = "INSERT INTO object_pic_list( OBJECT_ID, PICTURE_ID, IMG_FILE_PATH ) VALUES(" + objectPictureVO.ObjectId + "," + objectPictureVO.ObjectPictureId + ",'"+ objectPictureVO.ExtendPath +"')";
                    cmd = new MySqlCommand(command, newConnection);
                    cmd.ExecuteNonQuery();
                    newConnection.Close();
                }
                catch (Exception ex)
                {
                    log.Fatal("createNewImageMainFile::"+ex);
                    //Console.WriteLine("createNewImageMainFile::" + ex);
                    newConnection.Close();
                    throw ex;
                }
                finally
                {
                    log.Debug("createNewImageMainFile::finally");
                    //Console.WriteLine("createNewImageMainFile::finally" );
                    newConnection.Close();
                }
            }
        }


        private int queryObjectIdMax()
        {
            String command = "SELECT IFNULL(MAX( OBJECT_ID ),0) AS MAXV FROM object_list";
            MySqlCommand cmd = new MySqlCommand(command, getConnection());
            
            using (MySqlDataReader data_reader = cmd.ExecuteReader())
            {
                if (data_reader.HasRows)
                {
                    while (data_reader.Read())
                    {
                        //連線成功的話會輸出totrec的值1
                        return data_reader.GetInt32("MAXV");
                    }
                }
            }
            return 0;
        }

        private int queryPictureIdMax(int objectId)
        {
            String command = "SELECT IFNULL(MAX( PICTURE_ID ),0) AS MAXV from object_pic_list where OBJECT_ID = "+objectId;
            MySqlCommand cmd = new MySqlCommand(command, getConnection());
            
            using (MySqlDataReader data_reader = cmd.ExecuteReader())
            {
                if (data_reader.HasRows)
                {
                    while (data_reader.Read())
                    {
                        //連線成功的話會輸出totrec的值1
                        return data_reader.GetInt32("MAXV");
                    }
                }
            }
            return 0;
        }


    }
}
