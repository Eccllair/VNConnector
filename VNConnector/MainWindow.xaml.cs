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
            taskDispatcher = new TaskDispatcher();

            string pwd = Passwd.Generate();
            taskDispatcher.Run(new Task(() => CreatePassword(pwd)), "ChangePwd");
            pwd_TextBox.Dispatcher.Invoke(() => pwd_TextBox.Text = pwd);

            //TODO run from dispathcer
            taskDispatcher.Run(new Task(Update), "update");
        }

        //TODO переделать на Tasks
        TaskDispatcher taskDispatcher;

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
                Users_ListView.Dispatcher.Invoke(() => Users_ListView.ItemsSource = VNC.GetClients());
                ShowIP();

                Thread.Sleep(300);
            }
        }

        private void VNCSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (taskDispatcher.TasksExists("VNSSwitch"))
            {
                if (taskDispatcher.TasksCompleted("VNSSwitch")) taskDispatcher.DeleteCompleted("VNSSwitch");
                else return;
            }
            taskDispatcher.Run(() =>
            {
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
                StatusChangeCheck.RunSynchronously();
            }, "VNSSwitch");

    }

        private void change_pwd_Button_Click(object sender, RoutedEventArgs e)
        {
            if (taskDispatcher.TasksExists("ChangePwd"))
            {
                if (taskDispatcher.TasksCompleted("ChangePwd")) taskDispatcher.DeleteCompleted("ChangePwd");
                else return;
            }
            taskDispatcher.Run(() =>
            {
                Task reload_VNC_task = null;
                List<Task> task_list = new List<Task>();
                bool VNCEnabled = VNC.GetStatus() == VNCStatuses.ENABLED;

                Loading loading = new Loading(pwdLoading_Image);
                if (VNCEnabled)
                {
                    reload_VNC_task = new Task(VNC.Reload);
                    task_list.Add(reload_VNC_task);
                }
                Task change_pwd_task = new Task(() =>
                {
                    CreatePassword(pwd_TextBox.Text);
                    if (VNCEnabled) reload_VNC_task.Start();
                });
                task_list.Add(change_pwd_task);

                loading.TrackedTasks = task_list;
                Task.Run(loading.Start);
                pwd_TextBox.Dispatcher.Invoke(change_pwd_task.RunSynchronously);
                change_pwd_task.Wait();
            }, "ChangePwd");
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
            StatusEllipse.Dispatcher.DisableProcessing();
            StatusLabel.Dispatcher.DisableProcessing();
            VNCSwitchButton.Dispatcher.DisableProcessing();
            Users_ListView.Dispatcher.DisableProcessing();
            pwd_TextBox.Dispatcher.DisableProcessing();
            ip_TextBox.Dispatcher.DisableProcessing();
            taskDispatcher.CloseAll();
        }
    }
}
