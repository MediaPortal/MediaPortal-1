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
using System.Windows.Forms;
using System.Drawing;

using Un4seen.Bass;
using Un4seen.Bass.AddOn.Vis;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Tags;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using System.Runtime.InteropServices;

namespace MediaPortal.Visualization
{

  abstract public class VisualizationBase : IDisposable, IVisualization
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
    };

    protected delegate int ThreadSafeRenderDelegate();
    public delegate void VisualizationCreatedDelegate(object sender);
    public event VisualizationCreatedDelegate VisualizationCreated;

    protected VisualizationInfo VizPluginInfo = null;
    protected static VisualizationWindow _VisualizationWindow;
    protected static BassAudioEngine _Bass = null;
    protected bool _Initialized = false;
    protected bool _IsPreviewVisualization = false;


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

    virtual public bool PreRenderRequired
    {
      get { return false; }
    }

    virtual public bool IsPreviewVisualization
    {
      get { return _IsPreviewVisualization; }
      set { _IsPreviewVisualization = value; }
    }

    #endregion

    public VisualizationBase()
    {
    }

    public VisualizationBase(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : this()
    {
      VizPluginInfo = vizPluginInfo;
      VisualizationWindow = vizCtrl;
    }

    #region IDisposable Members

    virtual public void Dispose()
    {
    }

    #endregion

    protected void VisualizationLoaded()
    {
      if (this.VisualizationCreated != null)
        VisualizationCreated(this);
    }

    #region IVisualization Members
    virtual public bool IsEngineInstalled()
    {
      return false;
    }

    virtual public bool IsWinampVis()
    {
      return false;
    }

    virtual public bool Initialize()
    {
      return false;
    }

    virtual public bool InitializePreview()
    {
      _IsPreviewVisualization = true;
      return false;
    }

    virtual public bool Config()
    {
      return false;
    }

    virtual public int PreRenderVisualization()
    {
      return 0;
    }

    virtual public int RenderVisualization()
    {
      return 0;
    }

    virtual public bool Start()
    {
      return false;
    }

    virtual public bool Pause()
    {
      return false;
    }

    virtual public bool Stop()
    {
      return false;
    }

    virtual public bool WindowSizeChanged(Size newSize)
    {
      return true;
    }

    virtual public bool WindowChanged(VisualizationWindow vizWindow)
    {
      VisualizationWindow = vizWindow;
      return false;
    }

    virtual public bool SetOutputContext(VisualizationBase.OutputContextType outputType)
    {
      return false;
    }

    virtual public bool Close()
    {
      return false;
    }

    #endregion
  }
}
