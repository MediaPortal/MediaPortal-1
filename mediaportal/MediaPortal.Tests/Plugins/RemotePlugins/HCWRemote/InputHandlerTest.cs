#region Copyright (C) 2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2006 Team MediaPortal - Author: mPod
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

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
    public void GetPathDefaultXml()
    {
      bool result = false;
      string xmlFile = "TestDefault";
      InputHandler inputHandler = new InputHandler(xmlFile, out result);
      Assert.AreEqual("InputDeviceMappings\\defaults\\TestDefault.xml", inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void GetPathCustomXml()
    {
      bool result = false;
      string xmlFile = "TestCustom";
      InputHandler inputHandler = new InputHandler(xmlFile, out result);
      Assert.AreEqual("InputDeviceMappings\\custom\\TestCustom.xml", inputHandler.GetXmlPath(xmlFile));
    }

    [Test]
    public void GetPathFail()
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
      string xmlPath = "InputDeviceMappings\\defaults\\TestDefault.xml";
      InputHandler inputHandler = new InputHandler(xmlPath, out result);
      Assert.IsTrue(inputHandler.LoadMapping(xmlPath));
    }

    [Test]
    public void GetXmlVersion()
    {
      bool result = false;
      string xmlFile = "TestCustom";
      InputHandler inputHandler = new InputHandler(xmlFile, out result);
      Assert.AreEqual(3, inputHandler.GetXmlVersion("InputDeviceMappings\\defaults\\TestDefault.xml"));
    }

    [Test]
    public void GetMapping()
    {
      bool result = false;
      string xmlFile = "TestDefault";
      InputHandler inputHandler = new InputHandler(xmlFile, out result);

      int    layer       = 0;
      string condition   = "*";
      string conProperty = "-1";
      string command     = "ACTION";
      string cmdProperty = "93";
      int    cmdKeyChar  = 48;
      int    cmdKeyCode  = 0;
      string sound       = "cursor.wav";

      InputHandler.Mapping mapExpected = new InputHandler.Mapping(layer, condition, conProperty, command, cmdProperty, cmdKeyChar, cmdKeyCode, sound);
      InputHandler.Mapping mapTest = inputHandler.GetMapping(0);

      Assert.AreEqual(mapExpected.Layer      , mapTest.Layer);
      Assert.AreEqual(mapExpected.Condition  , mapTest.Condition);
      Assert.AreEqual(mapExpected.ConProperty, mapTest.ConProperty);
      Assert.AreEqual(mapExpected.Command    , mapTest.Command);
      Assert.AreEqual(mapExpected.CmdProperty, mapTest.CmdProperty);
      Assert.AreEqual(mapExpected.CmdKeyChar , mapTest.CmdKeyChar);
      Assert.AreEqual(mapExpected.CmdKeyCode , mapTest.CmdKeyCode);
      Assert.AreEqual(mapExpected.Sound      , mapTest.Sound);
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
