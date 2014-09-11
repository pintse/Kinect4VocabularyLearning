using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Ryan.Kinect.Toolkit.GestureCommands;
using Ryan.Kinect.Toolkit.DAO;
using Ryan.Common;
using Ryan.Kinect.Toolkit.VO;

namespace Ryan.Kinect.Toolkit
{
    /// <summary>
    /// 初始化處理控制
    /// </summary>
    public class InitialProcessHandler
    {
        private static ILog log = LogManager.GetLogger(typeof(InitialProcessHandler));

        private static volatile InitialProcessHandler _Myself;
        private static readonly object ticket = new object();

        private InitialProcessHandler(List<string> kinectsId) 
        {

            initialGlobeValueData(kinectsId);

            initialGestureCommandsHandler();

        }


        public static InitialProcessHandler getInsatnce(List<string> kinectsId)
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new InitialProcessHandler(kinectsId);

                    }
                }
                
            }
            return _Myself;
        }

        private static void initialGlobeValueData(List<string> kinectsId)
        {
            SettingDAO.getInstance().initializeSettings();
            CommonFacade.getInsatnce4First(GlobalValueData.DBSettingVO);

            foreach (string data in kinectsId)
            {
                GlobalValueData.GestureCommandMessage.Add(data,new Queue<string>());  
            }

            UserDAO.getInstance().retrieveUserData();

        }

        public void setKinectType(GlobalValueData.KinectTypes kinectType)
        {
            GlobalValueData.KinectType = kinectType;
        }


        private static void initialGestureCommandsHandler()
        {
            GestureCommandsHandler gch = GestureCommandsHandler.getInstance();
            gch.prepareRelatedData();
        }
    }
}
