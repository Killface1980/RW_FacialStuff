using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Aliens
{
    class AlienStuff
    {
        protected static AlienStuff instance;

        public static AlienStuff Instance
        {
            get
            {
                if (AlienStuff.instance == null)
                {
                    AlienStuff.instance = new AlienStuff();
                }
                return AlienStuff.instance;
            }
        }

        public Providers Providers
        {
            get;
            set;
        }
    }
}
