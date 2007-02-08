using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BlasterTest
{
	public enum Port
	{
		One,
		Two,
		Both,
	}

	public enum Speed
	{
		Fast,
		Medium,
		Slow,
	}

	public enum Status
	{
		None,
		Failed,
		Success,
	}

	[Serializable(), DefaultPropertyAttribute("Name")]
	public class BlasterCommand //: ISerializable
	{
		#region Construction

		public BlasterCommand(string commandName)
		{
			_commandName = commandName;
			_commandData = null;
		}

		public BlasterCommand(string commandName, byte[] commandData)
		{
			_commandName = commandName;
			_commandData = commandData;
		}

		protected BlasterCommand(SerializationInfo info, StreamingContext context)
		{
			_commandName = info.GetString("Name");
			_commandDesc = info.GetString("Description");
			_commandData = info.GetValue("Data", typeof(byte[])) as byte[];
			_commandPort = (Port)info.GetValue("Port", typeof(Port));
			_commandSpeed = (Speed)info.GetValue("Speed", typeof(Speed));
		}

		#endregion Construction

		#region Properties

		[Category("General"), DefaultValue("")]
		public string Name
		{ get { return _commandName; } set { _commandName = value; } } 

		[Category("General")]
		public string Description
		{ get { return _commandDesc; } set { _commandDesc = value; } } 

		[Category("General")]
		public string Device
		{ get { return _deviceName; } set { _deviceName = value; } } 

		[Category("Settings"), DefaultValue(Port.One)]
		public Port Port
		{ get { return _commandPort; } set { _commandPort = value; } } 

		[Category("Settings"), DefaultValue(Speed.Fast)]
		public Speed Speed
		{ get { return _commandSpeed; } set { _commandSpeed = value; } } 

		[Category("Advanced")]
		public string Length
		{ get { return _commandData == null ? "n/a" : _commandData.Length + " byte(s)"; } } 
		
		[Category("Advanced"), Browsable(false)]
		public byte[] RawData
		{ get { return _commandData; } set { _commandData = value; } }

		[Category("Advanced")]
		public string Data
		{ get { return _commandData == null ? "n/a" : BitConverter.ToString(_commandData).Replace("-", ""); } }

		[Category("Settings"), Browsable(false)]
		public Status Status
		{ get { return _commandStatus; } set { _commandStatus = value; } }

		[Category("Settings (Device)")]
		public int Delay
		{ get { return 0; } set { /* _commandStatus = value; */ } }

		[Category("Settings (Device)"), DisplayName("Name")]
		public string DeviceName
		{ get { return "Pace 3100"; } set { /* _commandStatus = value; */ } }

		#endregion Properties

		#region Members

		string						_commandName;
		string						_commandDesc;
		Port						_commandPort = Port.One;
		Speed						_commandSpeed = Speed.Fast;
		byte[]						_commandData;

		[NonSerialized()]
		Status						_commandStatus;

		[NonSerialized()]
		string						_deviceName;

		#endregion Members
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class DisplayNameAttribute : Attribute
	{
		#region Construction

		public DisplayNameAttribute(string displayName)
		{
			_displayName = displayName;
		}

		#endregion Construction

		#region Properties
        		
		public string DisplayName	{ get { return _displayName; } }

		#endregion Properties

		#region Members

		string						_displayName;
		
		#endregion Members
	}
}
