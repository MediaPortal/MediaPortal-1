
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Reflection;
using MediaPortal.GUI.Library;

namespace MediaPortal.Profile
{
	public class Xml :IDisposable
	{
		// Fields
        //static private string		m_rootName = "profile";
        //static private Encoding m_encoding = Encoding.UTF8;
        //static XmlDocument			m_doc=null;
        //static string						m_strFileName="";
        //static bool							m_bChanged=false;

        private string m_rootName = "profile";
        private Encoding m_encoding = Encoding.UTF8;
        XmlDocument m_doc = null;
        string m_strFileName = "";
        bool m_bChanged = false;

		/// <summary>
		///   Initializes a new instance of the Xml class by setting the <see cref="Profile.Name" /> to <see cref="Profile.DefaultName" />. </summary>
		public Xml()
		{
		}

		/// <summary>
		///   Initializes a new instance of the Xml class by setting the <see cref="Profile.Name" /> to the given file name. </summary>
		/// <param name="fileName">
		///   The name of the XML file to initialize the <see cref="Profile.Name" /> property with. </param>
		public Xml(string fileName) 
		{
			if (m_strFileName!=fileName)
			{
				Save();
				m_doc=null;
			}
			m_strFileName=fileName;
		}

		public void Clear()
		{
			m_doc=null;
			m_bChanged=false;
		}

		/// <summary>
		///   Retrieves an XMLDocument object based on the XML file (Name). </summary>
		/// <returns>
		///   The return value is the XMLDocument object based on the file, 
		///   or null if the file does not exist. </returns>
		private XmlDocument GetXmlDocument()
		{

			if (!File.Exists(m_strFileName))
			{
				if (File.Exists(m_strFileName+".bak"))
				{
					XmlDocument docBak = new XmlDocument();
					docBak.Load(m_strFileName+".bak");
					return docBak;
				}
				return null;
			}

			XmlDocument doc = new XmlDocument();
			doc.Load(m_strFileName);
			if (doc!=null && doc.DocumentElement!=null && doc.DocumentElement.ChildNodes!=null) return doc;
			if (File.Exists(m_strFileName+".bak"))
			{
				doc = new XmlDocument();
				doc.Load(m_strFileName+".bak");
			}						
			return doc;
		}

		/// <summary>
		///   Retrieves the XPath string used for retrieving a section from the XML file. </summary>
		/// <returns>
		///   An XPath string. </returns>
		/// <seealso cref="GetEntryPath" />
		private string GetSectionsPath(string section)
		{
			return "section[@name=\"" + section + "\"]";
		}
		                              
		/// <summary>
		///   Retrieves the XPath string used for retrieving an entry from the XML file. </summary>
		/// <returns>
		///   An XPath string. </returns>
		/// <seealso cref="GetSectionsPath" />
		private string GetEntryPath(string entry)
		{
			return "entry[@name=\"" + entry + "\"]";
		}

		/// <summary>
		///   Sets the value for an entry inside a section. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry. </param>
		/// <param name="entry">
		///   The name of the entry where the value will be set. </param>
		/// <param name="value">
		///   The value to set. If it's null, the entry is removed. </param>
		/// <exception cref="InvalidOperationException"><see cref="Profile.ReadOnly" /> is true. </exception>
		/// <exception cref="InvalidOperationException"><see cref="Profile.Name" /> is null or empty. </exception>
		/// <exception cref="ArgumentNullException">Either section or entry is null. </exception>
		/// <remarks>
		///   If the XML file does not exist, it is created.
		///   The <see cref="Profile.Changing" /> event is raised before setting the value.  
		///   If its <see cref="ProfileChangingArgs.Cancel" /> property is set to true, this method 
		///   returns immediately without setting the value.  After the value has been set, 
		///   the <see cref="Profile.Changed" /> event is raised. </remarks>
		/// <seealso cref="GetValue" />
    public void Save()
    {
			lock (typeof(MediaPortal.Profile.Xml))
			{
				if (m_doc==null) return;
				if (m_doc.DocumentElement==null) return;
				if (m_doc.ChildNodes.Count==0) return;
				if (m_doc.DocumentElement.ChildNodes==null) return;
				if (!m_bChanged) return;
				try
				{
					try
					{
						System.IO.File.Delete(m_strFileName+".bak");
						System.IO.File.Move(m_strFileName, m_strFileName+".bak");
					}
					catch (Exception) {}

					using (StreamWriter stream = new StreamWriter(m_strFileName, false))
					{
						m_doc.Save(stream);		
						m_doc=null;
						stream.Flush();
						stream.Close();
					}
					m_bChanged=false;
				}
				catch(Exception ex)
				{
					Log.Write("Unable to save {0} {1}",ex.Message);
				}
				m_doc=null;
			}
    }

		public void SetValue(string section, string entry, object value)
		{
      lock ( typeof(Xml) )
      {
        // If the value is null, remove the entry
        if (value == null)
        {
          RemoveEntry(section, entry);
          return;
        }
  			
        string valueString = value.ToString();

        // If the file does not exist, use the writer to quickly create it
        if (!File.Exists(m_strFileName))
        {	
          XmlTextWriter writer = new XmlTextWriter(m_strFileName, m_encoding);			
          writer.Formatting = Formatting.Indented;
  	            
          writer.WriteStartDocument();
  				
          writer.WriteStartElement(m_rootName);			
          writer.WriteStartElement("section");
          writer.WriteAttributeString("name", null, section);				
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("name", null, entry);				
          if (valueString != "")
            writer.WriteString(valueString);
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.Close();            				
          m_doc=null;
          return;
        }
  			
        
        if (m_doc==null)
        {
          m_doc=GetXmlDocument();
        }
        if (m_doc==null) return;

        XmlElement root = m_doc.DocumentElement;
  			
        // Get the section element and add it if it's not there
        XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
        if (sectionNode == null)
        {
          XmlElement element = m_doc.CreateElement("section");
          XmlAttribute attribute = m_doc.CreateAttribute("name");
          attribute.Value = section;
          element.Attributes.Append(attribute);			
          sectionNode = root.AppendChild(element);			
        }

        // Get the entry element and add it if it's not there
        XmlNode entryNode = sectionNode.SelectSingleNode(GetEntryPath(entry));
        if (entryNode == null)
        {
          XmlElement element = m_doc.CreateElement("entry");
          XmlAttribute attribute = m_doc.CreateAttribute("name");
          attribute.Value = entry;
          element.Attributes.Append(attribute);			
          entryNode = sectionNode.AppendChild(element);			
        }

        // Add the value and save the file
        //if (valueString != "")
        entryNode.InnerText = valueString;
        m_bChanged=true;
      }
		}

    public string GetValueAsString(string section, string entry, string strDefault)
    {
      string strValue=(string)GetValue(section,entry);
      if( strValue==null) return strDefault;
      if (strValue.Length==0) return strDefault;
      return strValue;
    }

    public bool GetValueAsBool(string section, string entry, bool bDefault)
    {
      string strValue=(string)GetValue(section,entry);
      if( strValue==null) return bDefault;
      if (strValue.Length==0) return bDefault;
      if (strValue=="yes") return true;
      return false;
    }
    public int GetValueAsInt(string section, string entry, int iDefault)
    {
      string strValue=(string)GetValue(section,entry);
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
      string strValue=(string)GetValue(section,entry);
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
		/// <summary>
		///   Retrieves the value of an entry inside a section. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry with the value. </param>
		/// <param name="entry">
		///   The name of the entry where the value is stored. </param>
		/// <returns>
		///   The return value is the entry's value, or null if the entry does not exist. </returns>
		/// <exception cref="ArgumentNullException">Either section or entry is null. </exception>
		/// <seealso cref="SetValue" />
		/// <seealso cref="Profile.HasEntry" />

		public object GetValue(string section, string entry)
		{
      lock ( typeof(Xml) )
      {
  			
        try
        { 	
          if (m_doc==null)
            m_doc = GetXmlDocument();
          if (m_doc==null) return null;

          XmlElement root = m_doc.DocumentElement;
  				
          XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
          if (entryNode==null) return null;
          return entryNode.InnerText;
        }
        catch
        {	
          return null;
        }
      }
		}

		/// <summary>
		///   Removes an entry from a section. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry. </param>
		/// <param name="entry">
		///   The name of the entry to remove. </param>
		/// <exception cref="InvalidOperationException"><see cref="Profile.ReadOnly" /> is true. </exception>
		/// <exception cref="ArgumentNullException">Either section or entry is null. </exception>
		/// <remarks>
		///   The <see cref="Profile.Changing" /> event is raised before removing the entry.  
		///   If its <see cref="ProfileChangingArgs.Cancel" /> property is set to true, this method 
		///   returns immediately without removing the entry.  After the entry has been removed, 
		///   the <see cref="Profile.Changed" /> event is raised. </remarks>
		/// <seealso cref="RemoveSection" />
		public void RemoveEntry(string section, string entry)
		{

			// Verify the file exists
      if (m_doc == null)
      {
        m_doc = GetXmlDocument();
        if (m_doc==null) return;
      }

			// Get the entry's node, if it exists
			XmlElement root = m_doc.DocumentElement;			
			XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
			if (entryNode == null)
				return;

			
			entryNode.ParentNode.RemoveChild(entryNode);			
      m_bChanged=true;
		}

		/// <summary>
		///   Removes a section. </summary>
		/// <param name="section">
		///   The name of the section to remove. </param>
		/// <exception cref="InvalidOperationException"><see cref="Profile.ReadOnly" /> is true. </exception>
		/// <exception cref="ArgumentNullException">section is null. </exception>
		/// <remarks>
		///   The <see cref="Profile.Changing" /> event is raised before removing the section.  
		///   If its <see cref="ProfileChangingArgs.Cancel" /> property is set to true, this method 
		///   returns immediately without removing the section.  After the section has been removed, 
		///   the <see cref="Profile.Changed" /> event is raised. </remarks>
		/// <seealso cref="RemoveEntry" />
		public void RemoveSection(string section)
		{

			// Verify the file exists
      if (m_doc==null)
      {
        m_doc = GetXmlDocument();
        if (m_doc == null)
          return;
      }
			// Get the section's node, if it exists
			XmlElement root = m_doc.DocumentElement;			
			XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
			if (sectionNode == null)
				return;
			
			root.RemoveChild(sectionNode);
      m_bChanged=true;
    }
    #region IDisposable Members

    public void Dispose()
    {
      Save();
    }

    #endregion
  }
}
