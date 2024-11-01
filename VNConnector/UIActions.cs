using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Windows.Shapes;
using System.Net;
using System.Windows;

namespace VNConnector
{
    internal class Notification
    {
        public Notification(string Text) 
        {
            this.Text = Text;
        }

        public string Text { get; set; }
        public int LifeTime = 3000;
        public SolidColorBrush Color = Brushes.Blue;

        public void Popup(Label label)
        {
            Visibility prev_visibility = label.Visibility;
            label.Dispatcher.Invoke(() => {
                label.Foreground = Color;
                label.Content = Text;
                label.Visibility = Visibility.Visible;
            });
            Thread.Sleep(LifeTime);
            label.Dispatcher.Invoke(() => label.Visibility = prev_visibility);
        }
    }

    public class Loading
    {
        public Loading(Image Source)
        {
            this.Source = Source;
        }

        public Image Source { get; set; }

        public int RotationDegree = 30;
        public int TimeDelta = 150;
        public List<Task> TrackedTasks = null;

        private bool Finish = false;


        public void Start()
        {
            int angle = 0;
            Visibility prev_visibility = Source.Visibility;
            Source.Dispatcher.Invoke(() => {
                Source.Visibility = Visibility.Visible;
                Source.RenderTransformOrigin = new Point(0.5, 0.5);
            });
            while (!(TrackedTasks.Where(TrackedTask => TrackedTask?.IsCompleted == true).Count() == TrackedTasks.Count()) && !Finish)
            {
                angle = (angle + RotationDegree) % 360;
                Source.Dispatcher.Invoke(() =>
                {
                    Source.RenderTransform = new RotateTransform(angle);
                });
                Thread.Sleep(TimeDelta);
            }
            Source.Dispatcher.Invoke(() => Source.Visibility = prev_visibility);
        }

        public void Stop() { Finish = true; }
    }

    internal class UIActions
    {
        static private Dictionary<MessageTypes, SolidColorBrush> MessageColor = new Dictionary<MessageTypes, SolidColorBrush>()
        {
            { MessageTypes.INFO, Brushes.Blue },
            { MessageTypes.WARNING, Brushes.Yellow },
            { MessageTypes.ERROR, Brushes.Red },
        };

        static private Dictionary<VNCStatuses, SolidColorBrush> StatusColor = new Dictionary<VNCStatuses, SolidColorBrush>()
        {
            { VNCStatuses.ENABLED, Brushes.Green },
            { VNCStatuses.DISABLED, Brushes.Red },
        };

        static private Dictionary<VNCStatuses, SolidColorBrush> StatusStrokeColor = new Dictionary<VNCStatuses, SolidColorBrush>()
        {
            { VNCStatuses.ENABLED, Brushes.DarkGreen },
            { VNCStatuses.DISABLED, Brushes.DarkRed },
        };

        static private Dictionary<VNCStatuses, string> StatusLabelText = new Dictionary<VNCStatuses, string>()
        {
            { VNCStatuses.ENABLED, "доступ открыт" },
            { VNCStatuses.DISABLED, "доступ закрыт" },
        };

        static private Dictionary<VNCStatuses, string> StatusTextBoxText = new Dictionary<VNCStatuses, string>()
        {
            { VNCStatuses.ENABLED, "закрыть доступ" },
            { VNCStatuses.DISABLED, "открыть доступ" },
        };

        public static void UpdateStatusEllipse(Ellipse ellipse, VNCStatuses status)
        {
            ellipse.Fill = StatusColor[status];
            ellipse.Stroke = StatusStrokeColor[status];
        }

        public static void UpdateStatusLabel(Label label, VNCStatuses status)
        {
            label.Content = StatusLabelText[status];
        }

        public static void UpdateStatusButton(Button label, VNCStatuses status)
        {
            label.Content = StatusTextBoxText[status];
        }

        public static void UpdateClientList(StackPanel panel, List<TCPHelper.TcpRow> userList)
        {
            //UserList;
        }
    }
}
