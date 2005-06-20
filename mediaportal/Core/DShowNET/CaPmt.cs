using System;

namespace DShowNET
{
	public class CaPmt
	{
		#region Constructors

		public CaPmt(int programNumber) : this(programNumber, ListManagement.Only) 
		{
		}

		public CaPmt(int programNumber, ListManagement listManagement)
		{
			_buffer = new byte[2048];

			// list management
			_buffer[_offset++] = (byte)listManagement;
				
			// program number
			_buffer[_offset++] = (byte)((programNumber >> 8) & 0xFF);
			_buffer[_offset++] = (byte)((programNumber & 0xFF));

			// reserved, version number & current next indicator
			_buffer[_offset++] = 0x01;

			// reserved & program information length
			_buffer[_offset++] = 0x00;
			_buffer[_offset++] = 0x00;
		}

		#endregion Constructors

		#region Enums

		public enum ListManagement
		{
			More,
			First,
			Last,
			Only,
			Add,
			Update,
		}

		public enum CommandId
		{
			OkDescrambling,
			OkMmi,
			Query,
			NotSelected,
		}

		#endregion Enums

		#region Methods

		public void AddCaDescriptor(int ca_system_id, int ca_pid, byte[] data)
		{
			if(_infoLengthPos == 0)
				throw new InvalidOperationException("Adding CA descriptor without program/stream");

			if(_offset + data.Length + 7 > _buffer.Length)
				throw new InvalidOperationException("Buffer overflow");
			
			// ca_pmt_cmd_id
			_buffer[_offset++] = (byte)CommandId.OkDescrambling;

			// CA descriptor tag & descriptor length
			_buffer[_offset++] = 0x09;
			_buffer[_offset++] = (byte)(4 + data.Length);

			_buffer[_offset++] = (byte)((ca_system_id >> 8) & 0xFF);
			_buffer[_offset++] = (byte)(ca_system_id & 0xFF);
			_buffer[_offset++] = (byte)((ca_pid >> 8) & 0xFF);
			_buffer[_offset++] = (byte)(ca_pid & 0xFF);

			if(data.Length > 0)
			{
				Array.Copy(data, 0, _buffer, _offset, data.Length);
				_offset += data.Length;
			}

			// update program_info_length/ES_info_length
			int l = _offset - _infoLengthPos - 2;

			_buffer[_infoLengthPos] = (byte)((l >> 8) & 0xFF);
			_buffer[_infoLengthPos + 1] = (byte)(l & 0xFF);
		}

		public void AddElementaryStream(int type, int pid)
		{
			if(_offset + 5 > _buffer.Length)
				throw new InvalidOperationException("Buffer overflow");

			_buffer[_offset++] = (byte)((type & 0xFF));
			_buffer[_offset++] = (byte)((pid >> 8) & 0xFF);
			_buffer[_offset++] = (byte)((pid & 0xFF));

			// ES_info_length
			_infoLengthPos = _offset;

			_buffer[_offset++] = 0x00;
			_buffer[_offset++] = 0x00;
		}

		#endregion Methods

		#region Properties

		public int Length
		{
			get { return _offset; }
		}

		public byte[] Data
		{
			get { return _buffer; }
		}

		#endregion Properties

		#region Fields

		int							_offset;
		int							_infoLengthPos;
		byte[]						_buffer; 

		#endregion Fields
	}
}
