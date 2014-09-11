using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Kinect.Toolbox;
using Ryan.Kinect.GestureCommand.DAO;
using Ryan.Kinect.GestureCommand.VO;

namespace Ryan.Kinect.GestureCommand.Service
{
    /// <summary>
    /// 手勢辨識模組全域資料服務
    /// </summary>
    public class GlobalDataService
    {
        private static GlobalDataService _GlobalDataService = new GlobalDataService();

        private GlobalDataService() { }

        public static GlobalDataService getInstance()
        {
            return _GlobalDataService;
        }

        public void prepareRelatedData()
        {
            GlobalData.GesturePostureSettings = GesturePostureConfigurationDAO.getInstance().retrieveGesturesPosturesConfiguration();
        }
        

    }
}
