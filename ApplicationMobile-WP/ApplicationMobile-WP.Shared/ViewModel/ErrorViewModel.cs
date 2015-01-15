using ApplicationMobile_WP.Exceptions;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.ViewModel
{
    class ErrorViewModel : ViewModelBase, INavigable
    {
        public RequestRiotAPIException Exception { get; set; }

        public void Activate(object parameter)
        {
            Exception = parameter as RequestRiotAPIException;
        }

        public void Desactivate(object parameter)
        {

        }
    }
}
