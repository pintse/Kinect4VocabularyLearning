using log4net;
using Ryan.ObjectRecognition.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// 辨識結果處理
    /// </summary>
    class RecongitionResultProcessor : Ryan.ObjectRecognition.Service.IRecongitionResultProcessor
    {
        private static RecongitionResultProcessor _RecongitionResultProcessor = new RecongitionResultProcessor();
        private static ILog log = LogManager.GetLogger(typeof(RecongitionResultProcessor));

        private RecongitionResultProcessor()
        {
        }

        public static RecongitionResultProcessor getInstance()
        {
            return _RecongitionResultProcessor;
        }

        public CongruousObjectVO determine(Dictionary<string, CongruousObjectVO> congruousObjectList) 
        {
            CongruousObjectVO congruousObject = new CongruousObjectVO();
            congruousObject.ObjectPicture = new ObjectPictureVO();
            congruousObject.ObjectPicture.ObjectName = "";
            congruousObject.RecognitionScoreSet.Add("Color", 0);
            congruousObject.RecognitionScoreSet.Add("SURF", 0);
            foreach (KeyValuePair<string, CongruousObjectVO> kvp in congruousObjectList)
            {
                log.Debug(kvp.Key + "-SURF::" + (kvp.Value.RecognitionScoreSet.ContainsKey("SURF") ? kvp.Value.RecognitionScoreSet["SURF"] + "" : ""));
                if (kvp.Value.RecognitionScoreSet.ContainsKey("SURF") && kvp.Value.RecognitionScoreSet["SURF"] > congruousObject.RecognitionScoreSet["SURF"])
                {
                    congruousObject = kvp.Value;
                }
                else if ((kvp.Value.RecognitionScoreSet.ContainsKey("SURF") && kvp.Value.RecognitionScoreSet["SURF"] == congruousObject.RecognitionScoreSet["SURF"]) &&
                    (kvp.Value.RecognitionScoreSet.ContainsKey("Color") && kvp.Value.RecognitionScoreSet["Color"] > congruousObject.RecognitionScoreSet["Color"]))
                {
                    congruousObject = kvp.Value;
                }
                else if (kvp.Value.RecognitionScoreSet.ContainsKey("Color") && kvp.Value.RecognitionScoreSet["Color"] > congruousObject.RecognitionScoreSet["Color"])
                {
                    congruousObject = kvp.Value;
                }
            }

            return congruousObject;
        }
    }
}
