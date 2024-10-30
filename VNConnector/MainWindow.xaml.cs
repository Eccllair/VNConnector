using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace VNConnector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void CreatePassword()
        {
            //TODO добавить запуск скрипта setpassword
            string passwd = Passwd.Generate();
            try
            {
                //Passwd.Set(passwd);
                pwd.Text = passwd;
            }
            catch (Exception ex)
            {
                UIActions.ShowMessage(pwd_label_message_holder, "не удалось задать пароль", MessageTypes.ERROR);
                //TODO запись в 
            }
        }


        public void Update(bool IsActive)
        {

            while (true)
            {
                VNCStatuses status = VNC.GetStatus();
                UIActions.ChangeStatusEllipse(StatusEllipse, status);
                UIActions.ChangeStatusLabel(StatusLabel, status);
                VNC.GetClients();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            CreatePassword();
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreatePassword();
            Update(IsActive);
        }

        private void VNCSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            VNC.Open();
        }
    }
}
