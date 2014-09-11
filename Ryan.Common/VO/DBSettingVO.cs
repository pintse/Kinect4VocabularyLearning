using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryan.Common.VO
{
    /// <summary>
    /// 資料庫連線參數Value Object
    /// </summary>
    public class DBSettingVO
    {
        public string ServerIP { get; set; }
        public string ServerPort { get; set; }
        public string Database { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
    }
}
