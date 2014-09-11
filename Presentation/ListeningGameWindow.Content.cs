using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.Content.VO;
using Ryan.Kinect.GestureCommand.VO;
using Ryan.Kinect.Toolkit.ContentProcess;

namespace Presentation
{
    /// <summary>
    /// 處理遊戲學習內容呈現
    /// </summary>
    partial class ListeningGameWindow
    {
        void setQuestion()
        {

            _KinectProcessor.StartObjectRecFlag = false;
            _KinectProcessor.BRCS_SourceReadyFlag = false;
            _KinectProcessor.StartObjectRecThreadFlag = false;
            _KinectProcessor.StartCutImageTime = 0;

            if (ContentsIndex >= Contents.Count)
                ContentsIndex = 0;

            Vocabulary = this.Contents.ElementAt(ContentsIndex).Value;
            ContentsIndex++;

            List<string[]> data = new List<string[]>(); 
            data.Add(new string[]{Vocabulary.Vocabulary, ContentHandler.WORD_SPEECH_SPEED.ToString()});
            data.Add(new string[]{Vocabulary.Vocabulary, ContentHandler.WORD_SPEECH_SPEED.ToString()});
            _ContentHandler.speechIntoQueue(data);

            setTaskRecognition();

            if (Vocabulary.ActionRecognition != null && Vocabulary.ActionRecognition != "")
            {
                this.WaitAnswerTime = WaitAnswerTimeConst;
            }
            else
            {
                this.WaitAnswerTime = WaitObjectAnswerTimeConst;
            }
        }

        private void setTaskRecognition()
        {
            _KinectProcessor.RecognitionFinishTriggerMag = null;
            _KinectProcessor.cleanTaskRecognitions();

            this.ObjectRecButton.Visibility = System.Windows.Visibility.Hidden;
            _KinectProcessor.cleanObjectRecogntionArea();

            string action = this.Vocabulary.ActionRecognition;
            if (action != "")
            {
                _KinectProcessor.addTaskRecognitionActions((GlobalData.GestureTypes)Enum.Parse(typeof(GlobalData.GestureTypes), action, true));

            }
            else if (this.Vocabulary.ObjectRecognition != "")
            {
                _KinectProcessor.drawObjectRecogntionArea();
                this.ObjectRecButton.Visibility = System.Windows.Visibility.Visible;
                _KinectProcessor.addTaskRecognitionObjects(this.Vocabulary.ObjectRecognition);

            }

        }

        private void setNextQuestion()
        {
            this.WaitAnswerTime = WaitAnswerTimeConst;
            setQuestion();
        }

        public void showQResultList()
        {
            this.lbQResultList.Items.Clear();

            Dictionary< VocabularyVO, string > qlist = _ContentHandler.retrievePlayRecoedDetailVocabularys(_Player.userID, TaskType.ToString(), RoundId);
           
            foreach (var q in qlist)
            {
                if (q.Value == "w")
                {
                    MyItem myItem = new MyItem(q.Key.Vocabulary, q.Key.ID.ToString());
                    this.lbQResultList.Items.Add(myItem);
                }
            }

            this.lbQResultList.Visibility = System.Windows.Visibility.Visible;

        }
    }
}
