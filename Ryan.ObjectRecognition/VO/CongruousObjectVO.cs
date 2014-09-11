using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.ObjectRecognition.VO
{
    /// <summary>
    /// 候選物件Value Object
    /// </summary>
    public class CongruousObjectVO
    {
        private ObjectPictureVO _ObjectPicture;

        public ObjectPictureVO ObjectPicture
        {
            get { return _ObjectPicture; }
            set { _ObjectPicture = value; }
        }

        private Dictionary<string, double> _RecognitionScoreSet = new Dictionary<string,double>();

        public Dictionary<string, double> RecognitionScoreSet
        {
            get { return _RecognitionScoreSet; }
            set { _RecognitionScoreSet = value; }
        }

        
        
    }
}
