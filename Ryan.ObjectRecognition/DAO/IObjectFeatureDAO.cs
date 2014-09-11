using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.VO;

namespace Ryan.ObjectRecognition.DAO
{
    /// <summary>
    /// 物件特徵資料存取程式實做介面
    /// </summary>
    public interface IObjectFeatureDAO
    {
        Dictionary<string, CongruousObjectVO> fillFeatures2Objects(Dictionary<string, CongruousObjectVO> candidateObjects);
    }
}
