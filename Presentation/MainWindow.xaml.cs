using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Speech.Synthesis;
using Ryan.Kinect.Toolkit;
using System.Threading;
using System.Xml;
using System.IO;
using System.Net.Sockets;
using log4net;
using Ryan.Kinect.Toolkit.GestureCommands;
using Ryan.Kinect.Toolkit.VO;
using Ryan.Common;
using Ryan.Common.VO;
using Ryan.Kinect.Toolkit.ObjectProcess;

namespace Presentation
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private static ILog log = LogManager.GetLogger(typeof(MainWindow));
        
        public static Dictionary<string, string> PlayersName = new Dictionary<string, string>();
        Dictionary<string, Window> knowtable = new Dictionary<string, Window>();
        Boolean Limit1KinectFlg = false; 
        public static Boolean ServerFlg = false;
        public static Boolean NewPlayersFlg = true;

        public static Boolean startedObjectReconFlag = false;
        public static List<RecognitionWindow> RecognitionWindows = new List<RecognitionWindow>();
        //TODO 20140910
        //public static XmlDocument xmlPlayers = GlobalValueData.getXMLPlayers();  

        
        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private delegate void DummyDelegate();

        private DateTime EndTime=DateTime.Now.AddMinutes(40);  //壓力測試時間
        

        public MainWindow()
        {
            Console.WriteLine("載入" + AppDomain.CurrentDomain.BaseDirectory.ToString() + "log4netconfig.xml");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory.ToString() + "log4netconfig.xml")); 

            InitializeComponent();
            //DiscoverKinectSensor();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Unloaded += new RoutedEventHandler(MainWindow_Unloaded);

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensor_StatusChanged;

            GlobalValueData.KinectTypes kinectType = GlobalValueData.KinectTypes.Kinect4Windows;
            List<string> kinectIdents = new List<string>();
            foreach (var s in KinectSensor.KinectSensors)
            {
                if (kinectType == GlobalValueData.KinectTypes.Kinect4Windows && s.Status == KinectStatus.Connected)
                {
                    try
                    {
                        s.Start();
                        s.DepthStream.Enable();
                        s.DepthStream.Range = DepthRange.Near;  //如果是Xbox，在這一行就會丟出InvalidOperationException
                        s.DepthStream.Range = DepthRange.Default;
                        s.DepthStream.Disable();
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        log.Info(ex);
                        //TODO kinectType = GlobalValueData.KinectTypes.Kinect4Xbox; 
                        kinectType = GlobalValueData.KinectTypes.Kinect4Windows;  //開發器材將改為以Kinect4Xbox，原本只能運作在Kinect4Windows的也改成可運做在Kinect4Xbox
                    }
                    catch (Exception ex)
                    {
                        log.Fatal(ex);
                        throw ex;
                    }
                    finally
                    {
                        s.Stop();
                    }
                }
                kinectIdents.Add(s.DeviceConnectionId);
            }

            try
            {
                InitialProcessHandler iph = InitialProcessHandler.getInsatnce(kinectIdents);
                iph.setKinectType(kinectType);
                string user ="";
                foreach(var play in GlobalValueData.Players)
                {
                    user += play.playerName+ "  ";
                }
                this.tbUserInfo.Text = "你是" + user + "嗎？如果不是，請找管理員處理!";
            }
            catch (SoftwareException re)
            {
                MessageBox.Show(re.Message+":"+GlobalCommonVO.IPTail);
                throw re;
            }

            
            
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var cw in RecognitionWindows)
            {
                cw.Close();
            }
            //this.RequestStop();
        }

        void KinectSensor_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            string info = "狀態：" + e.Status + ", 感應器ID:" + e.Sensor.UniqueKinectId;
            TextBlock tb = new TextBlock() { Text = info };
            status.Items.Add(tb);

            WindowOpenAndClose(e.Status, e.Sensor);

        }

        private bool OpenMainMenuWindow(KinectStatus status, KinectSensor sensor)
        {
            switch (status)
            {
                case KinectStatus.Connected:
                    if (!knowtable.ContainsKey(sensor.DeviceConnectionId))
                    {
                        MainMenuWindow mmw = new MainMenuWindow(sensor);

                        mmw.Show();
                        knowtable.Add(sensor.DeviceConnectionId, mmw);
                        return true;
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (knowtable.ContainsKey(sensor.DeviceConnectionId))
                    {

                        ((RecognitionWindow)knowtable[sensor.DeviceConnectionId]).Close();
                        knowtable.Remove(sensor.DeviceConnectionId);
                    }
                    break;
            }
            return false;
        }

        private void DiscoverKinectSensor4Server(string windowType = "ColorWindow2")
        {
            string info = "偵測到" + KinectSensor.KinectSensors.Count + "台感應器";
            TextBlock tb = new TextBlock() { Text = info, Foreground = Brushes.Red };
            status.Items.Add(tb);
            int i = 0;
            bool flag = false;
            foreach (var s in KinectSensor.KinectSensors)
            {
                Util.WriteLine(this, "DiscoverKinectSensor::" + (++i));
                string info2 = "偵測到感應器ID:" + s.UniqueKinectId + ", 連線ID:" + s.DeviceConnectionId + ", 狀態:" + s.Status;
                Console.WriteLine("info2::" + info2);
                TextBlock t = new TextBlock() { Text = info2 };
                status.Items.Add(t);

                if (windowType == "MainMenuWindow")
                {
                    if (!flag)
                        flag = OpenMainMenuWindow(s.Status, s);
                }
                else
                {
                    WindowOpenAndClose(s.Status, s);
                }

                if (Limit1KinectFlg)
                {
                    break;
                }
            }
        }

        private void DiscoverKinectSensor(string windowType = "MainMenuWindow")
        {
            string info = "偵測到" + KinectSensor.KinectSensors.Count + "台感應器";
            TextBlock tb = new TextBlock() { Text = info, Foreground = Brushes.Red };
            status.Items.Add(tb);
            int i = 0;
            bool flag= false;
            foreach (var s in KinectSensor.KinectSensors)
            {
                Util.WriteLine(this, "DiscoverKinectSensor::" + (++i));
                string info2 = "偵測到感應器ID:" + s.UniqueKinectId + ", 連線ID:" + s.DeviceConnectionId + ", 狀態:" + s.Status;
                Console.WriteLine("info2::" + info2);
                TextBlock t = new TextBlock() { Text = info2 };
                status.Items.Add(t);

                if (windowType == "MainMenuWindow")
                {
                    if (!flag)
                    {
                        ObjectRecognitionHandler orh = ObjectRecognitionHandler.getInstance();
                        orh.prepareRelatedData();
                        flag = OpenMainMenuWindow(s.Status, s);
                    }
                }
                else
                {
                    WindowOpenAndClose(s.Status, s);
                }

                if (Limit1KinectFlg)
                {
                    break;
                }
            }
        }

        void WindowOpenAndClose(KinectStatus status, KinectSensor sensor)
        {
            throw new SoftwareException("The function is not published openly");
        }

        private void btnServer_Click(object sender, RoutedEventArgs e)
        {
            throw new SoftwareException("The function is not published openly");
        }

        private void btnClient_Click(object sender, RoutedEventArgs e)
        {
            ServerFlg = false;
            this.Title = this.Title + " - Client";

            DiscoverKinectSensor("MainMenuWindow");

        }

        private void CreateSocketClient()
        {
            throw new SoftwareException("The function is not published openly");
        }


        /// <summary>
        /// 將最新資料送給每台有安裝Kinect的機器
        /// </summary>
        private void broadcastNewPlayersXML()
        {

            throw new SoftwareException("The function is not published openly");

        }

        private void updateLbSocketLog()
        {

            throw new SoftwareException("The function is not published openly");


        }


        /*private void prepareGestureCommandsRule()
        {
            GestureCommandsHandler gch = GestureCommandsHandler.getInstance();
            gch.prepareRelatedData();
        }*/

        private void checkBox1Kinect_Checked(object sender, RoutedEventArgs e)
        {
            Limit1KinectFlg = true;
        }

        private void checkBox1Kinect_Unchecked(object sender, RoutedEventArgs e)
        {
            Limit1KinectFlg = false;
        }

        private void btnTestSocket_Click(object sender, RoutedEventArgs e)
        {
            //registerClinet();
        }

        private void btnTestDB_Click(object sender, RoutedEventArgs e)
        {

            ObjectRecognitionHandler orh = ObjectRecognitionHandler.getInstance();
            //orh.test();
        }

        private void btnBuildData_click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObjectRecognitionHandler orh = ObjectRecognitionHandler.getInstance();
                orh.buildNewPictureData();
                MessageBox.Show("物件辨識建檔完成");
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                MessageBox.Show(ex.Message);
            }
        }

        
        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            //ObjectRecognitionHandler orh = ObjectRecognitionHandler.getInstance();
            //orh.test();

            //GestureCommandsHandler gch = GestureCommandsHandler.getInstance();
            //gch.prepareRelatedData();

            Console.WriteLine("沒做什麼事");
        }

        private void checkBoxObjectRecongition_Checked(object sender, RoutedEventArgs e)
        {
            startedObjectReconFlag = false;            
        }

        private void checkBoxObjectRecongition_Unchecked(object sender, RoutedEventArgs e)
        {
            startedObjectReconFlag = true;            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }

    public class MyItem
    {
        public string text;
        public string value;

        public MyItem(string text, string value)
        {
            this.text = text;
            this.value = value;
        }

        public override string ToString()
        {
            return text;
        }
    } 

}