using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using MySql.Data.MySqlClient;
using Ryan.Common;
using Ryan.Common.DAO;
using Ryan.Common.VO;
using Ryan.Kinect.Toolkit.VO;

namespace Ryan.Kinect.Toolkit.DAO
{
    class UserDAO : BaseDAO
    {
        private static UserDAO _MySelf = new UserDAO();
        private static ILog log = LogManager.GetLogger(typeof(UserDAO));

        private UserDAO() { }

        public static UserDAO getInstance()
        {
            return _MySelf;
        }

        public void retrieveUserData()
        {

            string day = "0" + DateTime.Now.Day.ToString();
            day = day.Substring(day.Length - 2);
            //string mmdd = DateTime.Now.Month.ToString() + day;
            string mmdd = "102"; //for test

            string command = "select ex_date, computer_id, time_section, user_id, order_num , b.id, b.name, b.group_id " +
                    " from computer_player a join player b on (a.user_id = b.id) " +
                    " where a.ex_date = ?ex_date " +
                    " and a.time_section = ( select time_sections.now_time_section FROM time_sections ) " +
                    " and a.computer_id = ?ip " +
                    " order by a.order_num ";
            try
            {
                MySqlConnection newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);

                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[2];

                parameters[0] = new MySqlParameter("?ex_date", MySqlDbType.VarChar);
                parameters[0].Value = mmdd;

                parameters[1] = new MySqlParameter("?ip", MySqlDbType.VarChar);
                parameters[1].Value = GlobalCommonVO.IPTail;

                cmd.Parameters.AddRange(parameters);


                //Data ResultSet
                using (MySqlDataReader data_reader = cmd.ExecuteReader())
                {
                    if (data_reader.HasRows)
                    {
                        while (data_reader.Read())
                        {
                            Player player = new Player();
                            player.userID = data_reader.GetString("id");
                            player.playerName = data_reader.GetString("name");
                            player.groupID = data_reader.GetString("group_id");
                            GlobalValueData.Players.Add(player);
                        }
                    }
                    else
                    {
                        throw new SoftwareException("使用者設定錯誤，請通知管理者");
                    }
                }
                newConnection.Close();
                newConnection.Dispose();
                newConnection = null;

            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                Console.WriteLine(ex);
                throw ex;
            }

        }

    }
}
