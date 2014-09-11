using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using MySql.Data.MySqlClient;
using Ryan.Common;
using Ryan.Common.DAO;
using Ryan.Common.VO;
using Ryan.Content.VO;

namespace Ryan.Content.DAO
{
    /// <summary>
    /// 軌跡紀錄DAO
    /// </summary>
    public class RecordDAO  : BaseDAO
    {
        private static RecordDAO _Myself = new RecordDAO();
        private static ILog log = LogManager.GetLogger(typeof(RecordDAO));

        private RecordDAO()
        {
        }

        public static RecordDAO getInstance()
        {
            return _Myself;
        }

        public int insertMainRecord(string userId, string kind)
        {
            int maxRoundId = getMaxRoundId(userId, kind) + 1;

            MySqlConnection newConnection = null;
            try
            {

                string command = "insert into play_record ( player_id ,kind ,round_id,start_time,end_time,ip) VALUES " +
                    " (?userId,?kind, ?roundId, DATE_FORMAT(now(),'%y%m%d%H%i%s'),'',?ip) ";

                newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);
                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[4];

                parameters[0] = new MySqlParameter("?userId", MySqlDbType.VarChar);
                parameters[0].Value = userId;

                parameters[1] = new MySqlParameter("?kind", MySqlDbType.VarChar);
                parameters[1].Value = kind;

                parameters[2] = new MySqlParameter("?roundId", MySqlDbType.VarChar);
                parameters[2].Value = maxRoundId;

                parameters[3] = new MySqlParameter("?ip", MySqlDbType.VarChar);
                parameters[3].Value = GlobalCommonVO.IPTail;


                cmd.Parameters.AddRange(parameters);
                int effecRowCount = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                if (newConnection != null)
                {
                    newConnection.Close();
                    newConnection.Dispose();
                    newConnection = null;
                }
            }

            return maxRoundId;
        }

        public int getMaxRoundId(string userId, string kind)
        {
            MySqlConnection newConnection = null;
            try
            {
                int result = 0;
                string command = "select ifnull(max(round_id),0) as max_round_id from play_record where player_id = ?userId AND kind = ?kind";

                newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);

                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[2];

                parameters[0] = new MySqlParameter("?userId", MySqlDbType.VarChar);
                parameters[0].Value = userId;
                parameters[1] = new MySqlParameter("?kind", MySqlDbType.VarChar);
                parameters[1].Value = kind;

                cmd.Parameters.AddRange(parameters);

                using (MySqlDataReader data_reader = cmd.ExecuteReader())
                {
                    if (data_reader.HasRows)
                    {
                        while (data_reader.Read())
                        {
                            result = data_reader.GetInt16("max_round_id");
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
            finally
            {
                if (newConnection != null)
                {
                    newConnection.Close();
                    newConnection.Dispose();
                    newConnection = null;
                }
            }

        }

        public void updateMainRecord(string userId, string kind, int roundId, int score = 0, int wrong=0, int correct = 0)
        {
            MySqlConnection newConnection = null;
            try
            {
                string command = "update play_record SET end_time =  DATE_FORMAT(now(),'%y%m%d%H%i%s'), score = ?score, wrong = ?wrong, correct = ?correct "+
                    " WHERE player_id = ?userId AND kind = ?kind AND round_id = ?roundId";

                newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);
                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[6];

                parameters[0] = new MySqlParameter("?userId", MySqlDbType.VarChar);
                parameters[0].Value = userId;

                parameters[1] = new MySqlParameter("?kind", MySqlDbType.VarChar);
                parameters[1].Value = kind;

                parameters[2] = new MySqlParameter("?roundId", MySqlDbType.Int16);
                parameters[2].Value = roundId;

                parameters[3] = new MySqlParameter("?score", MySqlDbType.Int16);
                parameters[3].Value = score;

                parameters[4] = new MySqlParameter("?wrong", MySqlDbType.Int16);
                parameters[4].Value = wrong;

                parameters[5] = new MySqlParameter("?correct", MySqlDbType.Int16);
                parameters[5].Value = correct;

                cmd.Parameters.AddRange(parameters);
                int effecRowCount = cmd.ExecuteNonQuery();

                if (effecRowCount <= 0)
                {
                    throw new SoftwareException("沒有更新到play_record的資料");
                }

            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                if (newConnection != null)
                {
                    newConnection.Close();
                    newConnection.Dispose();
                    newConnection = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kind">單字學習/聽力練習</param>
        /// <param name="type">做動作/進來看</param>
        /// <param name="vocabularyId"></param>
        public void updateLearnRecord(string userId, string kind, string type, string vocabularyId)
        {

            //throw new SoftwareException("The function is not published openly");
        }

        private void insertLearnRecord(string userId, string kind, string type, string vocabularyId)
        {
            //throw new SoftwareException("The function is not published openly");

        }

        public void insertPlayRecordDetail(string userId, string kind, int roundId, string questionId, string result, int spendTime)
        {
            string command = "insert into play_record_detail (player_id,record_time,kind,round_id,question_id,result,spend_time) VALUES " +
                        " ( ?userId,DATE_FORMAT(now(),'%y%m%d%H%i%s'),?kind,?roundId,?questionId,?result, ?spendTime ) ";

            MySqlConnection newConnection = null;
            try
            {

                newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);
                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[6];

                parameters[0] = new MySqlParameter("?userId", MySqlDbType.VarChar);
                parameters[0].Value = userId;

                parameters[1] = new MySqlParameter("?kind", MySqlDbType.VarChar);
                parameters[1].Value = kind;

                parameters[2] = new MySqlParameter("?roundId", MySqlDbType.Int16);
                parameters[2].Value = roundId;

                parameters[3] = new MySqlParameter("?questionId", MySqlDbType.VarChar);
                parameters[3].Value = questionId;

                parameters[4] = new MySqlParameter("?result", MySqlDbType.VarChar);
                parameters[4].Value = result;

                parameters[5] = new MySqlParameter("?spendTime", MySqlDbType.Int16);
                parameters[5].Value = spendTime;

                cmd.Parameters.AddRange(parameters);
                int effecRowCount = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                if (newConnection != null)
                {
                    newConnection.Close();
                    newConnection.Dispose();
                    newConnection = null;
                }
            }
        }

        public Dictionary<VocabularyVO, string> retrievePlayRecoedDetailVocabularys(string userId, string kind, int roundId)
        {
            try
            {
                Dictionary<VocabularyVO, string> result = new Dictionary<VocabularyVO, string>();


                string command = "select result, question_id, b.vocabulary, b.phonogram, b.chinese_meaning, b.content_unit, "+
                                " b.vocabulary_type, b.action_recognition, b.object_recognition, b.is_enable, b.vocabulary_kind "+
                                " FROM play_record_detail a join vocabulary_list b on a.question_id = b.id "+
                                " where a.player_id = ?userId AND a.kind = ?kind AND a.round_id = ?roundId";

                MySqlConnection newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);

                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[3];

                parameters[0] = new MySqlParameter("?userId", MySqlDbType.VarChar);
                parameters[0].Value = userId;

                parameters[1] = new MySqlParameter("?kind", MySqlDbType.VarChar);
                parameters[1].Value = kind;

                parameters[2] = new MySqlParameter("?roundId", MySqlDbType.Int16);
                parameters[2].Value = roundId;

                cmd.Parameters.AddRange(parameters);

                //Data ResultSet
                using (MySqlDataReader data_reader = cmd.ExecuteReader())
                {
                    if (data_reader.HasRows)
                    {
                        while (data_reader.Read())
                        {

                            VocabularyVO data = new VocabularyVO();
                            data.ID = data_reader.GetInt16("question_id");
                            data.Vocabulary = data_reader.GetString("vocabulary");
                            //data.Phonogram = data_reader.GetString("phonogram"); //null
                            data.ChineseMeaning = data_reader.GetString("chinese_meaning");
                            data.ContentUnit = data_reader.GetString("content_unit");
                            data.Type = data_reader.GetString("vocabulary_type");
                            //data.ActionRecognition = data_reader.GetString("action_recognition");
                            //.ObjectRecognition = data_reader.GetString("object_recognition"); null
                            //data.IsEnable = data_reader.GetString("is_enable");
                            string vocabularyKind = data_reader.GetString("vocabulary_kind");
                            if (vocabularyKind == "A")
                                data.Kind = VocabularyVO.Kinds.Action;
                            else if (vocabularyKind == "O")
                                data.Kind = VocabularyVO.Kinds.Object;
                            else
                                data.Kind = VocabularyVO.Kinds.None;
                            
                            result.Add( data , data_reader.GetString("result"));
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

        public string retrievePlayGameFlag(string computerId)
        {
            string result = "";
            try
            {
                string command = "select play_flag FROM play_game_conf where computer_id = ?computerId";

                MySqlConnection newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);

                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[1];

                parameters[0] = new MySqlParameter("?computerId", MySqlDbType.VarChar);
                parameters[0].Value = computerId;

                cmd.Parameters.AddRange(parameters);

                //Data ResultSet
                using (MySqlDataReader data_reader = cmd.ExecuteReader())
                {
                    if (data_reader.HasRows)
                    {
                        while (data_reader.Read())
                        {
                            result = data_reader.GetString("play_flag");
                           
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

        public string retrievePlaySentenceGameFlag(string computerId)
        {
            throw new SoftwareException("The function is not published openly");

        }

        public void insertPlaySentenceRecordDetail(string userId, string kind, int roundId, string sentenceCateId, string conversationId, int historyId, string sentenceId, int vocabularyId, string result, int spendTime)
        {
            throw new SoftwareException("The function is not published openly");
        }

        public List<string[]> retrievePlaySentenceWrongRecoed(string userId, string kind, int roundId)
        {
            throw new SoftwareException("The function is not published openly");

        }

    }
}
