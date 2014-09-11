using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using log4net;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using Ryan.Content.VO;
using Ryan.Kinect.GestureCommand.VO;
using Ryan.Kinect.Toolkit;
using Ryan.Kinect.Toolkit.ContentProcess;
using Ryan.Kinect.Toolkit.KinectProcess;
using Ryan.Kinect.Toolkit.VO;

namespace Presentation
{
    /// <summary>
    /// 字彙學習、聽力練習。處理window畫面呈現基本控制與背景執行序控制
    /// </summary>
    public partial class RecognitionWindow : Window, MyWindow
    {
        #region 服務
        private static ILog log = LogManager.GetLogger(typeof(RecognitionWindow));

        //private static IImageCut _ImageCut = ToolkitServiceFactory.getInstance().getImageCutInstance();
        //private static JointProcessService _JointProcessService = JointProcessService.getInstance();
        
        private ContentHandler _ContentHandler = ContentHandler.getInstance();

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private delegate void DummyDelegate();

        public KinectSensor Kinect;
        private KinectProcessor _KinectProcessor;

        //private BackgroundWorker background;
        public Queue<BackgroundWorker> backgrounds = new Queue<BackgroundWorker>();
        public BackgroundWorker ObjectRecognitionBackground = new BackgroundWorker();

        int LabelInfoShowTimes;

        int helpTriggerTime = 0;
        int helpTime = 0;
        int MenuButtonTime = 0;

        #endregion

        private string ObjectRecogResult = "";

        private bool MouseControlFlg = false;

        #region 任務
        private MainMenuWindow.TaskTypes TaskType { get; set; }
        private string ItemId { get; set; }
        private Window ParentWindow { get; set; }
        /*  //移到KinectProcessor裡
        private List<GlobalData.GestureTypes> TaskRecognitionActions = new List<GlobalData.GestureTypes>();
        private List<string> TaskRecognitionObjects = new List<string>();
        private List<string> TaskRecognitions = new List<string>();*/
        private VocabularyVO Vocabulary, NextVocabulary;
        private bool NoneRecognitionFlage = false;
        #endregion

        Player _Player;
        bool RecordingFlag = false;
        int RoundId;
        const int UsingCountDownConst = 180;
        int UsingCountDown = UsingCountDownConst;

        int ContentsIndex = 0;
        Dictionary<string, VocabularyVO> Content = new Dictionary<string, VocabularyVO>();

        public RecognitionWindow(KinectSensor sensor)
            : this()
        {
            Kinect = sensor;

            /// <param name="ui">呼叫的window</param>
            /// <param name="kinect"></param>
            /// <param name="kinectCanvas">skeletonDisplayManager使用畫骨架</param>
            /// <param name="kinectDisplay">顯示Kinect彩色影像</param>
            /// <param name="maskedColor">顯示去背影像</param>
            /// <param name="gesturesCanvas">顯示手勢節點的軌跡</param>
            /// <param name="labelTrackings">顯示被辨識出的玩家名稱（有兩個）</param>
            /// <param name="tbGestureDetected">用來塞值觸發UI去檢查是否已經辨識完成及後續動作</param>
            /// <param name="textBlockRecognitionResult">辨識結果（有兩個：手勢動作、物件）</param>
            /// <param name="stabilitiesList">偵測到的玩家姿勢穩定訊息</param>
            /// <param name="objectCanvas">物件辨識物件擺放位置框</param>
            _KinectProcessor = new KinectProcessor(this, this.Kinect, this.kinectCanvas, this.kinectDisplay, this.MaskedColor, this.gesturesCanvas,
                new Label[2] , this.objectCanvas);
        }

        public RecognitionWindow()
        {
            InitializeComponent();
            Loaded += RecognitionWindow_Loaded;
            Unloaded += RecognitionWindow_Unloaded;
            Closing += RecognitionWindow_Closing;

            _Player = GlobalValueData.Players[0];

            _timer.Elapsed += delegate
            {
                this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    (DummyDelegate)
                    delegate
                    {

                        UsingCountDown--;
                        if (RecordingFlag && UsingCountDown <= 0)
                        {
                            RecordingFlag = false;
                            _ContentHandler.updateMainRecord(_Player.userID, TaskType.ToString(), RoundId);
                        }

                        if (this.labelInfo.Visibility == System.Windows.Visibility.Visible)
                            LabelInfoShowTimes++;

                        if (LabelInfoShowTimes > 2)
                        {
                            LabelInfoShowTimes = 0;
                            this.labelInfo.Content = "";
                            this.labelInfo.Visibility = System.Windows.Visibility.Hidden;
                        }

                        if (helpTriggerTime > 0)
                            helpTriggerTime--;

                        if (MenuButtonTime > 0)
                            MenuButtonTime--;

                        if (_KinectProcessor.BRCS_SourceReadyFlag)
                        {
                            _KinectProcessor.StartCutImageTime++;
                        }

                    });
            };
            _timer.Start();

            log.Debug("RecognitionWindow() end");

        }

        void RecognitionWindow_Closing(object sender, CancelEventArgs e)
        {
            if (RecordingFlag)
            {
                RecordingFlag = false;
                _ContentHandler.updateMainRecord(_Player.userID, TaskType.ToString(), RoundId);
            }
            _KinectProcessor.close(this);

            Console.WriteLine("執行RecognitionWindow_Closing!!");
            _KinectProcessor.Clean();
            cleanKinect();
        }

        void RecognitionWindow_Loaded(object sender, RoutedEventArgs e)
        {

            ObjectRecognitionBackground.WorkerReportsProgress = true;
            ObjectRecognitionBackground.WorkerSupportsCancellation = true;
            ObjectRecognitionBackground.DoWork += new DoWorkEventHandler(ObjectRecognitionBackground_DoWork);
            ObjectRecognitionBackground.ProgressChanged += new ProgressChangedEventHandler(ObjectRecognitionBackground_ProgressChanged);
            ObjectRecognitionBackground.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ObjectRecognitionBackground_RunWorkerCompleted);

            InitializeKinect1();
            _KinectProcessor.InitializeGestureAndPostureDetectors();

            controlUIElement();            

        }

        private void ObjectRecognitionBackground_DoWork(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            _KinectProcessor.GestureCommand = GlobalData.GestureTypes.Init;

            object[] datas = (object[])e.Argument;
            string result = _KinectProcessor.startObjectRecognitionNew((Player)datas[0], (Dictionary<JointType, Bitmap>)datas[1]);
            //recordTracking(Skeleton skeleton);
            worker.ReportProgress(50, result);
           
            
        }

        private void ObjectRecognitionBackground_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if (e.UserState != null)
            {
                _KinectProcessor.StartObjectRecFlag = false;
                _KinectProcessor.BRCS_SourceReadyFlag = false;
                _KinectProcessor.StartObjectRecThreadFlag = false;
                _KinectProcessor.StartCutImageTime = 0;

                if (this.Vocabulary.ObjectRecognition == e.UserState.ToString())
                {
                    //this.labelInfo.Content = this.Vocabulary.Vocabulary;
                    this.labelInfo.Content = "Good!!";
                    this.labelInfo.Visibility = System.Windows.Visibility.Visible;
                    _ContentHandler.speech(this.Vocabulary.Vocabulary, ContentHandler.WORD_SPEECH_SPEED);
                    _KinectProcessor.drawObjectRecogntionArea();
                    this.checkFinishRecognition();
                    _ContentHandler.updateLearnRecord(_Player.userID, this.TaskType.ToString(), "action", this.Vocabulary.ID.ToString());
                }
                else
                {

                    this.labelInfo.Visibility = System.Windows.Visibility.Visible;
                    this.labelInfo.Content = "Try Again!!!";
                    MediaPlayer player = new MediaPlayer();
                    player.Open(new Uri(GlobalValueData.ErrorWavPath, UriKind.Relative));
                    player.Play();
                }
            }

            this.ObjectRecButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void ObjectRecognitionBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        void RecognitionWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _KinectProcessor.close(this);

            Console.WriteLine("執行ColorWindow_Unloaded!!");
            _KinectProcessor.Clean();
            cleanKinect();
        }

        public void stopBackgroundJob()
        {
            if (this.ObjectRecognitionBackground.IsBusy)
                this.ObjectRecognitionBackground.CancelAsync();
        }

        public void startBackgroundJob()
        {
            if (!this.ObjectRecognitionBackground.IsBusy)
                this.ObjectRecognitionBackground.RunWorkerAsync();
        }

        private void noneRecognitionView()
        {
            //The function is not published openly
        }

        

        private VocabularyVO getNextVocabularyItme(List<VocabularyVO> vocabularys)
        {
            bool locationFlg = false;
            foreach (var vocabulary in vocabularys)
            {
                if (locationFlg)
                    return vocabulary;

                if (vocabulary.ID == int.Parse(this.ItemId))
                {
                    locationFlg = true;
                }
            }
            return vocabularys[0];
        }

        private VocabularyVO getNextVocabularyItmeByRandom()
        {
            return _ContentHandler.retrieveVocabularyByRandom();
        }

        private VocabularyVO getNextVocabularyItmeByUnfamiliar()
        {
            if (ContentsIndex >= Content.Count)
            {
                ContentsIndex = 0;
            }
            Vocabulary = this.Content.ElementAt(ContentsIndex).Value;
            ContentsIndex++;
            
            return Vocabulary;
        }

        public new void Show()
        {

            base.Show();
            startAllFrames();

            this._timer.Start();

            if (this.TaskType == MainMenuWindow.TaskTypes.Vocabulary)
            {
                this.SOSButton.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (this.TaskType == MainMenuWindow.TaskTypes.Listening)
            {
                this.tbAllTimeInfo.Text = "請仔細聆聽，可按右下角的「i」出現三次提示！";
                this.SOSButton.Visibility = System.Windows.Visibility.Visible;
            }
            
            if (this.Vocabulary.Kind== VocabularyVO.Kinds.None)
                this.noneRecognitionView();

            if (!RecordingFlag)
            {
                RoundId = _ContentHandler.insertMainRecord(_Player.userID, TaskType.ToString());
                RecordingFlag = true;
            }
            UsingCountDown = UsingCountDownConst;            
        }

        public new void Hide()
        {
            this._timer.Stop();

            stopAllFrames();

            base.Hide();

            if (RecordingFlag)
            {
                RecordingFlag = false;
                _ContentHandler.updateMainRecord(_Player.userID, TaskType.ToString(), RoundId);
            }

        }


        private void controlUIElement()
        {
            this.labelInfo.Content = "";
            this.labelInfo.Visibility = System.Windows.Visibility.Hidden;

        }

        private void btnVocabularys_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ((MainMenuWindow)ParentWindow).showAndSetupWindow();

        }

        private void btnSpeak_Click(object sender, RoutedEventArgs e)
        {
            _ContentHandler.speech(this.Vocabulary.Vocabulary, ContentHandler.WORD_SPEECH_SPEED);
            
        }

        private void btnSpeakSentence_Click(object sender, RoutedEventArgs e)
        {
            _ContentHandler.speech(retrieveSentence()[0], ContentHandler.SENTENCE_SPEECH_SPEED);
            
        }


        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NextButton_Click(sender, e);
        }

        private void initalListeningUI()
        {
            
            textCommands.Visibility = Visibility.Hidden;
            tbMessages.Visibility = Visibility.Hidden;
        }

        private void tbGestureDetected_TextChanged(object sender, TextChangedEventArgs e)
        {
            log.Debug("tbGestureDetected_TextChanged::" + ((TextBox)sender).Text);      
        }



        public void gestureFinish(string gesture)
        {
            if (this.Vocabulary.ActionRecognition == gesture)
            {
                _ContentHandler.speech(this.Vocabulary.Vocabulary, ContentHandler.WORD_SPEECH_SPEED);
                this.labelInfo.Visibility = System.Windows.Visibility.Visible;
                this.labelInfo.Content = "Good!! Do Again!";
                this.checkFinishRecognition();

                _ContentHandler.updateLearnRecord(_Player.userID, this.TaskType.ToString(), "action", this.Vocabulary.ID.ToString());
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NextButton.Visibility == System.Windows.Visibility.Hidden)
            {
                this.NextButton.Visibility = System.Windows.Visibility.Visible;
                this.BackButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.NextButton.Visibility = System.Windows.Visibility.Hidden;
                this.BackButton.Visibility = System.Windows.Visibility.Hidden;
            }

            MenuButtonTime = 3;
        }

        private void SpeakSentenceButton_Click(object sender, RoutedEventArgs e)
        {
            _ContentHandler.speech(retrieveSentence()[0], ContentHandler.SENTENCE_SPEECH_SPEED);
            UsingCountDown = UsingCountDownConst;
            _ContentHandler.updateLearnRecord(_Player.userID, this.TaskType.ToString(), "listenSentence", this.Vocabulary.ID.ToString());
            
        }

        private void ObjectRecButton_Click(object sender, RoutedEventArgs e)
        {
            //觸發物件辨識
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - _KinectProcessor.ObjectRecStartTime.Ticks);

            if (ts.TotalMinutes > 0.05)  //間隔三秒才可在觸發
            {
                _KinectProcessor.ObjectRecStartTime = DateTime.Now;
                _KinectProcessor.StartObjectRecFlag = true;
                _KinectProcessor.cleanTaskRecognitionActions();
                this.ObjectRecButton.Visibility = System.Windows.Visibility.Hidden;
                this.labelInfo.Content = "Start Object Recognition";
                this.labelInfo.Visibility = System.Windows.Visibility.Visible;

            }
        }

        private void SOSButton_Click(object sender, RoutedEventArgs e)
        {

            if (helpTriggerTime <= 0)  //間隔五秒才可在觸發
            {
                helpTriggerTime = 3;

                _ContentHandler.updateVocabularyStatistics(_Player.userID, "ls", "p", this.Vocabulary.ID + "", 1);

                switch (helpTime)
                {
                    case 0:
                        List<string[]> data = new List<string[]>();
                        for (int i = 0; i < 2; i++)
                        {
                            data.Add(new string[] { this.Vocabulary.Vocabulary, ContentHandler.WORD_SPEECH_SPEED.ToString() });
                        }

                        _ContentHandler.speechIntoQueue(data);
                        break;
                    case 1:
                        this.textCommands.Text = this.Vocabulary.Vocabulary;
                        this.textCommands.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case 2:
                        showVocabularyImage();
                        break;
                    default:
                        if (this.Vocabulary.Kind == VocabularyVO.Kinds.Action)
                        {
                            this.labelInfo.Content = "求救已經用完，請跟著圖片做！";
                        }
                        else
                        {
                            this.labelInfo.Content = "求救已經用完，請找出最相關的圖片！";
                        }
                        this.labelInfo.Visibility = System.Windows.Visibility.Visible;
                        break;
                }


                helpTime++;
            }
            else
            {
                this.labelInfo.Content = "想一下，"+helpTriggerTime+"秒後再求救！";
                this.labelInfo.Visibility = System.Windows.Visibility.Visible;
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ((MainMenuWindow)ParentWindow).showAndSetupWindow();

            this.NextButton.Visibility = System.Windows.Visibility.Hidden;
            this.BackButton.Visibility = System.Windows.Visibility.Hidden;

        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.TaskType == MainMenuWindow.TaskTypes.Vocabulary)
            {
                retrieveNextVocabulary();
            }
            else if (this.TaskType == MainMenuWindow.TaskTypes.Listening)
            {
                retrieveNextVocabulary4Listening();
            }

            this.NextButton.Visibility = System.Windows.Visibility.Hidden;
            this.BackButton.Visibility = System.Windows.Visibility.Hidden;

            if (!RecordingFlag)
            {
                RoundId = _ContentHandler.insertMainRecord(_Player.userID, TaskType.ToString());
                RecordingFlag = true;
            }

            UsingCountDown = UsingCountDownConst;
        }
    }
}
