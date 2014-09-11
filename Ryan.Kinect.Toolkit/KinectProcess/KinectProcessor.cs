using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Kinect.Toolbox;
using Kinect.Toolbox.Record;
using log4net;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using Ryan.Common.DAO;
using Ryan.Common.Service;
using Ryan.Kinect.GestureCommand.Service;
using Ryan.Kinect.GestureCommand.VO;
using Ryan.Kinect.Toolkit.GestureCommands;
using Ryan.Kinect.Toolkit.ImageProcess;
using Ryan.Kinect.Toolkit.DAO;
using Ryan.Kinect.Toolkit.VO;


namespace Ryan.Kinect.Toolkit.KinectProcess
{
    /// <summary>
    /// Presentation Kinect 串流相關處理
    /// </summary>
    public partial class KinectProcessor
    {
        private static ILog log = LogManager.GetLogger(typeof(KinectProcessor));

        private static ImageCutAlpha _ImageCut = ImageCutAlpha.getInstance();

        public const int OraX = 100, OraY = 150, OraW = 200, OraH = 180;
        //設定是否使用MouseController2
        public bool MouseController2Flag = false;


        #region UI
        private Window UI;
        private KinectSensor Kinect;
        private System.Windows.Controls.Image kinectDisplay, MaskedColor;
        private Canvas kinectCanvas, gesturesCanvas, objectCanvas;
        private Label[] LabelTrackings = new Label[2];
        public bool MouseControlFlg = false;
        #endregion

        #region Kinect 影像串流相關

        readonly ColorStreamManager colorManager = new ColorStreamManager();
        readonly DepthStreamManager depthManager = new DepthStreamManager();
        public SkeletonDisplayManager skeletonDisplayManager;

        /// <summary>
        /// Our core library which does background 
        /// </summary>
        public BackgroundRemovedColorStream backgroundRemovedColorStream;

        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        //private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
        public const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        public const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        public WriteableBitmap foregroundBitmap;

        public ColorImageFrame cframe;
        public DepthImageFrame dframe;

        public int PixelDataLength;

        public short[] DepthPixels;

        public int skeletonFrameCount = 0;

        public KinectRecorder recorder;
        public KinectReplay replay;

        #endregion

        public readonly ContextTracker contextTracker = new ContextTracker();
        public EyeTracker eyeTracker;

        //去背串流的Source已經設定好
        public bool BRCS_SourceReadyFlag = false;

        public Queue<Skeleton> RecognitionSkeleton = new Queue<Skeleton>();

        #region 玩家資訊
        /// <summary>
        /// Intermediate storage for the skeleton data received from the sensor
        /// </summary>
        public Skeleton[] skeletons;

        /// <summary>
        /// 記錄這次運作過程中[已偵測判斷完成]的Player<TrackingId, Player>
        /// </summary>
        private Dictionary<int, Player> recordedCurrentPlayerDic = new Dictionary<int, Player>();

        /// <summary>
        /// 註記目前被Kenect給予TrackingId的個體
        /// TODO考慮改別種資料結構
        /// </summary>
        private Player[] TrackingIds = new Player[] { new Player(), new Player(), new Player(), new Player(), new Player(), new Player(), };
        /// <summary>
        /// 已被追蹤的玩家
        /// </summary>
        public Dictionary<int, Player> TrackedPlayers = new Dictionary<int, Player>();


        /// <summary>
        /// 正在進行收集比對母體資料的暫存
        /// </summary>
        private Dictionary<string, double> recordingPlayerSkeletonDic = new Dictionary<string, double>();


        /// <summary>
        /// 前一個frame顯示的Player的Tracked Id，當一個frame的期間只處理一個Player時使用。例如：去背處理
        /// the skeleton that is currently tracked by the app
        /// </summary>
        public int currentlyTrackedSkeletonId;
        private List<int> NearPlayerIds = new List<int>();


        #endregion


        #region 手勢
        public Dictionary<GlobalData.GestureTypes, GestureDetector> GestureDetectorList = new Dictionary<GlobalData.GestureTypes, GestureDetector>();
        public Dictionary<GlobalData.GestureTypes, PostureDetector> PostureDetectorList = new Dictionary<GlobalData.GestureTypes, PostureDetector>();
        public Dictionary<GlobalData.GestureTypes, CombinedGesturePostureDetector> CombinedGestureDetectorList = new Dictionary<GlobalData.GestureTypes, CombinedGesturePostureDetector>();

        TemplatedPostureDetector templatePostureDetector;

        private bool recordNextFrameForPosture;

        //AudioStreamManager audioManager;

        
        //KinectRecorder recorder;
        //KinectReplay replay;

        BindableNUICamera nuiCamera;

        public GlobalData.GestureTypes GestureCommand;

        public string ActionRecognitionResults, ObjectRecognitionResults;
        public string RecognitionFinishTriggerMag;

        #endregion

        public Boolean DoJobFlag = false;


        /// <summary>
        /// 系統設定的手勢指令
        /// </summary>
        private List<GlobalData.GestureTypes> GestureCommands = new List<GlobalData.GestureTypes>();
        /// <summary>
        /// 有些任務行為包含了和手勢指令一樣的行為，避免同樣的行為Detector重複增加節點軌跡，所以過濾掉重複的。紀錄手勢指令的軌跡，用這個判斷
        /// </summary>
        public List<GlobalData.GestureTypes> GestureRemindCommands;

        Queue<System.Windows.Shapes.Path> myPaths = new Queue<System.Windows.Shapes.Path>();

        public List<GlobalData.GestureTypes> TaskRecognitionActions = new List<GlobalData.GestureTypes>();
        public List<string> TaskRecognitionObjects = new List<string>();
        public List<string> TaskRecognitions = new List<string>();
        public bool TaskRecognitionFinish = false;

        /// <summary>
        /// 觸發物件辨識的時間
        /// </summary>
        public DateTime ObjectRecStartTime;
        public bool StartObjectRecFlag = false;
        public bool StartCutImageFlag = false;
        public int StartCutImageTime = 0;
        public bool StartObjectRecThreadFlag = false;

        public bool SavePlayerImageFlag = false;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ui">呼叫的window</param>
        /// <param name="kinect"></param>
        /// <param name="kinectCanvas">skeletonDisplayManager使用畫骨架</param>
        /// <param name="kinectDisplay">顯示Kinect彩色影像</param>
        /// <param name="maskedColor">顯示去背影像</param>
        /// <param name="gesturesCanvas">顯示手勢節點的軌跡</param>
        /// <param name="labelTrackings">顯示被辨識出的玩家名稱（有兩個）</param>
        /// <param name="tbGestureDetected">用來塞值觸發UI去檢查是否已經辨識完成及後續動作</param>

        /// <param name="objectCanvas">物件辨識物件擺放位置框</param>
        public KinectProcessor(Window ui, KinectSensor kinect, Canvas kinectCanvas, System.Windows.Controls.Image kinectDisplay,
            System.Windows.Controls.Image maskedColor, Canvas gesturesCanvas, Label[] labelTrackings, Canvas objectCanvas)
        {
            //_GestureProcessor = GestureProcessor.getInstance();

            this.UI = ui;
            this.Kinect = kinect;
            this.kinectCanvas = kinectCanvas;
            this.kinectDisplay = kinectDisplay;
            this.MaskedColor = maskedColor;
            this.gesturesCanvas = gesturesCanvas;
            this.LabelTrackings = labelTrackings;
            this.objectCanvas = objectCanvas;

            
        }

        public void InitializeKinect()
        {
            try
            {

                this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(Kinect);
                this.backgroundRemovedColorStream.Enable(KinectProcessor.ColorFormat, KinectProcessor.DepthFormat);

                this.skeletonDisplayManager = new SkeletonDisplayManager(Kinect, kinectCanvas);

                //Kinect.ElevationAngle = 0;

                this.skeletons = new Skeleton[Kinect.SkeletonStream.FrameSkeletonArrayLength];

                //kinectDisplay.DataContext = colorManager;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                log.Fatal(ex);
                GlobalValueData.Messages.Add(ex.Message);
                throw ex;

            }
        }

        public void close(Window ui)
        {

        }


        public void InitializeGestureAndPostureDetectors()
        {
            if (Kinect == null)
                return;

            this.LoadAllGestureDetectors();  //Ryan:載入所有手勢樣本資料
            this.LoadAllPostureDetectors();
            this.LoadAlllCombinedGesturePostureDetector();

            this.generateRemidCommands();
        }
 
        public void beforeSkeletonsProcess(IDisposable icframe, IDisposable idframe, IDisposable isframe, out byte[] pixelData, out short[] depthPixelData)
        {
            this.DoJobFlag = false;
            this.cframe = (ColorImageFrame)icframe;
            this.dframe = (DepthImageFrame)idframe;

            SkeletonFrame sframe = (SkeletonFrame)isframe;

            sframe.GetSkeletons(ref this.skeletons);
            this.ChooseSkeleton(this.skeletons); //只追蹤一人，決定追蹤某一人

            this.DoJobFlag = true;

            this.PixelDataLength = this.cframe.PixelDataLength;
            pixelData = new byte[this.cframe.PixelDataLength];
            this.cframe.CopyPixelDataTo(pixelData);

            depthPixelData = new short[this.dframe.PixelDataLength];
            this.dframe.CopyPixelDataTo(depthPixelData);

            this.DepthPixels = depthPixelData;

            if (SavePlayerImageFlag)
            {
                //ImageDAO.getInstance().saveBitmap(ImageChangeService.getInstance().convertByteArray2Bitmap(pixelData), "player");  //(Bitmap b, string fileName)
                SavePlayerImageFlag = false;
            }
        }

        public void afterSkeletonsProcess(IDisposable isframe, Skeleton skeleton)
        {
            DepthImagePixel[] depthImagePixels = new DepthImagePixel[this.dframe.PixelDataLength];

            this.dframe.CopyDepthImagePixelDataTo(depthImagePixels);

            if (skeleton != null && this.StartObjectRecFlag) 
            {
                ImageCutAlpha.getInstance().filterDepthPixelPlayer(ref depthImagePixels, JointType.HandLeft, skeleton, this.dframe, Kinect);
                this.BRCS_SourceReadyFlag = true;
            }
            this.backgroundRemovedColorStream.ProcessColor(this.cframe.GetRawPixelData(), this.cframe.Timestamp);
            this.backgroundRemovedColorStream.ProcessSkeleton(this.skeletons, ((SkeletonFrame)isframe).Timestamp);
            this.backgroundRemovedColorStream.ProcessDepth(depthImagePixels, this.dframe.Timestamp);
            

            ReplaySkeletonFrame replaySkeletonFrame = (SkeletonFrame)isframe;
            this.skeletonDisplayManager.Draw(replaySkeletonFrame.Skeletons, false, this.currentlyTrackedSkeletonId);  //第二個參數為seatedMode.IsChecked == true；這個method會在影像上畫出骨架線條與關節。我加了第3個參數

        }


        /// <summary>
        /// Use the sticky skeleton logic to choose a player that we want to set as foreground. This means if the app
        /// is tracking a player already, we keep tracking the player until it leaves the sight of the camera, 
        /// and then pick the closest player to be tracked as foreground.
        /// </summary>
        public void ChooseSkeleton(Skeleton[] skels)
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var nearestSkeleton = 0;

            foreach (var skel in skels)
            {
                if (null == skel)
                {
                    continue;
                }

                //如果有其他玩家也靠近這台Kinect，就抓圖
                if (skel.Position.Z < 2 && !NearPlayerIds.Exists(delegate(int data) { return data == skel.TrackingId; }))
                {
                    NearPlayerIds.Add(skel.TrackingId);
                    SavePlayerImageFlag = true;
                }

                if (skel.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (skel.TrackingId == this.currentlyTrackedSkeletonId)
                {
                    isTrackedSkeltonVisible = true;
                    //break;
                }                               

                if (!isTrackedSkeltonVisible && skel.Position.Z < 2 && skel.Position.Z < nearestDistance)
                {
                    nearestDistance = skel.Position.Z;
                    nearestSkeleton = skel.TrackingId;
                }
            }

            if (!isTrackedSkeltonVisible && nearestSkeleton != 0)
            {
                this.backgroundRemovedColorStream.SetTrackedPlayer(nearestSkeleton);
                if (this.currentlyTrackedSkeletonId != nearestSkeleton)
                {
                    //讓UI讀這個flag去存玩家影像
                    SavePlayerImageFlag = true;
                }

                this.currentlyTrackedSkeletonId = nearestSkeleton;
            }
        }

        public void RecordTrackingIds(Skeleton skeleton, int frameIdx)
        {
            if (TrackingIds[frameIdx].playerId == skeleton.TrackingId)
            {
                return;
            }
            else  //不一樣的Player
            {
                if (TrackingIds[frameIdx].color != null) //表示之前已經有記錄過者，但這不意謂已被辨識
                { //將之前追蹤記錄清除

                    if (recordedCurrentPlayerDic.ContainsKey(TrackingIds[frameIdx].playerId))  //表示之前這個位置是已被辨識者，顏色要還不同地方
                    {
                        recordedCurrentPlayerDic.Remove(TrackingIds[frameIdx].playerId);
                        //WriteLine("移除::"+TrackingIds[frameIdx].playerId+", "+recordedCurrentPlayerDic.Remove(TrackingIds[frameIdx].playerId));
                    }
                    else //處理尚未被辨識
                    {

                    }

                }

                TrackingIds[frameIdx] = null;

                Player newPlayer = new Player();
                newPlayer.playerId = skeleton.TrackingId;

                newPlayer.frameIdx = frameIdx;
                newPlayer.playerName = "";
                newPlayer.trackingState = skeleton.TrackingState;

                TrackingIds[frameIdx] = newPlayer;
            }
        }


        public Skeleton CloneSkeleton(Skeleton skOrigin)
        {
            // isso serializa o skeleton para a memoria e recupera novamente, fazendo uma cópia do objeto
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(ms, skOrigin);

            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();

            return obj as Skeleton;
        }

        public void addTaskRecognitionActions(GlobalData.GestureTypes gestureType)
        {
            this.TaskRecognitionActions.Add(gestureType);
            this.TaskRecognitions.Add(gestureType.ToString());
            TaskRecognitionFinish = false;
        }
        public void addTaskRecognitionObjects(string objectName)
        {
            this.TaskRecognitionObjects.Add(objectName);
            this.TaskRecognitions.Add(objectName);
            TaskRecognitionFinish = false;
        }
        public void cleanTaskRecognitions()
        {
            this.TaskRecognitionActions.Clear();
            this.TaskRecognitionObjects.Clear();
            this.TaskRecognitions.Clear();
        }

        public int countTaskRecognitions()
        {
            return this.TaskRecognitions.Count();
        }

        public void cleanTaskRecognitionActions()
        {
            foreach (var gesture in this.TaskRecognitionActions)
            {
                try
                {
                    this.TaskRecognitions.Find(delegate(string data) { return data == gesture.ToString(); });
                }
                catch (System.ArgumentNullException ane)
                {
                    log.Debug(ane);
                    TaskRecognitionActions.Remove(gesture);
                    cleanTaskRecognitionActions();
                    break;
                }
            }
        }

        public ColorImagePoint MapToColorImage(Joint jp)
        {
            //ColorImagePoint cp = Kinect.MapSkeletonPointToColor(jp.Position, Kinect.ColorStream.Format);
            ColorImagePoint cp = Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(jp.Position, Kinect.ColorStream.Format);
            return cp;
        }

        public void Clean()
        {
            
            CloseGestureDetector();

            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
            }

            if (eyeTracker != null)
            {
                eyeTracker.Dispose();
                eyeTracker = null;
            }

        }
    }

}
