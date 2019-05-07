using Akka.Actor;
using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaBiz
{
    /// <summary>
    /// Componente client della comunicazione peer to peer
    /// Si connette al server ed invia messaggi
    /// </summary>
    public class AkkaClient
    {
        #region Fields

        private Config config;
        private ActorSystem system;
        private ActorSelection actor;

        #endregion

        #region Constructor

        public AkkaClient()
        {
            this.config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = remote
                    }

                    remote {
                        dot-netty.tcp {
                            port = 0
                            hostname = 127.0.0.1
                        }
                    }
                }
            ");


            //this.config = ConfigurationFactory.ParseString(@"
            //    akka {
            //        actor {
            //            provider = remote
            //            serializers {
            //                snapshot = ""LeafAkka.Biz.SnapshotSerializer, LeafAkka.Biz""
            //            }

            //            serialization-bindings {
            //                ""LeafAkka.Biz.Snapshot, LeafAkka.Biz"" = snapshot
            //            }

            //        }

            //        remote {
            //            maximum-payload-bytes = 30000000 bytes
            //            dot-netty.tcp {
            //                port = 0
            //                hostname = localhost
            //                message-frame-size =  30000000b
            //                send-buffer-size =  30000000b
            //                receive-buffer-size =  30000000b
            //                maximum-frame-size = 30000000b
            //            }
            //        }
            //    }
            //");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Avvio della comunicazione
        /// </summary>
        /// <param name="serverPort"></param>
        public void Start(string serverIp, int serverPort)
        {
            this.system = ActorSystem.Create("MyRemoteClient", config);
            this.actor = system.ActorSelection($"akka.tcp://MyRemoteServer@{serverIp}:{serverPort}/user/SimpleActor");
        }

        /// <summary>
        /// Stop della comunicazione
        /// </summary>
        public void Stop()
        {
            this.system.Dispose();
        }

        /// <summary>
        /// Invio messaggio
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(object message)
        {
            this.actor.Tell(message);
        }

        #endregion

    }
}
