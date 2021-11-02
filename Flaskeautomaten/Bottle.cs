using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaskeautomaten
{
    public class Bottle
    {
        static int allNumber = 0;
        public string name;
        public int number;
        
        public Bottle(string name)
        {
            this.name = name;
            number = allNumber;
            allNumber++;
        }

    }
}
