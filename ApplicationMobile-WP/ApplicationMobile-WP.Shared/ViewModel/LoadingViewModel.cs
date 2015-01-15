using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.ViewModel
{
    class LoadingViewModel : ViewModelBase, INavigable
    {
        public static bool ComeFromBackButton;
        public void Activate(object parameter)
        {
            ComeFromBackButton = true;
        }

        public void Desactivate(object parameter)
        {
            
        }
    }
}
