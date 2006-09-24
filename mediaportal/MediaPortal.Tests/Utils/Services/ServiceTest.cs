using System;
using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
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
        }

        /// <summary>
        /// Tests whether we are getting an exception when we try to add a service that is already
        /// registered.
        /// </summary>
        [Test]
        [
            ExpectedException(typeof(ArgumentException),
                "A service of type MediaPortal.Services.ILog is already present")]
        public void TestAddDuplicateService1()
        {
            ServiceProvider provider = new ServiceProvider();
            ILog log1 = new NoLog();
            ILog log2 = new NoLog();
            provider.Add<ILog>(log1);
            provider.Add<ILog>(log2);
        }

        /// <summary>
        /// Tests whether we can replace a service callback with a real service implementation using the Add method
        /// </summary>
        [Test]
        public void TestAddDuplicateService2()
        {
            ServiceProvider provider = new ServiceProvider();
            provider.Add<ILog>(new ServiceCreatorCallback<ILog>(ServiceRequested));
            ILog log1 = new NoLog();
            provider.Add<ILog>(log1);
        }

        private ILog ServiceRequested(ServiceProvider provider)
        {
            return new NoLog();
        }
    }
}