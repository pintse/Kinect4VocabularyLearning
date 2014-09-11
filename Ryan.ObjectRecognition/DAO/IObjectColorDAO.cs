using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.VO;

namespace Ryan.ObjectRecognition.DAO
{
    /// <summary>
    /// 物件顏色特徵資料存取程式實做介面
    /// </summary>
    public interface IObjectColorDAO
    {
        List<ObjectPictureVO> getObjectsColorsDistribution();
        void saveColor(ObjectPictureVO objectVO);
    }
}
