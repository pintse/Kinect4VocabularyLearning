using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using Microsoft.Kinect;
using log4net;

namespace Kinect.Toolbox
{
    public abstract class GestureDetector
    {
        private static ILog log = LogManager.GetLogger(typeof(GestureDetector));

        public int MinimalPeriodBetweenGestures { get; set; }

        readonly List<Entry> entries = new List<Entry>();

        public event Action<string> OnGestureDetected;

        DateTime lastGestureDate = DateTime.Now;

        readonly int windowSize; // Number of recorded positions

        // For drawing
        public Canvas DisplayCanvas
        {
            get;
            set;
        }

        public Color DisplayColor
        {
            get;
            set;
        }

        protected GestureDetector(int windowSize = 20)
        {
            this.windowSize = windowSize;
            MinimalPeriodBetweenGestures = 0;
            DisplayColor = Colors.Red;
        }

        protected List<Entry> Entries
        {
            get { return entries; }
        }

        public int WindowSize
        {
            get { return windowSize; }
        }

        public virtual void Add(SkeletonPoint position, KinectSensor sensor)
        {
            Entry newEntry = new Entry {Position = position.ToVector3(), Time = DateTime.Now};
            Entries.Add(newEntry);

            // Drawing
            if (DisplayCanvas != null)
            {
                newEntry.DisplayEllipse = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    StrokeThickness = 2.0,
                    Stroke = new SolidColorBrush(DisplayColor),
                    StrokeLineJoin = PenLineJoin.Round
                };

                Vector2 vector2 = Tools.Convert(sensor, position);

                float x = (float)(vector2.X * DisplayCanvas.ActualWidth);
                float y = (float)(vector2.Y * DisplayCanvas.ActualHeight);

                Canvas.SetLeft(newEntry.DisplayEllipse, x - newEntry.DisplayEllipse.Width / 2);
                Canvas.SetTop(newEntry.DisplayEllipse, y - newEntry.DisplayEllipse.Height / 2);

                DisplayCanvas.Children.Add(newEntry.DisplayEllipse);  //Ryan:Mark 這一行畫圈圈的紅點軌跡不會出現
            }

            // Remove too old positions
            if (Entries.Count > WindowSize)
            {
                Entry entryToRemove = Entries[0];
                
                if (DisplayCanvas != null)
                {
                    DisplayCanvas.Children.Remove(entryToRemove.DisplayEllipse);  //Ryan:Mark 這一行，已在畫面上產生的軌跡紅點，若不是因為畫圈圈的過程產生的，就不會被清除。有這一行，軌跡紅點會依先進先出的順序自動清除
                }

                //log.Debug(entryToRemove.Time + "::" + entryToRemove.Position.X + "," + entryToRemove.Position.Y + "," + entryToRemove.Position.Z);
                Entries.Remove(entryToRemove);
            }

            // Look for gestures
            LookForGesture();
        }

        protected abstract void LookForGesture();

        protected void RaiseGestureDetected(string gesture)
        {
            Console.WriteLine("辨識到手勢：" + gesture);
            // Too close?
            if (DateTime.Now.Subtract(lastGestureDate).TotalMilliseconds > MinimalPeriodBetweenGestures)
            {
                if (OnGestureDetected != null)
                {
                    Console.WriteLine("辨識到手勢並通知事件：" + gesture);
                    OnGestureDetected(gesture);
                }

                lastGestureDate = DateTime.Now;
            }

            Entries.ForEach(e=>
                                {
                                    if (DisplayCanvas != null)
                                        DisplayCanvas.Children.Remove(e.DisplayEllipse);
                                });
            Entries.Clear();
        }
    }
}