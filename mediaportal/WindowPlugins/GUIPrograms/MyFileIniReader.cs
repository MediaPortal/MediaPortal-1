using System;
using System.IO;
using System.Text;

using MediaPortal.GUI.Library;		


namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for MyFileIniReader.
	/// </summary>
	public class MyFileIniReader
	{
		private string m_FileName = "";

		// event: read new section line
		public delegate void IniEventHandler (string strLine);
		public event IniEventHandler OnReadNewSection = null;
		// event: read new entry line
		public event IniEventHandler OnReadNewEntry = null;
		// event: read a no-section, no-entry line
		public event IniEventHandler OnReadAdditionalLine = null;
		// event: read the end of a section
		public event IniEventHandler OnReadEndSection = null;

		private bool IsSectionLine(string strVal)
		{
			return (strVal.StartsWith("[")) && (strVal.EndsWith("]"));
		}

		private bool IsEntryValueLine(string strLine)
		{
			int nFirstSpacePos = strLine.IndexOf(" ");
			int nFirstEqualPos = strLine.IndexOf("=");
			bool bRes = false;
			if (nFirstEqualPos == -1) 
			{ 
				bRes = false; 
			}
			else if (nFirstSpacePos == -1)
			{
				bRes = true;
			}
			else 
			{
				bRes = (nFirstEqualPos < nFirstSpacePos);
			}
			return bRes;
		}

		public void Start()
		{
			string strNextLine;
			bool HasReadOneSection = false;
			if (m_FileName =="") return;
			if (!System.IO.File.Exists(m_FileName)) 
			{
				Log.Write("MyFileIniReader: INI-File not found ({0})", m_FileName);
				return;
			} 
			StreamReader reader = new StreamReader(m_FileName, Encoding.GetEncoding(1252));
			try
			{
				do
				{
					strNextLine = reader.ReadLine().Trim();

					// check if line is a section
					if (IsSectionLine(strNextLine))
					{
						if (HasReadOneSection)
						{
							this.OnReadEndSection("");
						}
						this.OnReadNewSection(strNextLine);
						HasReadOneSection = true;
					}
					else if (IsEntryValueLine(strNextLine)) 
					{
						this.OnReadNewEntry(strNextLine);
					}
					else
					{
						// otherwise send the line as is and decide elsewhere if this is useful!
						this.OnReadAdditionalLine(strNextLine);
					}
				}
				while (reader.Peek() != -1);
				if (HasReadOneSection)
				{
					this.OnReadEndSection("");
				}
			}
			finally
			{
				reader.Close();
			}

		}



		public MyFileIniReader(string strFileName)
		{
			m_FileName = strFileName;
		}
	}
}
