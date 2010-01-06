/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Runtime.InteropServices;
using DirectShowLib;
//using DShowNET.TsFileSink;
using TvLibrary.Log;
using System.Windows.Forms;

namespace SetupTv.Sections
{
  internal class Player
  {
    [ComImport, Guid("b9559486-E1BB-45D3-A2A2-9A7AFE49B23F")]
    protected class TsReader {}

    protected IFilterGraph2 _graphBuilder;
    protected DsROTEntry _rotEntry;
    protected IBaseFilter _tsReader;
    protected IPin _pinVideo;
    protected IPin _pinAudio;
    private IMediaControl _mediaCtrl;
    protected IVideoWindow _videoWin;
    protected Form _form;

    public bool Play(string fileName, Form form)
    {
      _form = form;
      Log.WriteFile("play:{0}", fileName);
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);

      TsReader reader = new TsReader();
      _tsReader = (IBaseFilter)reader;
      Log.Info("TSReaderPlayer:add TsReader to graph");
      _graphBuilder.AddFilter(_tsReader, "TsReader");

      #region load file in TsReader

      Log.WriteFile("load file in Ts");
      IFileSourceFilter interfaceFile = (IFileSourceFilter)_tsReader;
      if (interfaceFile == null)
      {
        Log.WriteFile("TSReaderPlayer:Failed to get IFileSourceFilter");
        return false;
      }
      int hr = interfaceFile.Load(fileName, null);

      if (hr != 0)
      {
        Log.WriteFile("TSReaderPlayer:Failed to load file");
        return false;
      }

      #endregion

      #region render pin

      Log.Info("TSReaderPlayer:render TsReader outputs");
      IEnumPins enumPins;
      _tsReader.EnumPins(out enumPins);
      IPin[] pins = new IPin[2];
      int fetched;
      while (enumPins.Next(1, pins, out fetched) == 0)
      {
        if (fetched != 1) break;
        PinDirection direction;
        pins[0].QueryDirection(out direction);
        if (direction == PinDirection.Input)
        {
          Marshal.ReleaseComObject(pins[0]);
          continue;
        }
        _graphBuilder.Render(pins[0]);
        Marshal.ReleaseComObject(pins[0]);
      }
      Marshal.ReleaseComObject(enumPins);

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

      Log.WriteFile("run graph");
      _mediaCtrl = (IMediaControl)_graphBuilder;
      hr = _mediaCtrl.Run();
      Log.WriteFile("TSReaderPlayer:running:{0:X}", hr);

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
      if (_pinAudio != null)
      {
        Marshal.ReleaseComObject(_pinAudio);
        _pinAudio = null;
      }
      if (_pinVideo != null)
      {
        Marshal.ReleaseComObject(_pinVideo);
        _pinVideo = null;
      }
      if (_tsReader != null)
      {
        Marshal.ReleaseComObject(_tsReader);
        _tsReader = null;
      }
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
        _rotEntry = null;
      }

      if (_graphBuilder != null)
      {
        Marshal.ReleaseComObject(_graphBuilder);
        _graphBuilder = null;
      }
    }

    public void ResizeToParent()
    {
      _videoWin.SetWindowPosition(_form.ClientRectangle.X, _form.ClientRectangle.Y, _form.ClientRectangle.Width,
                                  _form.ClientRectangle.Height);
    }
  }
}