using System;
using MediaPortal.Utils.Services;
using NUnit.Framework;

namespace MediaPortal.Tests.Utils.Services
{
    [TestFixture]
    [Category("ServiceProvider")]
    public class ServiceTest
    {
        [Test]
        public void TestServiceCreatorCallback()
        {
            ServiceProvider provider = new ServiceProvider();
            provider.Add<ILog>(new ServiceCreatorCallback<ILog>(ServiceRequested));
            ILog log = provider.Get<ILog>();
            Assert.IsNotNull(log);
            log.Dispose();
        }

        /// <summary>
        /// Tests whether we are getting an exception when we try to add a service that is already
        /// registered.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException),"A service of type MediaPortal.Utils.Services.ILog is already present")]
        public void TestAddDuplicateService1()
        {
            ServiceProvider provider = new ServiceProvider();
            using (ILog log1 = new Log("blabla", Log.Level.Warning))
            {
                using (ILog log2 = new Log("lala", Log.Level.Warning))
                {
                    provider.Add<ILog>(log1);
                    provider.Add<ILog>(log2);
                }
            }
        }
        
        /// <summary>
        /// Tests whether we can replace a service callback with a real service implementation using the Add method
        /// </summary>
        [Test]
        public void TestAddDuplicateService2()
        {
            ServiceProvider provider = new ServiceProvider();
            provider.Add<ILog>(new ServiceCreatorCallback<ILog>(ServiceRequested));
            using (ILog log1 = new Log("blabla", Log.Level.Warning))
            {
                    provider.Add<ILog>(log1);
            }
        }
        
        private ILog ServiceRequested(ServiceProvider provider)
        {
            return new Log("blabla",Log.Level.Warning);
        }
    }
}
