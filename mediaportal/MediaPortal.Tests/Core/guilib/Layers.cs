#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using MediaPortal.GUI.Library;

namespace MediaPortal.Tests.Core.guilib
{
  [TestFixture]
  public class Layers
  {
    #region DummyLayer
    class DummyLayer : IRenderLayer
    {
      bool _rendered = false;
      string _name;
      bool _render = true;
      public DummyLayer(string name)
      {
        _name = name;
      }
      public bool Render
      {
        get
        {
          return _render;
        }
        set {
          _render=value;
        }
      }
      public bool Rendered
      {
        get
        {
          return _rendered;
        }
        set
        {
          _rendered = value;
        }
      }
      public bool ShouldRenderLayer() 
      { 
        return Render; 
      }
      public void RenderLayer(float timePassed) 
      {
        _rendered = true;
      }
    }
    #endregion

    [Test]
    public void Register()
    {
      IRenderLayer guiLayer = new DummyLayer("gui");
      IRenderLayer videoLayer = new DummyLayer("video");
      GUILayerManager.RegisterLayer(guiLayer, GUILayerManager.LayerType.Gui);
      GUILayerManager.RegisterLayer(videoLayer, GUILayerManager.LayerType.Video);

      Assert.AreEqual(GUILayerManager.GetLayer(GUILayerManager.LayerType.Gui), guiLayer);
      Assert.AreEqual(GUILayerManager.GetLayer(GUILayerManager.LayerType.Video), videoLayer);
    }

    [Test]
    public void UnRegister()
    {
      IRenderLayer guiLayer = new DummyLayer("gui");
      IRenderLayer videoLayer = new DummyLayer("video");
      GUILayerManager.RegisterLayer(guiLayer, GUILayerManager.LayerType.Gui);
      GUILayerManager.RegisterLayer(videoLayer, GUILayerManager.LayerType.Video);
      GUILayerManager.UnRegisterLayer(guiLayer);
      Assert.AreEqual(GUILayerManager.GetLayer(GUILayerManager.LayerType.Gui), null);
      Assert.AreEqual(GUILayerManager.GetLayer(GUILayerManager.LayerType.Video), videoLayer);
    }/*
    [Test]
    public void TestRender()
    {
      DummyLayer guiLayer = new DummyLayer("gui");
      DummyLayer videoLayer = new DummyLayer("video");
      DummyLayer noRenderLayer = new DummyLayer("dontrender");
      GUILayerManager.RegisterLayer(guiLayer, GUILayerManager.LayerType.Gui);
      GUILayerManager.RegisterLayer(videoLayer, GUILayerManager.LayerType.Video);
      GUILayerManager.RegisterLayer(noRenderLayer, GUILayerManager.LayerType.Topbar1);

      GUILayerManager.Render(1);
      Assert.IsTrue(guiLayer.Rendered);
      Assert.IsTrue(videoLayer.Rendered);
      Assert.IsFalse(noRenderLayer.Rendered);
    }*/
  }
}
