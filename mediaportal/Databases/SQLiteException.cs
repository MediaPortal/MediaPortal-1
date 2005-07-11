using System;

namespace SQLite.NET
{
	/// <summary>
	/// 
	/// </summary>
	public class SQLiteException : Exception
	{
		private SQLiteClient.ResultCode errorCode;
 
		// Methods
		public SQLiteException(string message) : base(message)
		{
		}
 

		// Properties
		public SQLiteException(string message, SQLiteClient.ResultCode code) : base(message)
		{
			this.errorCode = code;
		}
 

		// Fields
		public SQLiteClient.ResultCode ErrorCode
		{
			get
			{
				return this.errorCode;
			}
		}
 
	}
 

}
