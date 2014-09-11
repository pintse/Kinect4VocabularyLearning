using System;
using System.Linq;
using System.IO;
using Microsoft.Kinect;

namespace Kinect.Toolbox
{
    public class TemplatedGestureDetector : GestureDetector  //Ryan:Template based search作法
    {
        public float Epsilon { get; set; }
        public float MinimalScore { get; set; }
        public float MinimalSize { get; set; }
        readonly LearningMachine learningMachine;
        RecordedPath path;
        readonly string gestureName;

        public bool IsRecordingPath
        {
            get { return path != null; }
        }

        public LearningMachine LearningMachine
        {
            get { return learningMachine; }
        }

        public TemplatedGestureDetector(string gestureName, Stream kbStream, int windowSize = 60)
            : base(windowSize)
        {
            Epsilon = 0.035f;
            MinimalScore = 0.80f;
            MinimalSize = 0.1f;
            this.gestureName = gestureName;
            learningMachine = new LearningMachine(kbStream);
        }

        public override void Add(SkeletonPoint position, KinectSensor sensor)
        {
            base.Add(position, sensor);  //Ryan:劃軌跡和清軌跡，最後呼叫LookForGesture
            //Console.WriteLine("Ryan::TemplatedGestureDetector.Add(SkeletonPoint position, KinectSensor sensor)");
            if (path != null)
            {
                Console.WriteLine("Ryan::TemplatedGestureDetector.Add(SkeletonPoint position, KinectSensor sensor)::(path != null)");
                path.Points.Add(position.ToVector2());  //(path!=null)表示開啟了Capture Gesture功能，所以這裡紀錄目前這個frame的相關的節點位置
            }
        }

        protected override void LookForGesture()  //Ryan:Template based search作法
        {  //進行手勢比對，得到結果。這個比對每個frame都會進來比對，也就是無時無刻都會進來依照現在的位置（猜測應該還有含之前走過的點（已經在先前的frame比對時先被紀錄下來））比對
            //Console.WriteLine("Ryan::TemplatedGestureDetector.LookForGesture()");
            if (LearningMachine.Match(Entries.Select(e => new Vector2(e.Position.X, e.Position.Y)).ToList(), Epsilon, MinimalScore, MinimalSize))  //Ryan:成立，表示有找到符合的手勢
            {
                Console.WriteLine("Ryan::TemplatedGestureDetector.LookForGesture() true");
                RaiseGestureDetected(gestureName);
            }
        }

        public void StartRecordTemplate()
        {
            path = new RecordedPath(WindowSize);
        }

        public void EndRecordTemplate()
        {
            Console.WriteLine("Ryan::TemplatedGestureDetector.EndRecordTemplate()::path::" + path);
            LearningMachine.AddPath(path);
            path = null;
        }

        public void SaveState(Stream kbStream)
        {
            LearningMachine.Persist(kbStream);
        }
    }
}
