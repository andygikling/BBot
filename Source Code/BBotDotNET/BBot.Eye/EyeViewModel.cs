using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBot
{
    public class EyeViewModel
    {
        EyeModel model;

        public EyeViewModel(IRobotConnection Connection)
        {
            model = new EyeModel(Connection);
        }

        public EyeModel EyeModel
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
            }
        }
    }
}
