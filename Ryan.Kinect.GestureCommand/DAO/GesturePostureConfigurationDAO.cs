using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Microsoft.Kinect;
using MySql.Data.MySqlClient;
using Ryan.Common.DAO;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.DAO
{
    public class GesturePostureConfigurationDAO : BaseDAO
    {
        private static GesturePostureConfigurationDAO _Myself = new GesturePostureConfigurationDAO();
        private static ILog log = LogManager.GetLogger(typeof(GesturePostureConfigurationDAO));

        private GesturePostureConfigurationDAO()
        {
        }

        public static GesturePostureConfigurationDAO getInstance()
        {
            return _Myself;
        }

        public List<GesturePostureVO> retrieveGesturesPosturesConfiguration()
        {
            try
            {
                List<GesturePostureVO> result = new List<GesturePostureVO>();


                string command = "select a.id, a.type, a.name, a.algorithm, a.detector, ifnull(a.gesture_joint,'') as gesture_joint, a.epsilon, a.command_flg, "+
                    " b.sub_id FROM gesture_posture_list a left join combined_gesture_posture_sublist b on a.id = b.id " +
                    " where a.is_enable = 'Y' " +
                    " order by a.type, a.id";

                MySqlConnection newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);

                //Data ResultSet
                using (MySqlDataReader data_reader = cmd.ExecuteReader())
                {
                    if (data_reader.HasRows)
                    {
                        List<GlobalData.GestureTypes> combinations = null;
                        GesturePostureVO vo = null;
                        string id = "";
                        while (data_reader.Read())
                        {
                            if (id != data_reader.GetString("id"))
                            {
                                combinations = null;
                                if (data_reader.GetString("type") == "c" || data_reader.GetString("type") == "s")
                                {
                                    combinations = new List<GlobalData.GestureTypes>();
                                    combinations.Add((GlobalData.GestureTypes)Enum.Parse(typeof(GlobalData.GestureTypes), data_reader.GetString("sub_id"), true));
                                }
                                else
                                {
                                    id = data_reader.GetString("id");
                                }
                                vo = new GesturePostureVO((GlobalData.GestureTypes)Enum.Parse(typeof(GlobalData.GestureTypes), data_reader.GetString("id"), true),
                                    data_reader.GetString("type"), data_reader.GetString("name"), data_reader.GetString("algorithm"), combinations,
                                    GlobalData.DetectorPackageName +"."+ data_reader.GetString("detector"), data_reader.GetString("gesture_joint"), data_reader.GetInt16("epsilon"), data_reader.GetString("command_flg"));

                                result.Add(vo);
                                id = data_reader.GetString("id");
                            }
                            else
                            {
                                if (data_reader.GetString("type") == "c" || data_reader.GetString("type") == "s")
                                {
                                    combinations.Add((GlobalData.GestureTypes)Enum.Parse(typeof(GlobalData.GestureTypes), data_reader.GetString("sub_id"), true));
                                }
                                id = data_reader.GetString("id");
                            }
                        }
                    }
                }
                newConnection.Close();
                newConnection.Dispose();
                newConnection = null;

                return result;

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
