using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using log4net;

namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// The class is not published openly
    /// </summary>
    public class ClassifiedColor
    {
        private static ClassifiedColor _ClassifiedColor = new ClassifiedColor();

        private static ILog log = LogManager.GetLogger(typeof(ClassifiedColor));

        private ClassifiedColor()
        {
        }//建構子

        public static ClassifiedColor getInstance()
        {
            return _ClassifiedColor;
        }

    }
}
