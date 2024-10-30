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
        public Button Button { get; set; }
        private string Pid { get; set; }

        public Client(string addres, string button, string pid) {
        
        }

        public static void Disconnect()
        {
            Process.Start($"{Config.AppPath}\\VNC-bin\\winvnc.exe -kill");
        }
    }
}
