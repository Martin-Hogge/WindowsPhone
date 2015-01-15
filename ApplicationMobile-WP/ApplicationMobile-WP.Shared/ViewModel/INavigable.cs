using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.ViewModel
{
    interface INavigable
    {
        void Activate(object parameter);
        void Desactivate(object parameter);
    }
}
