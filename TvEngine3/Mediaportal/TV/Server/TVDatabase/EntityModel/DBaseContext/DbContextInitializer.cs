using System;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.DBaseContext
{
    public class DbContextInitializer
    {
        private static readonly object _syncLock = new object();
        private static DbContextInitializer _instance;

        protected DbContextInitializer() { }

        private bool _isInitialized = false;

        public static DbContextInitializer Instance()
        {
            if (_instance == null) {
                lock (_syncLock) {
                    if (_instance == null) {
                        _instance = new DbContextInitializer();
                    }
                }
            }

            return _instance;
        }

        /// <summary>
        /// This is the method which should be given the call to intialize the ObjectContext; e.g.,
        /// ObjectContextInitializer.Instance().InitializeObjectContextOnce(() => InitializeObjectContext());
        /// where InitializeObjectContext() is a method which calls ObjectContextManager.Init()
        /// </summary>
        /// <param name="initMethod"></param>
        public void InitializeObjectContextOnce(Action initMethod) {
            lock (_syncLock) {
                if (!_isInitialized) {
                    initMethod();
                    _isInitialized = true;
                }
            }
        }

    }
}
