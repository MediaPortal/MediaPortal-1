using System;

namespace SQLite.NET
{
	/// <summary>
	/// 
	/// </summary>
	public class SQLiteException : Exception
	{
		private SQLiteClient.SqliteError errorCode;
 
		// Methods
		public SQLiteException(string message) : base(message)
		{
		}
 

		// Properties
		public SQLiteException(string message, SQLiteClient.SqliteError code) : base(message)
		{
			this.errorCode = code;
		}
 

		// Fields
		public SQLiteClient.SqliteError ErrorCode
		{
			get
			{
				return this.errorCode;
			}
		}
 
	}
 

}
