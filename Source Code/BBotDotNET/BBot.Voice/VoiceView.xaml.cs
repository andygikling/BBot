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

namespace BBot
{
    /// <summary>
    /// Interaction logic for VoiceBox.xaml
    /// </summary>
    public partial class VoiceView : UserControl
    {
         
        public VoiceView()
        {
            InitializeComponent();
        }                   

        private void btnSpeak_Click(object sender, RoutedEventArgs e)
        {
            VoiceViewModel v = (VoiceViewModel)DataContext;
            v.SendCurrentSentence(this.txtSentence.Text);
        }

        private void btn_Voice_Stop_Click(object sender, RoutedEventArgs e)
        {
            VoiceViewModel v = (VoiceViewModel)DataContext;
            v.StopTalking();
        }

        private void btn_Voice_SendSettings_Click(object sender, RoutedEventArgs e)
        {
            VoiceViewModel v = (VoiceViewModel)DataContext;
            v.SendSettings();
        }


    }


}
