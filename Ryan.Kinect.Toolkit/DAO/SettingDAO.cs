using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using log4net;
using MySql.Data.MySqlClient;
using Ryan.Common.DAO;
using Ryan.Common.VO;
using Ryan.Kinect.Toolkit.VO;

namespace Ryan.Kinect.Toolkit.DAO
{
    public class SettingDAO
    {
        private static SettingDAO _SettingDAO = new SettingDAO();
        private static ILog log = LogManager.GetLogger(typeof(SettingDAO));

        private XmlDocument _XMLSetting = new XmlDocument();

        private SettingDAO()
        {
            
            try
            {
                string xmlFilePath = Path.Combine(Environment.CurrentDirectory, @"Data\Setting.xml");

                _XMLSetting.Load(xmlFilePath);
                Console.WriteLine("載入::"+xmlFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }

        }

        public static SettingDAO getInstance()
        {
            return _SettingDAO;
        }

        public void initializeSettings()
        {   
            initializeSettings(_XMLSetting);
        }

        private static void initializeSettings(XmlDocument xmlSetting)
        {
            DBSettingVO dbSettingVO = new DBSettingVO();
                       
            XmlNodeList nodeList = xmlSetting.ChildNodes; //整個文件
            
            foreach (System.Xml.XmlNode node0 in nodeList) //最外層範圍內的資料
            {
                log.Debug("node0.InnerText::" + node0.InnerText + ", node0.LocalName::" + node0.LocalName + ", node0.Name::" + node0.Name);

                if (node0.Name == "settings")
                {
                   
                    foreach (System.Xml.XmlNode node1 in node0.ChildNodes)    //每筆資料
                    {
                        //log.Debug("node1.Name::" + node1.Name + ", node1.InnerText::" + node1.InnerText);
                        if (node1.Name == "db_server")
                            dbSettingVO.ServerIP = node1.InnerText;
                    

                    }
                }

            }

            GlobalValueData.DBSettingVO = dbSettingVO;
        }

    }
}
