using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kinect.Toolbox;
using Kinect.Toolbox.Record;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using Ryan.Common.DAO;
using Ryan.Common.Service;
using Ryan.Kinect.GestureCommand.VO;
using Ryan.Kinect.Toolkit;
using Ryan.Kinect.Toolkit.ContentProcess;
using Ryan.Kinect.Toolkit.GestureCommands;
using Ryan.Kinect.Toolkit.KinectProcess;
using Ryan.Kinect.Toolkit.VO;

namespace Presentation
{
    /// <summary>
    /// 處理Kinect串流影像
    /// </summary>
    partial class RecognitionWindow
    {
        private void InitializeKinect1()
        {
            log.Debug("Initialize()");

            if (Kinect != null)
            {
                log.Info("參數設定運轉的Kinect Type(1:w,2:x)=" + GlobalValueData.KinectType);

                this.Title += "感應器ID:" + Kinect.UniqueKinectId + ", 連線ID:" + Kinect.DeviceConnectionId + ", 狀態:" + Kinect.Status;

                _KinectProcessor.InitializeKinect();
            }

            log.Debug("Initialize() end");
        }

        

        public void startBackgroundRemovedFrame()
        {

            _KinectProcessor.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;
        }

        public void stopBackgroundRemovedFrame()
        {
            _KinectProcessor.backgroundRemovedColorStream.BackgroundRemovedFrameReady -= this.BackgroundRemovedFrameReadyHandler;
        }

        public void startAllFrames()
        {

            this.Kinect.AllFramesReady += myKinect_AllFramesReady;

            startBackgroundRemovedFrame();
        }

        public void stopAllFrames()
        {

            this.Kinect.AllFramesReady -= myKinect_AllFramesReady;

            stopBackgroundRemovedFrame();
            log.Debug("stopAllFrames()");
        }

        void myKinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            try
            {
                using (IDisposable icframe = e.OpenColorImageFrame(), idframe = e.OpenDepthImageFrame(), isframe = e.OpenSkeletonFrame())
                {
                    if (icframe != null && idframe != null && isframe != null)
                    {

                        byte[] pixelData;
                        short[] depthPixelData;
                        _KinectProcessor.beforeSkeletonsProcess(icframe, idframe, isframe, out pixelData, out depthPixelData);

                        Skeleton skeleton = null;

                        try
                        {
                            for (int i = 0; i < _KinectProcessor.skeletons.Length; i++) //fSkeletons.Length=6
                            {
                                if (_KinectProcessor.currentlyTrackedSkeletonId != 0 && _KinectProcessor.skeletons[i].TrackingId != _KinectProcessor.currentlyTrackedSkeletonId)
                                    continue;

                                skeleton = _KinectProcessor.CloneSkeleton(_KinectProcessor.skeletons[i]);

                                _KinectProcessor.RecordTrackingIds(_KinectProcessor.skeletons[i], i);

                                if (_KinectProcessor.skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                                {
                                    try
                                    {
                                        if (!_KinectProcessor.TrackedPlayers.ContainsKey(_KinectProcessor.skeletons[i].TrackingId))
                                        {
                                            Player newPlayer = new Player();
                                            newPlayer.playerId = _KinectProcessor.skeletons[i].TrackingId;
                                            newPlayer.trackingState = _KinectProcessor.skeletons[i].TrackingState;
                                            _KinectProcessor.TrackedPlayers.Add(_KinectProcessor.skeletons[i].TrackingId, newPlayer);
                                        }

                                        ColorImagePoint cpl = _KinectProcessor.MapToColorImage(_KinectProcessor.skeletons[i].Joints[JointType.HandLeft]);
                                        ColorImagePoint cpr = _KinectProcessor.MapToColorImage(_KinectProcessor.skeletons[i].Joints[JointType.HandRight]);
                                        
                                        handTriggerEvent(cpl, cpr);

                                        _KinectProcessor.contextTracker.Add(_KinectProcessor.skeletons[i].Position.ToVector3(), _KinectProcessor.skeletons[i].TrackingId);


                                        //這四行執行時間較久
                                        _KinectProcessor.addJointPositionOnlyMust(_KinectProcessor.skeletons[i], _KinectProcessor.TaskRecognitionActions);
                                        _KinectProcessor.addJointPositionOnlyMust(_KinectProcessor.skeletons[i], _KinectProcessor.GestureRemindCommands);
                                        _KinectProcessor.trackPosturesOnlyMust(_KinectProcessor.skeletons[i], _KinectProcessor.TaskRecognitionActions);
                                        _KinectProcessor.trackPosturesOnlyMust(_KinectProcessor.skeletons[i], _KinectProcessor.GestureRemindCommands);

                                        bool isShouldersTowardsSensor = _KinectProcessor.contextTracker.IsShouldersTowardsSensor(_KinectProcessor.skeletons[i]);
                                        bool isStableRelativeToCurrentSpeed = _KinectProcessor.contextTracker.IsStableRelativeToCurrentSpeed(_KinectProcessor.skeletons[i].TrackingId);


                                        _KinectProcessor.cleanTaskRecognitionActions();


                                    }
                                    catch (Exception ex)
                                    {
                                        log.Fatal(ex);
                                        throw ex;
                                    }

                                } //有被追蹤

                            }  //骨架數

                            _KinectProcessor.afterSkeletonsProcess(isframe, skeleton);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        pixelData = null;
                        depthPixelData = null;
                    } //有frame
                }  //串流

            }
            catch (InvalidOperationException ex)
            {
                log.Fatal(ex);
                GlobalValueData.Messages.Add(ex.Message);
                Console.WriteLine(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                GlobalValueData.Messages.Add(ex.Message);
                Console.WriteLine(ex);
                throw ex;

            }
        }

        /// <summary>
        /// Handle the background removed color frame ready event. The frame obtained from the background removed
        /// color stream is in RGBA format.
        /// </summary>
        /// <param name="sender">object that sends the event</param>
        /// <param name="e">argument of the event</param>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            short[] depthPixels = new short[_KinectProcessor.DepthPixels.Length];
            _KinectProcessor.DepthPixels.CopyTo(depthPixels, 0);

            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    if (null == _KinectProcessor.foregroundBitmap || _KinectProcessor.foregroundBitmap.PixelWidth != backgroundRemovedFrame.Width
                        || _KinectProcessor.foregroundBitmap.PixelHeight != backgroundRemovedFrame.Height)
                    {
                        _KinectProcessor.foregroundBitmap = new WriteableBitmap(backgroundRemovedFrame.Width, backgroundRemovedFrame.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

                         // Set the image we display to point to the bitmap where we'll put the image data
                        this.MaskedColor.Source = _KinectProcessor.foregroundBitmap;
                    }


                    // Write the pixel data into our bitmap
                    _KinectProcessor.foregroundBitmap.WritePixels(
                        new Int32Rect(0, 0, _KinectProcessor.foregroundBitmap.PixelWidth, _KinectProcessor.foregroundBitmap.PixelHeight),
                        backgroundRemovedFrame.GetRawPixelData(),
                        //brPixelData,
                        _KinectProcessor.foregroundBitmap.PixelWidth * sizeof(int),
                        0);

                    Skeleton recognizedSkeleton = null;

                    if (backgroundRemovedFrame.TrackedPlayerId != 0)
                    {
                        foreach (Skeleton skeleton in _KinectProcessor.skeletons)
                        {
                            if (skeleton.TrackingId == backgroundRemovedFrame.TrackedPlayerId)
                            {
                                recognizedSkeleton = skeleton;

                                if (_KinectProcessor.StartCutImageTime > 0 && !_KinectProcessor.StartObjectRecThreadFlag)
                                {
                                    doObjectRecognition(this.MaskedColor);
                                    _KinectProcessor.StartObjectRecThreadFlag = true;
                                }

                            }
                        }
                    }

                    _KinectProcessor.GestureCommand = GlobalData.GestureTypes.Init;

                }
            }
        }

        void doObjectRecognition(System.Windows.Controls.Image source)
        {
            object[] data = new object[2];
            data[0] = _KinectProcessor.TrackedPlayers[_KinectProcessor.currentlyTrackedSkeletonId];
            data[1] = _KinectProcessor.doObjectRecognitionNew(source); //Dictionary<JointType, Bitmap>

            this.ObjectRecognitionBackground.RunWorkerAsync(data);
        }




        private void handTriggerEvent(ColorImagePoint cpl, ColorImagePoint cpr)
        {
            int wh = 60;
            double newLeft = cpr.X - wh / 2;
            double newTop = cpr.Y - wh / 2;

            double newLeftl = cpl.X - wh / 2;
            double newTopl = cpl.Y - wh / 2;

            if (this.ObjectRecButton.Visibility == System.Windows.Visibility.Visible && this.MaskedColor.ActualHeight > 0)
            {

                if (newLeft > this.ObjectRecButton.Margin.Left && newLeft < (640 - this.ObjectRecButton.Margin.Right)
                    && newTop > this.ObjectRecButton.Margin.Top && newTop < (480 - this.ObjectRecButton.Margin.Bottom))
                {
                    ObjectRecButton.Hovering();
                    return;
                }
                else
                {
                    ObjectRecButton.Release();
                }
            
            }

            if (this.MenuButton.Visibility == System.Windows.Visibility.Visible && this.MaskedColor.ActualHeight > 0)
            {

                if (newLeft > this.MenuButton.Margin.Left && newLeft < (640 - this.MenuButton.Margin.Right)
                    && newTop > this.MenuButton.Margin.Top && newTop < (480 - this.MenuButton.Margin.Bottom))
                {
                    if (MenuButtonTime <= 0)
                    {
                        MenuButton.Hovering();
                        return;
                    }
                }
                else
                {
                    MenuButton.Release();
                }
                
            }

            if (this.NextButton.Visibility == System.Windows.Visibility.Visible && this.MaskedColor.ActualHeight > 0)
            {

                if (newLeftl > this.NextButton.Margin.Left && newLeftl < (640 - this.NextButton.Margin.Right)
                    && newTopl > this.NextButton.Margin.Top && newTopl < (480 - this.NextButton.Margin.Bottom))
                {
                    NextButton.Hovering();
                    return;
                }
                else
                {
                    NextButton.Release();
                }

            }

            if (this.BackButton.Visibility == System.Windows.Visibility.Visible && this.MaskedColor.ActualHeight > 0)
            {

                if (newLeftl > this.BackButton.Margin.Left && newLeftl < (640 - this.BackButton.Margin.Right)
                    && newTopl > this.BackButton.Margin.Top && newTopl < (480 - this.BackButton.Margin.Bottom))
                {
                    BackButton.Hovering();

                    return;
                }
                else
                {

                    BackButton.Release();
                }

            }

            if (this.SpeakSentenceButton.Visibility == System.Windows.Visibility.Visible && this.MaskedColor.ActualHeight > 0)
            {

                if (newLeft > this.SpeakSentenceButton.Margin.Left && newLeft < (640 - this.SpeakSentenceButton.Margin.Right)
                    && newTop > this.SpeakSentenceButton.Margin.Top && newTop < (480 - this.SpeakSentenceButton.Margin.Bottom))
                {
                    SpeakSentenceButton.Hovering();
                    return;
                }
                else
                {
                    SpeakSentenceButton.Release();
                }
                
            }

            if (this.SOSButton.Visibility == System.Windows.Visibility.Visible && this.MaskedColor.ActualHeight > 0)
            {

                if (newLeft > this.SOSButton.Margin.Left && newLeft < (640 - this.SOSButton.Margin.Right)
                    && newTop > this.SOSButton.Margin.Top && newTop < (480 - this.SOSButton.Margin.Bottom))
                {
                    SOSButton.Hovering();
                    return;
                }
                else
                {
                    SOSButton.Release();
                }

            }
        

            /*
                        //=========================
                        //這段是為了測試，看左右手掌位置
                        LeftHand.X1 = cpl.X;
                        LeftHand.X2 = cpl.X + 10;
                        LeftHand.Y1 = cpl.Y;
                        LeftHand.Y2 = cpl.Y + 10;

                        RightHand.X1 = cpr.X;
                        RightHand.X2 = cpr.X + 10;
                        RightHand.Y1 = cpr.Y;
                        RightHand.Y2 = cpr.Y + 10;

                        LeftHand.Visibility = Visibility.Visible;
                        RightHand.Visibility = Visibility.Visible;
                        //=========================
             */
        }

        public void cleanKinect()
        {
            if (this.Kinect != null)
            {
                this.stopAllFrames();
                this.stopBackgroundJob();
                this.Kinect.Stop();
                this.Kinect = null;
            }
        }
    }
}
