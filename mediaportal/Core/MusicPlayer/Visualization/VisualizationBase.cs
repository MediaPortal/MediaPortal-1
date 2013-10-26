#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.MusicPlayer.BASS;
using MediaPortal.Player;
using BassVis_Api;

namespace MediaPortal.Visualization
{
  public abstract class VisualizationBase : IDisposable, IVisualization
  {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;
    }

    public enum OutputContextType
    {
      WindowHandle = 1,
      DeviceContext = 2,
    } ;

    protected delegate int ThreadSafeRenderDelegate();

    public delegate void VisualizationCreatedDelegate(object sender);

    public event VisualizationCreatedDelegate VisualizationCreated;

    protected VisualizationInfo VizPluginInfo = null;
    protected static VisualizationWindow _VisualizationWindow;
    protected static BassAudioEngine _Bass = null;
    protected bool _Initialized = false;
    protected bool _IsPreviewVisualization = false;
    protected BASSVIS_PARAM _visParam = null;
    protected BASSVIS_PARAM _baseVisParam = null;

    #region Properties

    public static BassAudioEngine Bass
    {
      get { return _Bass; }
      set { _Bass = value; }
    }

    public static VisualizationWindow VisualizationWindow
    {
      get { return _VisualizationWindow; }
      set { _VisualizationWindow = value; }
    }

    public bool Initialized
    {
      get { return _Initialized; }
    }

    public BASSVIS_PARAM VizParam
    {
      get { return _visParam; }
    }

    public virtual bool PreRenderRequired
    {
      get { return false; }
    }

    public virtual bool IsPreviewVisualization
    {
      get { return _IsPreviewVisualization; }
      set { _IsPreviewVisualization = value; }
    }

    #endregion

    public VisualizationBase() {}

    public VisualizationBase(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : this()
    {
      VizPluginInfo = vizPluginInfo;
      VisualizationWindow = vizCtrl;

      // Init BAssVis
      IntPtr hInstance = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]);
      switch (VizPluginInfo.VisualizationType)
      {
        case VisualizationInfo.PluginType.Sonique:
          BassVis.BASSVIS_Init(BASSVISKind.BASSVISKIND_SONIQUE, GUIGraphicsContext.form.Handle);
          _visParam = new BASSVIS_PARAM(BASSVISKind.BASSVISKIND_SONIQUE);
          break;

        case VisualizationInfo.PluginType.Winamp:
          BassVis.BASSVIS_Init(BASSVISKind.BASSVISKIND_WINAMP, GUIGraphicsContext.form.Handle);
          _visParam = new BASSVIS_PARAM(BASSVISKind.BASSVISKIND_WINAMP);
          break;

        case VisualizationInfo.PluginType.Bassbox:
          BassVis.BASSVIS_Init(BASSVISKind.BASSVISKIND_BASSBOX, GUIGraphicsContext.form.Handle);
          _visParam = new BASSVIS_PARAM(BASSVISKind.BASSVISKIND_BASSBOX);
          break;
      }
    }

    #region IDisposable Members

    public virtual void Dispose() {}

    #endregion

    protected void VisualizationLoaded()
    {
      if (this.VisualizationCreated != null)
      {
        VisualizationCreated(this);
      }
    }

    #region IVisualization Members

    public virtual bool IsEngineInstalled()
    {
      return false;
    }

    public virtual bool IsWinampVis()
    {
      return false;
    }

    public virtual bool IsSoniqueVis()
    {
      return false;
    }

    public virtual bool IsBassboxVis()
    {
      return false;
    }

    public virtual bool IsWmpVis()
    {
      return false;
    }

    public virtual bool Initialize()
    {
      return false;
    }

    public virtual bool InitializePreview()
    {
      _IsPreviewVisualization = true;
      return false;
    }

    public virtual bool Config()
    {
      return false;
    }

    public virtual int PreRenderVisualization()
    {
      return 0;
    }

    public virtual int RenderVisualization()
    {
      return 0;
    }

    public virtual bool Start()
    {
      return false;
    }

    public virtual bool Pause()
    {
      return false;
    }

    public virtual bool Stop()
    {
      return false;
    }

    public virtual bool WindowSizeChanged(Size newSize)
    {
      return true;
    }

    public virtual bool WindowChanged(VisualizationWindow vizWindow)
    {
      VisualizationWindow = vizWindow;
      return false;
    }

    public virtual bool SetOutputContext(OutputContextType outputType)
    {
      return false;
    }

    public virtual bool Close()
    {
      try
      {
        if (_visParam != null)
        {
          BassVis.BASSVIS_Quit(_visParam);
          _visParam = null;
        }
        return true;
      }
      catch (Exception) {}
      return false;
    }

    #endregion
  }
}