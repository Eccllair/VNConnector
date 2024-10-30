using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VNConnector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void CreatePassword(string pwd = null)
        {
            string passwd = pwd;
            if (passwd == null) passwd = Passwd.Generate();
            pwd_TextBox.Text = passwd;
            try
            {
                Passwd.Set(passwd);
            }
            catch (Exception)
            {
                UIActions.ShowMessage(pwd_label_message_holder, "не удалось задать пароль", MessageTypes.ERROR);
                //TODO запись в лог
            }
        }

        private void ShowIP()
        {
            //TODO обработать ошибку отсутствующего подключения
            UIActions.ChangeIPTextBox(ip_TextBox, HostActions.GetLocalIPAddress());
        }


        public void Update()
        {
            while (true)
            {
                VNCStatuses status = VNC.GetStatus();
                //TODO нормальное завершение задачи
                StatusEllipse.Dispatcher.Invoke((Action)(() =>
                {
                UIActions.ChangeStatusEllipse(StatusEllipse, status);
                }));
                StatusLabel.Dispatcher.Invoke((Action)(() =>
                {
                UIActions.ChangeStatusLabel(StatusLabel, status);
                }));
                VNCSwitchButton.Dispatcher.Invoke((Action)(() =>
                {
                    UIActions.ChangeStatusButton(VNCSwitchButton, status);
                }));
                //IEnumerable<TcpConnectionInformation> ts = VNC.GetClients();
                Thread.Sleep(300);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            CreatePassword(); //TODO parralel and loading
            ShowIP(); //TODO parralel and loading
            new Thread(() => { Update(); }).Start();
        }
        
        private void VNCSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            switch (VNC.GetStatus())
        {
                case VNCStatuses.ENABLED:
                    VNC.Close();
                    break;
                case VNCStatuses.DISABLED:
                    VNC.Open();
                    break;
            }
        }

        private void change_pwd_Button_Click(object sender, RoutedEventArgs e)
        {
            Thread changePwdThread = new Thread(() => { CreatePassword(pwd_TextBox.Text); });
            new Thread(() => { UIActions.Loading(pwdLoading_Image, changePwdThread); }).Start();
        }
    }
}
