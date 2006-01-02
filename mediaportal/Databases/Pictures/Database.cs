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
using System.Collections;
using SQLite.NET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Database;

namespace MediaPortal.Picture.Database
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class PictureDatabase : IDisposable
	{
		bool disposed=false;
		SQLiteClient m_db=null;

    

		public PictureDatabase()
		{
			Open();
		}
		void Open()
		{
			lock (typeof(PictureDatabase))
			{
				Log.WriteFile(Log.LogType.Log,false,"opening picture database");
				try 
				{
					// Open database

					String strPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.
						GetExecutingAssembly().Location); 
					try
					{
						System.IO.Directory.CreateDirectory(strPath+@"\database");
					}
					catch(Exception){}
					m_db = new SQLiteClient(strPath+@"\database\PictureDatabase.db3");
					m_db.Execute("PRAGMA cache_size=2000;\n");
					m_db.Execute("PRAGMA synchronous='OFF';\n");
					m_db.Execute("PRAGMA count_changes=1;\n");
					m_db.Execute("PRAGMA full_column_names=0;\n");
          m_db.Execute("PRAGMA short_column_names=0;\n");
          m_db.Execute("PRAGMA auto_vacuum=1;\n");
          m_db.Execute("vacuum");
					CreateTables();

				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"picture database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				Log.WriteFile(Log.LogType.Log,false,"picture database opened");
			}
		}
		bool CreateTables()
		{
			lock (typeof(PictureDatabase))
			{
				if (m_db==null) return false;
				//Changed mbuzina
				AddTable("picture","CREATE TABLE picture ( idPicture integer primary key, strFile text, iRotation integer, strDateTaken text);\n");
				//End Changed
				return true;
			}
		}
		public bool AddTable( string strTable, string strSQL)
		{
			//	lock (typeof(DatabaseUtility))
		
			Log.Write("AddTable: {0}",strTable);
			if (m_db==null) 
			{
				Log.Write("AddTable: database not opened");
				return false;
			}
			if (strSQL==null) 
			{
				Log.Write("AddTable: no sql?");
				return false;
			}
			if (strTable==null) 
			{
				Log.Write("AddTable: No table?");
				return false;
			}
			if (strTable.Length==0) 
			{
				Log.Write("AddTable: empty table?");
				return false;
			}
			if (strSQL.Length==0) 
			{
				Log.Write("AddTable: empty sql?");
				return false;
			}

			//Log.Write("check for  table:{0}", strTable);
			SQLiteResultSet results;
			results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+strTable+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
			if (results!=null)
			{
				if (results.Rows.Count>0) 
					// Changed mbuzina
				{
					if(DatabaseUtility.TableColumnExists(m_db,strTable,"strDateTaken"))
					{
						return false;
					} 
					else 
					{
						try 
						{
							m_db.Execute("alter table "+strTable+" add strDateTaken text");
						}
						catch(SQLite.NET.SQLiteException ex) 
						{
							Log.WriteFile(Log.LogType.Log,true,"DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message,ex.StackTrace,strSQL);
						} 
						return true;
					}
				}
				//End Change
			}

			try 
			{
				//Log.Write("create table:{0}", strSQL);
				m_db.Execute(strSQL);
				//Log.Write("table created");
			}
			catch (SQLite.NET.SQLiteException ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message,ex.StackTrace,strSQL);
			}
			return true;
		}

		public int AddPicture(string strPicture, int iRotation)
		{
			lock (typeof(PictureDatabase))
			{
				if (m_db==null) return -1;
				string strSQL="";
				try
				{
					int lPicId=-1;
					SQLiteResultSet results;
					string strPic=strPicture;
					string	strDateTaken;
					DatabaseUtility.RemoveInvalidChars(ref strPic);

					strSQL=String.Format("select * from picture where strFile like '{0}'",strPic);
					results=m_db.Execute(strSQL);
					if (results!=null && results.Rows.Count>0) 
					{
						lPicId=System.Int32.Parse( DatabaseUtility.Get(results,0,"idPicture"));
						return lPicId;
					}
					//Changed mbuzina
					using (ExifMetadata extractor = new ExifMetadata())
					{
						ExifMetadata.Metadata metaData=extractor.GetExifMetadata(strPic);
						strDateTaken      = System.DateTime.Parse(metaData.DatePictureTaken.DisplayValue).ToString("yyyy-MM-dd HH:mm:ss");

						// Smirnoff: Query the orientation information
//						if(iRotation == -1)
							iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Hex));
					}

					strSQL=String.Format ("insert into picture (idPicture, strFile, iRotation, strDateTaken) values(null, '{0}',{1},'{2}')", strPic, iRotation,strDateTaken);
					//End Changed

					results=m_db.Execute(strSQL);
					lPicId=m_db.LastInsertID();
					return lPicId;
				}
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return -1;
			}
		}

		public void DeletePicture(string strPicture)
		{
			lock (typeof(PictureDatabase))
			{
				if (m_db==null) return;
				string strSQL="";
				try
				{					
					string strPic=strPicture;
					DatabaseUtility.RemoveInvalidChars(ref strPic);

					strSQL=String.Format("delete from picture where strFile like '{0}'",strPic);
					m_db.Execute(strSQL);
				}
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"MediaPortal.Picture.Database exception deleting picture err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return;
			}
		}

		public int GetRotation(string strPicture)
		{
			lock (typeof(PictureDatabase))
			{
				if (m_db==null) return -1;
				string strSQL="";
				try
				{
					SQLiteResultSet results;
					string strPic=strPicture;
					int iRotation;
					DatabaseUtility.RemoveInvalidChars(ref strPic);

					strSQL=String.Format("select * from picture where strFile like '{0}'",strPic);
					results=m_db.Execute(strSQL);
					if (results!=null && results.Rows.Count>0) 
					{
						iRotation=System.Int32.Parse( DatabaseUtility.Get(results,0,"iRotation"));
						return iRotation;
					}

					ExifMetadata extractor = new ExifMetadata();
					ExifMetadata.Metadata metaData=extractor.GetExifMetadata(strPicture);
					iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Hex));

					AddPicture(strPicture, iRotation);
					return 0;
				}
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return 0;
			}
		}

		public void SetRotation(string strPicture, int iRotation)
		{
			lock (typeof(PictureDatabase))
			{
				if (m_db==null) return ;
				string strSQL="";
				try
				{
					SQLiteResultSet results;
					string strPic=strPicture;
					DatabaseUtility.RemoveInvalidChars(ref strPic);

					long lPicId=AddPicture(strPicture, iRotation);
					if (lPicId>=0)
					{
						strSQL=String.Format("update picture set iRotation={0} where strFile like '{1}'",iRotation,strPic);
						results=m_db.Execute(strSQL);
					}
				}
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		//Changed mbuzina
		public DateTime GetDateTaken(string strPicture)
		{
			lock (typeof(PictureDatabase))
			{
				if (m_db==null) return DateTime.MinValue;
				string strSQL="";
				try
				{
					SQLiteResultSet results;
					string strPic=strPicture;
					string strDateTime;
					DatabaseUtility.RemoveInvalidChars(ref strPic);

					strSQL=String.Format("select * from picture where strFile like '{0}'",strPic);
					results=m_db.Execute(strSQL);
					if (results!=null && results.Rows.Count>0) 
					{
						strDateTime = DatabaseUtility.Get(results,0,"strDateTaken");
						if(strDateTime != String.Empty && strDateTime != "") 
						{
							DateTime dtDateTime=DateTime.ParseExact(strDateTime,"yyyy-MM-dd HH:mm:ss",new System.Globalization.CultureInfo(""));
							return dtDateTime;
						}
					}
					AddPicture(strPicture,-1);
					using (ExifMetadata extractor = new ExifMetadata())
					{
						ExifMetadata.Metadata metaData=extractor.GetExifMetadata(strPic);
						strDateTime      = System.DateTime.Parse(metaData.DatePictureTaken.DisplayValue).ToString("yyyy-MM-dd HH:mm:ss");
					}
					if(strDateTime != String.Empty && strDateTime != "") 
					{
						DateTime dtDateTime=DateTime.ParseExact(strDateTime,"yyyy-MM-dd HH:mm:ss",new System.Globalization.CultureInfo(""));
						return dtDateTime;
					} 
					else 
					{
						return DateTime.MinValue;
					}
				}
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return DateTime.MinValue;
			}
		}
		//End Changed

		public int EXIFOrientationToRotation(int orientation)
		{
			Log.Write("Orientation: {0}", orientation);

			if(orientation == 6)
				return 1;

			if(orientation == 3)
				return 2;

			if(orientation == 8)
				return 3;

			return 0;
		}
			
		#region IDisposable Members

		public void Dispose()
		{
			if (!disposed)
			{
				disposed=true;
				if (m_db!=null)
				{
					try
					{
						m_db.Close();
					}
					catch (Exception){}
					m_db=null;
				}
			}
		}

		#endregion
	}
}
