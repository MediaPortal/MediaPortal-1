using System;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
    public class ObjectContextInitializer
    {
        private static readonly object syncLock = new object();
        private static ObjectContextInitializer instance;

        protected ObjectContextInitializer() { }

        private bool isInitialized = false;

        public static ObjectContextInitializer Instance()
        {
            if (instance == null) {
                lock (syncLock) {
                    if (instance == null) {
                        instance = new ObjectContextInitializer();
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// This is the method which should be given the call to intialize the ObjectContext; e.g.,
        /// ObjectContextInitializer.Instance().InitializeObjectContextOnce(() => InitializeObjectContext());
        /// where InitializeObjectContext() is a method which calls ObjectContextManager.Init()
        /// </summary>
        /// <param name="initMethod"></param>
        public void InitializeObjectContextOnce(Action initMethod) {
            lock (syncLock) {
                if (!isInitialized) {
                    initMethod();
                    isInitialized = true;
                }
            }
        }

    }
}
