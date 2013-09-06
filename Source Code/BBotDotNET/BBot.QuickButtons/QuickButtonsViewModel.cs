using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BBot
{
    public class QuickButtonsViewModel
    {
        QuickButtonsModel model;

        public QuickButtonsViewModel(IRobotConnection Connection)
        {
            model = new QuickButtonsModel(Connection);
        }


        public QuickButtonsModel QuickButtonsModel
        {
            get
            {
                return model;
            }
        }

        

    }
}
