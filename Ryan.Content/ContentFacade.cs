using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Ryan.Content.Service;
using Ryan.Content.VO;
using Ryan.Common;

namespace Ryan.Content
{
    /// <summary>
    /// Content模組對外接收服務請求
    /// </summary>
    public class ContentFacade
    {
        private static volatile ContentFacade _Myself;
        private static readonly object ticket = new object();
        private static ILog log = LogManager.GetLogger(typeof(ContentFacade));
        private VocabularyService _VocabularyService = VocabularyService.getInstance();
        private SpeechSynthesizeService _SpeechSynthesizeService = SpeechSynthesizeService.getInstance();
        private RecordService _RecordService = RecordService.getInstance();

        private ContentFacade() 
        {
            initialRelatedData();
        }

        public static ContentFacade getInstance()
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new ContentFacade();

                    }
                }

            }
            return _Myself;
        }

        private void initialRelatedData()
        {
            _VocabularyService.initialVocabularys();
        }

        public void speech(string words, int rate)
        {
            _SpeechSynthesizeService.speech(words, rate);
        }

        public void speech2(string words, int rate)
        {
            _SpeechSynthesizeService.speech2(words, rate);
        }

        public void speechIntoQueue(List<string[]> speechMetaData)
        {
            foreach(var strarray in speechMetaData)
            {
                _SpeechSynthesizeService.speakAsync(strarray[0], int.Parse(strarray[1]));
            }

            speechMetaData.Clear();
            speechMetaData = null;

        }

        public void cancelSpeak()
        {
            _SpeechSynthesizeService.cancelSpeak();
        }

        public VocabularyVO retrieveVocabularyByRandom()
        {
            return _VocabularyService.retrieveVocabularyByRandom();
        }

        public Dictionary<string, VocabularyVO> retrieveUnfamiliarVocabularys(string playerId, string groupId)
        {
            return _VocabularyService.retrieveUnfamiliarVocabularys(playerId, groupId);
        }

        public void updateVocabularyStatistics(string userId, string activeKind, string updateType, string vocabularyId, int value)
        {
            _VocabularyService.updateVocabularyStatistics(userId, activeKind, updateType, vocabularyId, value);
        }

        /////

        public int insertMainRecord(string userId, string kind)
        {
            return _RecordService.insertMainRecord(userId, kind);
        }

        public void updateMainRecord(string userId, string kind, int roundId, int score = 0, int wrong = 0, int correct = 0)
        {
            _RecordService.updateMainRecord(userId, kind, roundId, score, wrong, correct);
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
            _RecordService.updateLearnRecord(userId, kind, type, vocabularyId);
        }

        public void insertPlayRecordDetail(string userId, string kind, int roundId, string questionId, string result, int spendTime)
        {
            _RecordService.insertPlayRecordDetail(userId, kind, roundId, questionId, result, spendTime);
        }

        public void insertPlaySentenceRecordDetail(string userId, string kind, int roundId, string sentenceCateId, string conversationId, int historyId, string sentenceId, int vocabularyId, string result, int spendTime)
        {
            _RecordService.insertPlaySentenceRecordDetail(userId, kind, roundId, sentenceCateId, conversationId, historyId, sentenceId, vocabularyId, result, spendTime);
        }

        public Dictionary<VocabularyVO, string> retrievePlayRecoedDetailVocabularys(string userId, string kind, int roundId)
        {
            return _RecordService.retrievePlayRecoedDetailVocabularys(userId, kind, roundId);
        }

        public List<string[]> retrievePlaySentenceWrongRecoed(string userId, string kind, int roundId)
        {
            return _RecordService.retrievePlaySentenceWrongRecoed(userId, kind, roundId);
        }

        public bool retrievePlayGameFlag()
        {
            return _RecordService.retrievePlayGameFlag();

        }

        public bool retrievePlaySentenceGameFlag()
        {
            throw new SoftwareException("The function is not published openly"); 
        }

    }
}
