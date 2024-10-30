using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNConnector
{
    internal class Passwd
    {
        private static Random random = new Random();

        public static string Generate()
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void Set(string pwd)
        {
            Process.Start($"{Config.AppPath}\\VNC-bin\\createpassword {pwd}");
        }
    }
}
