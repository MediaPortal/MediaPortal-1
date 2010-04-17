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

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Un4seen.Bass.AddOn.Vis;

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
    protected BASS_VIS_PARAM _visParam = null;

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

    public BASS_VIS_PARAM VizParam
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
      switch (VizPluginInfo.VisualizationType)
      {
        case VisualizationInfo.PluginType.Sonique:
          BassVis.BASS_VIS_Init(BASSVISPlugin.BASSVISKIND_SONIQUE,
                                BassVis.GetWindowLongPtr(GUIGraphicsContext.form.Handle, (int)GWLIndex.GWL_HINSTANCE),
                                GUIGraphicsContext.form.Handle);
          _visParam = new BASS_VIS_PARAM(BASSVISPlugin.BASSVISKIND_SONIQUE);
          break;

        case VisualizationInfo.PluginType.Winamp:
          BassVis.BASS_VIS_Init(BASSVISPlugin.BASSVISKIND_WINAMP,
                                BassVis.GetWindowLongPtr(GUIGraphicsContext.form.Handle, (int)GWLIndex.GWL_HINSTANCE),
                                GUIGraphicsContext.form.Handle);
          _visParam = new BASS_VIS_PARAM(BASSVISPlugin.BASSVISKIND_WINAMP);
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
        if (_visParam.VisHandle != 0)
        {
          // wait max 5 loops for Vis to get freed
          // Might cause infinite loop with some vis
          int i = 0;
          try
          {
            BassVis.BASS_VIS_Free(_visParam);
            while (!BassVis.BASS_VIS_IsFree(_visParam) && i < 5)
            {
              BassVis.BASS_VIS_Free(_visParam);
              i++;
              System.Threading.Thread.Sleep(20);
            }
          }
          catch (AccessViolationException) {}

          _visParam.VisHandle = 0;
        }

        if (_visParam != null)
        {
          BassVis.BASS_VIS_Quit(_visParam);
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