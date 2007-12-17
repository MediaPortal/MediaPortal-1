using NUnit.Framework;
using ProjectInfinity.Logging;
using ProjectInfinity.Settings;
using ProjectInfinity.Tests.Services.Mocks;

namespace ProjectInfinity.Tests.Services
{
  /// <summary>
  /// <see cref="ServiceScope"/> tests.
  /// </summary>
  [TestFixture]
  public class ServiceScopeTests
  {
    /// <summary>
    /// Resets the <see cref="ServiceScope"/> before every test.
    /// </summary>
    [SetUp]
    public void Reset()
    {
      ServiceScope.Reset();
    }

    /// <summary>
    /// Tests whether an exception is thrown when a requested service does not exist.
    /// </summary>
    [Test]
    [ExpectedException(typeof (ServiceNotFoundException),
      "Could not find the ProjectInfinity.Tests.Services.Mocks.IDummy service")]
    public void NoServiceTest()
    {
      ServiceScope.Get<IDummy>();
    }

    /// <summary>
    /// Tests whether a service added to the scope is available
    /// </summary>
    [Test]
    public void ServiceExistsTest()
    {
      ServiceScope.Add<IDummy>(new Service1());
      object o = ServiceScope.Get<IDummy>();
      Assert.IsNotNull(o, "expected service not found");
      Assert.IsInstanceOfType(typeof (Service1), o, "service is not of the correct type");
    }

    /// <summary>
    /// Tests whether a service is no longer available when the <see cref="ServiceScope"/> is disposed.
    /// </summary>
    [Test]
    [ExpectedException(typeof (ServiceNotFoundException),
      "Could not find the ProjectInfinity.Tests.Services.Mocks.IDummy service")]
    public void ServiceOutOfScopeTest()
    {
      using (new ServiceScope())
      {
        ServiceScope.Add<IDummy>(new Service1());
        object o = ServiceScope.Get<IDummy>();
        Assert.IsNotNull(o, "expected service not found");
      }
      ServiceScope.Get<IDummy>(); //result is not important, this method should throw an exception
    }

    /// <summary>
    /// Tests whether a servce remains in scope, even if a new <see cref="ServiceScope"/> ServiceScope is created
    /// </summary>
    [Test]
    public void ServiceScopeTest()
    {
      ServiceScope.Add<IDummy>(new Service1());
      using (new ServiceScope())
      {
        Assert.IsInstanceOfType(typeof (Service1), ServiceScope.Get<IDummy>(), "service is not of the correct type");
      }
    }

    /// <summary>
    /// Tests whether the scopes work correctly
    /// </summary>
    [Test]
    public void ScopeTest()
    {
      ServiceScope.Add<IDummy>(new Service1());
      using (new ServiceScope())
      {
        //IDummy is now of type Service1
        Assert.IsInstanceOfType(typeof (Service1), ServiceScope.Get<IDummy>(), "service is not of the correct type");
        ServiceScope.Add<IDummy>(new Service2());
        //IDummy is now of type Service2
        Assert.IsInstanceOfType(typeof (Service2), ServiceScope.Get<IDummy>(), "service is not of the correct type");
        using (new ServiceScope())
        {
          ServiceScope.Add<IDummy>(new Service3());
          //IDummy is now of type Service3
          Assert.IsInstanceOfType(typeof (Service3), ServiceScope.Get<IDummy>(), "service is not of the correct type");
        }
        //IDummy is now again of type Service2
        Assert.IsInstanceOfType(typeof (Service2), ServiceScope.Get<IDummy>(), "service is not of the correct type");
      }
      //IDummy is now again of type Service1
      Assert.IsInstanceOfType(typeof (Service1), ServiceScope.Get<IDummy>(), "service is not of the correct type");
    }

    [Test]
    public void TestDefaultServices()
    {
      ILogger log = ServiceScope.Get<ILogger>();
      Assert.IsNotNull(log);
      log.Debug("Test");
      ISettingsManager mgr = ServiceScope.Get<ISettingsManager>();
      Assert.IsNotNull(mgr);
      mgr.Save("hello");
    }

  }
}