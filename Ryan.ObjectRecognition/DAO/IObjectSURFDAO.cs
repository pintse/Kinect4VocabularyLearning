using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ryan.ObjectRecognition.VO;
using Ryan.ObjectRecognition.SURF;

namespace Ryan.ObjectRecognition.DAO
{
    /// <summary>
    /// 物件SURF特徵資料存取程式實做介面
    /// </summary>
    public interface IObjectSURFDAO
    {
        void saveSURFIPoints(ObjectPictureVO objectVO);
    }
}
