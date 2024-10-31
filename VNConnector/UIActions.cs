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
            Visibility prev_visibility = message_holder.Visibility;
            ThreadDispatcher.StartUIAction(message_holder, () => {
                message_holder.Foreground = MessageColor[type];
                message_holder.Content = message;
                message_holder.Visibility = Visibility.Visible;
            });
            Thread.Sleep(3000);
            ThreadDispatcher.StartUIAction(message_holder, () => message_holder.Visibility = prev_visibility);
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

            int angle = 0;
            Visibility prev_visibility = loading_image.Visibility;
            loading_image.Visibility = Visibility.Visible;
            while (thread.ThreadState != ThreadState.Stopped && thread.ThreadState != ThreadState.WaitSleepJoin)
            {
                ThreadState test = thread.ThreadState;
                loading_image.Dispatcher.Invoke(() =>
                {
                    loading_image.RenderTransformOrigin = new Point(0.5, 0.5);
                    loading_image.RenderTransform = new RotateTransform(angle);
                });
                Thread.Sleep(300);
                angle += 32;
            }
            loading_image.Visibility = prev_visibility;
        }
    }
}
