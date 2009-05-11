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
using System.IO;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace DShowNET.Helper
{
	/// <summary>
	/// 
	/// </summary>
	public class VideoProcAmp: IDisposable
	{
    protected IAMVideoProcAmp _interfaceVideoProcAmp=null;
		public VideoProcAmp(IAMVideoProcAmp amp)
		{
			_interfaceVideoProcAmp=amp;
      Contrast = ContrastDefault;
      Brightness = BrightnessDefault;
      Gamma = GammaDefault;
      Saturation = SaturationDefault;
      Sharpness = SharpnessDefault;
		}

    public void Dispose()
    {
      _interfaceVideoProcAmp = null;
    }

    public int Brightness
    {
      get 
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        try{
          _interfaceVideoProcAmp.Get(VideoProcAmpProperty.Brightness,out uiValue,out flags);
        }
        catch (Exception){}
        return ToPercent(VideoProcAmpProperty.Brightness,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.Brightness,value);
        try{
          _interfaceVideoProcAmp.Set(VideoProcAmpProperty.Brightness,uiValue,VideoProcAmpFlags.Manual);
        }
        catch (Exception){}

      }
    }

    public int Hue
    {
      get
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        try{
          _interfaceVideoProcAmp.Get(VideoProcAmpProperty.Hue,out uiValue,out flags);
        }
        catch (Exception){}
        return ToPercent(VideoProcAmpProperty.Hue,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.Hue,value);
        try{
          _interfaceVideoProcAmp.Set(VideoProcAmpProperty.Hue,uiValue,VideoProcAmpFlags.Manual);
        }
        catch (Exception){}
      }
    }


    public int Contrast
    {
      get
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        try 
        {
          int hr=_interfaceVideoProcAmp.Get(VideoProcAmpProperty.Contrast,out uiValue,out flags);
					if (hr<0) return 62;
        }
        catch (Exception){}

        return ToPercent(VideoProcAmpProperty.Contrast,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.Contrast,value);
        try 
        {
          _interfaceVideoProcAmp.Set(VideoProcAmpProperty.Contrast,uiValue,VideoProcAmpFlags.Manual);
        }
        catch (Exception){}
      }
    }



    public int Saturation
    {
      get
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        try{
          _interfaceVideoProcAmp.Get(VideoProcAmpProperty.Saturation,out uiValue,out flags);
        }
        catch (Exception){}
        return ToPercent(VideoProcAmpProperty.Saturation,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.Saturation,value);
        try
        {
          _interfaceVideoProcAmp.Set(VideoProcAmpProperty.Saturation,uiValue,VideoProcAmpFlags.Manual);
        }
        catch (Exception){}

      }
    }


    public int Sharpness
    {
      get
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        try{
          _interfaceVideoProcAmp.Get(VideoProcAmpProperty.Sharpness,out uiValue,out flags);
        }
        catch (Exception){}
        return ToPercent(VideoProcAmpProperty.Sharpness,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.Sharpness,value);
        try
        {
          _interfaceVideoProcAmp.Set(VideoProcAmpProperty.Sharpness,uiValue,VideoProcAmpFlags.Manual);
        }
        catch (Exception){}
      }
    }


    public int Gamma
    {
      get
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        try {
          _interfaceVideoProcAmp.Get(VideoProcAmpProperty.Gamma,out uiValue,out flags);
        } 
        catch (Exception){}
        return ToPercent(VideoProcAmpProperty.Gamma,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.Gamma,value);
        try{
          _interfaceVideoProcAmp.Set(VideoProcAmpProperty.Gamma,uiValue,VideoProcAmpFlags.Manual);
        } 
        catch (Exception){}
      }
    }


    public int ColorEnable
    {
      get
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        _interfaceVideoProcAmp.Get(VideoProcAmpProperty.ColorEnable,out uiValue,out flags);
        return ToPercent(VideoProcAmpProperty.ColorEnable,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.ColorEnable,value);
        _interfaceVideoProcAmp.Set(VideoProcAmpProperty.ColorEnable,uiValue,VideoProcAmpFlags.Manual);
      }
    }


    public int WhiteBalance
    {
      get
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        _interfaceVideoProcAmp.Get(VideoProcAmpProperty.WhiteBalance,out uiValue,out flags);
        return ToPercent(VideoProcAmpProperty.WhiteBalance,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.WhiteBalance,value);
        _interfaceVideoProcAmp.Set(VideoProcAmpProperty.WhiteBalance,uiValue,VideoProcAmpFlags.Manual);
      }
    }


    public int BacklightCompensation
    {
      get
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        _interfaceVideoProcAmp.Get(VideoProcAmpProperty.BacklightCompensation,out uiValue,out flags);
        return ToPercent(VideoProcAmpProperty.BacklightCompensation,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.BacklightCompensation,value);
        _interfaceVideoProcAmp.Set(VideoProcAmpProperty.BacklightCompensation,uiValue,VideoProcAmpFlags.Manual);
      }
    }



    public int Gain
    {
      get 
      {
        if (_interfaceVideoProcAmp == null) return 50;
        int uiValue=0;
        VideoProcAmpFlags flags;
        _interfaceVideoProcAmp.Get(VideoProcAmpProperty.Gain,out uiValue,out flags);
        return ToPercent(VideoProcAmpProperty.Gain,uiValue);
      }
      set
      {
        if (_interfaceVideoProcAmp == null) return;
        int uiValue=FromPercent(VideoProcAmpProperty.Gain,value);
        _interfaceVideoProcAmp.Set(VideoProcAmpProperty.Gain,uiValue,VideoProcAmpFlags.Manual);
      }
    }


    int ToPercent(VideoProcAmpProperty proc, int ivalue)
    {

      if (_interfaceVideoProcAmp == null) return 50;
      int iMin,iMax,iDelta,iDefault;
      VideoProcAmpFlags flags;
      try
      {
        _interfaceVideoProcAmp.GetRange(proc,out iMin,out iMax,out iDelta, out iDefault,out flags);
        float fWidth=iMax-iMin;
        float fpos=(ivalue-iMin);
        return (int)Math.Floor(0.5f+(fpos/fWidth)*100.0f);
      }
      catch (Exception)
      {
      }
      return 0;
    }

    int FromPercent(VideoProcAmpProperty proc, int ivalue)
    {

      if (_interfaceVideoProcAmp == null) return 50;
      int iMin,iMax,iDelta,iDefault;
      VideoProcAmpFlags flags;
      try
      {
        _interfaceVideoProcAmp.GetRange(proc,out iMin,out iMax,out iDelta, out iDefault,out flags);
        float fWidth=iMax-iMin;
        float fPos =((float)ivalue) / 100.0f;
        fPos *= fWidth;
        fPos += iMin;
        return (int)Math.Floor(0.5f+fPos);
      }
      catch(Exception)
      {
      }
      return 0;
    }

    public int BrightnessDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.Brightness,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.Brightness,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }

    
    public int BacklightCompensationDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.BacklightCompensation,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.BacklightCompensation,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }
    public int ContrastDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.Contrast,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.Contrast,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }

    public int GainDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.Gain,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.Gain,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }

    public int GammaDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.Gamma,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.Gamma,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }


    public int HueDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.Hue,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.Hue,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }

    public int SaturationDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.Saturation,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.Saturation,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }


    public int SharpnessDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.Sharpness,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.Sharpness,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }

    public int WhiteBalanceDefault
    {
      get 
      {
        try
        {
          if (_interfaceVideoProcAmp == null) return 50;
          int iMin,iMax,iDelta,iDefault;
          VideoProcAmpFlags flags;
          _interfaceVideoProcAmp.GetRange(VideoProcAmpProperty.WhiteBalance,out iMin,out iMax,out iDelta, out iDefault,out flags);
          return ToPercent(VideoProcAmpProperty.WhiteBalance,iDefault);
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }



	}
}
