using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MySql.Data.MySqlClient;
using Ryan.Common.VO;

namespace Ryan.Common.DAO
{
    /// <summary>
    /// 資料庫DAO物件的基礎父元件
    /// </summary>
    public class BaseDAO : IBaseDAO
    {
        //連接字串
        private static String conString = "";
        private static MySqlConnection _Connection;

        private static ILog log = LogManager.GetLogger(typeof(BaseDAO));

        private object _LockObject = new object();

        static BaseDAO()  
        {

            try
            {
                conString = "SERVER = " + ((GlobalCommonVO.getInstance().DBSettingVO.ServerIP != null) ? GlobalCommonVO.getInstance().DBSettingVO.ServerIP : "") +
                    "; PORT=3306 " +
                    "; DATABASE = " + ((GlobalCommonVO.getInstance().DBSettingVO.Database != null) ? GlobalCommonVO.getInstance().DBSettingVO.Database : "RecognitionSys") +
                    "; User ID = " + ((GlobalCommonVO.getInstance().DBSettingVO.UserID != null) ? GlobalCommonVO.getInstance().DBSettingVO.UserID : "ryan") +
                    "; PASSWORD = " + ((GlobalCommonVO.getInstance().DBSettingVO.Password != null) ? GlobalCommonVO.getInstance().DBSettingVO.Password : "1234") + ";default command timeout=300000;";


                //取得MySQLConnection
                _Connection = new MySqlConnection(conString);
                _Connection.Open();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message+"::"+ex.StackTrace );
                log.Fatal("conString::" + conString +"::"+ ex.Message + "::" + ex.StackTrace);
            }
        }

        protected MySqlConnection getConnection()
        {
            if (_Connection == null)
            {
                _Connection = new MySqlConnection(conString);
                _Connection.Open();
            }

            return _Connection;
        }

        protected MySqlConnection getNewConnection()
        {
            try
            {
                return new MySqlConnection(conString);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return getNewConnection();
            }
        }

        public void testDBase()
        {
            try
            {

                String command = "SELECT count(*) totrec FROM surf_ipoint";
                MySqlCommand cmd = new MySqlCommand(command, getConnection());
                //Data ResultSet
                using (MySqlDataReader data_reader = cmd.ExecuteReader())
                {
                    if (data_reader.HasRows)
                    {
                        while (data_reader.Read())
                        {
                            //連線成功的話會輸出totrec的值1
                            Console.WriteLine("BaseDAO.testDB data::" + data_reader.GetString("totrec"));

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //失敗會有錯誤訊息
                Console.WriteLine(ex);
            }

        }
    }
}