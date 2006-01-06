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
    public void GetFileDefault()
    {
      bool result = false;
      string xmlFile = "TestDefault";
      InputHandler inputHandler = new InputHandler(xmlFile, out result);
      Assert.AreEqual("InputDeviceMappings\\defaults\\TestDefault.xml", inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void GetFileCustom()
    {
      bool result = false;
      string xmlFile = "TestCustom";
      InputHandler inputHandler = new InputHandler(xmlFile, out result);
      Assert.AreEqual("InputDeviceMappings\\custom\\TestCustom.xml", inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void GetFileFail()
    {
      bool result = false;
      string xmlFile = "TestFail";
      InputHandler inputHandler = new InputHandler(xmlFile, out result);
      Assert.AreNotEqual("InputDeviceMappings\\defaults\\TestFail.xml", inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void LoadMapping()
    {
      bool result = false;
      string xmlPath = "TestDefault.xml";
      InputHandler inputHandler = new InputHandler(xmlPath, out result);
      Assert.IsTrue(inputHandler.LoadMapping(xmlPath));
    }

    [Test]
    public void MapCommand()
    {
      bool result = false;
      string xmlFile = "TestDefault";
      int newCommand = 0;
      InputHandler inputHandler = new InputHandler(xmlFile, out result);
      Assert.IsTrue(result);
      Assert.IsTrue(inputHandler.MapAction(newCommand));
    }

  }
}
