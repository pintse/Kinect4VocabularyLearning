using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Ryan.Kinect.Toolkit.VO
{
    /// <summary>
    /// 關節點Value Object
    /// </summary>
    public class JointInfo
    {
        public JointInfo(JointType name)
        {
            this.Name = name;
        }
        public JointType Name { get; private set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public int Depth { get; set; }
        public int X2D { get; set; }
        public int Y2D { get; set; }
        //private String realName;
        public String RealName
        {
            get
            {
                if (Name == JointType.HandLeft)
                {
                    return "左手";
                }
                else if (Name == JointType.HandRight)
                {
                    return "右手";
                }
                else
                {
                    return Name.ToString();
                }
            

            }
        }
    }
}
