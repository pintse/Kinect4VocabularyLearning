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
using Kinect.Toolbox;
using log4net;
using Microsoft.Kinect;
using Ryan.Content.VO;
using Ryan.Kinect.GestureCommand.VO;
using Ryan.Kinect.Toolkit;
using Ryan.Kinect.Toolkit.ContentProcess;
using Ryan.Kinect.Toolkit.KinectProcess;
using Ryan.Kinect.Toolkit.VO;

namespace Presentation
{
    /// <summary>
    /// 字彙聽力遊戲。處理window畫面呈現基本控制與背景執行序控制
    /// </summary>
    public partial class ListeningGameWindow : Window, MyWindow
    {

        #region 服務
        private static ILog log = LogManager.GetLogger(typeof(ListeningGameWindow));

        private ContentHandler _ContentHandler = ContentHandler.getInstance();

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);

        private delegate void DummyDelegate();

        public KinectSensor Kinect;
        private KinectProcessor _KinectProcessor;

        public BackgroundWorker ObjectRecognitionBackground = new BackgroundWorker();

        int LabelInfoShowTimes;

        #endregion

        #region 任務
        private MainMenuWindow.TaskTypes TaskType { get; set; }
        private string ItemId { get; set; }
        private Window ParentWindow { get; set; }
        private VocabularyVO Vocabulary;

        bool prepareFlag = false;
        const int TotalTimeConst = 300; 
        int TotalTime = 0;
        const int WaitAnswerTimeConst = 10;
        const int WaitObjectAnswerTimeConst = 20;
        int WaitAnswerTime = WaitAnswerTimeConst;
        
        int score = 0;
        int totalNum = 0;
        int correctNum = 0;
        int errorNum = 0;
        int PrepareCountdownNum = -1;
        bool StartGameFlag = false;
        bool ChangeQuestionFlag = false;
        int ShowResultTimes = 1;

        int RoundId;

        Dictionary<string, VocabularyVO> Contents;

        int ContentsIndex;
        #endregion

        Player _Player;


        public ListeningGameWindow(KinectSensor sensor)
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
        public ListeningGameWindow()
        {
            InitializeComponent();
            Loaded += ListeningGameWindow_Loaded;
            Unloaded += ListeningGameWindow_Unloaded;

            _Player = GlobalValueData.Players[0]; 

            _timer.Elapsed += delegate
            {
                this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    (DummyDelegate)
                    delegate
                    {
                        if (PrepareCountdownNum >= 0)
                        {
                            this.imgGameOver.Visibility = System.Windows.Visibility.Hidden;
                            this.labCountdownNum.Visibility = System.Windows.Visibility.Visible;

                                this.labCountdownNum.Content = PrepareCountdownNum;

                                if (PrepareCountdownNum == 0)
                                {
                                    RoundId = _ContentHandler.insertMainRecord(_Player.userID, TaskType.ToString());
                                    TotalTime = TotalTimeConst;
                                    ChangeQuestionFlag = true;
                                }

                            PrepareCountdownNum--;

                        }
                        else
                        {
                            this.labCountdownNum.Visibility = System.Windows.Visibility.Hidden;
                            

                            if (TotalTime > 0)
                            {
                                score = correctNum;
                                this.tbTotalTime.Text = TotalTime / 60 + ":" + TotalTime % 60;
                                this.tbTime.Text = WaitAnswerTime.ToString();
                                this.tbSCORE.Text = score.ToString();
                                this.tbTotal.Text = totalNum.ToString();
                                this.tbCorrect.Text = correctNum.ToString();
                                this.tbWrong.Text = errorNum.ToString();

                                StartGameFlag = true;

                                if (ChangeQuestionFlag)
                                {
                                    ChangeQuestionFlag = false;
                                    setNextQuestion();
                                }

                                if (WaitAnswerTime == 1)
                                {
                                    errorNum++;
                                    totalNum++;

                                    showResult(false, this.Vocabulary.ID);

                                    ChangeQuestionFlag = true;
                                }
                                if (!ChangeQuestionFlag)
                                {
                                    TotalTime--;
                                    WaitAnswerTime--;
                                }
                            }
                            else
                            {
                                if (StartGameFlag)  //遊戲結束
                                {
                                    this.imgGameOver.Visibility = System.Windows.Visibility.Visible;
                                    this.NextButton.Visibility = System.Windows.Visibility.Hidden;
                                    this.BackButton.Visibility = System.Windows.Visibility.Visible;
                                    _ContentHandler.updateMainRecord(_Player.userID, TaskType.ToString(), RoundId, int.Parse(this.tbSCORE.Text), int.Parse(this.tbWrong.Text), int.Parse(this.tbCorrect.Text));
                                    showQResultList();
                                    log.Info("userID:" + _Player.userID + ", total:" + this.tbTotal.Text + ", correct:" + this.tbCorrect.Text + ", wrong:" + this.tbWrong.Text);
                                    StartGameFlag = false;

                                    this.ObjectRecButton.Visibility = System.Windows.Visibility.Hidden;
                                    this._KinectProcessor.cleanObjectRecogntionArea();
                                    this._KinectProcessor.TaskRecognitionActions.Clear();
                                    this._KinectProcessor.TaskRecognitionObjects.Clear();
                                    this._KinectProcessor.TaskRecognitions.Clear();
                                }
                            }

                            
                        }
                        

                        if (this.labelInfo.Visibility == System.Windows.Visibility.Visible)
                            LabelInfoShowTimes++;

                        if (LabelInfoShowTimes > 2)
                        {
                            LabelInfoShowTimes = 0;
                            this.labelInfo.Content = "";
                            this.labelInfo.Visibility = System.Windows.Visibility.Hidden;
                        }

                        if (this.textCommands.Visibility == System.Windows.Visibility.Visible)
                        {
                            if (this.ShowResultTimes > 0)
                            {
                                ShowResultTimes--;
                            }
                            else
                            {
                                this.textCommands.Visibility = System.Windows.Visibility.Hidden;
                                ShowResultTimes = 1;
                            }

                        }

                        if (_KinectProcessor.BRCS_SourceReadyFlag)
                        {
                            _KinectProcessor.StartCutImageTime++;
                        }

                    });
            };
            _timer.Start();

        }

        public void setParentWindow(MainMenuWindow.TaskTypes taskType, Window parentWindow)
        {
            this.TaskType = taskType;
            this.ParentWindow = parentWindow;
        }

        void ListeningGameWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _KinectProcessor.close(this);

            Console.WriteLine("執行ColorWindow_Unloaded!!");
            _KinectProcessor.Clean();
            cleanKinect();
        }

        void ListeningGameWindow_Loaded(object sender, RoutedEventArgs e)
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

        public new void Show()
        {
            base.Show();
            startAllFrames();

            prepareFlag = true;
           
            this._timer.Start();

            this.lbQResultList.Visibility = System.Windows.Visibility.Hidden;
        }

        public new void Hide()
        {
            this._timer.Stop();

            stopAllFrames();

            base.Hide();
        }

        public void resetScoreStatistic()
        {
            score = 0;
            totalNum = 0;
            correctNum = 0;
            errorNum = 0;
            this.lbQResultList.Items.Clear();

            this.tbSCORE.Text = score.ToString();
            this.tbTotal.Text = totalNum.ToString();
            this.tbCorrect.Text = correctNum.ToString();
            this.tbWrong.Text = errorNum.ToString();

        }

        private void controlUIElement()
        {
            this.labelInfo.Content = "";
            this.labelInfo.Visibility = System.Windows.Visibility.Hidden;

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

        private void ObjectRecognitionBackground_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            _KinectProcessor.GestureCommand = GlobalData.GestureTypes.Init;

            object[] datas = (object[])e.Argument;
            string result = _KinectProcessor.startObjectRecognitionNew((Player)datas[0], (Dictionary<JointType, Bitmap>)datas[1]);

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
                    showResult(true, this.Vocabulary.ID);

                    this.checkFinishRecognition();
                }
                else
                {
                    this.labelInfo.Content = "再試一次!!!";
                    this.labelInfo.Visibility = System.Windows.Visibility.Visible;

                }
            }
            
            if (_KinectProcessor.TaskRecognitionObjects.Count > 0)
            {
                this.ObjectRecButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ObjectRecognitionBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        public void gestureFinish(string gesture)
        {
            if (this.Vocabulary.ActionRecognition == gesture)
            {
                showResult(true, this.Vocabulary.ID);
                this.checkFinishRecognition();
            }
        }

        void showResult(bool result, int vocabularyId)
        {
            string vocabulary = this.Contents[vocabularyId.ToString()].Vocabulary;

            string cw = "";


            if (result)
            {
                this.labelInfo.Content = "答對了" + vocabulary;
                _ContentHandler.updateVocabularyStatistics(_Player.userID, "ls", "c", vocabularyId+"", 1);
                log.Info("userID:" + _Player.userID + ", correct:" + vocabularyId + "-" + vocabulary);
                cw = "c";
            }
            else
            {
                this.labelInfo.Content = "時間到:" + vocabulary;
                _ContentHandler.updateVocabularyStatistics(_Player.userID, "ls", "w", vocabularyId + "", 1);
                log.Info("userID:" + _Player.userID + ", wrong:" + vocabularyId + "-" + vocabulary);

                cw = "w";
            }

            this.labelInfo.Visibility = System.Windows.Visibility.Visible;

            int spendTime = 0;

            if (Vocabulary.ActionRecognition != null && Vocabulary.ActionRecognition != "")
            {
                spendTime = WaitAnswerTimeConst - WaitAnswerTime;
            }
            else
            {
                spendTime = WaitObjectAnswerTimeConst - WaitAnswerTime;
            }
            
            _ContentHandler.insertPlayRecordDetail(_Player.userID, this.TaskType.ToString(), this.RoundId, this.Vocabulary.ID.ToString(), cw, spendTime);
        }

        private void ObjectRecButton_Click(object sender, RoutedEventArgs e)
        {
            //觸發物件辨識
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - _KinectProcessor.ObjectRecStartTime.Ticks);

            
            _KinectProcessor.ObjectRecStartTime = DateTime.Now;
            _KinectProcessor.StartObjectRecFlag = true;
            _KinectProcessor.cleanTaskRecognitionActions();
            this.ObjectRecButton.Visibility = System.Windows.Visibility.Hidden;
            this.labelInfo.Content = "Start Object Recognition";
            this.labelInfo.Visibility = System.Windows.Visibility.Visible;
            
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
            prepareFlag = true;
            this.NextButton.Visibility = System.Windows.Visibility.Hidden;
            this.BackButton.Visibility = System.Windows.Visibility.Hidden;
            this.imgGameOver.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
