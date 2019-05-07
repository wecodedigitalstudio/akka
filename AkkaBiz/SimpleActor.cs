using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaBiz
{
    /// <summary>
    /// Attore delegato alla ricezione dei messaggi
    /// </summary>
    public class SimpleActor : ReceiveActor
    {
        public SimpleActor()
        {
            

            Receive<string>(msg => 
            {
                Context.Watch(Sender);
                Console.WriteLine($"Msg received {msg}");
            });
            Receive<Terminated>(t =>
            {
                Console.WriteLine($"Terminated");
            });
        }

        //protected override void OnReceive(object message)
        //{

        //    switch (message)
        //    {
        //        case Terminated T:
        //            Console.WriteLine($"Terminated");
        //            break;
        //        default:
        //            Context.Watch()
        //            Console.WriteLine($"Msg received {message}");
        //            break;
        //    }

        //}
    }
}
