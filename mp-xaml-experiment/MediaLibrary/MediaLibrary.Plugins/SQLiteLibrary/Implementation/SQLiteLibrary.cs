

using System;
using MediaLibrary;
using MediaLibrary.Database;
using System.Collections;
using Microsoft.Win32;

namespace SQLiteLibrary
{
    public class SQLiteLibrary : IMLDatabasePlugin
    {
        private ArrayList _Sections;
        private IMLHashItem _Properties;
        private string dataPath;

        private bool HasSections
        {
            get { return _Sections != null; }
        }

        public SQLiteLibrary()
        {
        }

        #region IMLPlugin Methods

        public bool GetProperties(IMLPluginProperties Properties)
        {
            IMLPluginProperty Prop;
            Prop = Properties.AddNew("Path");
            {
                Prop.Caption = "Select the location of the library";
                Prop.DataType = "folder";
                Prop.DefaultValue = ".\\library";
                Prop.HelpText = "";
                Prop.IsMandatory = true;
            }
            Prop = Properties.AddNew("AllTags");
            {
                Prop.Caption = "Retrun all tags for each view step";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Warning: Enabling this setting can degrade performance on Sections 10,000 "
                    + "items and above";
                Prop.IsMandatory = true;
            }
            Prop = Properties.AddNew("CacheSize");
            {
                Prop.Caption = "Specify the size of cache in MB";
                Prop.DataType = "int";
                Prop.DefaultValue = 2;
                Prop.HelpText = "Warning: Enabling this setting may speed up respose time when in modules, but "
                    + "it will also consume as much ram as you specify per section";
                Prop.IsMandatory = true;
            }
            Prop = Properties.AddNew("InMemory");
            {
                Prop.Caption = "Use in-memory-database";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Warning: Enabling this setting will speed up respose time when in modules, but "
                    + "it will also increase the load time for each module";
                Prop.IsMandatory = true;
            }
            return true;
        }

        public bool SetProperties(IMLHashItem Properties, out string Error)
        {
            dataPath = Convert.ToString(Properties["Path"]);
            _Properties = Properties;
            Error = "";
            return true;
        }

        public bool ValidateProperties(IMLPluginProperties PropCollection, IMLHashItem PropValues)
        {
            return true;
        }

        public bool EditCustomProperty(IntPtr Window, string PropertyName, ref string Value)
        {
            return true;
        }

        #endregion

        #region IMLDatabasePlugin Methods

        public void LoadPlugin(IMLSystem SystemObject)
        {

        }

        public int SectionCount
        {
            get 
            {
                if (!HasSections)
                    GetSectionList();
                return _Sections.Count; 
            }
        }

        public bool DeleteSection(string section)
        {
            if (!HasSections)
                GetSectionList();
            try
            {
                if (DbFactory.DeleteSection(ProviderType.SQLite, section, _Properties))
                {
                    for (int i = 0; i < _Sections.Count; i++)
                    {
                        if (_Sections[i].ToString().ToLower().Trim() == section.ToLower().Trim())
                        {
                            _Sections.RemoveAt(i);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }

        public IMLSection FindSection(string Section, bool Create)
        {
            MLSection oSection;
            bool bExists = false;
            if (!HasSections)
                GetSectionList();
            try
            {
                for (int i = 0; i < _Sections.Count; i++)
                {
                    if (_Sections[i].ToString().ToLower().Trim() == Section.ToLower().Trim())
                    {
                        bExists = true;
                        break;
                    }
                }
                if (bExists == false)
                {
                    if (Create)
                    {
                        oSection = new MLSection();
                        oSection.Name = Section;
                        oSection.FileName = dataPath + "\\" + Section + ".mlf";
                        oSection.Database = new MLItemDataSource(DbFactory.GetProvider(ProviderType.SQLite,oSection.FileName,_Properties));
                        _Sections.Add(Section);
                        _Sections.Sort();
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    oSection = new MLSection();
                    oSection.Name = Section;
                    oSection.FileName = dataPath + "\\" + Section + ".mlf";
                    oSection.Database = new MLItemDataSource(DbFactory.GetProvider(ProviderType.SQLite, oSection.FileName, _Properties));
                }
                return oSection as IMLSection;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }
       
        public string Sections(int Index)
        {
            if (!HasSections)
                GetSectionList();
            return _Sections[Index].ToString();
        }

        private void GetSectionList()
        {
            _Sections = new ArrayList();
            ArrayList lst = DbFactory.GetSections(ProviderType.SQLite, _Properties);
            if(lst != null)
                _Sections.AddRange(lst);
        }

        #endregion

    }
}