using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ryan.ObjectRecognition.VO;
using MySql.Data.MySqlClient;
using Ryan.ObjectRecognition.SURF;
using System.Threading;
using log4net;
using System.Threading.Tasks;

namespace Ryan.ObjectRecognition.DAO
{

    /// <summary>
    /// SURF特徵點資料存取
    /// </summary>
    public class ObjectSURFDAO : Ryan.Common.DAO.BaseDAO, IObjectSURFDAO, IObjectFeatureDAO
    {

        private static Queue<Object> _ThreadQueue = new Queue<Object>();
        private static volatile ObjectSURFDAO _Myself;  //需要在 new Queue<Object>(); 之後
        private static readonly object ticket = new object();
        private static ILog log = LogManager.GetLogger(typeof(ObjectSURFDAO));

        private ObjectSURFDAO()
        {
            for (int i = 0; i < 100; i++)
            {
                _ThreadQueue.Enqueue(new Object());
            }
        }

        public static ObjectSURFDAO getInstance()
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new ObjectSURFDAO();

                    }
                }
                
            }
            return _Myself;
        }




        public void saveSURFIPoints(ObjectPictureVO objectVO)
        {

            log.Info("saveSURFIPoint()::ObjectId::" + objectVO.ObjectId + ", objectVO.ObjectPictureId::" + objectVO.ObjectPictureId + ",start...");

            log.Debug("objectVO.FeaturePoints.Count::" + objectVO.FeaturePoints.Count);

            int i = 0;

            foreach (IPoint p in objectVO.FeaturePoints)
            {
                saveIPoint(objectVO, i, p);
             
                i++;

            }

            log.Info("saveSURFIPoint()::ObjectId::" + objectVO.ObjectId + ", objectVO.ObjectPictureId::" + objectVO.ObjectPictureId + ", objectVO.ExtendPath::" + objectVO.ExtendPath + ",end...");

        }

        private Object getToken()
        {

            try
            {
                Object token = null;
                lock (this)
                {
                    if (_ThreadQueue.Count > 0)
                    {
                        token = _ThreadQueue.Dequeue();

                    }

                    if (DateTime.Now.Minute % 10 == 0 && (DateTime.Now.Second == 0 || DateTime.Now.Second == 30))
                    {
                        log.Debug("###_ThreadQueue.Count::" + _ThreadQueue.Count);
                    }
                }

                if (token == null)
                {

                    Thread.Sleep(1000);
                    token = getToken();
                }


                return token;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }


        }

        private void releaseToken(Object token)
        {

            _ThreadQueue.Enqueue(token);

        }
       

        private void saveIPointParallel(ObjectPictureVO objectVO, int i, IPoint p)
        {
            Object token = getToken();
            saveIPoint(objectVO, i, p);
            releaseToken(token);
        }

        private void saveIPoint(ObjectPictureVO objectVO, int i, IPoint p)
        {
            MySqlConnection newConnection = null;
            try
            {
                newConnection = getNewConnection();
                newConnection.Open();

                String command = "INSERT INTO surf_ipoint (   OBJECT_ID  ,PICTURE_ID  ,SN  ,X  ,Y  ,SCALE  ,RESPONSE  ,ORIENTATION  ,LAPLACIAN  ,DESCRIPTOR_LENGTH) " +
                    "VALUES (" + objectVO.ObjectId + ", " + objectVO.ObjectPictureId + " ," + i + " ," + p.x + " ," + p.y + " ," + p.scale + " ," + p.response + " ," + p.orientation + " ," + p.laplacian + " ," + p.descriptorLength + " )";
                MySqlCommand cmd = new MySqlCommand(command, newConnection);
                cmd.CommandTimeout = 0;

                MySqlParameter[] parameters = new MySqlParameter[3];  
  

                
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                newConnection.Close();
                newConnection.Dispose();
                newConnection = null;

                saveDescriptor(objectVO, i, p, 0);


            }
            catch (Exception ex)
            {
                log.Fatal("saveSURFIPoint()::ObjectId::" + objectVO.ObjectId + ", objectVO.ObjectPictureId::" + objectVO.ObjectPictureId + ", objectVO.ExtendPath::" + objectVO.ExtendPath + " >> \n" + ex);

            }
            finally
            {
                if (newConnection != null)
                {
                    newConnection.Close();
                }

            }
        }

        private void saveDescriptor(ObjectPictureVO objectVO, int i, IPoint p, int countTime)
        {
            MySqlConnection newConnection = null;
            int[] sn = new int[16];
            float[] descriptorT = new float[16];  
            int index = 0;
            if (p.descriptor.Length != 64)
            {
                log.Fatal("p.descriptor.Length 不是64，無法處理");
                throw new Exception("p.descriptor.Length 不是64，無法處理");
            }
            for (int j = 0; j < p.descriptor.Length; j++)
            {
                if (j==0){
                    index = 0;
                }else{
                    index = j%16;
                }

                sn[index] = j;
                descriptorT[index] = p.descriptor[j];

                if (index == 15 || j == p.descriptor.Length - 1)
                {
                    string command = "insert into surf_ipoint_descriptor( OBJECT_ID, PICTURE_ID, IPOINT_SN, "+
                        "SN, DESCRIPTOR,SN1, DESCRIPTOR1,SN2, DESCRIPTOR2,SN3, DESCRIPTOR3,SN4, DESCRIPTOR4,SN5, DESCRIPTOR5,SN6, DESCRIPTOR6,SN7, DESCRIPTOR7, "+
                        "SN8, DESCRIPTOR8,SN9, DESCRIPTOR9,SN10, DESCRIPTOR10,SN11, DESCRIPTOR11,SN12, DESCRIPTOR12,SN13, DESCRIPTOR13,SN14, DESCRIPTOR14,SN15, DESCRIPTOR15 "+  
                        ") values (" + objectVO.ObjectId + "," + objectVO.ObjectPictureId + "," + i + "," +
                        sn[0] + "," + descriptorT[0] + "," + sn[1] + "," + descriptorT[1] + "," + sn[2] + "," + descriptorT[2] + "," + sn[3] + "," + descriptorT[3] + "," + sn[4] + "," + descriptorT[4] + "," + sn[5] + "," + descriptorT[5] + "," + sn[6] + "," + descriptorT[6] + "," + sn[7] + "," + descriptorT[7] + "," +
                        sn[8] + "," + descriptorT[8] + "," + sn[9] + "," + descriptorT[9] + "," + sn[10] + "," + descriptorT[10] + "," + sn[11] + "," + descriptorT[11] + "," + sn[12] + "," + descriptorT[12] + "," + sn[13] + "," + descriptorT[13] + "," + sn[14] + "," + descriptorT[14] + "," + sn[15] + "," + descriptorT[15] + ")";


                    try
                    {
                        newConnection = getNewConnection();
                        newConnection.Open();

                        MySqlCommand cmd = new MySqlCommand(command, newConnection);
                        cmd.CommandTimeout = 0;
                        
                        cmd.ExecuteNonQuery();

                        cmd.Dispose();
                        cmd = null;

                        newConnection.Close();
                        newConnection.Dispose();
                        newConnection = null;

                        for (int c=0; c < 16; c++)
                        {
                            sn[c] = 0;
                            descriptorT[c] = 0;
                        }

                        
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex + "\n ObjectId::" + objectVO.ObjectId + ", objectVO.ObjectPictureId::" + objectVO.ObjectPictureId + ", objectVO.ExtendPath::" + objectVO.ExtendPath);

                    }
                }
            }
        }


        public Dictionary<string, CongruousObjectVO> fillFeatures2Objects(Dictionary<string, CongruousObjectVO> candidateObjects)
        {

            string command = "select a.OBJECT_ID, a.PICTURE_ID, b.IPOINT_SN, a.X, a.Y, a.SCALE, a.RESPONSE, a.ORIENTATION, a.LAPLACIAN, a.DESCRIPTOR_LENGTH, " +
                                "b.SN, b.DESCRIPTOR, b.SN1, b.DESCRIPTOR1, b.SN2, b.DESCRIPTOR2, b.SN3, b.DESCRIPTOR3, b.SN4, b.DESCRIPTOR4, " +
                                "b.SN5, b.DESCRIPTOR5, b.SN6, b.DESCRIPTOR6, b.SN7, b.DESCRIPTOR7, b.SN8, b.DESCRIPTOR8, b.SN9, b.DESCRIPTOR9, " +
                                "b.SN10, b.DESCRIPTOR10, b.SN11, b.DESCRIPTOR11, b.SN12, b.DESCRIPTOR12, b.SN13, b.SN14, b.DESCRIPTOR13, b.DESCRIPTOR14, b.SN15, b.DESCRIPTOR15 " +
                                "FROM surf_ipoint a join surf_ipoint_descriptor b " +
                                "on a.OBJECT_ID = b.OBJECT_ID AND a.PICTURE_ID = b.PICTURE_ID AND a.SN = b.IPOINT_SN " +
                                "WHERE a.OBJECT_ID in (select object_list.OBJECT_ID FROM object_list where ENABLE = 'Y') "+
                                "ORDER BY a.OBJECT_ID, a.PICTURE_ID, b.IPOINT_SN, b.SN";

            MySqlConnection newConnection = getNewConnection();
            newConnection.Open();

            MySqlCommand cmd = new MySqlCommand(command, newConnection);
            //Data ResultSet

            IPoint ipoint = null;
            using (MySqlDataReader data_reader = cmd.ExecuteReader())
            {
                if (data_reader.HasRows)
                {
                    while (data_reader.Read())
                    {
                        ObjectPictureVO pictureVO = candidateObjects[data_reader.GetString("OBJECT_ID") + GlobalData.IMAGE_NANE_DELIMITER + data_reader.GetString("PICTURE_ID")].ObjectPicture; 
                        int sn = data_reader.GetInt32("SN");
                        int ipoint_sn = data_reader.GetInt32("IPOINT_SN");
                        

                        if (sn == 0)
                        {
                            ipoint = null;
                            ipoint = new IPoint();    

                            ipoint.descriptorLength = data_reader.GetInt32("DESCRIPTOR_LENGTH");

                            ipoint.laplacian = data_reader.GetInt32("LAPLACIAN");
                            ipoint.orientation = data_reader.GetFloat("ORIENTATION");
                            ipoint.response = data_reader.GetFloat("RESPONSE");
                            ipoint.scale = data_reader.GetFloat("SCALE");
                            ipoint.x = data_reader.GetFloat("X");
                            ipoint.y = data_reader.GetFloat("Y");

                            ipoint.descriptor = new float[data_reader.GetInt32("DESCRIPTOR_LENGTH")];
                            ipoint.descriptor[sn] = data_reader.GetFloat("DESCRIPTOR");
                            
                            for(int i = 1 ; i < 16 ; i++)
                            {
                                ipoint.descriptor[i] = data_reader.GetFloat("DESCRIPTOR" + i);
                            }

                            pictureVO.FeaturePoints.Add(ipoint);
                        
                        }
                        else
                        {
                            pictureVO.FeaturePoints[ipoint_sn].descriptor[sn] = data_reader.GetFloat("DESCRIPTOR");

                            for (int i = 1; i < 16; i++)
                            {
                                pictureVO.FeaturePoints[ipoint_sn].descriptor[data_reader.GetInt32("SN" + i)] = data_reader.GetFloat("DESCRIPTOR" + i);
                            }
                        }

                    }
                }
            }
            newConnection.Close();
            newConnection.Dispose();
            newConnection = null;

            return candidateObjects;
            
        }

    }
}
