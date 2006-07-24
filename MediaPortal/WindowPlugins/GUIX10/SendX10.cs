/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using KCS.xAP.Framework;
using KCS.xAP.Framework.Message;
using KCS.xAP.Framework.Transport;
using X10Unified;

namespace MediaPortal.GUI.X10Plugin
{
	
	/// <summary>
	/// Summary description for SendX10.
	/// </summary>
	public class SendX10
	{
		private string xapComputer;
		private string strHeader;
		/* SAMPLE		
		xap-header
		{
			v=12
			Hop=1
			UID=FF123400
			Class=xap-x10.request
			Source=KCSoft.Send.Anya
			Target=ERSP.X10.nopapxp
		}
		xap-x10.request
		{
			command=on
			device=A1
		}
		*/

		private int ComPort = 1;
		X10Unified.Senders.Firecracker _Firecracker;

		private int CMDevice = (int)CMDevices.CM11;
		public enum CMDevices 
		{
			CM11 = 0,
			CM17 = 1
		}

		/// <summary>
		/// CM11, use xAPFramework on xapComputer
		/// </summary>
		/// <param name="CMDevice"></param>
		/// <param name="xapComputer"></param>
		public SendX10(int CMDevice, string xapComputer, int COMPort)
		{
			this.CMDevice = (int)CMDevice;
			this.xapComputer = xapComputer;
			this.ComPort = COMPort;

			if (this.CMDevice == (int)CMDevices.CM11) // CM11 use xAPFramework 
			{
				strHeader = "xap-header\r\n";
				strHeader += "{\r\n";
				strHeader += "v=12\r\n";
				strHeader += "Hop=1\r\n";
				strHeader += "UID=FF123400\r\n";
				strHeader += "Class=xap-x10.request\r\n";
				strHeader += "Source=NOPAP.Send.Anya\r\n";
				strHeader += "Target=ERSP.X10." + xapComputer + "\r\n";
				strHeader += "}\r\n";
			} 
			else // CM17 use Firecracker class 
			{
				_Firecracker = X10Unified.Senders.Firecracker.GetInstance(ComPort);
			}
		}

		public bool sendX10Command(string house, string command)
		{
			return sendX10Command(house, 0, command, 0);
		}
		public bool sendX10Command(string house, int unit, string command)
		{
			return sendX10Command(house, unit, command, 0);
		}
		public bool sendX10Command(string house, string unit, string command)
		{
			try { return sendX10Command(house, System.Convert.ToInt32(unit), command, 0); } 
			catch { return false; }
		}
		public bool sendX10Command(string house, string unit, string command, string data)
		{
			try { return sendX10Command(house, System.Convert.ToInt32(unit), command, System.Convert.ToInt32(data)); } 
			catch { return false; }
		}

		public bool sendX10Command(string house, int unit, string command, int data)
		{
			if (CMDevice == (int)CMDevices.CM11)
				return sendX10XAP(house, unit, command, data);
			else if (CMDevice == (int)CMDevices.CM17)
				return sendX10Firecracker(house, unit, command, data);
			else
				return false;
		}

		private bool sendX10Firecracker(string house, int unit, string command, int data)
		{
			char housecode = house.Trim()[0];
			string strCommand = command.Trim().ToLower();
			
			if (strCommand == "on")
				return _Firecracker.SendCommand(housecode, unit, X10Unified.Senders.Firecracker.Commands.TurnOn);
			else if (strCommand == "off")
				return _Firecracker.SendCommand(housecode, unit, X10Unified.Senders.Firecracker.Commands.TurnOff);
			else if (strCommand == "bright")
				return _Firecracker.SendCommand(housecode, unit, X10Unified.Senders.Firecracker.Commands.MakeBrighter);
			else if (strCommand == "dim")
				return _Firecracker.SendCommand(housecode, unit, X10Unified.Senders.Firecracker.Commands.MakeDimmer);
			else if (strCommand == "all_lights_on")
				return _Firecracker.SendCommand(housecode, unit, X10Unified.Senders.Firecracker.Commands.TurnOn);
			else if (strCommand == "all_lights_off")
				return _Firecracker.SendCommand(housecode, unit, X10Unified.Senders.Firecracker.Commands.TurnOff);
			else if (strCommand == "all_units_off")
				return _Firecracker.SendCommand(housecode, unit, X10Unified.Senders.Firecracker.Commands.TurnOff);

			return false;
		}

		private bool sendX10XAP(string house, int unit, string command, int data)
		{
			string msg = buildXapMessage(house.Trim(), unit, command.ToLower().Trim(), data);
			if (msg == null)
				return false;

			try
			{
				xAPRawMessage       raw_message = new xAPRawMessage(msg);
				xAPMessageReader    reader      = new xAPMessageReader (raw_message);
				xAPMessage          xap_message = reader.ReadMessage();

				Console.Write("Sending: {0} ... ", xap_message.Header.Class);

				if (!xap_message.IsValid(xAPSettings.sm_validity_failure))
					throw new FormatException (xAPSettings.sm_validity_failure.GetReason("The xAP message is invalid"));

				xAPSender sender = new xAPSender();
				sender.Send(xap_message);
				sender.Close();

				Console.WriteLine("Sent");

				return true;
			}
			catch (xAPMessageReaderException e)
			{
				Console.WriteLine("Error message:  {0}", e.Message);
				Console.WriteLine("Problem line:   {0}: '{1}'", e.ErrorLineNumber, e.ErrorLine);
				return false;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}

		private string buildXapMessage(string house, int unit, string command, int data)
		{
			string request = "xap-x10.request\r\n";
			request += "{\r\n";
			if (command.Equals("on") || command.Equals("off")) 
			{
				request += "command=" + command + "\r\n";
				request += "device=" + house.ToUpper() + unit + "\r\n";
			} 
			else if (command.Equals("dim") || command.Equals("bright"))  
			{
				request += "command=" + command + "\r\n";
				request += "device=" + house.ToUpper() + unit + "\r\n";
				request += "count=" + data + "\r\n";
			} 
			else if (command.Equals("all_lights_on") || command.Equals("all_lights_off") || command.Equals("all_units_off"))
			{
				request += "command=" + command + "\r\n";
				request += "device=" + house.ToUpper() + "\r\n";
			} 
			else 
				return null;

			request += "}\r\n";

			return strHeader + request;
		}
	}
}
