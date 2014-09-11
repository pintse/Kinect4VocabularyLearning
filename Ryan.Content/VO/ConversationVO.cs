using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.Content.VO
{
    /// <summary>
    /// 會話Value Object
    /// </summary>
    public class ConversationVO
    {
        public ConversationVO() { }

        public ConversationVO(string groupId, string conversationId, int historyId, string conversationName, Dictionary<string, string> sentence,
            Dictionary<string, string> recognizeActionVocabulary, Dictionary<string, string> recognizeObjectVocabulary)
        {
            this.GroupId = groupId;
            this.ConversationId = conversationId;
            this.HistoryId = historyId;
            this.ConversationName = conversationName;
            this.Sentences = sentence;
            this.RecognizeActionVocabulary = recognizeActionVocabulary;
            this.RecognizeObjectVocabulary = recognizeObjectVocabulary;
            this.PK = new ConversationPK(this.GroupId,this.ConversationId,this.HistoryId);
            this.RecognizeVocabularys = new Dictionary<string,List<string>>();
            this.SentencesChinese = new Dictionary<string, string>();
        }

        public ConversationPK PK
        {
            get;
            private set;
        }

        public string GroupId
        { 
            get; 
            set;
        }

        public string ConversationId
        {
            get;
            set;
        }

        public int HistoryId
        {
            get;
            set;
        }

        public Dictionary<string, string> Sentences
        {
            get;
            set;
        }

        public Dictionary<string, string> SentencesChinese
        {
            get;
            set;
        }

        public Dictionary<string,List<string>> RecognizeVocabularys
        {
            get;
            set;
        }

        public Dictionary<string, string> RecognizeObjectVocabulary
        {
            get;
            set;
        }

        public Dictionary<string, string> RecognizeActionVocabulary
        {
            get;
            set;
        }

        public string ConversationName
        {
            get;
            set;
        }

        public string FinishFlg
        {
            get;
            set;
        }

        public List<ConversationCommentVO> Comments = new List<ConversationCommentVO>();
 
        public class ConversationPK : IEquatable<ConversationPK>
        {
            public ConversationPK(string groupId, string conversationId, int historyId)
            {
                this.GroupId = groupId;
                this.ConversationId = conversationId;
                this.HistoryId = historyId;
            }

            public string GroupId
            {
                get;
                private set;
            }
            public string ConversationId
            {
                get;
                private set;
            }
            public int HistoryId
            {
                get;
                private set;
            }

            public static bool operator ==(ConversationPK a, ConversationPK b)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(a, b))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (((object)a == null) || ((object)b == null))
                {
                    return false;
                }

                // Return true if the fields match:
                return a.GroupId == b.GroupId && a.ConversationId == b.ConversationId && a.HistoryId == b.HistoryId;
            }

            public static bool operator !=(ConversationPK a, ConversationPK b)
            {
                return !(a == b);
            }

           
            public bool Equals(ConversationPK other)
            {
                return this == other;
            }
            public override int GetHashCode()
            {
                return (GroupId+ConversationId+HistoryId).GetHashCode();
            }
        }
    }
}
