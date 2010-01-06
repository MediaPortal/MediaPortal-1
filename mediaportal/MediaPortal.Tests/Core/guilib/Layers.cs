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

using MediaPortal.GUI.Library;
using NUnit.Framework;

namespace MediaPortal.Tests.Core.guilib
{
  [TestFixture]
  public class Layers
  {
    #region DummyLayer

    private class DummyLayer : IRenderLayer
    {
      private bool _rendered = false;
      private string _name;
      private bool _render = true;

      public DummyLayer(string name)
      {
        _name = name;
      }

      public bool Render
      {
        get { return _render; }
        set { _render = value; }
      }

      public bool Rendered
      {
        get { return _rendered; }
        set { _rendered = value; }
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
    }

    /*
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