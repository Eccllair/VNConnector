using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VNConnector
{
    public enum MessageTypes
    {
        INFO = 0,
        WARNING = 1,
        ERROR = 2,
    }

    public enum VNCStatuses
    {
        DISABLED = 0,
        ENABLED = 1,
    }
}
