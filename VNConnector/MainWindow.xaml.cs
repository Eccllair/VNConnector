using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            threadDispatcher = new TaskDispatcher();

            string pwd = Passwd.Generate();
            Task.Run(() => CreatePassword(pwd));
            pwd_TextBox.Dispatcher.Invoke(() => pwd_TextBox.Text = pwd);

            //TODO run from dispathcer
            Task.Run(Update);
        }

        //TODO переделать на Tasks
        TaskDispatcher threadDispatcher;

        private void CreatePassword(string pwd = null)
        {
            try
            {
                VNC.SetPassword(pwd);
                Task.Run(() => new Notification("пароль успешно изменен").Popup(pwd_label_message_holder));
            }
            catch (Win32Exception w)
            {
                Task.Run(() => {
                    Notification error = new Notification(w.Message);
                    error.Color = Brushes.Red;
                    error.Popup(pwd_label_message_holder);
                });
            }
            catch (Exception)
            {
                Task.Run(() => {
                    Notification error = new Notification("не удалось задать пароль");
                    error.Color = Brushes.Red;
                    error.Popup(pwd_label_message_holder);
                });
            }
        }

        private void ShowIP()
        {
            //TODO обработать ошибку отсутствующего подключения
            //TODO TaskDispatcher
            ip_TextBox.Dispatcher.Invoke(() =>
            {
                string ip = HostActions.GetLocalIPAddress();
                ip_TextBox.Text = (ip != "127.0.0.1" && ip != "0.0.0.0") ? ip : "-";
            });
        }

        public void Update()
        {
            while (true)
            {
                //TODO get VNC port
                //TODO исправить ошибки из консоли
                VNCStatuses status = VNC.GetStatus();

                StatusEllipse.Dispatcher.Invoke(() => UIActions.UpdateStatusEllipse(StatusEllipse, status));
                StatusLabel.Dispatcher.Invoke(() => UIActions.UpdateStatusLabel(StatusLabel, status));
                VNCSwitchButton.Dispatcher.Invoke(() => UIActions.UpdateStatusButton(VNCSwitchButton, status));
                VNCSwitchButton.Dispatcher.Invoke(() => Users_ListView.ItemsSource = VNC.GetClients());
                ShowIP();

                Thread.Sleep(300);
            }
        }

        private void VNCSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO не запускать новый процесс, сели предыдущий ещё запущен
            VNCStatuses prev_status = VNC.GetStatus();
            Task StatusChangeCheck = new Task(() =>
            {
                while (VNC.GetStatus() == prev_status) { Thread.Sleep(300); }
            });

            Loading loading = new Loading(VNCSwitchLoading_Image);
            loading.TrackedTasks = new List<Task>() { StatusChangeCheck };
            Task.Run(loading.Start);

            switch (VNC.GetStatus())
            {
                case VNCStatuses.ENABLED:
                    VNC.Close();
                    break;
                case VNCStatuses.DISABLED:
                    VNC.Open();
                    break;
            }
            StatusChangeCheck.Start();
        }

        private void change_pwd_Button_Click(object sender, RoutedEventArgs e)
        {
            Loading loading = new Loading(pwdLoading_Image);
            Task reload_VNC_task = new Task(VNC.Reload);
            Task change_pwd_task = new Task(() =>
            {
                CreatePassword(pwd_TextBox.Text);
                reload_VNC_task.Start();
            });
            loading.TrackedTasks = new List<Task> { change_pwd_task, reload_VNC_task };
            Task.Run(loading.Start);
            pwd_TextBox.Dispatcher.Invoke(change_pwd_task.RunSynchronously);
        }

        private void disconnect_client(object sender, RoutedEventArgs e)
        {
            VNC.DisconnectClient(Int32.Parse((string)((Button)sender).Tag));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            VNC.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TODO переделать на Task
            threadDispatcher.CloseAll();
        }
    }
}
