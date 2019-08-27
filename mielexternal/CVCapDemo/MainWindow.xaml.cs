using CommonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using CVCap;

namespace CVCapDemo
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HelperLib.DisableMaximaizeButton(Window.GetWindow(this));
            HelperLib.DisableMinimaizeButton(Window.GetWindow(this));
        }

        private void ImageCapture_Click(object sender, RoutedEventArgs e)
        {
            //HelperLib.CreateFolder("C:/CVCap/test_today/3001");
            string filename = string.Format("BangCap_{0}.jpg", DateTime.Now.ToString("yyyyMMdd_hhmmss_fff"));
            //Debug.WriteLine(filename);

            HelperLib.Capture("C:\\CVCap", filename, true);
        }

        private void AVICapture_Click(object sender, RoutedEventArgs e)
        {
            if (Recorder.Instance.IsRecording == false)
            {
                string filename = string.Format("BangCap_{0}.avi", DateTime.Now.ToString("yyyyMMdd_hhmmss_fff"));
                if (Recorder.Instance.Start("C:\\CVCap\\Media", filename, "XVID", 15, 1280, 720) == true)
                {
                    //MessageBox.Show("레코딩 시작");
                    AVICapture.Content = "Stop R";
                }
                else
                {
                    MessageBox.Show("레코딩 시작 실패");
                }
            }
            else
            {
                if (Recorder.Instance.Stop() == true)
                {
                    MessageBox.Show("레코딩 종료");
                    AVICapture.Content = "Start R";
                }
                else
                {
                    MessageBox.Show("레코딩 종료 실패");
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Recorder.Instance.Stop();
        }
    }
}
