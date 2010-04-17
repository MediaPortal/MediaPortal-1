#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.InputDevices;
using NUnit.Framework;

namespace MediaPortal.Tests.Core.InputDevices
{
  [TestFixture]
  [Category("InputHandler")]
  public class InputHandlerTest
  {
    [SetUp]
    public void Init() {}

    [Test]
    public void GetPathDefaultXml()
    {
      string xmlFile = "TestDefault";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(Config.GetFile(Config.Dir.CustomInputDefault, "TestDefault.xml"), inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void GetPathCustomXml()
    {
      string xmlFile = "TestCustom";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile + ".xml"), inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void GetPathCustomFail()
    {
      string xmlFile = "TestFallbackVersion";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(Config.GetFile(Config.Dir.CustomInputDefault, "TestFallbackVersion.xml"),
                      inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    [ExpectedException(typeof (XmlException))]
    public void CorruptXml()
    {
      string xmlFile = "TestCorrupt";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(Config.GetFile(Config.Dir.CustomInputDefault, "TestCorrupt.xml"), inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void GetPathFail()
    {
      string xmlFile = "TestFail";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(string.Empty, inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void GetPathFallbackVersion()
    {
      string xmlFile = "TestFallbackVersion";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(Config.GetFile(Config.Dir.CustomInputDefault, "TestFallbackVersion.xml"),
                      inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void LoadMapping()
    {
      string xmlFile = "TestDefault";
      string xmlPath = Config.GetFile(Config.Dir.CustomInputDefault, "TestDefault.xml");
      InputHandler inputHandler = new InputHandler(xmlFile);
      inputHandler.LoadMapping(xmlPath);
    }

    [Test]
    public void GetXmlVersion()
    {
      string xmlFile = "TestDefault";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(3, inputHandler.GetXmlVersion(Config.GetFile(Config.Dir.CustomInputDefault, "TestDefault.xml")));
    }

    [Test]
    public void CheckXmlVersionDefaultFail()
    {
      string xmlFile = "TestVersion";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(false, inputHandler.CheckXmlFile(Config.GetFile(Config.Dir.CustomInputDefault, "TestVersion.xml")));
    }

    [Test]
    public void CheckXmlVersionCustomFail()
    {
      string xmlFile = "TestVersion2";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(false, inputHandler.CheckXmlFile(Config.GetFile(Config.Dir.CustomInputDevice, "TestVersion2.xml")));
    }

    [Test]
    public void MappingConstructor()
    {
      int layer = 0;
      string condition = "*";
      string conProperty = "-1";
      string command = "ACTION";
      string cmdProperty = "93";
      int cmdKeyChar = 48;
      int cmdKeyCode = 0;
      string sound = "cursor.wav";
      bool focus = true;

      InputHandler.Mapping mapTest = new InputHandler.Mapping(layer, condition, conProperty, command, cmdProperty,
                                                              cmdKeyChar, cmdKeyCode, sound, focus);

      Assert.AreEqual(layer, mapTest.Layer);
      Assert.AreEqual(condition, mapTest.Condition);
      Assert.AreEqual(conProperty, mapTest.ConProperty);
      Assert.AreEqual(command, mapTest.Command);
      Assert.AreEqual(cmdProperty, mapTest.CmdProperty);
      Assert.AreEqual(cmdKeyChar, mapTest.CmdKeyChar);
      Assert.AreEqual(cmdKeyCode, mapTest.CmdKeyCode);
      Assert.AreEqual(sound, mapTest.Sound);
      Assert.AreEqual(focus, mapTest.Focus);
    }

    [Test]
    public void GetMapping()
    {
      string xmlFile = "TestDefault";
      InputHandler inputHandler = new InputHandler(xmlFile);

      int layer = 0;
      string condition = "*";
      string conProperty = "-1";
      string command = "ACTION";
      string cmdProperty = "93";
      int cmdKeyChar = 48;
      int cmdKeyCode = 0;
      string sound = "cursor.wav";
      bool focus = true;

      InputHandler.Mapping mapExpected = new InputHandler.Mapping(layer, condition, conProperty, command, cmdProperty,
                                                                  cmdKeyChar, cmdKeyCode, sound, focus);
      InputHandler.Mapping mapTest = inputHandler.GetMapping("0");

      Assert.AreEqual(mapExpected.Layer, mapTest.Layer);
      Assert.AreEqual(mapExpected.Condition, mapTest.Condition);
      Assert.AreEqual(mapExpected.ConProperty, mapTest.ConProperty);
      Assert.AreEqual(mapExpected.Command, mapTest.Command);
      Assert.AreEqual(mapExpected.CmdProperty, mapTest.CmdProperty);
      Assert.AreEqual(mapExpected.CmdKeyChar, mapTest.CmdKeyChar);
      Assert.AreEqual(mapExpected.CmdKeyCode, mapTest.CmdKeyCode);
      Assert.AreEqual(mapExpected.Sound, mapTest.Sound);
      Assert.AreEqual(mapExpected.Focus, mapTest.Focus);
    }

    [Test]
    public void GetMappingNotFound()
    {
      string xmlFile = "TestDefault";
      InputHandler inputHandler = new InputHandler(xmlFile);
      Assert.AreEqual(null, inputHandler.GetMapping("1"));
    }

    [Test]
    public void MapCommand()
    {
      string xmlFile = "TestDefault";
      int newCommand = 0;
      InputHandler inputHandler = new InputHandler(xmlFile);
      inputHandler.MapAction(newCommand);
    }
  }
}