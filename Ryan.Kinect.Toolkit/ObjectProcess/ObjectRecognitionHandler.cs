using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.Kinect.Toolkit.ImageProcess;
using System.Drawing;
using Ryan.ObjectRecognition;
using log4net;
using Microsoft.Kinect;
using Ryan.Kinect.Toolkit.VO;

namespace Ryan.Kinect.Toolkit.ObjectProcess
{
    /// <summary>
    ///  Presentation Object Handler
    /// </summary>
    public class ObjectRecognitionHandler
    {
        private static ObjectRecognitionHandler _ObjectRecognitionHandler = new ObjectRecognitionHandler();
        private static ILog log = LogManager.GetLogger(typeof(ObjectRecognitionHandler));

        private ObjectRecognitionHandler()
        {
        }

        public static ObjectRecognitionHandler getInstance()
        {
            return _ObjectRecognitionHandler;
        }

        public void prepareRelatedData()
        {
            try
            {
                ObjectRecognitionFacade orf = ObjectRecognitionFacade.getInstance();
                orf.prepareRelatedData();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }


        public string recognizeObject4BackgroundRemoved(Bitmap colorBitmap)
        {
            
            Bitmap oBitmap = null;
            string result = "";
            ObjectRecognitionFacade orf=null;
            try
            {
                //oBitmap = _ImageCut.GetObjectBitmap(colorBitmap, jointPosition); 
                orf = ObjectRecognitionFacade.getInstance();
                result = orf.recognizeObjects(colorBitmap);
                return result;
            }
            catch (Exception ex)
            {
                log.Fatal("colorBitmap::" + colorBitmap + ",oBitmap::" + oBitmap + ",orf::" + orf + "," + ex);
                throw ex;
            }
            finally
            {
                if (colorBitmap != null)
                {
                    colorBitmap.Dispose();
                    colorBitmap = null;
                }
                if (oBitmap != null)
                {
                    oBitmap.Dispose();
                    oBitmap = null;
                }

            }
            
        }


        public void buildNewPictureData()
        {
            ObjectRecognitionFacade orf = ObjectRecognitionFacade.getInstance();
            orf.buildNewPicturesData();
        }
    }
}
