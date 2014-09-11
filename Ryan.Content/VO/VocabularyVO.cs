using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryan.Content.VO
{
    /// <summary>
    /// 字彙Value Object
    /// </summary>
    public class VocabularyVO
    {
        public enum Kinds { Action, Object, All, None }
        public int ID{get;set;}
        public string Vocabulary{get;set;}
        public string Phonogram{get;set;}
        public string ChineseMeaning{get;set;}
        public string ContentUnit{get;set;}
        /// <summary>
        /// 詞性
        /// </summary>
        public string Type{get;set;}
        public string ActionRecognition{get;set;}
        public string ObjectRecognition{get;set;}
        public string IsEnable{get;set;}
        /// <summary>
        /// 字彙分類。A:Action、O:Object、N:Noun
        /// </summary>
        public Kinds Kind { get; set; }

        public List<string> Sentences { get; set; }
        /// <summary>
        /// 必須與Sentence順序對應
        /// </summary>
        public List<string> SentencesChinese { get; set; }
    }
}
