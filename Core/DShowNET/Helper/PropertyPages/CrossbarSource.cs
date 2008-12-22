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
	///  audio/Video device. This class is used on filters that
	///  support the IAMCrossbar interface such as TV Tuners.
	/// </summary>
	public class CrossbarSource : Source
	{
		// --------------------- Private/Internal properties -------------------------

		internal IAMCrossbar			Crossbar;			// crossbar filter (COM object)
		internal int					OutputPin;			// output pin number on the crossbar
		internal int					InputPin;			// input pin number on the crossbar
		internal PhysicalConnectorType	ConnectorType;		// type of the connector



		// ----------------------- Public properties -------------------------

		/// <summary> Enabled or disable this source. </summary>
		public override bool Enabled
		{
			get 
			{
				int i;
				if ( Crossbar.get_IsRoutedTo( OutputPin, out i ) == 0 )
					if ( InputPin == i )
						return( true );
				return( false );
			}

			set
			{
				if ( value )
				{
					// Enable this route
					int hr = this.Crossbar.Route( this.OutputPin, this.InputPin );
					if ( hr < 0 )
						Marshal.ThrowExceptionForHR( hr );
				}
				else
				{
					// Disable this route by routing the output
					// pin to input pin -1
					int hr = this.Crossbar.Route( this.OutputPin, -1 );
					if ( hr < 0 )
						Marshal.ThrowExceptionForHR( hr );
				}
			}
		}


		
		// -------------------- Constructors/Destructors ----------------------

		/// <summary> Constructor. This class cannot be created directly. </summary>
		internal CrossbarSource( IAMCrossbar crossbar, int outputPin, int inputPin, PhysicalConnectorType connectorType )
		{
			this.Crossbar = crossbar;
			this.OutputPin = outputPin;
			this.InputPin = inputPin;
			this.ConnectorType = connectorType;
			this.name = getName( connectorType );
		}

		// --------------------------- Private methods ----------------------------
    public bool IsTuner
    {
      get
      {
        if (this.ConnectorType==PhysicalConnectorType.Video_Tuner) return true;
        return false;
      }
    }
    public bool IsComposite
    {
      get
      {
        if (this.ConnectorType==PhysicalConnectorType.Video_Composite) return true;
        return false;
      }
    }
    public bool IsSVHS
    {
      get
      {
        if (this.ConnectorType==PhysicalConnectorType.Video_SVideo) return true;
        return false;
      }
		}
		public bool IsRgb
		{
			get
			{
				if (this.ConnectorType==PhysicalConnectorType.Video_RGB) return true;
				return false;
			}
		}

		/// <summary> Retrieve the friendly name of a connectorType. </summary>
		private string getName( PhysicalConnectorType connectorType )
		{
			string name;
			switch( connectorType )
			{
				case PhysicalConnectorType.Video_Tuner:				name = "Video Tuner";			break;
				case PhysicalConnectorType.Video_Composite:			name = "Video Composite";		break;
				case PhysicalConnectorType.Video_SVideo:			name = "Video S-Video";			break;
				case PhysicalConnectorType.Video_RGB:				name = "Video RGB";				break;
				case PhysicalConnectorType.Video_YRYBY:				name = "Video YRYBY";			break;
				case PhysicalConnectorType.Video_SerialDigital:		name = "Video Serial Digital";	break;
				case PhysicalConnectorType.Video_ParallelDigital:	name = "Video Parallel Digital";break;
				case PhysicalConnectorType.Video_SCSI:				name = "Video SCSI";			break;
				case PhysicalConnectorType.Video_AUX:				name = "Video AUX";				break;
				case PhysicalConnectorType.Video_1394:				name = "Video Firewire";		break;
				case PhysicalConnectorType.Video_USB:				name = "Video USB";				break;
				case PhysicalConnectorType.Video_VideoDecoder:		name = "Video Decoder";			break;
				case PhysicalConnectorType.Video_VideoEncoder:		name = "Video Encoder";			break;
				case PhysicalConnectorType.Video_SCART:				name = "Video SCART";			break;

				case PhysicalConnectorType.Audio_Tuner:				name = "Audio Tuner";			break;
				case PhysicalConnectorType.Audio_Line:				name = "Audio Line In";			break;
				case PhysicalConnectorType.Audio_Mic:				name = "Audio Mic";				break;
				case PhysicalConnectorType.Audio_AESDigital:		name = "Audio AES Digital";		break;
				case PhysicalConnectorType.Audio_SPDIFDigital:		name = "Audio SPDIF Digital";	break;
				case PhysicalConnectorType.Audio_SCSI:				name = "Audio SCSI";			break;
				case PhysicalConnectorType.Audio_AUX:				name = "Audio AUX";				break;
				case PhysicalConnectorType.Audio_1394:				name = "Audio Firewire";		break;
				case PhysicalConnectorType.Audio_USB:				name = "Audio USB";				break;
				case PhysicalConnectorType.Audio_AudioDecoder:		name = "Audio Decoder";			break;

				default:											name = "Unknown Connector";		break;
			}
			return( name );
		}


		
		// -------------------- IDisposable -----------------------

		/// <summary> Release unmanaged resources. </summary>
		public override void Dispose()
		{
			if ( Crossbar != null )
				DirectShowUtil.ReleaseComObject( Crossbar );
			Crossbar = null;
			base.Dispose();
		}	
	}
}
