using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.Content.VO
{
    /// <summary>
    /// 會話評論Value Object
    /// </summary>
    public class ConversationCommentVO
    {
        public string GroupId; 
        public string ConversationId; 
        public int HistoryId; 
        public string FromGroupId;
        public string Comment;
    }
}
