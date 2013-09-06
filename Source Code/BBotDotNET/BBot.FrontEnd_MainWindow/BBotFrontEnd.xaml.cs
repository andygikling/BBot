using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BBot
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class BBotMainWindow : Window
	{
        //Vars for the main window
        bool _settingsLoaded;

        //Make our user control models for serialization and data store
        
        //Make the window's viewModel
        BBotFrontEndViewModel mainWidgetsViewModel;

   

        /// <summary>
        /// Constructor
        /// </summary>
		public BBotMainWindow()
		{
			this.InitializeComponent();

            mainWidgetsViewModel = new BBotFrontEndViewModel();

            //Set Widgets' DataContext
            this.connectionControlView.DataContext = mainWidgetsViewModel.ConnectionViewModel;
            this.voiceViewControl.DataContext = mainWidgetsViewModel.VoiceViewModel;
            this.botTerminalControlView.DataContext = mainWidgetsViewModel.BotTerminalViewModel;
            this.legsView.DataContext = mainWidgetsViewModel.LegslViewModel;
            this.quickButtonsView.DataContext = mainWidgetsViewModel.QuickButtonsViewModel;
            
            //this.cameraView.DataContext = mainWidgetsViewModel.

            this.Closing += BBotMainWindow_Closing;
		}

        void BBotMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Call close
            mainWidgetsViewModel.ConnectionViewModel.CloseComPort();
        }

	
	}
}
