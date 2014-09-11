using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Ryan.Kinect.GestureCommand.VO
{
    /// <summary>
    /// 手勢設計Value Object
    /// </summary>
    public class GesturePostureVO
    {
        public GesturePostureVO(GlobalData.GestureTypes id, string type, string name, string algorithm, List<GlobalData.GestureTypes> combinations, string detector, string gestureJoint, int epsilon, string commandFlg)
        {
            this.ID = id;
            this.Type = type;
            this.Name = name;
            this.Algorithm = algorithm;
            this.Combinations = combinations;
            this.Detector = detector;
            if (gestureJoint != null && gestureJoint != "")
            {
                this.GestureJoint = (JointType)Enum.Parse(typeof(JointType), gestureJoint, true);
            }

            this.Epsilon = epsilon;
            this.CommandFlg = commandFlg;
        }

        public GlobalData.GestureTypes ID
        {
            get;
            private set;
        }

        public string Type
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Detector
        {
            get;
            private set;
        }

        public string Algorithm
        {
            get;
            private set;
        }

        public JointType GestureJoint
        {
            get;
            private set;
        }

        public int Epsilon
        {
            get;
            private set;
        }

        public List<GlobalData.GestureTypes> Combinations
        {
            get;
            private set;
        }

        public string CommandFlg
        {
            get;
            private set;
        }
    }
}
