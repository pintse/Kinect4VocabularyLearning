using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.Service;

namespace Ryan.Kinect.GestureCommand
{
    /// <summary>
    /// 手勢辨識模組功能入口介面
    /// </summary>
    public class GestureCommandRecognitionFacade
    {
        private static GestureCommandRecognitionFacade _GestureCommandRecognitionFacade = new GestureCommandRecognitionFacade();
        private static ILog log = LogManager.GetLogger(typeof(GestureCommandRecognitionFacade));

        private GestureCommandRecognitionFacade()
        {
            Console.WriteLine("載入" + AppDomain.CurrentDomain.BaseDirectory.ToString() + "log4netconfig.xml");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory.ToString() + "log4netconfig.xml"));
        }

        public static GestureCommandRecognitionFacade getInstance()
        {
            
            return _GestureCommandRecognitionFacade;
        }

        public void prepareRelatedData()
        {
            GlobalDataService.getInstance().prepareRelatedData();
        }

    }
}
