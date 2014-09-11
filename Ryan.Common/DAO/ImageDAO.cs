using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Ryan.Common.DAO
{
    /// <summary>
    /// 影像檔案DAO
    /// </summary>
    public class ImageDAO
    {
        private static ImageDAO _ImageDAO = new ImageDAO();
        private static ILog log = LogManager.GetLogger(typeof(ImageDAO));

        private static String fileFolderRoot = "C:/temp/KinectCodeTestPicture/";

        private ImageDAO() { }

        public static ImageDAO getInstance()
        {
            if (_ImageDAO == null)
            {
                _ImageDAO = new ImageDAO();
            }
            return _ImageDAO;
        }

        public void saveBitmap(Bitmap b, string fileName)
        {
            Bitmap oBitmap = null;
            try
            {
                oBitmap = new Bitmap(b);
                String fullFileName = Thread.CurrentThread.ManagedThreadId + "_" + DateTime.Now.ToFileTime().ToString() + "_" + fileName + ".png";
                oBitmap.Save(fileFolderRoot + fullFileName);
                log.Debug(fileFolderRoot + fullFileName);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                if (oBitmap != null)
                {
                    oBitmap.Dispose();
                    oBitmap = null;
                }
            }
        }

        public void saveByteArray2ImageFile(byte[] source, string fileName, ImageFormat format)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(source, 0, source.Length);
            using (Image image = Image.FromStream(stream))
            {
                image.Save(fileFolderRoot + Thread.CurrentThread.ManagedThreadId + "_" + DateTime.Now.ToFileTime().ToString() + "_" + fileName + ".png", format);  // Or Png
            }
        }
    }
}
