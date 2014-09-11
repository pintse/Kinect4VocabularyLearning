using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Kinect;
using System.IO;
using log4net;
using Ryan.Common.DAO;
using Ryan.Common.Service;
using System.Threading;
using System.Windows;
using System.Drawing.Imaging;
using Ryan.Kinect.Toolkit.VO;

namespace Ryan.Kinect.Toolkit.ImageProcess
{
    /// <summary>
    /// Presentation Image Processor
    /// </summary>
    public class ImageCutAlpha
    {
        private static ILog log = LogManager.GetLogger(typeof(ImageCutAlpha));

        private static ImageCutAlpha _ImageCutAlpha = new ImageCutAlpha();

        private ImageCutAlpha() { }

        public static ImageCutAlpha getInstance()
        {
            return _ImageCutAlpha;
        }

        public void filterDepthPixelPlayer(ref DepthImagePixel[] depthPixel, JointType jointType, Skeleton skeleton, DepthImageFrame depthFrame, KinectSensor kinect)
        {
            int distance = (int)(skeleton.Joints[jointType].Position.Z * 100);
            int[] lessenedImageWH = getLessenedImageWH(distance);

            //log.Debug("4::" + source + ",LHandPosition::" + jointInfo.Depth + ", frameWeight::" + frameWeight + ", frameHeight::" + frameHeight);

            DepthImagePoint dmp = kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[jointType].Position, depthFrame.Format);

            int extendPixel = 50;
            lessenedImageWH[0] += extendPixel;
            lessenedImageWH[1] += extendPixel;

            int startX = dmp.X - (lessenedImageWH[0] / 2) ;
            int endX = startX + lessenedImageWH[0];
            int startY = (int)(dmp.Y - lessenedImageWH[1] / 1.5f);
            int endY = startY + lessenedImageWH[1] ;
            if (startY < 0)
                startY = 0;
            if (startX < 0)
                startX = 0;
            if (endX >= depthFrame.Width)
                endX = depthFrame.Width - 1;
            if (endY >= depthFrame.Height)
                endY = depthFrame.Height - 1;


            for (int w = startX; w <= endX; w++)
            {
                for (int h = startY; h <= endY; h++)
                {
                    int i = h * depthFrame.Width + w;
                    if (depthPixel[i].PlayerIndex > 0 && depthPixel[i].Depth / 10 > (distance +5))
                    {
                        depthPixel[i].PlayerIndex = 0;
                    }

                }
            }
        }

        private int[] getLessenedImageWH(int distance)
        {
            int[] lessenedImageWH = new int[2];  //0:width, 1:hight
            lessenedImageWH[0] = 140;
            lessenedImageWH[1] = 140;

            if (distance != 0)
            {  
                float fX = ((140 - 80) * ((float)(distance - 80) / (240 - 80)));
                fX = 140 - fX;

                if (fX < 80)
                    fX = 80;

                lessenedImageWH[0] = (int)fX;
                lessenedImageWH[1] = (int)fX;
            }

            return lessenedImageWH;
        }


        public Bitmap cutSpecifiedArea(System.Drawing.Bitmap source, int x, int y, int w, int h, bool fliterFlg = true)
        {

            try
            {
                //Bitmap source = _ImageChangeService.convertWriteableBitmap2Bitmap(sourcew);

                Bitmap resultImage = new Bitmap((int)w, (int)h);
                Graphics pickedG = Graphics.FromImage(resultImage);

                pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //System.Drawing.Rectangle fromP = new System.Drawing.Rectangle(jointPosition.X2D - frameWeight / 2, (int)(jointPosition.Y2D - frameWeight / 1.5f), frameWeight, frameHeight); //舊圖的開始位置與大小
                System.Drawing.Rectangle fromP = new System.Drawing.Rectangle((int)x, (int)y, (int)w, (int)h); //舊圖的開始位置與大小
                System.Drawing.Rectangle toP = new System.Drawing.Rectangle(0, 0, (int)w, (int)h);//新圖的 指定畫圖開始位置與大小

                pickedG.DrawImage(source, toP, fromP, System.Drawing.GraphicsUnit.Pixel);//src要畫的原圖部分  des新圖的指定畫圖開始位置與大小

                //Bitmap ObjectBitmap = new Bitmap((System.Drawing.Image)pickedImage.Clone());

                pickedG.Dispose();

                //ImageDAO.getInstance().saveBitmap(resultImage, "");
                Bitmap zoomedImage = imageZoom(resultImage);
                return zoomedImage;
            }//try
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }

        Bitmap imageZoom(Bitmap bimap)
        {
            int w = bimap.Width * 3;
            int h = bimap.Height * 3;
            Bitmap resizeBmp = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(resizeBmp);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.DrawImage(bimap, new System.Drawing.Rectangle(0, 0, w, h), new System.Drawing.Rectangle(0, 0, bimap.Width, bimap.Height), GraphicsUnit.Pixel);
            Bitmap objectbimap = new Bitmap((System.Drawing.Image)resizeBmp.Clone());
            //objectbimap.Save("C:/KinectObjectEFLDataBase/ObjectDataBase/sepObject.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            g.Dispose();
            resizeBmp.Dispose();

            return objectbimap;
        }//imageZoom


        //TODO 20140910
        /*
        public Dictionary<JointType, System.Drawing.Bitmap> getHandsBitmap(Bitmap source, Player player, short[] depthPixels)
        {
            
            ImageDAO.getInstance().saveBitmap(source,"source");

            Dictionary<JointType, System.Drawing.Bitmap> handsBitmap = new Dictionary<JointType, System.Drawing.Bitmap>();
            handsBitmap.Add(JointType.HandLeft, lessenImage(ref source, player.jointInfos[JointType.HandLeft]));
            handsBitmap.Add(JointType.HandRight, lessenImage(ref source, player.jointInfos[JointType.HandRight]));

            ImageDAO.getInstance().saveBitmap(source, "source_final");

            if (source != null)
            {
                source.Dispose();
                source = null;
            }

            return handsBitmap;
        }

        private Bitmap lessenImage(ref Bitmap source, JointInfo jointInfo)
        {
            if (jointInfo.Depth > 280)
            {
                log.Info("超過辨識距離280");
                return null;
            }

            try
            {
                int[] lessenedImageWH = getLessenedImageWH(jointInfo.Depth);

                //log.Debug("4::" + source + ",LHandPosition::" + jointInfo.Depth + ", frameWeight::" + frameWeight + ", frameHeight::" + frameHeight);
                //log.Debug("4::" + source + "," + jointInfo.Name + "::" + jointInfo.Depth + ", frameWidth::" + lessenedImageWH[0] + ", frameHeight::" + lessenedImageWH[1]);

                int startX = jointInfo.X2D - lessenedImageWH[0] / 2;
                int endX = startX + lessenedImageWH[0];
                int startY = (int)(jointInfo.Y2D - lessenedImageWH[1] / 1.5f);
                int endY = startY + lessenedImageWH[1];


                Bitmap resultImage = new Bitmap(lessenedImageWH[0], lessenedImageWH[1]);
                Graphics pickedG = Graphics.FromImage(resultImage);

                pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                
                //System.Drawing.Rectangle fromP = new System.Drawing.Rectangle(jointPosition.X2D - frameWeight / 2, (int)(jointPosition.Y2D - frameWeight / 1.5f), frameWeight, frameHeight); //舊圖的開始位置與大小
                System.Drawing.Rectangle fromP = new System.Drawing.Rectangle(startX, startY, lessenedImageWH[0], lessenedImageWH[1]); //舊圖的開始位置與大小
                System.Drawing.Rectangle toP = new System.Drawing.Rectangle(0, 0, lessenedImageWH[0], lessenedImageWH[1]);//新圖的 指定畫圖開始位置與大小

                pickedG.DrawImage(source, toP, fromP, System.Drawing.GraphicsUnit.Pixel);//src要畫的原圖部分  des新圖的指定畫圖開始位置與大小

                //Bitmap ObjectBitmap = new Bitmap((System.Drawing.Image)pickedImage.Clone());

                pickedG.Dispose();

                //ImageDAO.getInstance().saveBitmap(resultImage, "" + jointInfo.Name);

                return resultImage;
            }//try
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }//lessenImage
        
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="depthPixels">深度影像陣列</param>
        /// <param name="colorPixels">彩色影像陣列</param>
        /// <param name="zeroPixels">即時黑白影像陣列</param>
        /// <param name="LHandPosition">左手關節 x , y , z</param>
        /// <param name="DepthValue">深度值</param>
        /// <returns>裁剪後的影像</returns>
        public Bitmap GetObjectBitmap(short[] depthPixels, byte[] colorPixels, byte[] zeroPixels, int[] jointPosition, float DepthValue, KinectSensor kinect, DepthImageFrame depthframe, ColorImageFrame colorframe)
        {
            log.Info("This filterUser method is Not Implemented");
            return null;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempObjectBitmap"></param>
        /// <param name="LHandPosition"></param>
        /// <returns></returns>
        public Bitmap GetObjectBitmap(Bitmap tempObjectBitmap, JointInfo jointPosition)
        {
            return imageLessen(jointPosition, tempObjectBitmap);//進行裁切
        }
        

        /// <summary>
        /// 取得原始完整畫面轉換成Bitmap
        /// </summary>
        /// <param name="depthPixels"></param>
        /// <param name="colorPixels"></param>
        /// <param name="zeroPixels"></param>
        /// 
        /// <param name="DepthValue"></param>
        /// <returns></returns>
        public Bitmap GetDisplayBitmap(short[] depthPixels, byte[] colorPixels, byte[] zeroPixels, float DepthValue)
        {
            BitmapSource ImageObjectSource = BitmapSource.Create(640, 480, 96, 96, PixelFormats.Pbgra32, null, colorPixels, 640 * 4); //此處可直接修改圖片大小
            Bitmap tempObjectBitmap = SourceConvertMap(ImageObjectSource);
            ImageObjectSource = null;
            return tempObjectBitmap;
        }
        

        private Bitmap SourceConvertMap(BitmapSource source) //bitsource轉型bitmap
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
        

        private Bitmap imageLessen(JointInfo jointPosition, Bitmap im)
        {
            if (jointPosition.Depth > 280)
            {
                log.Info("超過辨識距離280");
                return null;
            }

            try
            {
                log.Debug("1::" + im + ",LHandPosition::" + jointPosition.Depth);
                int frameWeight = 140;
                int frameHeight = 140;

                if (jointPosition.Depth != 0)
                {  
                    //int fX = 240 - (LHandPosition[2] - 80);
                    float fX = ((140 - 80) * ((float)(jointPosition.Depth - 80) / (240 - 80)));
                    log.Debug("2::" + im + ",LHandPosition::" + jointPosition.Depth + ", fX::" + fX);
                    fX = 140 - fX;
                    log.Debug("2::" + im + ",LHandPosition::" + jointPosition.Depth + ", fX::" + fX);
                    
                    if (fX < 80)
                        fX = 80;

                    frameWeight = (int)fX;
                    frameHeight = (int)fX;
                }

                log.Debug("4::" + im + ",LHandPosition::" + jointPosition.Depth + ", frameWeight::" + frameWeight + ", frameHeight::" + frameHeight);

                Bitmap ObjectBitmap = new Bitmap(frameWeight, frameHeight);
                Graphics pickedG = Graphics.FromImage(ObjectBitmap);

                pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //
                // 摘要:
                //     使用指定的位置和大小，初始化 System.Drawing.Rectangle 類別的新執行個體。
                //
                // 參數:
                //   x:
                //     矩形左上角的 X 座標。=>LHandPosition[0] - frameWeight / 2
                //
                //   y:
                //     矩形左上角的 Y 座標。=>(int)(LHandPosition[1] - frameWeight / 1.5f)
                //
                //   width:
                //     矩形的寬度。=>frameWeight
                //
                //   height:
                //     矩形的高度。=>frameHeight
                System.Drawing.Rectangle fromP = new System.Drawing.Rectangle(jointPosition.X2D - frameWeight / 2, (int)(jointPosition.Y2D - frameWeight / 1.5f), frameWeight, frameHeight); //舊圖的開始位置與大小
                System.Drawing.Rectangle toP = new System.Drawing.Rectangle(0, 0, frameWeight, frameHeight);//新圖的 指定畫圖開始位置與大小
                
                pickedG.DrawImage(im, toP, fromP, System.Drawing.GraphicsUnit.Pixel);//src要畫的原圖部分  des新圖的指定畫圖開始位置與大小
                
                //Bitmap ObjectBitmap = new Bitmap((System.Drawing.Image)pickedImage.Clone());

                pickedG.Dispose();

                //ObjectBitmap.Save("C:/temp/KinectCodeTestPicture/debug/ObjectBitmap" + DateTime.Now.ToFileTime().ToString() + ".jpg");  ///TODO for debug

                return ObjectBitmap;
            }//try
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }//imageLessen
       

        public void filterDistance(ref byte[] colorpixelData, KinectSensor kinect, short[] depthPixels, DepthImageFrame depthframe, ColorImageFrame colorframe, int distanceDepth )
        {
            ColorImagePoint[] colorpoints = new ColorImagePoint[depthPixels.Length];
            kinect.MapDepthFrameToColorFrame(depthframe.Format, depthPixels, colorframe.Format, colorpoints);
            //kinect.CoordinateMapper.MapDepthFrameToColorFrame(depthframe.Format, depthPixels, colorframe.Format, colorpoints);

            //checkColorpoints(colorpoints); //TODO for test
            log.Debug("distanceDepth::" + distanceDepth);
            
            //convertTransparent2DefaultColor(ref colorpixelData);
            
            for (int i = 0; i < depthPixels.Length; i++)
            {
                PixelInRange(ref colorpixelData, i, depthPixels, colorpoints, colorframe, distanceDepth);
                //PixelInUser(ref colorpixelData, i, depthPixels, colorpoints, colorframe);
            }
            
        }


        private void convertTransparent2DefaultColor(ref byte[] colorpixelData)
        {
            for (int i = 3; i < colorpixelData.Length; i += 4)
            {
                if (colorpixelData[i] != 0xFF)
                {
                    colorpixelData[i - 3] = 0x00;
                    colorpixelData[i - 2] = 0x00;
                    colorpixelData[i - 1] = 0x00;
                }
 
            }
        }


        private void PixelInRange(ref byte[] colorpixelData, int i, short[] depthPixels, ColorImagePoint[] colorpoints, ColorImageFrame colorframe, int distanceDepth)
        {
            
            
            float pixeldistance = (float)(depthPixels[i] >> DepthImageFrame.PlayerIndexBitmaskWidth);
            pixeldistance /= 10;
            
            ColorImagePoint p = colorpoints[i];
            int colorimageindex = (p.X + (p.Y * colorframe.Width)) * colorframe.BytesPerPixel;

            int playerIndex = depthPixels[i] & DepthImageFrame.PlayerIndexBitmask;

            try
            {
                if (playerIndex > 0 &&  (pixeldistance > distanceDepth + 15))  //DepthValue=distanceDepth
                {
                    log.Debug("pixeldistance::" + pixeldistance);
                    colorpixelData[colorimageindex + 0] = 0x00;
                    colorpixelData[colorimageindex + 1] = 0x00;
                    colorpixelData[colorimageindex + 2] = 0x00;
                    colorpixelData[colorimageindex + 3] = 0x00;  //0xFF 不透明、0x00透明 
                }
                 

                
            }
            catch (Exception ex)
            {
                log.Debug("colorpixelData.Length::" + colorpixelData.Length + ", colorpoints.Length::" + colorpoints.Length + ", p.X::" + p.X + ", p.Y::" + p.Y + ", colorimageindex::" + colorimageindex + ", colorframe.Width::" + colorframe.Width +
                        ", colorframe.BytesPerPixel::" + colorframe.BytesPerPixel + ", pixeldistance::" + pixeldistance);
                log.Fatal(ex);
                
            }
            finally
            {

            }
            
        }

         
        public Bitmap cutSpecifiedArea(byte[] colorPixelData, short[] depthPixels, int x, int y, int w, int h, bool fliterFlg = true)
        {

            if (fliterFlg)
            {
                //取得中心點深度
                int centerX = (w / 2) + x;
                int centerY = (h / 2) + y;

                //深度影像使用較小解析度：
                centerX /= 2;
                centerY /= 2;
                int centerDepth = calcPointDistance(centerX, centerY, depthPixels, 320);

                int correctVal = 148;

                for (int j = y; j <= (y + h); j++)
                {
                    for (int i = x - correctVal; i <= (x + w); i++)
                    {
                        //深度影像使用較小解析度：
                        int depthX = i / 2;
                        int depthY = j / 2;
                        int depth = 0;
                        try
                        {
                            //X:Y=30:150, depthX:15 : depthY:240 , depthPixels.Length:76800
                            depth = calcPointDistance(depthX, depthY, depthPixels, 320);
                        }
                        catch (Exception ex)
                        {
                            log.Fatal("X:Y=" + i + ":" + j + "depthX:" + depthX + "depthY:" + depthY + "depthPixels.Length:" + depthPixels.Length + "\n" + ex);
                            throw ex;
                        }
                        if (depth - centerDepth > 50) //比中心點遠超過
                        {
                            int pointAddr = ((j * 640) + i) * 4;
                            pointAddr += correctVal;
                            colorPixelData[pointAddr] = 255;
                            colorPixelData[pointAddr + 1] = 255;
                            colorPixelData[pointAddr + 2] = 255;
                            colorPixelData[pointAddr + 3] = 0x00;  //0xFF 不透明、0x00透明 

                        }
                    }
                }
            }
            Bitmap source = _ImageChangeService.convertByteArray2Bitmap(colorPixelData);

            //System.Windows.Media.Imaging.WriteableBitmap sourcew = null; 
            //ImageDAO.getInstance().saveBitmap(source, "");
            
            try
            {
                //Bitmap source = _ImageChangeService.convertWriteableBitmap2Bitmap(sourcew);

                Bitmap resultImage = new Bitmap( (int)w,(int)h);
                Graphics pickedG = Graphics.FromImage(resultImage);

                pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //System.Drawing.Rectangle fromP = new System.Drawing.Rectangle(jointPosition.X2D - frameWeight / 2, (int)(jointPosition.Y2D - frameWeight / 1.5f), frameWeight, frameHeight); //舊圖的開始位置與大小
                System.Drawing.Rectangle fromP = new System.Drawing.Rectangle((int)x, (int)y, (int)w, (int)h); //舊圖的開始位置與大小
                System.Drawing.Rectangle toP = new System.Drawing.Rectangle(0, 0, (int)w, (int)h);//新圖的 指定畫圖開始位置與大小

                pickedG.DrawImage(source, toP, fromP, System.Drawing.GraphicsUnit.Pixel);//src要畫的原圖部分  des新圖的指定畫圖開始位置與大小

                //Bitmap ObjectBitmap = new Bitmap((System.Drawing.Image)pickedImage.Clone());

                pickedG.Dispose();

                //ImageDAO.getInstance().saveBitmap(resultImage, "");

                return resultImage;
            }//try
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw ex;
            }
        }
  
         
        public int calcPointDistance(int x, int y, short[] depthPixels, int depthFrameWidth)
        {
            int depth = 0;
            
            depth = depthPixels[(int)(x + (y * depthFrameWidth))] >> DepthImageFrame.PlayerIndexBitmaskWidth;
            return depth;
        }
        

        public string showPointDistance(int x, int y, short[] depthPixels, int depthFrameWidth)
        {
            int depth = 0;

            depth = depthPixels[(int)(x + (y * depthFrameWidth))] >> DepthImageFrame.PlayerIndexBitmaskWidth;
            //return "("+x+","+y+")"+depth;
            return ""+depth;
        }
        */

        //////////////////////////////////////////
        #region 測試區塊

        private void checkColorpoints(ColorImagePoint[] colorpoints)
        {
            int maxX = 0, maxY = 0;
            foreach (ColorImagePoint p in colorpoints)
            {
                if (p.X > maxX)
                    maxX = p.X;
                if (p.Y > maxY)
                    maxY = p.Y;
            }

            log.Debug("colorpoints.Length=" + colorpoints.Length + " , maxX=" + maxX + " , maxY=" + maxY);
        }

        private void filterPlayer(ref Bitmap source, int startX, int startY, int endX, int endY, short[] depthPixels, int distanceDepth)
        {
            for (int w = startX; w < endX; w++)
            {
                for (int h = startY; h < endY; h++)
                {
                    int i = h * source.Width + w;
                    float pixeldistance = 0;
                    try
                    {
                        //pixeldistance = (float)(depthPixels[i] >> DepthImageFrame.PlayerIndexBitmaskWidth);
                        //pixeldistance /= 10;
                        int playerIndex = depthPixels[i] & DepthImageFrame.PlayerIndexBitmask;
                        //ColorImagePoint p = colorpoints[i];
                        //int colorimageindex = (p.X + (p.Y * colorframe.Width)) * colorframe.BytesPerPixel;            


                        if (playerIndex > 0)  //DepthValue=distanceDepth
                        {
                            //System.Drawing.Color color = source.GetPixel(w, h);
                            source.SetPixel(w, h, System.Drawing.Color.White);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug("startX::" + startX + " , endX::" + endX + " , startY::" + startY + " , endY::" + endY +
                            " , w::" + w + " , h::" + h +
                            " , i::" + i + " , distanceDepth::" + distanceDepth +
                            " , pixeldistance::" + pixeldistance +
                            " , source.Width::" + source.Width + " , .Height::" + source.Height);
                        log.Fatal(ex);

                    }
                    finally
                    {

                    }
                }
            }
        }
        #endregion
        
    }
}
