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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LegsView : UserControl
    {
        public LegsView()
        {
            InitializeComponent();
        }

        private void btn_WalkForward_Click( object sender, RoutedEventArgs e )
        {
            LegsViewModel v = ( LegsViewModel ) DataContext;
            v.LegsModel.Throttle_Up();
        }

        private void btn_WalkBackward_Click( object sender, RoutedEventArgs e )
        {
            LegsViewModel v = ( LegsViewModel ) DataContext;
            v.LegsModel.Throttle_Down();
        }

        private void btn_WalkLeft_Click( object sender, RoutedEventArgs e )
        {
            LegsViewModel v = ( LegsViewModel ) DataContext;
            v.LegsModel.Throttle_Left();
        }

        private void btn_WalkRight_Click( object sender, RoutedEventArgs e )
        {
            LegsViewModel v = ( LegsViewModel ) DataContext;
            v.LegsModel.Throttle_Right();
        }

        private void btn_WalkStop_Click( object sender, RoutedEventArgs e )
        {
            LegsViewModel v = ( LegsViewModel ) DataContext;
            v.LegsModel.Throttle_Stop();
        }

      
    }
}
