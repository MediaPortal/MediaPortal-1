/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Runtime.InteropServices;
using DShowNET;

namespace DirectX.Capture
{

	/// <summary>
	///  Specify the frequency of the TV tuner.
	/// </summary>
	public enum TunerInputType
	{
		/// <summary> Cable frequency </summary>
		Cable,
		/// <summary> Antenna frequency </summary>
		Antenna
	}


	/// <summary>
	///  Control and query a hardware TV Tuner.
	/// </summary>
	public class Tuner : IDisposable
	{
		// ---------------- Private Properties ---------------

		protected IAMTVTuner tvTuner = null;		



		// ------------------- Constructors ------------------

		/// <summary> Initialize this object with a DirectShow tuner </summary>
		public Tuner(IAMTVTuner tuner)
		{
			tvTuner = tuner;
		}

    public IAMTVTuner TvTuner
    {
      get { return tvTuner;}
    }


		// ---------------- Public Properties ---------------


    public DShowNET.AMTunerModeType Mode
    {
      get
      {
        DShowNET.AMTunerModeType mode;
        int hr = tvTuner.get_Mode( out mode );
        if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
        return( mode );
      }
      set
      {
        int hr = tvTuner.put_Mode( value);
        if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
      }
    }

    public void SetTuningSpace(int iTuningSpace)
    {
      int hr = tvTuner.put_TuningSpace(iTuningSpace);
      if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
    }

		/// <summary>
		///  Get or set the TV Tuner channel.
		/// </summary>
		public int Channel
		{
			get
			{
				int channel;
				int v, a;
				int hr = tvTuner.get_Channel( out channel, out v, out a );
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				return( channel );
			}

			set
			{
				int hr = tvTuner.put_Channel( value, AMTunerSubChannel.Default, AMTunerSubChannel.Default );
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
			}
		}

    public int Country
    {
      get
      {
        int country;
        int hr = tvTuner.get_CountryCode( out country);
        if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
        return( country);
      }

      set
      {
        int hr = tvTuner.put_CountryCode( value);
        if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
      }
    }

		/// <summary>
		///  Get or set the tuner frequency (cable or antenna).
		/// </summary>
		public TunerInputType InputType
		{
			get
			{
				DShowNET.TunerInputType t;
				int hr = tvTuner.get_InputType( 0, out t );
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				return( (TunerInputType) t );
			}
			set
			{
				DShowNET.TunerInputType t = (DShowNET.TunerInputType) value;
				int hr = tvTuner.put_InputType( 0, t );
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
			}
		}

		/// <summary>
		///  Indicates whether a signal is present on the current channel.
		///  If the signal strength cannot be determined, a NotSupportedException
		///  is thrown.
		/// </summary>
		public bool SignalPresent
		{
			get
			{
				AMTunerSignalStrength sig;
				int hr = tvTuner.SignalPresent( out sig );
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				if ( sig == AMTunerSignalStrength.NA ) throw new NotSupportedException("Signal strength not available.");
				return( sig == AMTunerSignalStrength.SignalPresent );
			}
		}

		// ---------------- Public Methods ---------------

		public void Dispose()
		{
			if ( tvTuner != null )
				Marshal.ReleaseComObject( tvTuner ); tvTuner = null;
		}

		// get minimum and maximum channels
		public int[] ChanelMinMax
		{
			get
			{
				int min;
				int max;
				int hr = tvTuner.ChannelMinMax(out min, out max);
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				int[] myArray = new int[] {min,max};
				return myArray;
			}
		}

		// useful for checking purposes
		public int GetVideoFrequency
		{
			get
			{
				int theFreq;
				int hr = tvTuner.get_VideoFrequency(out theFreq);
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				return theFreq;
			}
		}

		// not that useful, but...
		public int GetAudioFrequency
		{
			get
			{
				int theFreq;
				int hr = tvTuner.get_AudioFrequency (out theFreq);
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				return theFreq;
			}
		}

		//set this to your country code. Frequency Overrides should be set to this code
		public int TuningSpace
		{
			get
			{
				int tspace;
				int hr = tvTuner.get_TuningSpace(out tspace);
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				return tspace;
			}
			set
			{
				int tspace = value;
				int hr = tvTuner.put_TuningSpace(tspace);
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
			}
		}

		//Frequency Overrides are stored in the registry, in a key labeled TS[CountryCode]-[TunerInputType ], so for cable tv in Portugal it would be TS351-1
		public bool SetFrequencyOverride(int channel, int Frequency, int TuningSpace, TunerInputType InputType)
		{
			try
			{
				int IType;
				if (InputType == TunerInputType.Cable )
				{
					IType=1;
				}
				else
				{
					IType=0;
				}
				Microsoft.Win32.RegistryKey LocaleOverride;
				LocaleOverride = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\TV System Services\\TVAutoTune\\TS" + TuningSpace.ToString() + "-" + IType.ToString(), true);
				if (LocaleOverride == null)
				{
					LocaleOverride = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\TV System Services\\TVAutoTune\\TS" + TuningSpace.ToString() + "-" + IType.ToString());
				}
				LocaleOverride.SetValue(channel.ToString(), Frequency);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public enum AMTunerModeType
		{
			Default = 0x0000, // AMTUNER_MODE_DEFAULT : default tuner mode
			TV = 0x0001, // AMTUNER_MODE_TV : tv
			FMRadio = 0x0002, // AMTUNER_MODE_FM_RADIO : fm radio
			AMRadio = 0x0004, // AMTUNER_MODE_AM_RADIO : am radio
			Dss = 0x0008 // AMTUNER_MODE_DSS : dss
		}

		public struct AvAudioModes
		{
			public bool Default, TV, FMRadio, AMRadio, Dss; 
			public AvAudioModes(bool Default, bool TV, bool FMRadio, bool AMRadio, bool Dss)
			{
				this.Default = Default;
				this.TV = TV;
				this.FMRadio = FMRadio;
				this.AMRadio = AMRadio;
				this.Dss = Dss;
			}
		}

		/// 
		/// Retrieves or sets the current mode on a multifunction tuner.
		/// 
		public AMTunerModeType AudioMode
		{
			get
			{
				DShowNET.AMTunerModeType AudioMode;
				int hr = tvTuner.get_Mode(out AudioMode);
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				return( (AMTunerModeType) AudioMode );
			}
			set
			{
				DShowNET.AMTunerModeType AudioMode = (DShowNET.AMTunerModeType) value;
				int hr = tvTuner.put_Mode(AudioMode);
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
			}
		}


		/// 
		/// Retrieves the tuner's supported modes.
		/// 
		public AvAudioModes AvailableAudioModes
		{
			get
			{
				DShowNET.AMTunerModeType AudioMode;
				int hr = tvTuner.GetAvailableModes(out AudioMode);
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );
				AvAudioModes AvModes;

				if ((int)AudioMode == (int)AMTunerModeType.TV)
				{
					AvModes = new AvAudioModes(true,true,false,false,false);
				}
				else if ((int)AudioMode == (int)AMTunerModeType.TV + (int)AMTunerModeType.AMRadio)
				{
					AvModes = new AvAudioModes(true,true,false,true,false);
				}
				else if ((int)AudioMode == (int)AMTunerModeType.TV + (int)AMTunerModeType.FMRadio)
				{
					AvModes = new AvAudioModes(true,true,true,false,false);
				}
				else if ((int)AudioMode == (int)AMTunerModeType.TV + (int)AMTunerModeType.Dss)
				{
					AvModes = new AvAudioModes(true,true,false,false,true);
				}
				else if ((int)AudioMode == (int)AMTunerModeType.TV + (int)AMTunerModeType.AMRadio + (int)AMTunerModeType.FMRadio)
				{
					AvModes = new AvAudioModes(true,true,true,true,false);
				}
				else if ((int)AudioMode == (int)AMTunerModeType.TV + (int)AMTunerModeType.AMRadio + (int)AMTunerModeType.Dss)
				{
					AvModes = new AvAudioModes(true,true,false,true,true);
				}
				else if ((int)AudioMode == (int)AMTunerModeType.TV + (int)AMTunerModeType.FMRadio + (int)AMTunerModeType.Dss)
				{
					AvModes = new AvAudioModes(true,true,true,false,true);
				}
				else if ((int)AudioMode == (int)AMTunerModeType.TV + (int)AMTunerModeType.AMRadio + (int)AMTunerModeType.FMRadio + (int)AMTunerModeType.Dss)
				{
					AvModes = new AvAudioModes(true,true,true,true,true);
				}
				else
				{
					AvModes = new AvAudioModes(false,false,false,false,false);
				}

				return( AvModes );
			}
		}

	}
}
