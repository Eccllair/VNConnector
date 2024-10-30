using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VNConnector
{
    public class Client
    {
        public string Addres { get; set; }
        private int Pid { get; set; }

        public Client(string addres, int pid) {
            Addres = addres;
            Pid = pid;
        }

        public void Disconnect()
        {
            Process.GetProcessById(Pid).Kill();
        }
    }
}
