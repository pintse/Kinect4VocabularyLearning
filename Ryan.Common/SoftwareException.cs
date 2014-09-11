using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.Common
{
    public class SoftwareException : Exception
    {
        public SoftwareException(string message)
            : base(message)
        {

        }
    }
}
