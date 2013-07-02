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

using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

//using DShowNET.TsFileSink;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  internal class Player
  {
 

    [ComImport, Guid("b9559486-E1BB-45D3-A2A2-9A7AFE49B23F")]
    protected class TsReader {}

    protected IFilterGraph2 _graphBuilder;
    protected DsROTEntry _rotEntry;
    protected IBaseFilter _tsReader;
    private IMediaControl _mediaCtrl;
    protected IVideoWindow _videoWin;
    protected Form _form;

    public bool Play(string fileName, Form form)
    {
      _form = form;
      this.LogDebug("play:{0}", fileName);
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);

      TsReader reader = new TsReader();
      _tsReader = (IBaseFilter)reader;
      this.LogInfo("TSReaderPlayer:add TsReader to graph");
      _graphBuilder.AddFilter(_tsReader, "TsReader");

      #region load file in TsReader

      this.LogDebug("load file in Ts");
      IFileSourceFilter interfaceFile = (IFileSourceFilter)_tsReader;
      if (interfaceFile == null)
      {
        this.LogDebug("TSReaderPlayer:Failed to get IFileSourceFilter");
        return false;
      }
      int hr = interfaceFile.Load(fileName, null);

      if (hr != 0)
      {
        this.LogDebug("TSReaderPlayer:Failed to load file");
        return false;
      }

      #endregion

      #region render pin

      this.LogInfo("TSReaderPlayer:render TsReader outputs");
      IEnumPins enumPins;
      _tsReader.EnumPins(out enumPins);
      try
      {
        IPin[] pins = new IPin[2];
        int fetched;
        while (enumPins.Next(1, pins, out fetched) == 0)
        {
          if (fetched != 1)
            break;
          try
          {
            PinDirection direction;
            pins[0].QueryDirection(out direction);
            if (direction == PinDirection.Input)
            {
              continue;
            }
            _graphBuilder.Render(pins[0]);
          }
          finally
          {
            Release.ComObject("Player TsReader pin", ref pins[0]);
          }
        }
      }
      finally
      {
        Release.ComObject("Player TsReader pin enumerator", ref enumPins);
      }

      #endregion

      _videoWin = _graphBuilder as IVideoWindow;
      if (_videoWin != null)
      {
        _videoWin.put_Visible(OABool.True);
        _videoWin.put_Owner(form.Handle);
        _videoWin.put_WindowStyle(
          (WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipSiblings + (int)WindowStyle.ClipChildren));
        _videoWin.put_MessageDrain(form.Handle);

        _videoWin.SetWindowPosition(form.ClientRectangle.X, form.ClientRectangle.Y, form.ClientRectangle.Width,
                                    form.ClientRectangle.Height);
      }

      this.LogDebug("run graph");
      _mediaCtrl = (IMediaControl)_graphBuilder;
      hr = _mediaCtrl.Run();
      this.LogDebug("TSReaderPlayer:running:{0:X}", hr);

      return true;
    }

    public void Stop()
    {
      if (_videoWin != null)
      {
        _videoWin.put_Visible(OABool.False);
      }
      if (_mediaCtrl != null)
      {
        _mediaCtrl.Stop();
      }
      Release.ComObject("Player TsReader", ref _tsReader);
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
        _rotEntry = null;
      }

      Release.ComObject("Player graph", ref _graphBuilder);
    }

    public void ResizeToParent()
    {
      _videoWin.SetWindowPosition(_form.ClientRectangle.X, _form.ClientRectangle.Y, _form.ClientRectangle.Width,
                                  _form.ClientRectangle.Height);
    }
  }
}