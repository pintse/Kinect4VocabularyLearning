using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ryan.Kinect.Toolkit
{
    /// <summary>
    /// 工具
    /// </summary>
    public class Util
    {
        public static void WriteLine(Object o, string data)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ", " + data);
        }
    }
}
