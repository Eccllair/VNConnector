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

namespace VNConnector
{
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

        public static void ShowMessage(Label message_holder, string message, MessageTypes type = MessageTypes.INFO)
        {
            System.Windows.Visibility prev_visibility = message_holder.Visibility;
            message_holder.Foreground = MessageColor[type];
            message_holder.Content = message;
            message_holder.Visibility = System.Windows.Visibility.Visible;
            Thread.Sleep(3000);
            message_holder.Visibility = prev_visibility;
        }
        
        public static void ChangeStatusEllipse(Ellipse ellipse, VNCStatuses status)
        {
            ellipse.Fill = StatusColor[status];
            ellipse.Stroke = StatusStrokeColor[status];
        }

        public static void ChangeStatusLabel(Label label, VNCStatuses status)
        {
            label.Content = StatusLabelText[status];
        }

        public static void ChangeStatusButton(Button label, VNCStatuses status)
        {
            label.Content = StatusTextBoxText[status];
        }

        public static void ChangeIPTextBox(TextBox textBox, string ip)
        {
            textBox.Text = ip;
        }

        public static void Loading(Image loading_image, Thread thread)
        {
            while (thread.ThreadState != ThreadState.Stopped)
            {
                loading_image.Dispatcher.Invoke((Action)(() =>
                {
                    int angle = 0;
                    RotateTransform rotateTransform = new RotateTransform(angle);
                    loading_image.RenderTransform = rotateTransform;
                    angle++;
                }));
                Thread.Sleep(300);
            }
        }
    }
}
