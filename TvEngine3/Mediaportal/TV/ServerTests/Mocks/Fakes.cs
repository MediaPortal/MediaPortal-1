using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvControl;
using TvDatabase;
using TvService;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks
{
  public class Fakes
  {
    public static IUser FakeUser()
    {
      return Isolate.Fake.Instance<User>();
    }

    public static TvBusinessLayer FakeTvBusinessLayer()
    {
      return Isolate.Fake.Instance<TvBusinessLayer>();
    }

    public static TVController FakeTVController()
    {
      return Isolate.Fake.Instance<TVController>();
    }

    
  }
}
