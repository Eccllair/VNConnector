using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using System.Net.NetworkInformation;
using System.Net;

namespace VNConnector
{
    internal class VNC
    {

        public static void Open()
        {
            //TODO обработка ошибки: файл не найден
            Process.Start($"{Config.AppPath}\\VNC-bin\\winvnc.exe");
        }

        public static void Close()
        {
            Process.Start($"{Config.AppPath}\\VNC-bin\\winvnc.exe", "-kill");
        }

        public static VNCStatuses GetStatus()
        {
            Process[] vnc = Process.GetProcessesByName("winvnc");
            if (vnc.Length > 0) return VNCStatuses.ENABLED;
            return VNCStatuses.DISABLED;
        }

        public static IEnumerable<TcpConnectionInformation> GetClients()
        {
            
            IPGlobalProperties ip = IPGlobalProperties.GetIPGlobalProperties();
            IEnumerable<TcpConnectionInformation> connections = ip.GetActiveTcpConnections().Where(
                tcp => tcp.LocalEndPoint.Port == 5900 && tcp.State == TcpState.Established
            );
            return connections;
        }
    }
}
