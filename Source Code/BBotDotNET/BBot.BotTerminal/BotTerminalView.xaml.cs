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
    public partial class BotTerminalControlView : UserControl
    {
        public BotTerminalControlView()
        {
            InitializeComponent();

            this.DataContextChanged += BotTerminalControlView_DataContextChanged;
        }

        void BotTerminalControlView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is BotTerminalViewModel)
            {
                (this.DataContext as BotTerminalViewModel).BotTerminalModel.PropertyChanged += BotTerminalModel_PropertyChanged;
            }
        }

        void BotTerminalModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Action a = () =>
                {
                    if (e.PropertyName == "TerminalHistoryWithTimeStamp")
                    {
                        txtHistory.ScrollToEnd();
                    }
                };

            if (txtHistory.Dispatcher.CheckAccess())
            {
                a();
            }
            else
            {
                txtHistory.Dispatcher.BeginInvoke(a);
            }
        }

        [ValueConversion(typeof(List<string>), typeof(string))]
        public class ListToStringConverter : IValueConverter
        {

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (targetType != typeof(string))
                    throw new InvalidOperationException("The target must be a String");

                return String.Join(", ", ((List<string>)value).ToArray());
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

    }

    
}
