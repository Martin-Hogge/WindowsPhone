using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ApplicationMobile_WP.Model
{
    public class Team
    {
        public ObservableCollection<Summoner> Users { get; set; }

        public Team()
        {
            Users = new ObservableCollection<Summoner>();
        }
    }
}
