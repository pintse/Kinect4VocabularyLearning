using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Ryan.Common.VO
{
    /// <summary>
    /// 全域共用變數Value Object
    /// </summary>
    public class GlobalCommonVO
    {
        private static GlobalCommonVO _GlobalDataVO = new GlobalCommonVO();
        private GlobalCommonVO() 
        {
            setIP();
        }

        public static GlobalCommonVO getInstance()
        {
            return _GlobalDataVO;

        }

        public DBSettingVO DBSettingVO { get; set; }


        public static string IP { get; private set; }
        public static string IPTail { get; private set; }
        

        public void test() { }

        private void setIP()
        {
            // 取得本機名稱
            string strHostName = Dns.GetHostName();

            // 取得本機的IpHostEntry類別實體，用這個會提示已過時
            //IPHostEntry iphostentry = Dns.GetHostByName(strHostName);
            // 取得本機的IpHostEntry類別實體，MSDN建議新的用法

            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            // 取得所有 IP 位址

            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                // 只取得IP V4的Address
                if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {

                    IP = ipaddress.ToString();
                    string[] ipSplit = GlobalCommonVO.IP.Split('.');
                    //IPTail = ipSplit[3];
                    IPTail = "13";  // for test
                    break;
                }

            }
        }

    }
}
