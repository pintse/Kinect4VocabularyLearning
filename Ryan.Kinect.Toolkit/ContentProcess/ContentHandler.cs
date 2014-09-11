using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Ryan.Common.VO;
using Ryan.Content;
using Ryan.Content.DAO;
using Ryan.Content.VO;

namespace Ryan.Kinect.Toolkit.ContentProcess
{
    /// <summary>
    /// Presentation Content Handler
    /// </summary>
    public class ContentHandler
    {
        private static ContentHandler _Myself = new ContentHandler();
        private static ILog log = LogManager.GetLogger(typeof(ContentHandler));
        public const int WORD_SPEECH_SPEED = -4;
        public const int SENTENCE_SPEECH_SPEED = -3;

        public const int SentenceGameTotalTimeConst = 300;
        public const int SentenceGameWaitAnswerTimeConst = 8;
        public const int SentenceGameWaitObjectAnswerTimeConst = 15;
        public const int SentenceGameExtraWaitAnswerTimeConst = 4;
        public const int SentenceGameExtraWaitObjectAnswerTimeConst = 7;

        private ContentFacade _ContentFacade;

        private Random _Random = new Random();

        private ContentHandler()
        {
            _ContentFacade = ContentFacade.getInstance();
        }

        public static ContentHandler getInstance()
        {
            return _Myself;
        }

        public Dictionary<string, VocabularyVO> retrieveVocabularys()
        {
            return GlobalDataVO.Vocabularys;
        }

        public List<VocabularyVO> retrieveActionVocabularys()
        {
            return GlobalDataVO.ActionVocabularys;
        }

        public List<VocabularyVO> retrieveObjectVocabularys()
        {
            return GlobalDataVO.ObjectVocabularys;
        }
        public List<VocabularyVO> retrieveOtherVocabularys()
        {
            return GlobalDataVO.OtherVocabularys;
        }

        public void speech(string words, int rate)
        {
            Object[] param = new Object[]{words, rate};
            ParameterizedThreadStart ParStart = new ParameterizedThreadStart(speech4Thread);
            Thread speechThread = new Thread(ParStart);
            speechThread.Start(param);  // 開始執行 SckSAcceptTd 這個執行緒
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="speechMetaData"></param>
        public void speechIntoQueue(List<string[]> speechMetaData)
        {
            ParameterizedThreadStart ParStart = new ParameterizedThreadStart(speechIntoQueue4Thread);
            Thread speechThread = new Thread(ParStart);
            speechThread.Start(speechMetaData);  // 開始執行 SckSAcceptTd 這個執行緒
        }

        public void cancelSpeak()
        {
            _ContentFacade.cancelSpeak();
        }

        public VocabularyVO retrieveVocabularyByRandom()
        {
            return _ContentFacade.retrieveVocabularyByRandom();
        }

        private void speech4Thread(object o)
        {
            Object[] param = (Object[])o;
            _ContentFacade.speech(param[0].ToString(), int.Parse( param[1].ToString()));
        }

        private void speechIntoQueue4Thread(object o)
        {
            List<string[]> param = (List<string[]>)o;
            _ContentFacade.speechIntoQueue(param);
        }

        public Dictionary<string, VocabularyVO> retrieveUnfamiliarVocabularys(string playerId, string groupId)
        {
            return _ContentFacade.retrieveUnfamiliarVocabularys(playerId, groupId);
        }

        public void updateVocabularyStatistics(string userId, string activeKind, string updateType, string vocabularyId, int value)
        {
            _ContentFacade.updateVocabularyStatistics(userId, activeKind, updateType, vocabularyId, value);
        }
       
        public int insertMainRecord(string userId, string kind)
        {
            return _ContentFacade.insertMainRecord(userId, kind);
        }

        public void updateMainRecord(string userId, string kind, int roundId, int score = 0, int wrong=0, int correct = 0)
        {
            _ContentFacade.updateMainRecord(userId, kind, roundId, score, wrong, correct);
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
            _ContentFacade.updateLearnRecord(userId, kind, type, vocabularyId);
        }

        public void insertPlayRecordDetail(string userId, string kind, int roundId, string questionId, string result, int spendTime)
        {
            _ContentFacade.insertPlayRecordDetail(userId, kind, roundId, questionId, result, spendTime);
        }

        public Dictionary<VocabularyVO, string> retrievePlayRecoedDetailVocabularys(string userId, string kind, int roundId)
        {
            return _ContentFacade.retrievePlayRecoedDetailVocabularys(userId, kind, roundId);
        }

        public bool retrievePlayGameFlag()
        {
            return _ContentFacade.retrievePlayGameFlag();

        }

    }
}
