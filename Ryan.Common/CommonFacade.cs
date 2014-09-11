using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Ryan.Common.VO;

namespace Ryan.Common
{
    /// <summary>
    /// Common模組對外接收服務請求
    /// </summary>
    public class CommonFacade
    {
        private static ILog log = LogManager.GetLogger(typeof(CommonFacade));
        private static readonly object ticket = new object();
        private static volatile CommonFacade _Myself;

        private CommonFacade() { }

        /// <summary>
        /// 取得CommonFacade實體，若無實體，則依照傳入的資料庫連線參數建立一個CommonFacade實體
        /// </summary>
        /// <param name="dbSettingVO"></param>
        /// <returns></returns>
        public static CommonFacade getInsatnce4First(DBSettingVO dbSettingVO)
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new CommonFacade();
                        configDB(dbSettingVO);
                    }
                }
            }
            return _Myself;
        }

        public static CommonFacade getInsatnce()
        {
            if (_Myself == null)
            {
                throw new SoftwareException("請使用getInsatnce4First(DBSettingVO dbSettingVO)取得實體");
            }
            return _Myself;
        }
        private static void configDB(DBSettingVO dbSettingVO)
        {
            GlobalCommonVO.getInstance().DBSettingVO = dbSettingVO;
        }
    }
}
