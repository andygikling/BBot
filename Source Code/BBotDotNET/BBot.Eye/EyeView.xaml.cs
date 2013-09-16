using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Expression.Encoder.Devices;
using WebcamControl;
using System.IO;
using System.Drawing.Imaging;

namespace BBot
{
    //Code adapted from: 
    // http://www.codeproject.com/Articles/285964/WPF-Webcam-Control
    // Thanks for the help!

    /// <summary>
    /// Interaction logic for CameraView.xaml
    /// </summary>
    public partial class EyeView
    {

        public EyeView()
        {
            InitializeComponent();

            // Bind the Video and Audio device properties of the
            // Webcam control to the SelectedValue property of 
            // the necessary ComboBox.
            Binding bndg_1 = new Binding("SelectedValue");
            bndg_1.Source = VidDvcsComboBox;
            WebCamCtrl.SetBinding(Webcam.VideoDeviceProperty, bndg_1);

            Binding bndg_2 = new Binding("SelectedValue");
            bndg_2.Source = AudDvcsComboBox;
            WebCamCtrl.SetBinding(Webcam.AudioDeviceProperty, bndg_2);

            // Create directory for saving video files.
            string vidPath = @"C:\VideoClips";

            if (Directory.Exists(vidPath) == false)
            {
                Directory.CreateDirectory(vidPath);
            }

            // Create directory for saving image files.
            string imgPath = @"C:\WebcamSnapshots";

            if (Directory.Exists(imgPath) == false)
            {
                Directory.CreateDirectory(imgPath);
            }

            // Set some properties of the Webcam control
            WebCamCtrl.VideoDirectory = vidPath;
            WebCamCtrl.VidFormat = VideoFormat.mp4;

            WebCamCtrl.ImageDirectory = imgPath;
            WebCamCtrl.PictureFormat = ImageFormat.Jpeg;

            WebCamCtrl.FrameRate = 30;
            WebCamCtrl.FrameSize = new System.Drawing.Size(266, 200);

            // Find a/v devices connected to the machine.
            FindDevices();

            VidDvcsComboBox.SelectedIndex = 0;
            AudDvcsComboBox.SelectedIndex = 0;

        }

        private void FindDevices()
        {
            var vidDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video);
            var audDevices = EncoderDevices.FindDevices(EncoderDeviceType.Audio);

            foreach (EncoderDevice dvc in vidDevices)
            {
                VidDvcsComboBox.Items.Add(dvc.Name);
            }

            foreach (EncoderDevice dvc in audDevices)
            {
                AudDvcsComboBox.Items.Add(dvc.Name);
            }

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Display webcam video on control.
                WebCamCtrl.FrameSize = new System.Drawing.Size(450, 310);
                WebCamCtrl.StartCapture();
            }
            catch (Microsoft.Expression.Encoder.SystemErrorException ex)
            {
                MessageBox.Show("Device is in use by another application");
            }
        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop the display of webcam video.
            WebCamCtrl.StopCapture();
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            // Start recording of webcam video to harddisk.
            WebCamCtrl.StartRecording();
        }

        private void StopRecordButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop recording of webcam video to harddisk.
            WebCamCtrl.StopRecording();
        }

        private void SnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            // Take snapshot of webcam image.
            WebCamCtrl.TakeSnapshot();
        }



    }
}
