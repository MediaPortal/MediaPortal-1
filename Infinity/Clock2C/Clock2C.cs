using System;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Plugins;
using ProjectInfinity.Messaging;

namespace Clock2C
{
    /// <summary>
    /// This plugin sends the current time in a message.
    /// </summary>
    [Plugin("Clock2C", "Sends the current time in a message.", AutoStart = true, ListInMenu = false)]
    public class Clock2CPlugin : IPlugin, IAutoStart
    {
        private System.Timers.Timer Timer;
        private Clock2CConfig cc;

        private ProjectInfinity.Logging.ILogger logger;
        public Clock2CPlugin()
        {
            cc = new Clock2CConfig(); // TODO: needs config.exe
            cc.Interval = 1000;
            cc.AllowRedefinition = true;
            cc.KVPairs.Add("short-date", "d");
            cc.KVPairs.Add("short-time", "t");
            cc.KVPairs.Add("long-date", "D");
            cc.KVPairs.Add("long-time", "T");
        }


        #region IPlugin Member

        public void Initialize(string id)        // TODO: why the hell is this not a message?
        {
            if (logger == null)
            {
                logger = ServiceScope.Get<ProjectInfinity.Logging.ILogger>();
            }
            logger.Debug("* Clock2C initialized with id=" + id);

            // ServiceScope.Get<IMessageBroker>().Register(this); crashes here!

            logger.Debug("* Clock2C initialization successful.");
        }

        #endregion


        #region IDisposable Member

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion


        #region IAutoStart Member

        public void Startup()
        {
            if (logger == null)
            {
                logger = ServiceScope.Get<ProjectInfinity.Logging.ILogger>();
            }
            logger.Debug("* Clock2C autostarted");

            ServiceScope.Get<IMessageBroker>().Register(this);
            logger.Debug("* Clock2C: IMessageBroker.Register(this)");
            //ServiceScope.Add<Clock2CPlugin>(this);
            //logger.Debug("* Clock2C: ServiceScope.Add<Clock2CPlugin>(this)");

            if (cc.Interval == 0)
                cc.Interval = 1000;

            Timer = new System.Timers.Timer();
            Timer.AutoReset = true;
            Timer.Interval = cc.Interval;
            Timer.Elapsed += new ElapsedEventHandler(OnTimer);
            Timer.Enabled = true;

            logger.Debug("* Clock2C autostart successful.");
        }

        #endregion


        #region Receiving foreign messages


        [MessageSubscription(typeof(ProjectInfinity.Messaging.PluginMessages.PluginStart))]
        private void PluginStart(ProjectInfinity.Messaging.PluginMessages.PluginStart e)
        //private void PluginStart(object Sender, object e)
        {
            // TODO: e is of type ProjectInfinity.Plugins.PluginStartStopEventArgs, 
            // but using this as argument raises an exception.
            // TODO: I receive a PluginStart message for every existing plugin.
            logger.Debug("* Clock2C received PluginStart.");

            // we start with publishing
            OnTimer(null, null);
            if (cc.Interval > 1000)			// for 1 update/minute we try to catch second 0
            {
                DateTime dt = DateTime.Now;
                Timer.Interval = (60 - dt.Second) * 1000;
            }
            else
            {
                Timer.Interval = cc.Interval;
            }
            Timer.Enabled = true;
        }


        [MessageSubscription(typeof(ProjectInfinity.Messaging.PluginMessages.PluginStop))]
        private void PluginStop(ProjectInfinity.Messaging.PluginMessages.PluginStop e)
        {
            // TODO: This is not received!
            logger.Debug("* Clock2C received PluginStop.");

            Timer.Enabled = false;
            OnTimer(null, null);

            Timer = null;
            cc = null;
            Timer = null;
            // GC.Collect();
            // GC.WaitForPendingFinalizers();
        }
        #endregion

        #region Receiving self defined messages

        [MessageSubscription(typeof(Clock2C.Add))]
        private void Add(Clock2C.Add e)
        {
            string k;

            foreach (KeyValuePair<string, string> kvp in e.NewPairs)
            {
                // TODO: This is possibly a bug
                k = kvp.Key.ToLower();

                if (!cc.KVPairs.ContainsKey(k) || cc.AllowRedefinition)
                {
                    cc.KVPairs[k] = kvp.Value;
                    logger.Debug("Added key " + k + "=" + kvp.Value);
                }
                else
                {
                    logger.Debug("Ignored key " + k + "=" + kvp.Value);
                }

            }
            OnTimer(null, null);
        }

        #endregion

        #region Sending messages

        [MessagePublication(typeof(Clock2C.Time))]
        public event MessageHandler<Clock2C.Time> Time;

        #endregion

        private void OnTimer(object source, ElapsedEventArgs e)
        {
            DateTime dt;

            if (source != null && Timer.Interval != cc.Interval) // since setting the timer resets its counter to zero,
                Timer.Interval = cc.Interval;   // we reset only if needed (more accuracy)


            dt = DateTime.Now;

            Time timee = new Time();


            foreach (KeyValuePair<string, string> kvp in cc.KVPairs)
            {
                try
                {
                    // TODO: This is possibly a bug

                    timee.Vars[kvp.Key] = dt.ToString(kvp.Value);
                }
                catch
                {
                    // TODO: This is possibly a bug
                    timee.Vars[kvp.Key] = string.Empty;
                }
            }

            Time(timee);
        }
    }
}
