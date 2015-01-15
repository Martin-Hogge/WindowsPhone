using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.Model
{
    public class KDA
    {
        public double Kill { get; set; }
        public double Death { get; set; }
        public double Assist { get; set; }

        public KDA ()
        {

        }

        public KDA (double kill, double death, double assist)
        {
            Kill = kill;
            Death = death;
            Assist = assist;
        }

        public override string ToString()
        {
            return String.Format("{0:0.00}/{1:0.00}/{2:0.00}", Kill, Death, Assist);
        }
    }
}
