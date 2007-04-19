#region Copyright (C) 2007 MeediOS

// Copyright (C) 2007 MeediOS
// http://www.MeediOS.com

//This Program is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 2, or (at your option)
//any later version.

//This Program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with GNU Make; see the file COPYING.  If not, write to
//the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
//http://www.gnu.org/copyleft/gpl.html

#endregion

using System;
using MeediOS;
using MeediOS.Library;
using MeediOS.Library.Database;
using System.Collections;
using Microsoft.Win32;

namespace MsSqlLibrary
{
    public class MsSqlLibrary : IMeedioLibraryPlugin
    {
        private ArrayList _Sections;
        private IMeedioItem _Properties;
        private string dataPath;

        public MsSqlLibrary()
        {
        }

        #region IMeedioPlugin Methods

        bool IMeedioPlugin.GetProperties(int Index, IMeedioPluginProperty Prop)
        {
            int i = 1;

            if (Index == i++)
            {
                Prop.Caption = "Select the location of the library";
                Prop.Name = "Path";
                Prop.DataType = "folder";
                Prop.DefaultValue = MeedioSystem.GetLibraryDirectoryS(null);
                Prop.HelpText = "";
                Prop.IsMandatory = true;
                return true;
            }
            if (Index == i++)
            {
                Prop.Caption = "Retrun all tags for each view step";
                Prop.Name = "AllTags";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Warning: Enabling this setting can degrade performance on Sections 10,000 "
                    + "items and above";
                Prop.IsMandatory = true;
                return true;
            }
            if (Index == i++)
            {
                Prop.Caption = "Specify the size of cache in MB";
                Prop.Name = "CacheSize";
                Prop.DataType = "int";
                Prop.DefaultValue = 2;
                Prop.HelpText = "Warning: Enabling this setting may speed up respose time when in modules, but "
                    + "it will also consume as much ram as you specify per section";
                Prop.IsMandatory = true;
                return true;
            }
            if (Index == i++)
            {
                Prop.Caption = "Use in-memory-database";
                Prop.Name = "InMemory";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Warning: Enabling this setting will speed up respose time when in modules, but "
                    + "it will also increase the load time for each module";
                Prop.IsMandatory = true;
                return true;
            }
            return false;
        }

        bool IMeedioPlugin.SetProperties(IMeedioItem Properties, out string Error)
        {
            dataPath = Convert.ToString(Properties["Path"]);
            _Properties = Properties;
            Error = "";
            return true;
        }

        bool IMeedioPlugin.EditCustomProperty(IntPtr Window, string PropertyName, ref string Value)
        {
            return true;
        }

        void IMeedioPlugin.OnMessage(IMeedioSystem MeedioSystem, IMeedioMessage Message)
        {
        }

        #endregion

        #region IMeedioLibrary Methods

        private bool HasSections
        {
            get { return _Sections != null; }
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
                        oSection.Database = new MLItemDataSource(DbFactory.GetProvider(ProviderType.SQLite, oSection.FileName, _Properties));
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

        public IMLImports GetImports()
        {
            DbProvider myProvider = DbFactory.GetProvider(ProviderType.SQLite, dataPath + "\\" + "library.imports", _Properties);
            IMLImportDataSource db = new MLImportDataSource(myProvider) as IMLImportDataSource;
            MLImports ips = db.GetImports() as MLImports;
            ips.Database = db;
            return ips as IMLImports;
        }

        public bool RunImport(int ImportID, IMLImportProgress Progress, out string ErrorText)
        {
            ErrorText = "";
            return true;
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
            if (lst != null)
                _Sections.AddRange(lst);
        }

        #endregion

    }
}
