using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Ryan.Kinect.Toolkit;
using Ryan.Kinect.Toolkit.ContentProcess;
using Ryan.Common;
using Ryan.Kinect.Toolkit.VO;

namespace Presentation
{
    /// <summary>
    /// MainMenuWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        private static ILog log = LogManager.GetLogger(typeof(MainMenuWindow));

        List< KinectSensor> kinectSensors = new List <KinectSensor>();
        Skeleton[] skeletons;

        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        //private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        List<UIElement> MainMenuList = new List<UIElement>(), VocabularyKindMenuList = new List<UIElement>(), DynamicMenuList = new List<UIElement>(), VocabularysMenuControlList = new List<UIElement>(), ListeningMenuList = new List<UIElement>();
        Grid NowMenu;
        List<UIElement> NowMenuList;
        Stack<List<UIElement>> BackMenuLists = new Stack<List<UIElement>>();
        Stack<Grid> BackMenuGrids = new Stack<Grid>();

        Window SubWindow;
        RecognitionWindow _RecognitionWindow;
        ListeningGameWindow _ListeningGameWindow;

        //SentenceGameWindow _SentenceGameWindow;  //The function is not published openly

        //SentenceWindow _SentenceWindow;  //The function is not published openly
        int NowPageNumber;

        VocabularyVO.Kinds VocabularyKind;

        public enum TaskTypes { Vocabulary, Listening, Cloze, Conversation, ConversationMaking, ListeningGame, PeerListeningGame, PeerListening, SentenceGame, PeerSentenceGame }
        TaskTypes TaskType;

        //double WindowLeft, WindowTop, WindowLeft2, WindowTop2;  //The function is not published openly

        public MainMenuWindow()
        {
            this.Title = "MainMenuWindow";
            InitializeComponent();
            Unloaded += MainMenuWindow_Unloaded;
        }

        void MainMenuWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var sensor in kinectSensors)
            {
                sensor.SkeletonFrameReady -= kinectSensor_SkeletonFrameReady;
                sensor.Stop(); 
            }
        }

        public MainMenuWindow(KinectSensor sensor)
            : this()
        {
            kinectSensors.Add(sensor);
        }

        private void MainMenuWindow_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {        
                initalMenuLists();
                //Initialize();
                InitializeKinect(kinectSensors[0]);
                this.Title += " 感應器ID:" + kinectSensors[0].UniqueKinectId + ", 連線ID:" + kinectSensors[0].DeviceConnectionId + ", 狀態:" + kinectSensors[0].Status;
                InitializeMouseController();
                showUserName();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                MessageBox.Show(ex.Message);
            }
        }

        void showUserName()
        {
            string user = "";
            foreach (var play in GlobalValueData.Players)
            {
                user += play.playerName + "  ";
            }
            this.tbUserName.Text = user ;
        }


        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    
                        kinectSensors.Add( e.Sensor);
                        //Initialize();
                        InitializeKinect(e.Sensor);
                        InitializeMouseController();
                    
                    break;
                case KinectStatus.Disconnected:  //TODO 還沒改
                    if (kinectSensors[0] == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect was disconnected");
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (kinectSensors[0] == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect is no more powered");
                    }
                    break;
                default:
                    MessageBox.Show("Unhandled Status: " + e.Status);
                    break;
            }
        }


        private void InitializeKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                try
                {
                    log.Info("參數設定運轉的Kinect Type(1:w,2:x)=" + GlobalValueData.KinectType);

                    ColorImageStream colorStream = sensor.ColorStream;
                    colorStream.Enable(ColorFormat);

                    //DepthImageStream depthStream = sensor.DepthStream;
                    //depthStream.Enable(DepthFormat);  // 調低，增進效能，並且不太影響精準度。這樣調整會造成物件辨識擷取圖片的錯誤，料想應該是深度影像比例和彩色影像解析度不同造成
                    
                    var parameters = new TransformSmoothParameters
                    {
                        Smoothing = 0.5f,
                        Correction = 0.5f,
                        Prediction = 0.5f,
                        JitterRadius = 0.05f,
                        MaxDeviationRadius = 0.04f

                    };

                    sensor.SkeletonFrameReady += kinectSensor_SkeletonFrameReady;
                    //sensor.DepthFrameReady += kinectSensor_DepthFrameReady;

                    presenceControl.SetKinectSensor(sensor);  //這裡面會設定深度資料解析度

                    sensor.SkeletonStream.Enable(parameters);//重要：要有啟動SkeletonStream，偵測玩家編號的功能才能達到                  

                    Console.WriteLine(sensor.DepthStream.Format.ToString());  //初始值：Resolution320x240Fps30
                    sensor.Start();

                    sensor.ElevationAngle = 7;
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    log.Fatal(ex);
                    GlobalValueData.Messages.Add(ex.Message);
                    throw ex;
                }
            }
        }

        private void DiscoverKinectSensor()
        {

            foreach (var sensor in KinectSensor.KinectSensors)
            {
                KinectSensor recSensor = this.kinectSensors.Find(delegate(KinectSensor s) { return s.UniqueKinectId == sensor.UniqueKinectId; });

                if (recSensor != null)
                    continue;

                kinectSensors.Add(sensor);
                InitializeKinect(sensor);
            }
        }

        /// <summary>
        /// for test:Kinect 深度串流運作是否正常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            //Console.WriteLine("aaaaa");
        }

        private void InitializeMouseController()
        {
            //if (1 == 1) return;
            MouseController.Current.DisableGestureClick = true;
            MouseController.Current.ImpostorCanvas = mouseCanvas;

            MouseController.Current.DataSmoothingFactor = 0.6f;
            MouseController.Current.PredictionFactor = 0.1f;
        }

        private void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                frame.GetSkeletons(ref skeletons);

                if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;

                foreach (var skeleton in skeletons)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                        continue;

                    foreach (Joint joint in skeleton.Joints)
                    {
                        if (joint.TrackingState != JointTrackingState.Tracked)
                            continue;

                        if (joint.JointType == JointType.HandRight)
                        {
                            MouseController.Current.SetHandPosition(kinectSensors[0], joint, skeleton);
                        }
                    }
                }
            }
        }

        private void Clean()
        {
            if (kinectSensors != null)
            {
                presenceControl.Clean();
                foreach (var sensor in kinectSensors)
                {
                    sensor.Dispose();
                }
                kinectSensors.Clear();
            }
        }

        private void clickBtnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void initalMenuLists()
        {
            NowMenu = gridMainMenu;
            NowMenuList = MainMenuList;

            MainMenuList.Add(btnVocabularyLearning);  //btnVocabularyLearning
            MainMenuList.Add(btnListeningLearning);  //btnListeningLearning

            VocabularyKindMenuList.Add(btnActions);
            VocabularyKindMenuList.Add(btnObjects);
            //VocabularyKindMenuList.Add(btnNouns);

            ListeningMenuList.Add(btnListeningPractice);
            ListeningMenuList.Add(btnListeningGame);

            DynamicMenuList.Add(btnItem1);
            DynamicMenuList.Add(btnItem2);
            DynamicMenuList.Add(btnItem3);
            DynamicMenuList.Add(btnItem4);
            DynamicMenuList.Add(btnItem5);
            DynamicMenuList.Add(btnItem6);
            //DynamicMenuList.Add(btnItem7);
            //DynamicMenuList.Add(btnItem8);
            VocabularysMenuControlList.Add(btnPrev);
            VocabularysMenuControlList.Add(btnNext);
        }

        private void clickVocabularyLearning(object sender, RoutedEventArgs e)
        {
            this.TaskType = TaskTypes.Vocabulary;
            changeMenu(gridVocabularyKindMenu, VocabularyKindMenuList);
        }


        private void disableMenu(Grid gridMenu, List<UIElement> uiElements)
        {

            gridMenu.Visibility = Visibility.Hidden;
            foreach (var ui in uiElements)
            {
                ui.Visibility = Visibility.Hidden;
                ui.IsEnabled = false;
                MagneticPropertyHolder.SetIsMagnetic(ui, false);
            }
        }

        private void enableMenu(Grid gridMenu, List<UIElement> uiElements)
        {
            gridMenu.Visibility = Visibility.Visible;
            foreach (var ui in uiElements)
            {
                ui.Visibility = Visibility.Visible;
                ui.IsEnabled = true;
                MagneticPropertyHolder.SetIsMagnetic(ui, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationG">將要顯示的Grid</param>
        /// <param name="destinationEs">將要顯示的選單</param>
        private void changeMenu(Grid destinationG, List<UIElement> destinationEs)
        {
            disableMenu(NowMenu,NowMenuList);
            //changeVisibleElementsMagnetic(NowMenu, false);

            BackMenuGrids.Push(NowMenu);
            BackMenuLists.Push(NowMenuList);

            NowMenu = destinationG;
            NowMenuList = destinationEs;

            enableMenu(destinationG, destinationEs);

        }

        private void enableMainMenu()
        {
            BackMenuLists.Push(MainMenuList);
            BackMenuGrids.Push(gridMainMenu);
            NowMenu = gridMainMenu;
            NowMenuList = MainMenuList;
            enableMenu(gridMainMenu, MainMenuList);
        }

        private void clickBtnHome(object sender, RoutedEventArgs e)
        {
            changeMenu(gridMainMenu, MainMenuList);

            BackMenuLists.Clear();
            BackMenuGrids.Clear();
        }

        private void clickBtnBack(object sender, RoutedEventArgs e)
        {
            if (BackMenuGrids.Count < 1)
            {
                clickBtnHome(sender, e);
            }
            else
            {
                changeMenu(BackMenuGrids.Pop(), BackMenuLists.Pop());
            }
        }

        private void btnActions_Click(object sender, RoutedEventArgs e)
        {
            changeMenu(gridVocabularysMenu, DynamicMenuList);
            modifyNowMenuList(DynamicMenuList, VocabularysMenuControlList);
            VocabularyKind =  VocabularyVO.Kinds.Action;
            changeVocabularysMenuPage(1);

        }

        private void btnObjects_Click(object sender, RoutedEventArgs e)
        {
            changeMenu(gridVocabularysMenu, DynamicMenuList);
            modifyNowMenuList(DynamicMenuList, VocabularysMenuControlList);
            VocabularyKind =  VocabularyVO.Kinds.Object;
            changeVocabularysMenuPage(1);
        }

        private void btnNouns_Click(object sender, RoutedEventArgs e)
        {
            changeMenu(gridVocabularysMenu, DynamicMenuList);
            modifyNowMenuList(DynamicMenuList, VocabularysMenuControlList);
            VocabularyKind =  VocabularyVO.Kinds.None;
            changeVocabularysMenuPage(1);
        }

        private void modifyNowMenuList(List<UIElement> listA, List<UIElement> listB)
        {
            List<UIElement> newNowMenu = new List<UIElement>(listA);
            foreach (var uie in listB)
            {
                newNowMenu.Add(uie);
            }

            this.NowMenuList = newNowMenu;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber">頁碼從1開始</param>
        private void changeVocabularysMenuPage(int pageNumber)
        {
            int itemAmount = 6;

                List<VocabularyVO> vocabularys = null;
                if (VocabularyKind == VocabularyVO.Kinds.Action)
                {
                    vocabularys = ContentHandler.getInstance().retrieveActionVocabularys();
                }
                else if (VocabularyKind == VocabularyVO.Kinds.Object)
                {
                    vocabularys = ContentHandler.getInstance().retrieveObjectVocabularys();
                }
                else
                {
                    vocabularys = ContentHandler.getInstance().retrieveOtherVocabularys();
                }

                int start = ((pageNumber - 1) * itemAmount); // 0 , 0
                int end = (vocabularys.Count <= (start + itemAmount)) ? vocabularys.Count : (start + itemAmount);  //0+itemAmount=itemAmount

                for (int i = start; i < end; i++)
                {
                    int j = i % itemAmount;
                    DynamicMenuList[j].Visibility = Visibility.Visible;
                    DynamicMenuList[j].IsEnabled = true;
                    MagneticPropertyHolder.SetIsMagnetic(DynamicMenuList[j], true);
                    ((Button)DynamicMenuList[j]).Content = vocabularys.ElementAt(i).Vocabulary;
                    ((Button)DynamicMenuList[j]).Uid = vocabularys.ElementAt(i).ID.ToString();
                }

                if (end == vocabularys.Count())
                {
                    if ((end % itemAmount) != 0)
                    {
                        for (int i = (end % itemAmount); i < DynamicMenuList.Count(); i++)
                        {
                            DynamicMenuList[i].Visibility = Visibility.Hidden;
                            DynamicMenuList[i].IsEnabled = false;
                            MagneticPropertyHolder.SetIsMagnetic(DynamicMenuList[i], false);
                            ((Button)DynamicMenuList[i]).Content = "";
                            ((Button)DynamicMenuList[i]).Uid = "";
                        }
                    }
                }

                if (pageNumber == 1)
                {
                    btnPrev.Visibility = Visibility.Hidden;
                    btnPrev.IsEnabled = false;
                    MagneticPropertyHolder.SetIsMagnetic(btnPrev, false);
                }
                else
                {
                    btnPrev.Visibility = Visibility.Visible;
                    btnPrev.IsEnabled = true;
                    btnPrev.Uid = "" + (pageNumber - 1);
                    btnPrev.Content = "Prev(" + btnPrev.Uid + ")";
                    MagneticPropertyHolder.SetIsMagnetic(btnPrev, true);
                }

                if (vocabularys.Count > end)
                {
                    btnNext.Visibility = Visibility.Visible;
                    btnNext.IsEnabled = true;
                    btnNext.Uid = "" + (pageNumber + 1);
                    btnNext.Content = "Next(" + btnNext.Uid + ")";
                    MagneticPropertyHolder.SetIsMagnetic(btnNext, true);
                }
                else
                {
                    btnNext.Visibility = Visibility.Hidden;
                    btnNext.IsEnabled = false;
                    MagneticPropertyHolder.SetIsMagnetic(btnPrev, true);
                    MagneticPropertyHolder.SetIsMagnetic(btnNext, false);
                }
            
            
            
        }


        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            changeVocabularysMenuPage(int.Parse(btnPrev.Uid));
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (this.TaskType == TaskTypes.Vocabulary)
            {
                changeVocabularysMenuPage(int.Parse(btnNext.Uid) );
            }
            else if (this.TaskType == TaskTypes.Conversation)
            {
                throw new SoftwareException("The function is not published openly");
                //changeConversationsMenuPage(int.Parse(btnNext.Uid));
            }
        }

        private void btnItem_Click(object sender, RoutedEventArgs e)
        {
            Button theButton = (Button)sender;
            switch(this.TaskType)
            {
                case TaskTypes.Vocabulary:
                    hideAndSetupRecognitionWindow(this.TaskType, theButton.Uid);
                    break;
                default:
                    break;
            }
        }


        public void showAndSetupWindow()
        {

            this.Left = this.SubWindow.Left;
            this.Top = this.SubWindow.Top;

            kinectSensors[0].SkeletonFrameReady += kinectSensor_SkeletonFrameReady;

            presenceControl.SetKinectSensor(kinectSensors[0]);

            MouseController.Current.DisableGestureClick = true;
            MouseController.Current.ImpostorCanvas = mouseCanvas;

            MouseController.Current.DataSmoothingFactor = 0.6f;
            MouseController.Current.PredictionFactor = 0.1f;

            changeVisibleElementsMagnetic(true);

            this.Show();
        }

        private void hideAndSetup()
        {
            log.Debug("有kinectSensors[0].SkeletonFrameReady -= kinectSensor_SkeletonFrameReady;");

            foreach (KinectSensor sensor in kinectSensors)
            {
                sensor.SkeletonFrameReady -= kinectSensor_SkeletonFrameReady;
            }

            changeVisibleElementsMagnetic(false);
            MouseController.Current.ImpostorCanvas = null;
        }

        private void hideAndSetupRecognitionWindow(TaskTypes taskType, string itemId)
        {
            hideAndSetup();
   
            if (_RecognitionWindow == null)
            {
                RecognitionWindow rw = new RecognitionWindow(kinectSensors[0]);
                _RecognitionWindow = rw;
            }

            _RecognitionWindow.setVocabularyContent(taskType, itemId, this);
            _RecognitionWindow.Left = this.Left;
            _RecognitionWindow.Top = this.Top;
            _RecognitionWindow.Show();

            this.SubWindow = _RecognitionWindow;

            this.Hide();
            presenceControl.Clean();
        }

        private void hideAndSetupPeerRecognitionWindow(TaskTypes taskType, string itemId)
        {
            throw new SoftwareException("The function is not published openly");
        }

        private void hideAndSetupListeningGameWindow(TaskTypes taskType)
        {
            hideAndSetup();

            if (_ListeningGameWindow == null)
            {
                ListeningGameWindow window = new ListeningGameWindow(kinectSensors[0]);
                _ListeningGameWindow = window;
            }

            _ListeningGameWindow.setParentWindow( TaskTypes.ListeningGame, this);
            _ListeningGameWindow.Left = this.Left;
            _ListeningGameWindow.Top = this.Top;
            _ListeningGameWindow.Show();

            this.SubWindow = _ListeningGameWindow;

            this.Hide();
            presenceControl.Clean();
        }

        private void hideAndSetupSentenceGameWindow(TaskTypes taskType)
        {
            throw new SoftwareException("The function is not published openly");
        }

        private void  hideAndSetupSentenceWindow(TaskTypes taskType)
        {
            throw new SoftwareException("The function is not published openly");
        }

        private void changeVisibleElementsMagnetic(Panel panel,bool magneticFlg)
        {
            
                UIElementCollection childenElements = panel.Children;
                foreach (UIElement ele in childenElements)
                {
                    if (ele.Visibility == Visibility.Visible)
                    {
                        if (ele.GetType().Name == "Button")
                        {
                            MagneticPropertyHolder.SetIsMagnetic(ele, magneticFlg);
                        }
                        else if (ele.GetType().Name == "Canvas" || ele.GetType().Name == "Grid")
                        {
                            //UIElementCollection childenElements1 =  ((Canvas)ele).Children;
                            changeVisibleElementsMagnetic((Panel)ele, magneticFlg);

                        }

                    }
                }
        }

        private void changeVisibleElementsMagnetic(bool magneticFlg)
        {
            IEnumerator enumerator = this.LogicalChildren;        

            while (enumerator.MoveNext())
            {
                Panel e = (Panel)enumerator.Current;
                
                changeVisibleElementsMagnetic(e,magneticFlg);

            } 
            
        }

        private void clickListeningLearning(object sender, RoutedEventArgs e)
        {
            this.TaskType = TaskTypes.Listening;
            changeMenu(gridListeningMenu, ListeningMenuList);
        }

        private void btnListeningGame_Click(object sender, RoutedEventArgs e)
        {
            if (ContentHandler.getInstance().retrievePlayGameFlag())
            {
                this.tbInfo.Text = "";
                this.tbInfo.Visibility = System.Windows.Visibility.Hidden;

                hideAndSetupListeningGameWindow(TaskTypes.ListeningGame);
                //hideAndSetupCompletedSentenceWindow(TaskTypes.Cloze);
            }
            else
            {
                this.tbInfo.Text = "現在不是遊戲時間";
                this.tbInfo.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void btnListeningPractice_Click(object sender, RoutedEventArgs e)
        {
            this.tbInfo.Visibility = System.Windows.Visibility.Hidden;
            hideAndSetupRecognitionWindow(TaskTypes.Listening, null);
        }

        private void btnPeerListeningPractice_Click(object sender, RoutedEventArgs e)
        {
            throw new SoftwareException("The function is not published openly");
        }

        private void btnSentenceGame_Click(object sender, RoutedEventArgs e)
        {
            throw new SoftwareException("The function is not published openly");
        }

        private void btnSentenceList_Click(object sender, RoutedEventArgs e)
        {
            hideAndSetupSentenceWindow(TaskTypes.Conversation);
        }
    }
}
