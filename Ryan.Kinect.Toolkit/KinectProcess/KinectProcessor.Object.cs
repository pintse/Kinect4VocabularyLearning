using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Ryan.Kinect.GestureCommand.VO;
using Ryan.Kinect.Toolkit.VO;
using Ryan.Kinect.Toolkit.ObjectProcess;

namespace Ryan.Kinect.Toolkit.KinectProcess
{
    /// <summary>
    /// Presentation Kinect 串流相關處理 -- 物件偵測處理
    /// </summary>
    partial class KinectProcessor
    {
        protected void DrawingLine(System.Windows.Point startPt, System.Windows.Point endPt)
        {
            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = startPt;
            myLineGeometry.EndPoint = endPt;

            System.Windows.Shapes.Path myPath = new System.Windows.Shapes.Path();
            myPath.Stroke = System.Windows.Media.Brushes.Red;
            myPath.StrokeThickness = 5;
            myPath.Data = myLineGeometry;

            this.objectCanvas.Children.Add(myPath);
            myPaths.Enqueue(myPath);
        }

        public void drawObjectRecogntionArea()
        {
            //double x = 30, y = 150, w = 280, h = 180;
            DrawingLine(new System.Windows.Point(OraX, OraY), new System.Windows.Point(OraX + OraW, OraY)); //上
            DrawingLine(new System.Windows.Point(OraX, OraY + OraH), new System.Windows.Point(OraX + OraW, OraY + OraH));  //下
            DrawingLine(new System.Windows.Point(OraX, OraY), new System.Windows.Point(OraX, OraY + OraH)); //左
            DrawingLine(new System.Windows.Point(OraX + OraW, OraY), new System.Windows.Point(OraX + OraW, OraY + OraH)); //右
        }

        public void cleanObjectRecogntionArea()
        {
            while (myPaths.Count > 0)
            {
                this.objectCanvas.Children.Remove(myPaths.Dequeue());
            }
            Console.WriteLine("");
        }

        public Dictionary<JointType, Bitmap> doObjectRecognitionNew(System.Windows.Controls.Image source)
        {
            bool flag = true;

            if (flag)
            {
                this.DoJobFlag = false;

                Dictionary<JointType, Bitmap> recBitmaps = new Dictionary<JointType, Bitmap>();//cutSpecifiedArea(Image source, int x, int y, int w, int h, bool fliterFlg = true)
                recBitmaps.Add(JointType.HandLeft, _ImageCut.cutSpecifiedArea(convertImage2Bitmap(source), OraX, OraY, OraW, OraH));
                //recBitmaps.Add(JointType.HandRight, _ImageCut.cutSpecifiedArea(pixelData, depthPixelData, OraX, OraY, OraW, OraH, false));

                return recBitmaps;
            }

            return null;

        }


        private System.Drawing.Bitmap convertImage2Bitmap(System.Windows.Controls.Image imageDisplay)
        {
            BitmapEncoder encoder = convertImage2BitmapEncoder(imageDisplay);

            // write the new file to disk
            try
            {
                System.Drawing.Bitmap bmp;
                using (MemoryStream outStream = new MemoryStream())
                {
                    encoder.Save(outStream);
                    //enc.Save(outStream);
                    bmp = new System.Drawing.Bitmap(outStream);
                }

                return bmp;

            }
            catch (Exception ex)
            {
                //this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteFailed, path);
                Console.WriteLine(ex);
                throw ex;

            }
        }


        private BitmapEncoder convertImage2BitmapEncoder(System.Windows.Controls.Image imageDisplay)
        {
            int colorWidth = (int)imageDisplay.ActualWidth;  
            int colorHeight = (int)imageDisplay.ActualHeight;
            //int colorWidth = this.foregroundBitmap.PixelWidth;  
            //int colorHeight = this.foregroundBitmap.PixelHeight;

            // create a render target that we'll render our controls to
            var renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                // render the color image masked out by players
                var colorBrush = new VisualBrush(imageDisplay);  //this.MaskedColor
                dc.DrawRectangle(colorBrush, null, new Rect(new System.Windows.Point(), new System.Windows.Size(colorWidth, colorHeight)));
            }

            renderBitmap.Render(dv);

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new BmpBitmapEncoder(); //PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            return encoder;
        }

        /// <summary>
        /// 搭配使用BackgroundRemovedColorStream的去背處理
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bmap"></param>
        public string startObjectRecognitionNew(Player player, Dictionary<JointType, Bitmap> bmaps)
        {
            foreach (KeyValuePair<JointType, Bitmap> kvp in bmaps)
            {
                CaptureObjectArgm captureObjectArgm = new CaptureObjectArgm();
                //captureObjectArgm.Player = player;
                //captureObjectArgm.JointInfo = player.jointInfos[kvp.Key]; //lHandPosition;
                captureObjectArgm.ColorBitmap = kvp.Value; // ;

                log.Debug("captureObjectArgm::::" + captureObjectArgm.ToString());

                return captureObject4BackgroundRemoved(captureObjectArgm);
            }

            return "";
        }


        public string captureObject4BackgroundRemoved(CaptureObjectArgm o)
        {

            log.Debug("captureObjectByBitmap start!");

            CaptureObjectArgm captureObjectArgm = (CaptureObjectArgm)o;

            try
            {
                ObjectRecognitionHandler objectRecognitionHandler = ObjectRecognitionHandler.getInstance();
                String result = objectRecognitionHandler.recognizeObject4BackgroundRemoved(captureObjectArgm.ColorBitmap);
                if (result != null && result != "")
                {
                    //this.ObjectRecogResult = captureObjectArgm.Player.playerName+(captureObjectArgm.JointInfo.RealName)+"拿"+result;

                    //this.TaskRecognitionActions.Remove((GlobalData.GestureTypes)Enum.Parse(typeof(GlobalData.GestureTypes), gesture, true));
                    this.TaskRecognitions.Remove(result);
                    log.Debug("remove " + result + "-->TaskRecognitions.Count::" + this.TaskRecognitions.Count);
                    //this.TextBlockRecognitionResults[1].Text = result;
                    //this.ObjectRecognitionResults = result;
                    //checkFinishRecognition(result);
                    return result;
                }
                else
                {
                    //this.TextBlockRecognitionResults[1].Text = "No Result";
                    //this.ObjectRecognitionResults = "No Result";
                    //return "No Result";
                }

                //captureObjectArgm.Player.holdObjects[captureObjectArgm.JointInfo.Name] = result;
                return result;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                if (captureObjectArgm.ColorBitmap != null)
                {
                    captureObjectArgm.ColorBitmap.Dispose();
                    captureObjectArgm.ColorBitmap = null;
                }
                log.Debug("captureObject end!");
            }


        }

    }

    /// ///////////////////////////////////////////////////////////////////////////////////////////////

    public class CaptureObjectArgm
    {
        short[] depthPixelData;

        public short[] DepthPixelData
        {
            get { return depthPixelData; }
            set { depthPixelData = value; }
        }

        byte[] pixelData;

        public byte[] PixelData
        {
            get { return pixelData; }
            set { pixelData = value; }
        }

        byte[] zeroPixels;

        public byte[] ZeroPixels
        {
            get { return zeroPixels; }
            set { zeroPixels = value; }
        }

        public Player Player { get; set; }


        JointInfo jointInfo;

        public JointInfo JointInfo
        {
            get { return jointInfo; }
            set { jointInfo = value; }
        }
        float depthValue;

        public float DepthValue
        {
            get { return depthValue; }
            set { depthValue = value; }
        }

        System.Drawing.Bitmap colorBitmap;

        public System.Drawing.Bitmap ColorBitmap
        {
            get { return colorBitmap; }
            set { colorBitmap = value; }
        }


        KinectSensor kinect;
        public KinectSensor Kinect
        {
            get { return kinect; }
            set { kinect = value; }
        }

        DepthImageFrame depthframe;
        public DepthImageFrame Depthframe
        {
            get { return depthframe; }
            set { depthframe = value; }
        }

        ColorImageFrame colorframe;
        public ColorImageFrame Colorframe
        {
            get { return colorframe; }
            set { colorframe = value; }
        }

        public override string ToString()
        {
            throw new NotImplementedException();           
        }
    }
}
