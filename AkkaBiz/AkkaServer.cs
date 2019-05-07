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
    /// Componente server della comunicazione peer to peer
    /// Apre la connessione e rimane di ascolto dei messaggi che possono arrivare da eventuali client
    /// </summary>
    public class AkkaServer
    {
        #region Fields

        private Config config;
        private ActorSystem system;

        #endregion

        #region Constructor

        public AkkaServer(string hostname, int port)
        {
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
            //                port = " + port + @"
            //                hostname = 0.0.0.0
            //                public-hostname = " + hostname + @"
            //                message-frame-size =  30000000b
            //                send-buffer-size =  30000000b
            //                receive-buffer-size =  30000000b
            //                maximum-frame-size = 30000000b
            //            }
            //        }
            //    }
            //");

            this.config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = remote
                    }

                    remote {
                        dot-netty.tcp {
                            port = " + port + @"
                            hostname = 0.0.0.0
                            public-hostname = 127.0.0.1
                        }
                    }
                }
            ");

        }

        #endregion

        #region Methods

        /// <summary>
        /// Avvio della comunicazione
        /// </summary>
        public void Start()
        {
            this.system = ActorSystem.Create("MyRemoteServer", config);
            var actor = this.system.ActorOf<SimpleActor>("SimpleActor");

        }



        /// <summary>
        /// Stop della comunicazione
        /// </summary>
        public void Stop()
        {
            this.system.Dispose();
        }

        #endregion

    }
}