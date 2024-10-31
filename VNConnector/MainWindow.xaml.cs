using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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
using static System.Net.Mime.MediaTypeNames;

namespace VNConnector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            threadDispatcher = new ThreadDispatcher();

            CreatePassword(); //TODO parralel and loading
            ShowIP(); //TODO parralel and loading

            threadDispatcher.AddThread(new Thread(() => { Update(); }), "Update");
            threadDispatcher.Start("Update");
        }

        ThreadDispatcher threadDispatcher;

        private void CreatePassword(string pwd = null)
        {
            string passwd = pwd;
            if (passwd == null) passwd = Passwd.Generate();
            pwd_TextBox.Text = passwd;
            try
            {
                Passwd.Set(passwd);
                UIActions.ShowMessage(pwd_label_message_holder, "пароль успешно изменен", MessageTypes.INFO);
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

                ThreadDispatcher.StartUIAction(StatusEllipse ,() => UIActions.ChangeStatusEllipse(StatusEllipse, status));
                ThreadDispatcher.StartUIAction(StatusLabel, () => UIActions.ChangeStatusLabel(StatusLabel, status));
                ThreadDispatcher.StartUIAction(VNCSwitchButton, () => UIActions.ChangeStatusButton(VNCSwitchButton, status));

                IEnumerable<TcpConnectionInformation> ts = VNC.GetClients();

                Thread.Sleep(300);
            }
        }

        private void VNCSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO ProcessDispatcher
            //TODO не запускать новый процесс, сели предыдущий ещё запущен
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
            //stop here
            threadDispatcher.AddThread(new Thread(() => UIActions.ShowMessage(pwd_label_message_holder, "тест", MessageTypes.INFO)),"test");
            threadDispatcher.Start("test");
            try
            {
                threadDispatcher.AddThread(new Thread(() =>
                {
                    Visibility prev_visibility = pwdLoading_Image.Visibility;
                    ThreadDispatcher.StartUIAction(pwdLoading_Image, () => pwdLoading_Image.Visibility = Visibility.Visible);
                    int angle = 0;
                    while (threadDispatcher.GetThread("pwd_Change")?.ThreadState != ThreadState.Stopped)
                    {
                        angle = (angle + 30) % 360;
                        ThreadDispatcher.StartUIAction(pwdLoading_Image, () =>
                        {
                            pwdLoading_Image.RenderTransformOrigin = new Point(0.5, 0.5);
                            pwdLoading_Image.RenderTransform = new RotateTransform(angle);
                        });
                        Thread.Sleep(150);
                    }
                    ThreadDispatcher.StartUIAction(pwdLoading_Image, () => pwdLoading_Image.Visibility = prev_visibility);
                }), "Loading");
                threadDispatcher.Start("Loading");
            }
            catch (ThreadExistsException) { }
            try
            {
                threadDispatcher.AddThread(new Thread(() =>
                {
                    //TODO убрать костыль
                    Thread.Sleep(150);
                    ThreadDispatcher.StartUIAction(pwd_TextBox, () => CreatePassword(pwd_TextBox.Text));
                }), "pwd_Change");
                //threadDispatcher.Start("pwd_Change");
            }
            catch (ThreadExistsException) { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            threadDispatcher.CloseAll();
        }
    }
}
