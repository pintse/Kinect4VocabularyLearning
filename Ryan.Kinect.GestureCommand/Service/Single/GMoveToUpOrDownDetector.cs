﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;

namespace Ryan.Kinect.GestureCommand.Service.Single
{
    public class GMoveToUpOrDownDetector : GestureDetector  //Ryan:Algorithmic search作法
    {
        public float MoveMinimalLength { get; set; }
        public float MoveMaximalWidth { get; set; }
        public int MoveMininalDuration { get; set; }
        public int MoveMaximalDuration { get; set; }

        readonly string GestureName; 

        public GMoveToUpOrDownDetector(int windowSize = 20)
            : base(windowSize)
        {
            MoveMinimalLength = 0.25f;
            MoveMaximalWidth = 0.15f;
            MoveMininalDuration = 250;
            MoveMaximalDuration = 2500;
        }

        public GMoveToUpOrDownDetector(string gestureName, int windowSize = 20)
            : this(windowSize)
        {
            this.GestureName = gestureName;
        }

        public GMoveToUpOrDownDetector()
            : this(20) 
        {
        }

        protected bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction,
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!heightFunction(Entries[0].Position, Entries[index].Position) || !directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    if (this.GestureName == "GShoulderMoveToDown")
                    {
                        PostureDetector.Coordinate4Test = "GShoulderMoveToDown:重設";
                    }
                    start = index;
                }

                if (lengthFunction(Entries[index].Position, Entries[start].Position))
                {
                    if (this.GestureName == "GShoulderMoveToDown")
                    {
                        PostureDetector.Coordinate4Test = "GShoulderMoveToDown:長度OK";
                    }
                    double totalMilliseconds = (Entries[index].Time - Entries[start].Time).TotalMilliseconds;
                    if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void LookForGesture()  //Ryan:Algorithmic search作法
        {


            if (this.GestureName.EndsWith("Up"))
            {
                if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < MoveMaximalWidth, // Height //設定heightFunction的定義Func<Vector3, Vector3, bool>，第一個Vector3為p1,第二個Vector3為p2，bool為『Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight』的運算結果，將這樣的定義當作參數傳入ScanPositions中，ScanPositions內使用這個『有運算定義』的參數給予p1,p2的值，然後得到運算後的結果
                    (p1, p2) => p2.Y - p1.Y > 0.01f, // Progression to right
                    (p1, p2) => Math.Abs(p2.Y - p1.Y) > MoveMinimalLength, // Length
                    MoveMininalDuration, MoveMaximalDuration)) // Duration
                {
                    RaiseGestureDetected(this.GestureName);
                    return;
                }
            }


            if (this.GestureName.EndsWith("Down"))
            {

                if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < MoveMaximalWidth, // Height //設定heightFunction的定義Func<Vector3, Vector3, bool>，第一個Vector3為p1,第二個Vector3為p2，bool為『Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight』的運算結果，將這樣的定義當作參數傳入ScanPositions中，ScanPositions內使用這個『有運算定義』的參數給予p1,p2的值，然後得到運算後的結果
                    (p1, p2) => p2.Y - p1.Y < -0.01f, // Progression to down
                    (p1, p2) => Math.Abs(p2.Y - p1.Y) > 0.2, //MoveMinimalLength, // Length
                    MoveMininalDuration, MoveMaximalDuration)) // Duration
                {
                    RaiseGestureDetected(this.GestureName);
                    return;
                }
            }
        }
    }
}

