using System;
using System.Collections;
using System.Diagnostics;
using SQLite.NET;

using Programs.Utils;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using GUIPrograms;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for appFilesEdit.
	/// </summary>
	public class appFilesEdit: ProgramsDatabase.AppItem
	{
		public appFilesEdit(SQLiteClient paramDB): base(paramDB)
		{
		}
	}
}
