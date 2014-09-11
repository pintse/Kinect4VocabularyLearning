using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.ObjectRecognition.SURF
{
    public class IPoint
    {
        public float x, y;

        public float scale;

        public float response;

        public float orientation;

        public int laplacian;

        public int descriptorLength;
        public float[] descriptor = null;

        public IPoint()
        {
            orientation = 0;
        }

        public void SetDescriptorLength(int Size)
        {
            descriptorLength = Size;
            descriptor = new float[Size];
        }

    }//class
}//namespace

