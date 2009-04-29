#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using DirectShowLib;
using MediaPortal.GUI.Library;

namespace DShowNET.Helper
{
  /// <summary>
  /// This class implements methods for vendor specific actions on tv capture cards.
  /// Currently we support
  /// - FireDTV digital cards
  /// - TechnoTrend digital cards
  /// - Twinhan digital cards
  /// </summary>
  public class VideoCaptureProperties : IDisposable
  {
    private bool disposed = false;
    private Twinhan _twinhan;
    private DigitalEverywhere _digitalEverywhere;
    private TechnoTrend _technoTrend;

    #region ctor/dtor

    public VideoCaptureProperties(IBaseFilter tunerfilter)
    {
      _twinhan = new Twinhan(tunerfilter);
      _digitalEverywhere = new DigitalEverywhere(tunerfilter);
      _technoTrend = new TechnoTrend(tunerfilter);
    }

    ~VideoCaptureProperties()
    {
      Dispose(false);
    }

    #endregion

    #region Other card properties

    public bool IsCAPMTNeeded
    {
      get { return _twinhan.IsTwinhan; }
    }

    public bool SupportsCamSelection
    {
      get
      {
        if (_twinhan.IsTwinhan)
        {
          return true;
        }
        return false;
      }
    }

    public bool SupportsHardwarePidFiltering
    {
      get
      {
        if (_digitalEverywhere.IsDigitalEverywhere)
        {
          return true;
        }
        return false;
      }
    }

    public bool Supports5vAntennae
    {
      get
      {
        if (_technoTrend.IsTechnoTrendUSBDVBT)
        {
          return true;
        }
        return false;
      }
    }

    public void EnableAntenna(bool onOff)
    {
      if (_technoTrend.IsTechnoTrendUSBDVBT)
      {
        _technoTrend.EnableAntenna(onOff);
      }
    }

    public bool SendPMT(string camType, int serviceId, int videoPid, int audioPid, byte[] PMT, int pmtLength,
                        byte[] caPmt, int caPmtLen)
    {
      if (_digitalEverywhere.IsDigitalEverywhere)
      {
        return _digitalEverywhere.SendPMTToFireDTV(PMT, pmtLength);
      }
      if (_twinhan.IsTwinhan)
      {
        _twinhan.SendPMT(camType, (uint) videoPid, (uint) audioPid, caPmt, caPmtLen);
        return true;
      }
      if (_technoTrend.IsTechnoTrend)
      {
        return _technoTrend.SendPMT(serviceId);
      }
      return false;
    }

    public bool SetHardwarePidFiltering(bool isDvbc, bool isDvbT, bool isDvbS, bool isAtsc, ArrayList pids)
    {
      if (_digitalEverywhere.IsDigitalEverywhere)
      {
        return _digitalEverywhere.SetHardwarePidFiltering(isDvbc, isDvbT, isDvbS, isAtsc, pids);
      }
      return false;
    }

    public bool SupportsDiseqCommand()
    {
      if (_digitalEverywhere.IsDigitalEverywhere)
      {
        return true;
      }
      if (_technoTrend.IsTechnoTrend)
      {
        return true;
      }
      if (this._twinhan.IsTwinhan)
      {
        return true;
      }
      return false;
    }

    public void SendDiseqCommand(int lowOsc, int hiOsc, int antennaNr, int frequency, int switchingFrequency,
                                 int polarisation, int diseqcType)
    {
      if (_digitalEverywhere.IsDigitalEverywhere)
      {
        _digitalEverywhere.SendDiseqCommand(antennaNr, frequency, switchingFrequency, polarisation);
        return;
      }
      if (_technoTrend.IsTechnoTrend)
      {
        _technoTrend.SendDiseqCommand(antennaNr, frequency, switchingFrequency, polarisation, diseqcType);
        return;
      }
      if (_twinhan.IsTwinhan)
      {
        _twinhan.SendDiseqCommand(antennaNr, frequency, switchingFrequency, polarisation, diseqcType, lowOsc, hiOsc);
      }
    }

    public bool IsCISupported()
    {
      if (_digitalEverywhere.IsDigitalEverywhere)
      {
        if (_digitalEverywhere.IsCamPresent())
        {
          return true;
        }
        return false;
      }

      if (_twinhan.IsTwinhan)
      {
        if (_twinhan.IsCamPresent())
        {
          return true;
        }
      }

      if (_technoTrend.IsTechnoTrend)
      {
        return _technoTrend.IsCamPresent();
      }
      return false;
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
      GC.Collect();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
        }
        try
        {
          _twinhan = null;
          _digitalEverywhere = null;
          if (_technoTrend != null)
          {
            _technoTrend.Dispose();
            _technoTrend = null;
          }
        }
        catch (Exception ex)
        {
          Log.Info("Hauppauge exception " + ex.Message);
          Log.Info("Hauppauge Disposed hcw.txt");
        }
      }
      disposed = true;
    }

    #endregion
  }
}