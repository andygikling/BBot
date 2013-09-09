using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBot
{
    class BBotFrontEndViewModel
    {

        //Make all our sub viewmodels
        ConnectionViewModel connectionViewModel;
        VoiceViewModel voiceViewModel;
        BotTerminalViewModel botTerminalViewModel;
        LegsViewModel legsViewModel;
        QuickButtonsViewModel quickButtonsViewModel;
        EyeViewModel eyeViewModel;

        //And make a list of Widgets that will be in the view and get passed around
        List<IRobotWidget> bBotWidgetsList;
        
        enum WidgetList
        {
            Connection, Voice, BotTerminal
        };

        public BBotFrontEndViewModel()
        {
            connectionViewModel = new ConnectionViewModel();
            voiceViewModel = new VoiceViewModel(connectionViewModel.ConnectionCOMModel);
            botTerminalViewModel = new BotTerminalViewModel(connectionViewModel.ConnectionCOMModel);
            legsViewModel = new LegsViewModel(connectionViewModel.ConnectionCOMModel);
            quickButtonsViewModel = new QuickButtonsViewModel(connectionViewModel.ConnectionCOMModel);
            eyeViewModel = new EyeViewModel(connectionViewModel.ConnectionCOMModel);

            //Add widgets to a list
            //Currently this list isn't used
            bBotWidgetsList = new List<IRobotWidget>();
            bBotWidgetsList.Add(voiceViewModel.VoiceModel);
            bBotWidgetsList.Add(botTerminalViewModel.BotTerminalModel);
            bBotWidgetsList.Add(legsViewModel.LegsModel);
            bBotWidgetsList.Add(quickButtonsViewModel.QuickButtonsModel);
            bBotWidgetsList.Add(eyeViewModel.EyeModel);

        }

        #region Properties (to be bound to)

        public List<IRobotWidget> IRobotWidgetList
        {
            get
            {
                return bBotWidgetsList;
            }
            set
            {
                bBotWidgetsList = value;
            }
        }

        public ConnectionViewModel ConnectionViewModel
        {
            get { return connectionViewModel; }
            set { connectionViewModel = value; }
        }

        public VoiceViewModel VoiceViewModel
        {
            get { return voiceViewModel; }
            set { voiceViewModel = value; }
        }

        public BotTerminalViewModel BotTerminalViewModel
        {
            get { return botTerminalViewModel; }
            set { botTerminalViewModel = value; }
        }

        public LegsViewModel LegslViewModel
        {
            get { return legsViewModel; }
            set { legsViewModel = value; }
        }

        public QuickButtonsViewModel QuickButtonsViewModel
        {
            get { return quickButtonsViewModel; }
            set { quickButtonsViewModel = value; }
        }

        public EyeViewModel EyeViewModel
        {
            get { return eyeViewModel; }
            set { eyeViewModel = value; }
        }

        #endregion


    }
}
