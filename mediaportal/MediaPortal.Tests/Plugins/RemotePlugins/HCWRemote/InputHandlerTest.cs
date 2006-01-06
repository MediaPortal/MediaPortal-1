using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace MediaPortal.Tests.Plugins.RemotePlugins.HCWRemote
{
  [TestFixture]
  [Category("InputHandler")]
  public class InputHandlerTest
  {
    
    [Test]
    public void InitializeInputHandlerDefault()
    {
      bool result = false;
      InputHandler hcwHandler = new InputHandler("Test", out result);
      Assert.IsTrue(result);
    }

    [Test]
    public void InitializeInputHandlerCustom()
    {
      bool result = false;
      InputHandler hcwHandler = new InputHandler("Test2", out result);
      Assert.IsTrue(result);
    }

    [Test]
    public void MapCommand()
    {
      bool result = false;
      int newCommand = 0;
      InputHandler hcwHandler = new InputHandler("Test", out result);
      Assert.IsTrue(result);
      Assert.IsTrue(hcwHandler.MapAction(newCommand));
    }

  }
}
