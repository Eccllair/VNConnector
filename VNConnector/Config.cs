using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VNConnector
{
    public class Config
    {
        public static readonly string AppPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        public static int VNCPort = 5900;
    }
}
