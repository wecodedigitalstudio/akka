using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaBiz
{
    public class AkkaPeerToPeer 
    {
        #region Fields

        /// <summary>
        /// Server della comunicazione
        /// </summary>
        private static AkkaServer server;

        /// <summary>
        /// Client della comunicazione
        /// </summary>
        private static AkkaClient client;

        #endregion

        #region Constructor

        public AkkaPeerToPeer()
        { }

        #endregion

        #region Methods

        /// <summary>
        /// Caricamento configurazione xml
        /// </summary>
        /// <param name="configuration"></param>
        public void Configure(RedundancyConfiguration config)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("redundancy");

            if (config != null)
            {
                configuration = config;

                log.Info($"Redundancy configured [local {configuration.LocalIp}:{configuration.LocalPort}, remote {configuration.RemoteIp}:{configuration.RemotePort}]");

                server = new AkkaServer(configuration.LocalIp, configuration.LocalPort);
                client = new AkkaClient(configuration.RemoteIp, configuration.RemotePort);

                operationMode = configuration.BootMode;

                timer = new LEAF.Common.Timer(configuration.PollingTime);
                timer.Tick += Timer_Tick;
            }

        }

        /// <summary>
        /// Inizializzazione controller
        /// </summary>
        /// <param name="managersToListen"></param>
        public void Init(List<IManager> managersToListen)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("redundancy");

            log.Info("Redundancy init");

            foreach (var manager in managersToListen)
                manager.OnError += Manager_OnError;

            foreach (var manager in managersToListen)
                manager.OnStart += Manager_OnStart;

            server.Start();
            log.Debug("server.Start() done");
            client.Start();
            log.Debug("client.Start() done");

            Thread.Sleep(5000);

        }

        /// <summary>
        /// Gestione errore di comunicazione manager da monitorare
        /// #11118
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_OnError(object sender, EventArgs e)
        {
            if (operationMode == OperationModeEnum.Master)
            {
                SendMessage("CommunicationError");
                operationMode = OperationModeEnum.Slave;
            }
        }

        /// <summary>
        /// Gestione ripristino connessione
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_OnStart(object sender, EventArgs e)
        {
            if (operationMode == OperationModeEnum.Slave)
            {
                SendMessage("CommunicationRecovery");
            }
        }

        /// <summary>
        /// Avvio comunicazione
        /// </summary>
        /// <param name="remotePort"></param>
        public void Start()
        {
            if (this.Enabled)
            {
                log.Info($"Redundancy starting as {operationMode}...");

                if (operationMode == OperationModeEnum.Slave)
                    app.RunAsSlave();
                else
                    app.RunAsMaster();

                timer.Start();
                log.Info("Redundancy started");
            }

        }

        /// <summary>
        /// Stop della comunicazione
        /// </summary>
        public void Stop()
        {
            log.Info("Redundancy stopping...");
            app.RunAsSlave();
            server.Stop();
            client.Stop();
            timer.Stop();
            log.Info("Redundancy stopped");
        }

        /// <summary>
        /// Polling per l'invio dell'immagine di processo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            log.Debug($"Timer tick {operationMode}");

            if (operationMode == OperationModeEnum.Master)
                SendMessage(app.Snapshot);

        }

        /// <summary>
        /// Invio messaggio
        /// </summary>
        /// <param name="message"></param>
        private static void SendMessage(object message)
        {
            try
            {
                client.SendMessage(message);

                if (message is string)
                    log.Info($"Message sent: {message} ({AkkaPeerToPeer.operationMode})");
            }
            catch (Exception exc)
            {
                //log.Info("Operation mode changed, new value SLAVE");
                //operationMode = OperationModeEnum.Slave;
                //app.RunAsSlave();
                log.Error("Errore invio messaggio", exc);
            }
        }

        /// <summary>
        /// Ricettore messaggi e aggiornamento immagine di processo
        /// </summary>
        /// <param name="message"></param>
        public static void OnMessageReceived(object message)
        {
            log.Debug("Snapshot received");

            Snapshot snapshot = message as Snapshot;

            //if (operationMode == OperationModeEnum.Master)
            //{
            //    log.Info("Operation mode changed, new value SLAVE");
            //    operationMode = OperationModeEnum.Slave;
            //    app.RunAsSlave();
            //}

            if (snapshot != null)
            {
                app.Snapshot = snapshot;
                return;
            }

            if (message.ToString() == "CommunicationError")
            {
                // Il Master non comunica più con il campo
                operationMode = OperationModeEnum.Master;
                app.RunAsMaster();
                return;
            }

            if (message.ToString() == "CommunicationRecovery")
            {
                // Il Master ha recuperato la comunicazione con il campo
                operationMode = OperationModeEnum.Slave;
                app.RunAsSlave();
                SendMessage("Ok");
                return;
            }

            if (message.ToString() == "Ok")
            {
                // Lo slave ha dato conferma di poter tornare master
                operationMode = OperationModeEnum.Master;
                app.RunAsMaster();
                return;
            }



        }






        #endregion

    }
}
