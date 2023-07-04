///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP
// Author: Aanand Narayanan
// Copyright (c) 2006-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace XNAQ3Lib.Q3BSP
{
    public class Q3BSPLogger
    {
        private StreamWriter sw = null;

        public Q3BSPLogger(string fileName)
        {
#if DEBUG
            sw = new StreamWriter(fileName, false, Encoding.ASCII);

            sw.WriteLine("[" + DateTime.Now.ToString() + "] Logging started.");

            sw.Flush();
#endif
        }

        public void WriteLine(string oneLine)
        {

            if (null != sw)
            {
#if DEBUG
                sw.WriteLine(oneLine);

                sw.Flush();
#endif
            }

        }
    }
}
