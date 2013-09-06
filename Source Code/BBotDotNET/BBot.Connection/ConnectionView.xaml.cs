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
    /// Interaction logic for ConnectionBox.xaml
    /// </summary>
    public partial class ConnectionView : UserControl
    {
   

        public ConnectionView()
        {
            InitializeComponent();

            this.lbl_COMPortStatus.Visibility = System.Windows.Visibility.Hidden;

        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            ConnectionViewModel m = (ConnectionViewModel)DataContext;
            m.ComPortConnected = true;
            lbl_COMPortStatus.Visibility = System.Windows.Visibility.Visible;

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            ConnectionViewModel m = (ConnectionViewModel)DataContext;
            m.ComPortConnected = false;
            lbl_COMPortStatus.Visibility = System.Windows.Visibility.Hidden;
        }

     
    }
	

}
