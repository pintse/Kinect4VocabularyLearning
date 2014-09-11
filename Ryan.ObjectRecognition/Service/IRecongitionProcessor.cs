using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.VO;
using Ryan.ObjectRecognition.SURF;
using System.Drawing;

namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// 辨識邏輯處理程式實做介面
    /// </summary>
    public interface IRecongitionProcessor
    {
        Dictionary<string, CongruousObjectVO> getBestCongruousObjects(Bitmap realBitamp, Dictionary<string, CongruousObjectVO> candidateObjects);
        void buildNewImageFeaturePersistence(ObjectPictureVO objectPictureVO);
    }
}
