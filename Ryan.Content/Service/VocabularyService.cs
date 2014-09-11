using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Ryan.Content.DAO;
using Ryan.Content.VO;

namespace Ryan.Content.Service
{
    /// <summary>
    /// 字彙服務
    /// </summary>
    class VocabularyService
    {
        private static VocabularyService _Myself = new VocabularyService();
        private static ILog log = LogManager.GetLogger(typeof(VocabularyService));

        private Random _Random = new Random();

        private VocabularyService() { }

        public static VocabularyService getInstance()
        {
            return _Myself;
        }

        private Dictionary<string, VocabularyVO> retrieveVocabularys()
        {
            return VocabularyDAO.getInstance().retrieveVocabularys();
        }

        public void initialVocabularys()
        {
            GlobalDataVO.Vocabularys = retrieveVocabularys();
            foreach (var vocabulary in GlobalDataVO.Vocabularys)
            {
                if (vocabulary.Value.Kind == VocabularyVO.Kinds.None)
                {
                    GlobalDataVO.OtherVocabularys.Add(vocabulary.Value);
                    continue;
                }

                if (vocabulary.Value.Kind == VocabularyVO.Kinds.Action || vocabulary.Value.Kind == VocabularyVO.Kinds.All)
                {
                    GlobalDataVO.ActionVocabularys.Add(vocabulary.Value);
                }
                if (vocabulary.Value.Kind== VocabularyVO.Kinds.Object || vocabulary.Value.Kind == VocabularyVO.Kinds.All)
                {
                    GlobalDataVO.ObjectVocabularys.Add(vocabulary.Value);
                }
            }
        }

        public void initialVocabularysByRecognition()
        {
            GlobalDataVO.Vocabularys = retrieveVocabularys();
            foreach (var vocabulary in GlobalDataVO.Vocabularys)
            {
                if ((vocabulary.Value.ActionRecognition == null || vocabulary.Value.ActionRecognition == "") && 
                    (vocabulary.Value.ObjectRecognition == null || vocabulary.Value.ObjectRecognition == ""))
                {
                    GlobalDataVO.OtherVocabularys.Add(vocabulary.Value);
                    continue;
                }

                if (vocabulary.Value.ActionRecognition != null && vocabulary.Value.ActionRecognition != "")
                {
                    GlobalDataVO.ActionVocabularys.Add(vocabulary.Value);
                }
                if (vocabulary.Value.ObjectRecognition != null && vocabulary.Value.ObjectRecognition != "")
                {
                    GlobalDataVO.ObjectVocabularys.Add(vocabulary.Value);
                }
            }
        }

        public VocabularyVO retrieveVocabularyByRandom()
        {
            int index = _Random.Next(0, GlobalDataVO.Vocabularys.Count - 1);

            VocabularyVO result = GlobalDataVO.Vocabularys.ElementAt(index).Value;

            if (result.Kind == VocabularyVO.Kinds.None)
                result = retrieveVocabularyByRandom();

            return result;
        }

        public Dictionary<string, VocabularyVO> retrieveUnfamiliarVocabularys(string playerId, string groupId)
        {
            return VocabularyDAO.getInstance().retrieveUnfamiliarVocabularys( playerId, groupId);
        }

        public void updateVocabularyStatistics(string userId, string activeKind, string updateType, string vocabularyId, int value)
        {
            VocabularyDAO.getInstance().updateVocabularyStatistics(userId, activeKind, updateType, vocabularyId, value);
        }
    }
}
