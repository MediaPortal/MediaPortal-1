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
using System.Runtime.InteropServices;
using DirectShowLib;

namespace DShowNET.Helper
{
  /// <summary>
  ///  Represents a physical connector or source on an 
  ///  audio device. This class is used on filters that
  ///  support the IAMAudioInputMixer interface such as 
  ///  source cards.
  /// </summary>
  public class AudioSource : Source
  {
    // --------------------- Private/Internal properties -------------------------

    internal IPin Pin; // audio mixer interface (COM object)


    // -------------------- Constructors/Destructors ----------------------

    /// <summary> Constructor. This class cannot be created directly. </summary>
    public AudioSource(IPin pin)
    {
      if ((pin as IAMAudioInputMixer) == null)
      {
        throw new NotSupportedException("The input pin does not support the IAMAudioInputMixer interface");
      }
      this.Pin = pin;
      this.name = getName(pin);
    }


    // ----------------------- Public properties -------------------------

    /// <summary> Enable or disable this source. For audio sources it is 
    /// usually possible to enable several sources. When setting Enabled=true,
    /// set Enabled=false on all other audio sources. </summary>
    public override bool Enabled
    {
      get
      {
        IAMAudioInputMixer mix = (IAMAudioInputMixer) Pin;
        bool e;
        mix.get_Enable(out e);
        return (e);
      }

      set
      {
        IAMAudioInputMixer mix = (IAMAudioInputMixer) Pin;
        mix.put_Enable(value);
      }
    }


    // --------------------------- Private methods ----------------------------

    /// <summary> Retrieve the friendly name of a connectorType. </summary>
    private string getName(IPin pin)
    {
      string s = "Unknown pin";
      PinInfo pinInfo = new PinInfo();

      // Direction matches, so add pin name to listbox
      int hr = pin.QueryPinInfo(out pinInfo);
      if (hr == 0)
      {
        s = pinInfo.name + "";
      }
      else
      {
        Marshal.ThrowExceptionForHR(hr);
      }

      // The pininfo structure contains a reference to an IBaseFilter,
      // so you must release its reference to prevent resource a leak.
      if (pinInfo.filter != null)
      {
        DirectShowUtil.ReleaseComObject(pinInfo.filter);
      }
      pinInfo.filter = null;

      return (s);
    }

    // -------------------- IDisposable -----------------------

    /// <summary> Release unmanaged resources. </summary>
    public override void Dispose()
    {
      if (Pin != null)
      {
        DirectShowUtil.ReleaseComObject(Pin);
      }
      Pin = null;
      base.Dispose();
    }
  }
}