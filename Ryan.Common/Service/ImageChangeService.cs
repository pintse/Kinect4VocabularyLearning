using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;

namespace Ryan.Common.Service
{
    /// <summary>
    /// 影像格式轉換服務
    /// </summary>
    public class ImageChangeService
    {
        private static ImageChangeService _ImageChangeService = new ImageChangeService();
        private static ILog log = LogManager.GetLogger(typeof(ImageChangeService));

        private ImageChangeService() { }

        public static ImageChangeService getInstance()
        {
            return _ImageChangeService;
        }

        public Bitmap convertByteArray2Bitmap(byte[] source)
        {
            BitmapSource ImageObjectSource = BitmapSource.Create(640, 480, 96, 96, PixelFormats.Pbgra32, null, source, 640 * 4); //use PixelFormats.Pbgra32

            Bitmap tempObjectBitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                System.Windows.Media.Imaging.BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(ImageObjectSource));
                enc.Save(outStream);
                tempObjectBitmap = new System.Drawing.Bitmap(outStream);
            }

            tempObjectBitmap.MakeTransparent(System.Drawing.Color.White);     // Change a color to be transparent

            return tempObjectBitmap;
        }

        public Bitmap convertWriteableBitmap2Bitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            bmp.MakeTransparent(System.Drawing.Color.Black);
            return bmp;
        }

        public Bitmap convertBitmapSource2Bitmap(BitmapSource source) //bitsource轉型bitmap
        {
            Bitmap tempbimap;
            using (MemoryStream outStream = new MemoryStream())
            {
                System.Windows.Media.Imaging.BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(source));
                enc.Save(outStream);
                tempbimap = new System.Drawing.Bitmap(outStream);
            }
            return tempbimap;
        }//getimage
    }
}
