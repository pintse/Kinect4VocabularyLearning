using System;
namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// 辨識結果處理程式實做介面
    /// </summary>
    interface IRecongitionResultProcessor
    {
        Ryan.ObjectRecognition.VO.CongruousObjectVO determine(System.Collections.Generic.Dictionary<string, Ryan.ObjectRecognition.VO.CongruousObjectVO> congruousObjectList);
    }
}
