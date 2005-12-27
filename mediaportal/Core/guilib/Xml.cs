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
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Reflection;
using MediaPortal.GUI.Library;

namespace MediaPortal.Profile
{
	#region XmlDoc class
	public class XmlDoc
	{
		XmlDocument document=null;
		bool        modified=false;
		string      filename=null;
		
		public XmlDoc(string xmlFileName)
		{
			filename=xmlFileName;
			document = new XmlDocument();
			if (File.Exists(filename))
			{
				document.Load(filename);
				if (document.DocumentElement==null) document=null;
			}
			else if (File.Exists(filename+".bak"))
			{
				document.Load(filename+".bak");
				if (document.DocumentElement==null) document=null;
			}
			if (document==null)
				document=new XmlDocument();
			
		}
		public string FileName
		{
			get { return filename;}
		}

		public object GetValue(string section, string entry)
		{
			if (document==null) return null;

			XmlElement root = document.DocumentElement;
			if (root == null) return null;
			XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
			if (entryNode==null) return null;
			return entryNode.InnerText;
		}

		public void Save()
		{
			if (!modified) return;
			if (document==null) return;
			if (document.DocumentElement==null) return;
			if (document.ChildNodes.Count==0) return;
			if (document.DocumentElement.ChildNodes==null) return;
			try
			{
				if (File.Exists(filename+".bak")) File.Delete(filename+".bak");
				if (File.Exists(filename)) File.Move(filename, filename+".bak");
			}
			catch (Exception) {}

			using (StreamWriter stream = new StreamWriter(filename, false))
			{
				document.Save(stream);		
				stream.Flush();
				stream.Close();
			}
			modified=false;
		}
		public void SetValue(string section, string entry, object value)
		{
			// If the value is null, remove the entry
			if (value == null)
			{
				RemoveEntry(section, entry);
				return;
			}
  			
			string valueString = value.ToString();

			if (document.DocumentElement==null)
			{
				XmlElement node = document.CreateElement("profile");
				document.AppendChild(node);
			}
			XmlElement root = document.DocumentElement;
			// Get the section element and add it if it's not there
			XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
			if (sectionNode == null)
			{
				XmlElement element = document.CreateElement("section");
				XmlAttribute attribute = document.CreateAttribute("name");
				attribute.Value = section;
				element.Attributes.Append(attribute);			
				sectionNode = root.AppendChild(element);			
			}

			// Get the entry element and add it if it's not there
			XmlNode entryNode = sectionNode.SelectSingleNode(GetEntryPath(entry));
			if (entryNode == null)
			{
				XmlElement element = document.CreateElement("entry");
				XmlAttribute attribute = document.CreateAttribute("name");
				attribute.Value = entry;
				element.Attributes.Append(attribute);			
				entryNode = sectionNode.AppendChild(element);			
			}
			entryNode.InnerText = valueString;
			modified=true;
		}

		public void RemoveEntry(string section, string entry)
		{

			// Verify the file exists
			if (document == null) return;
			if (document.DocumentElement==null) return;
			// Get the entry's node, if it exists
			XmlElement root = document.DocumentElement;			
			XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
			if (entryNode == null)
				return;
			entryNode.ParentNode.RemoveChild(entryNode);			
			modified=true;
		}
		private string GetSectionsPath(string section)
		{
			return "section[@name=\"" + section + "\"]";
		}
		private string GetEntryPath(string entry)
		{
			return "entry[@name=\"" + entry + "\"]";
		}
	}
	#endregion

	public class Xml :IDisposable
	{
		public Xml(string fileName) : this(fileName, true)
		{
		}

		public Xml(string fileName, bool isCached) 
		{
			xmlFileName=System.IO.Path.GetFileName(fileName);

			_isCached = isCached;

			if(_isCached)
			{
				foreach (XmlDoc doc in xmlCache)
				{
					string xmlName=System.IO.Path.GetFileName(doc.FileName);
					if (String.Compare(xmlName,xmlFileName,true)==0)
					{
						xmlDoc=doc;
						break;
					}
				}
			}

			if(xmlDoc==null)
			{
				xmlDoc = new XmlDoc(fileName);

				if(_isCached)
					xmlCache.Add(xmlDoc);
			}
		}
		
		public void SetValue(string section, string entry, object objValue)
		{
			xmlDoc.SetValue(section, entry, objValue);
		}
		public string GetValue(string section, string entry)
		{
			string strValue=(string)xmlDoc.GetValue(section,entry);
			if( strValue==null) return String.Empty;
			if (strValue.Length==0) return String.Empty;
			return strValue;
		}

		public string GetValueAsString(string section, string entry, string strDefault)
		{
			string strValue=(string)xmlDoc.GetValue(section,entry);
			if( strValue==null) return strDefault;
			if (strValue.Length==0) return strDefault;
			return strValue;
		}
		public bool GetValueAsBool(string section, string entry, bool bDefault)
		{
			string strValue=(string)xmlDoc.GetValue(section,entry);
			if( strValue==null) return bDefault;
			if (strValue.Length==0) return bDefault;
			if (strValue=="yes") return true;
			return false;
		}
		public int GetValueAsInt(string section, string entry, int iDefault)
		{
			string strValue=(string)xmlDoc.GetValue(section,entry);
			if( strValue==null) return iDefault;
			if (strValue.Length==0) return iDefault;
			try
			{
				int iRet=System.Int32.Parse(strValue);
				return iRet;
			}
			catch(Exception)
			{
			}
			return iDefault;
		}
    
		public float GetValueAsFloat(string section, string entry, float fDefault)
		{
			string strValue=(string)xmlDoc.GetValue(section,entry);
			if( strValue==null) return fDefault;
			if (strValue.Length==0) return fDefault;
			try
			{
				float fRet=(float)System.Double.Parse(strValue);
				return fRet;
			}
			catch(Exception)
			{
			}
			return fDefault;
		}
		public void SetValueAsBool(string section, string entry, bool bValue)
		{
			string strValue="yes";
			if (!bValue) strValue="no";
			SetValue(section,entry,strValue);
		}
		public void RemoveEntry(string section, string entry)
		{
			xmlDoc.RemoveEntry(section, entry);
		}
		#region IDisposable Members

		public void Dispose()
		{
			if(_isCached == false)
				xmlDoc.Save();
		}

		public void Clear()
		{
		}

		static public void SaveCache()
		{
			foreach (XmlDoc doc in xmlCache)
			{
				doc.Save();
			}
		}
		#endregion

		#region Fields

		bool						_isCached;
		static ArrayList			xmlCache=new ArrayList();
		string						xmlFileName;
		XmlDoc						xmlDoc=null;

		#endregion Fields
	}
}
