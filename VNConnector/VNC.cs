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
using System.Threading;

namespace VNConnector
{
    internal class VNC
    {

        public static void Open()
        {
            //TODO обработка ошибки: файл не найден
            Process.Start($"{Config.AppPath}\\winvnc.exe");
        }

        public static void Close()
        {
            Process.Start($"{Config.AppPath}\\winvnc.exe", "-kill");
        }

        public static void Reload()
        {
            Close();
            while (GetStatus() != VNCStatuses.DISABLED) { Thread.Sleep(300); }
            Open();
        }

        public static VNCStatuses GetStatus()
        {
            Process[] vnc = Process.GetProcessesByName("winvnc");
            if (vnc.Length > 0) return VNCStatuses.ENABLED;
            return VNCStatuses.DISABLED;
        }

        public static void SetPassword(string pwd)
        {
            Process.Start($"{Config.AppPath}\\createpassword", $"-secure {pwd}");
        }

        public static List<TCPHelper.TcpRow> GetClients()
        {
            return TCPHelper.GetExtendedTcpTable().Where(tcp => tcp.LocalPort == Config.VNCPort).ToList();
        }

        public static void DisconnectClient(int Pid)
        {
            Process.GetProcessById(Pid).Kill();
        }
    }
}
