using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryan.Content.VO
{
    /// <summary>
    /// Content全域變數
    /// </summary>
    public class GlobalDataVO
    {
        public static Dictionary<string, VocabularyVO> Vocabularys = new Dictionary<string,VocabularyVO>();
        public static List<VocabularyVO> ActionVocabularys = new List<VocabularyVO>();
        public static List<VocabularyVO> ObjectVocabularys = new List<VocabularyVO>();
        public static List<VocabularyVO> OtherVocabularys = new List<VocabularyVO>();
        public static Dictionary<ConversationVO.ConversationPK, ConversationVO> Conversations = new Dictionary<ConversationVO.ConversationPK, ConversationVO>(); 

    }
}
