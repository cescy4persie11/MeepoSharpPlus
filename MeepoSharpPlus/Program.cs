using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeepoSharpPlus
{
    class Program
    {
        private static BootStrap bootstrap;

        static void Main(string[] args)
        {
            bootstrap = new BootStrap();
            bootstrap.SubscribeEvents();
        }
    }
}
