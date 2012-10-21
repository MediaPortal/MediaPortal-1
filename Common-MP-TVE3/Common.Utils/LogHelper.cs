using System;
using System.Collections.Generic;

namespace MediaPortal.Common.Utils
{
    public static class LogHelper
    {
        private static readonly IDictionary<Type, ILogManager> _logManagers = new Dictionary<Type, ILogManager>();
        private static readonly object _logManagersLock = new object();

        public static ILogManager GetLogger(Type type)
        {
            ILogManager logManager;

            lock (_logManagersLock)
            {
                bool hasLogManager = _logManagers.TryGetValue(type, out logManager);
                if (!hasLogManager)
                {
                    logManager = new LogManager(type);
                    _logManagers[type] = logManager;
                }
            }
            return logManager;
        }

    }
}
