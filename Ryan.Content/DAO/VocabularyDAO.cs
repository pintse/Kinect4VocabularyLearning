using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MySql.Data.MySqlClient;
using Ryan.Common.DAO;
using Ryan.Content.VO;

namespace Ryan.Content.DAO
{
    /// <summary>
    /// 字彙DAO
    /// </summary>
    class VocabularyDAO : BaseDAO
    {
        private static VocabularyDAO _Myself = new VocabularyDAO();
        private static ILog log = LogManager.GetLogger(typeof(VocabularyDAO));

        private VocabularyDAO()
        {
        }

        public static VocabularyDAO getInstance()
        {
            return _Myself;
        }

        public Dictionary<string, VocabularyVO> retrieveVocabularys()
        {
            try
            {
                Dictionary<string, VocabularyVO> result = new Dictionary<string, VocabularyVO>();


                string command = "select a.id, vocabulary, ifnull(phonogram,'') as phonogram, chinese_meaning, content_unit, vocabulary_type, action_recognition, "+
                    " ifnull(object_recognition,'') as object_recognition, vocabulary_kind, " +
                    " b.text_sentence, ifnull(b.text_sentence_chinese,'') as text_sentence_chinese, ifnull(b.example_sentence,'') as example_sentence, "+
                    " ifnull(b.example_sentence_chinese,'') as example_sentence_chinese, ifnull(b.extra_sentence,'') as extra_sentence, ifnull(b.extra_sentence_chinese,'') as extra_sentence_chinese " +
                    " from vocabulary_list a join vocabulary_sentence b on a.id = b.id " +
                    " where a.is_enable = 'Y'";

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
                            List<string> sentence = new List<string>();
                            List<string> sentenceChinese = new List<string>();

                            VocabularyVO data = new VocabularyVO();
                            data.ID = data_reader.GetInt16("id");
                            data.Vocabulary = data_reader.GetString("vocabulary");
                            data.Phonogram = data_reader.GetString("phonogram");
                            data.ChineseMeaning = data_reader.GetString("chinese_meaning");
                            data.ContentUnit = data_reader.GetString("content_unit");
                            data.Type = data_reader.GetString("vocabulary_type");
                            data.ActionRecognition = data_reader.GetString("action_recognition");
                            data.ObjectRecognition = data_reader.GetString("object_recognition");
                            //data.IsEnable = data_reader.GetString("is_enable");
                            string vocabularyKind = data_reader.GetString("vocabulary_kind");
                            if (vocabularyKind=="A")
                                data.Kind = VocabularyVO.Kinds.Action;
                            else if (vocabularyKind == "O")
                                data.Kind = VocabularyVO.Kinds.Object;
                            else
                                data.Kind = VocabularyVO.Kinds.None;
                            
                            sentence.Add(data_reader.GetString("example_sentence"));
                            sentenceChinese.Add(data_reader.GetString("example_sentence_chinese"));
                            sentence.Add(data_reader.GetString("text_sentence"));
                            sentenceChinese.Add(data_reader.GetString("text_sentence_chinese"));
                            sentence.Add(data_reader.GetString("extra_sentence"));
                            sentenceChinese.Add(data_reader.GetString("extra_sentence_chinese"));

                            data.Sentences = sentence;
                            data.SentencesChinese = sentenceChinese;
                            result.Add( Convert.ToString(data_reader.GetInt16("id")), data);
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

        public Dictionary<string, VocabularyVO> retrieveUnfamiliarVocabularys(string playerId, string groupId)
        {
            try
            {
                Dictionary<string, VocabularyVO> result = new Dictionary<string, VocabularyVO>();


                string command = " select 1,a.id, a.vocabulary, a.action_recognition, ifnull(a.object_recognition,'') as object_recognition, a.vocabulary_kind, b.example_sentence, " +
" ifnull(b.extra_sentence,'') as extra_sentence, b.text_sentence,''  as err_sum, ifnull(a.phonogram,'') as phonogram, a.chinese_meaning, a.vocabulary_type,  " +
" example_sentence_chinese, text_sentence_chinese, ifnull(extra_sentence_chinese,'') as extra_sentence_chinese  " +
" FROM vocabulary_list a join vocabulary_sentence b on a.id = b.id  " +
" where a.is_enable = 'Y' and a.id not in ( select player_vocabulary_err.vocabulary_id FROM player_vocabulary_err where player_vocabulary_err.player_id = ?playerId ) " +
" union  " +
" select 2,a.id, a.vocabulary, a.action_recognition, ifnull(a.object_recognition,'') as object_recognition, a.vocabulary_kind, b.example_sentence,  " +
" ifnull(b.extra_sentence,'') as extra_sentence, b.text_sentence,''  as err_sum, ifnull(a.phonogram,'') as phonogram, a.chinese_meaning, a.vocabulary_type,  " +
" example_sentence_chinese, text_sentence_chinese, ifnull(extra_sentence_chinese,'') as extra_sentence_chinese  " +
" FROM vocabulary_list a join vocabulary_sentence b on a.id = b.id  " +
" where a.is_enable = 'Y' and a.id in ( select player_vocabulary_err.vocabulary_id FROM player_vocabulary_err where player_vocabulary_err.player_id = ?playerId and correct_sum = 0 AND wrong_sum = 0 ) " +
" union " +
" select * from  " +
" (select 3, a.id, a.vocabulary, a.action_recognition, ifnull(a.object_recognition,'') as object_recognition, a.vocabulary_kind, b.example_sentence,  " +
" ifnull(b.extra_sentence, '') as extra_sentence, b.text_sentence,  (((c.wrong_sum*11) + c.practice_sum) - (c.correct_sum*11)) as err_sum, ifnull(a.phonogram,'') as phonogram, a.chinese_meaning, a.vocabulary_type,  " +
" example_sentence_chinese, text_sentence_chinese, ifnull(extra_sentence_chinese, '') as extra_sentence_chinese  " +
" FROM vocabulary_list a join vocabulary_sentence b on (a.id = b.id)   " +
" join player_vocabulary_err c on a.id = c.vocabulary_id  " +
" where a.is_enable = 'Y' and c.player_id = ?playerId " +
" and (c.correct_sum <> 0 or c.wrong_sum <> 0) " +
" order by err_sum desc , (c.wrong_sum + c.correct_sum) asc, id desc ) d ";

                MySqlConnection newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);

                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[1];

                parameters[0] = new MySqlParameter("?playerId", MySqlDbType.VarChar);
                parameters[0].Value = playerId;

                //parameters[1] = new MySqlParameter("?conversation_id", MySqlDbType.VarChar);
                //parameters[1].Value = conversationId;

                cmd.Parameters.AddRange(parameters);


                //Data ResultSet
                using (MySqlDataReader data_reader = cmd.ExecuteReader())
                {
                    if (data_reader.HasRows)
                    {
                        while (data_reader.Read())
                        {
                            List<string> sentence = new List<string>();
                            List<string> sentenceChinese = new List<string>();

                            VocabularyVO data = new VocabularyVO();
                            data.ID = data_reader.GetInt16("id");
                            data.Vocabulary = data_reader.GetString("vocabulary");
                            data.Phonogram = data_reader.GetString("phonogram");
                            data.ChineseMeaning = data_reader.GetString("chinese_meaning");
                            //data.ContentUnit = data_reader.GetString("content_unit");
                            data.Type = data_reader.GetString("vocabulary_type");
                            data.ActionRecognition = data_reader.GetString("action_recognition");
                            data.ObjectRecognition = data_reader.GetString("object_recognition");
                            //data.IsEnable = data_reader.GetString("is_enable");
                            string vocabularyKind = data_reader.GetString("vocabulary_kind");
                            if (vocabularyKind == "A")
                                data.Kind = VocabularyVO.Kinds.Action;
                            else if (vocabularyKind == "O")
                                data.Kind = VocabularyVO.Kinds.Object;
                            else
                                data.Kind = VocabularyVO.Kinds.None;

                            sentence.Add(data_reader.GetString("example_sentence"));
                            sentenceChinese.Add(data_reader.GetString("example_sentence_chinese"));
                            sentence.Add(data_reader.GetString("text_sentence"));
                            sentenceChinese.Add(data_reader.GetString("text_sentence_chinese"));
                            sentence.Add(data_reader.GetString("extra_sentence"));
                            sentenceChinese.Add(data_reader.GetString("extra_sentence_chinese"));

                            data.Sentences = sentence;
                            data.SentencesChinese = sentenceChinese;
                            result.Add(Convert.ToString(data_reader.GetInt16("id")), data);
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

        public void updateVocabularyStatistics(string userId,string activeKind , string updateType,string vocabularyId, int value)
        {
            try{
            string command = "update player_vocabulary_err SET ";
            
            if (updateType == "c") 
                command += " correct_sum = correct_sum+?sumValue ";
            else if (updateType == "w") 
                 command += " wrong_sum = wrong_sum+?sumValue ";
            else if (updateType == "p") 
                command += " practice_sum = practice_sum+?sumValue ";

            command += " where player_vocabulary_err.player_id = ?player_id AND player_vocabulary_err.vocabulary_id = ?vocabulary_id AND player_vocabulary_err.kind = ?kind";

                MySqlConnection newConnection = getNewConnection();
                newConnection.Open();

                MySqlCommand cmd = new MySqlCommand(command, newConnection);
                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[4];  
  
                parameters[0] = new MySqlParameter("?sumValue", MySqlDbType.Int16);  
                parameters[0].Value = value;

                parameters[1] = new MySqlParameter("?player_id", MySqlDbType.VarChar);  
                parameters[1].Value = userId;

                parameters[2] = new MySqlParameter("?vocabulary_id", MySqlDbType.Int16);  
                parameters[2].Value = vocabularyId;

                parameters[3] = new MySqlParameter("?kind", MySqlDbType.VarChar);  
                parameters[3].Value = activeKind;

                cmd.Parameters.AddRange(parameters);
                int effecRowCount = cmd.ExecuteNonQuery();  
                if (effecRowCount <= 0 )
                {
                    insertVocabularyStatistics(userId, activeKind, updateType, vocabularyId, value);
                    //return;
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

        public void insertVocabularyStatistics(string userId, string activeKind, string updateType, string vocabularyId, int value)
        {
            try
            {
                string command = "insert into player_vocabulary_err ( player_id,vocabulary_id, kind, correct_sum,wrong_sum,practice_sum) VALUES "+
                    " ( ?player_id, ?vocabulary_id, ?kind, ?correct_sum, ?wrong_sum, ?practice_sum )";

                MySqlConnection newConnection = getNewConnection();
                newConnection.Open();

                int correctSum = 0;
                int wrongSum = 0;
                int practiceSum = 0;

                if (updateType == "c")
                    correctSum = value;
                else if (updateType == "w")
                    wrongSum = value;
                else if (updateType == "p")
                    practiceSum = value;


                MySqlCommand cmd = new MySqlCommand(command, newConnection);
                //加入參數  
                MySqlParameter[] parameters = new MySqlParameter[6];

                parameters[0] = new MySqlParameter("?player_id", MySqlDbType.VarChar);
                parameters[0].Value = userId;

                parameters[1] = new MySqlParameter("?vocabulary_id", MySqlDbType.Int16);
                parameters[1].Value = int.Parse(vocabularyId);

                parameters[2] = new MySqlParameter("?kind", MySqlDbType.VarChar);
                parameters[2].Value = activeKind;

                parameters[3] = new MySqlParameter("?correct_sum", MySqlDbType.Int32);
                parameters[3].Value = correctSum;
                parameters[4] = new MySqlParameter("?wrong_sum", MySqlDbType.Int32);
                parameters[4].Value = wrongSum;
                parameters[5] = new MySqlParameter("?practice_sum", MySqlDbType.Int32);
                parameters[5].Value = practiceSum;


                cmd.Parameters.AddRange(parameters);
                int effecRowCount = cmd.ExecuteNonQuery();

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
