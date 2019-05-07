using AkkaBiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaConsole_2
{
    class Program
    {
        static void Main(string[] args)
        {

            var server = new AkkaServer("127.0.0.1", 9092);
            var client = new AkkaClient();

            server.Start();
            client.Start("127.0.0.1", 9091);
            client.SendMessage("sono 2");


            Console.ReadKey();

        }
    }
}
