using log4net;
using Ryan.Common.VO;
using Ryan.Content.DAO;
using Ryan.Content.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.Content.Service
{
    /// <summary>
    /// 軌跡紀錄服務
    /// </summary>
    class RecordService
    {
        private RecordDAO _RecordDAO = RecordDAO.getInstance();

        private static RecordService _Myself = new RecordService();
        private static ILog log = LogManager.GetLogger(typeof(VocabularyService));

        private Random _Random = new Random();

        private RecordService() { }

        public static RecordService getInstance()
        {
            return _Myself;
        }


        public int insertMainRecord(string userId, string kind)
        {
            return _RecordDAO.insertMainRecord(userId, kind);
        }

        public void updateMainRecord(string userId, string kind, int roundId, int score = 0, int wrong = 0, int correct = 0)
        {
            _RecordDAO.updateMainRecord(userId, kind, roundId, score, wrong, correct);
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
            _RecordDAO.updateLearnRecord(userId, kind, type, vocabularyId);
        }

        public void insertPlayRecordDetail(string userId, string kind, int roundId, string questionId, string result, int spendTime)
        {
            _RecordDAO.insertPlayRecordDetail(userId, kind, roundId, questionId, result, spendTime);
        }

        public void insertPlaySentenceRecordDetail(string userId, string kind, int roundId, string sentenceCateId, string conversationId, int historyId, string sentenceId, int vocabularyId, string result, int spendTime)
        {
            _RecordDAO.insertPlaySentenceRecordDetail(userId, kind, roundId, sentenceCateId, conversationId, historyId, sentenceId, vocabularyId, result, spendTime);
        }

        public Dictionary<VocabularyVO, string> retrievePlayRecoedDetailVocabularys(string userId, string kind, int roundId)
        {
            return _RecordDAO.retrievePlayRecoedDetailVocabularys(userId, kind, roundId);
        }

        public List<string[]> retrievePlaySentenceWrongRecoed(string userId, string kind, int roundId)
        {
            return _RecordDAO.retrievePlaySentenceWrongRecoed(userId, kind, roundId);
        }

        public bool retrievePlayGameFlag()
        {
            string flag = _RecordDAO.retrievePlayGameFlag(GlobalCommonVO.IPTail);
            if (flag == "n")
            {
                return false;
            }
            else if (flag == "y")
            {
                return true;
            }
            else
            {
                log.Fatal(GlobalCommonVO.IPTail + "是否可遊戲設定錯誤");
                return false;
            }

        }

        
    }
}
