using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.ViewModel
{
    public class SingletonViewLocator
    {
        private static ViewModelLocator vml;

        public static ViewModelLocator getInstance ()
        {
            if (vml == null)
                vml = new ViewModelLocator();
            return vml;
        }
    }
}
