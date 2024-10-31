using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNConnector
{
    internal class ThreadExistsException : Exception
    {
        public ThreadExistsException(string message = "Thread already created. Use ThreadCollosionActions.REPLASE or ThreadCollosionActions.APPEND to avoid mistakes") : base(message)
        { }
    }


    internal class ThreadDoesNotExistsException : Exception
    {
        public ThreadDoesNotExistsException(string message = "There is no thread with the specified name") : base(message)
        { }
    }
}
