using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.Kinect.Toolkit
{
    /// <summary>
    /// 自定義視窗interface
    /// </summary>
    public interface MyWindow
    {
        /// <summary>
        /// 手勢行為物件辨識完成回call視窗處理後續訊息呈現
        /// </summary>
        /// <param name="gesture"></param>
        void gestureFinish(string gesture);
    }
}
