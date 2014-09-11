using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ryan.Content.VO;
using Ryan.Kinect.GestureCommand.VO;
using Ryan.Kinect.Toolkit;
using Ryan.Kinect.Toolkit.ContentProcess;
using WpfAnimatedGif;

namespace Presentation
{
    /// <summary>
    /// 處理學習內容呈現
    /// </summary>
    public partial class RecognitionWindow : Window
    {
        public void setVocabularyContent(MainMenuWindow.TaskTypes taskType, string itemId, Window parentWindow)
        {
            this.ItemId = itemId;
            this.TaskType = taskType;
            this.ParentWindow = parentWindow;
            this.textCommands.Visibility = System.Windows.Visibility.Visible;

            if(taskType == MainMenuWindow.TaskTypes.Vocabulary){
                this.Vocabulary = _ContentHandler.retrieveVocabularys()[ItemId];
                setVocabularyContent(this.Vocabulary);
            }
            else if (taskType == MainMenuWindow.TaskTypes.Listening)
            {
                Content = _ContentHandler.retrieveUnfamiliarVocabularys(_Player.userID, _Player.groupID);
                retrieveNextVocabulary4Listening();
            }

            _ContentHandler.updateLearnRecord(_Player.userID, this.TaskType.ToString(), "view", this.Vocabulary.ID.ToString());
        }

        private void retrieveNextVocabulary()
        {
            string nextItme = "";
            VocabularyVO nextVocabulary = null;
            if (this.Vocabulary.Kind == VocabularyVO.Kinds.Action)
            {
                nextVocabulary = getNextVocabularyItme(_ContentHandler.retrieveActionVocabularys());
            }
            else if (this.Vocabulary.Kind == VocabularyVO.Kinds.Object)
            {
                nextVocabulary = getNextVocabularyItme(_ContentHandler.retrieveObjectVocabularys());
            }
            else
            {
                nextVocabulary = getNextVocabularyItme(_ContentHandler.retrieveOtherVocabularys());
                this.noneRecognitionView();
            }

            nextItme = nextVocabulary.ID.ToString();

            this.setVocabularyContent(this.TaskType, nextItme, this.ParentWindow);
        }

        private void retrieveNextVocabulary4Listening()
        {
            this.SOSButton.Visibility = System.Windows.Visibility.Visible;
            helpTime = 0;

            VocabularyVO nextVocabulary = getNextVocabularyItmeByUnfamiliar();

            if (!(nextVocabulary.Kind == VocabularyVO.Kinds.Action) && !(nextVocabulary.Kind == VocabularyVO.Kinds.Object))
            {
                this.noneRecognitionView();
            }

            this.setVocabularyContent(nextVocabulary);

            initalListeningUI();
        }

        private void setVocabularyContent(VocabularyVO vocabulary)
        {
            this.Vocabulary = vocabulary;

            if (this.TaskType == MainMenuWindow.TaskTypes.Listening)
            {
                this.ItemId = "";
            }

            loadVocabularyContent();
            setTaskRecognition();

            
        }
        
        

        private void loadVocabularyContent()
        {
            
            textCommands.Text = this.Vocabulary.Vocabulary;
            tbMessages.Text = "";
            tbMessages.Visibility = System.Windows.Visibility.Hidden;

            if (this.TaskType == MainMenuWindow.TaskTypes.Vocabulary )
            {
                this.SpeakSentenceButton.Visibility = System.Windows.Visibility.Hidden;

                showVocabularyImage();
            }
            else
            {
                showTextContent();
            }


            speech(this.Vocabulary.Vocabulary, 2);
            
        }

        private void showVocabularyImage()
        {
            ImageBehavior.SetAnimatedSource(imageSample, null);
            try
            {
                if (this.Vocabulary.Kind == VocabularyVO.Kinds.Action)
                {

                    this.ObjectRecButton.Visibility = System.Windows.Visibility.Hidden;
                    var image = new BitmapImage();
                    image.BeginInit();

                    string filePath = Path.Combine(Environment.CurrentDirectory, @"Data\ContentPictures\Vocabulary\" + this.Vocabulary.Vocabulary + ".gif");
                    image.UriSource = new Uri(filePath);
                    image.EndInit();
                    this.imageSample.Visibility = System.Windows.Visibility.Visible;
                    ImageBehavior.SetAnimatedSource(imageSample, image);


                }
                else if (this.Vocabulary.Kind == VocabularyVO.Kinds.Object)
                {
                    this.imageSample.Source = null;
                    this.tbMessages.Text = "哪一個物件會是與這個單字最有關聯的呢？\n"+this.Vocabulary.ChineseMeaning.Substring(0,1);
                    this.tbMessages.Visibility = System.Windows.Visibility.Visible;

                }
            }
            catch (FileNotFoundException)
            {
                var image = new BitmapImage();
                image.BeginInit();

                string filePath = Path.Combine(Environment.CurrentDirectory, @"UIResource\unfinish.jpg"); 
                image.UriSource = new Uri(filePath);
                image.EndInit();
                this.imageSample.Source = image;
                this.ObjectRecButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void speech(string words, int times)
        {
            ParameterizedThreadStart ParStart = new ParameterizedThreadStart(speechAnotherThread);

            Thread speechThread = new Thread(ParStart);
            //speechThread.SetApartmentState(ApartmentState.STA);
            object[] data = new object[2];
            data[0] = times;
            data[1] = words;
            speechThread.Start(data);
        }

        private void speechAnotherThread(Object o)
        {
            Thread.Sleep(2000);
            List<string[]> speechDatas = new List<string[]>();

            object[] data = (object[])o;
            for (int i = 0; i < (int)data[0]; i++)
            {
                speechDatas.Add(new string[] { (string)data[1], ContentHandler.WORD_SPEECH_SPEED.ToString() });

            }
            _ContentHandler.speechIntoQueue(speechDatas);
        }

        private void showTextContent()
        {
            textCommands.Visibility = Visibility.Visible;
            tbMessages.Visibility = Visibility.Visible;

            this.textCommands.Text = this.Vocabulary.Vocabulary + "  " + this.Vocabulary.ChineseMeaning;

            tbMessages.Visibility = Visibility.Visible;

            tbMessages.Text = "";
            tbMessages.Text += this.Vocabulary.Type+" "+this.Vocabulary.Phonogram + "\n";

            string [] sentence = retrieveSentence();
            tbMessages.Text += sentence[0] + "\n" + sentence[1];
            this.imageSample.Visibility = System.Windows.Visibility.Hidden;
            ImageBehavior.SetAnimatedSource(imageSample, null);

            this.SpeakSentenceButton.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 設定辨識的標的是什麼
        /// </summary>
        private void setTaskRecognition()
        {
            _KinectProcessor.RecognitionFinishTriggerMag = null;
            _KinectProcessor.cleanTaskRecognitions();

            this.ObjectRecButton.Visibility = System.Windows.Visibility.Hidden;
            _KinectProcessor.cleanObjectRecogntionArea();

            this.tbAllTimeInfo.Text = "請跟著念單字發音喔！";
            string action = this.Vocabulary.ActionRecognition ;
            if (action!="")
            {
                _KinectProcessor.addTaskRecognitionActions((GlobalData.GestureTypes)Enum.Parse(typeof(GlobalData.GestureTypes), action, true));
                this.tbAllTimeInfo.Text = "請一邊做動作一邊念出來。做動作可以聽單字發音喔！";
            }else if (this.Vocabulary.ObjectRecognition != ""){

                _KinectProcessor.drawObjectRecogntionArea();
                this.ObjectRecButton.Visibility = System.Windows.Visibility.Visible;
                _KinectProcessor.addTaskRecognitionObjects(this.Vocabulary.ObjectRecognition);
                this.tbAllTimeInfo.Text = "請將物品置紅框內，並盡量讓物品最大，右手按綠鍵！";
            }

            if (this.TaskType == MainMenuWindow.TaskTypes.Listening)
            {
                this.tbAllTimeInfo.Text = "請仔細聆聽，可按右下角的「i」出現三次提示！";
            }
        }

        private string[] retrieveSentence()
        {
            int i = 0;
            foreach (var message in this.Vocabulary.Sentences)
            {
                if (message != "")
                {
                    return new string[] { message, this.Vocabulary.SentencesChinese[i] };
                }
                i++;
            }

            return new string[] { "", "" };
        }

        void checkFinishRecognition()
        {
            if (_KinectProcessor == null || _KinectProcessor.countTaskRecognitions() > 0)
                return;

            this.labelInfo.Content = "Good!!";

            //InitializeMouseControl();

            if (this.TaskType == MainMenuWindow.TaskTypes.Listening)
            {
                this.imageSample.Visibility = System.Windows.Visibility.Hidden;
                ImageBehavior.SetAnimatedSource(imageSample, null);
                this.imageSample.Source = null;

            }
            else if (this.TaskType == MainMenuWindow.TaskTypes.Vocabulary )
            {
                
            }

            showTextContent();
        }

        
    }
}
