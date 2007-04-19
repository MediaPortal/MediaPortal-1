using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using MediaLibrary.Settings;
using MediaLibrary.Configuration;

namespace MediaLibrary
{

    internal class MLImport : IMLImport
    {
        #region Members

        public IMediaLibrary _Library;
        private IMLImportPlugin _Plugin;
        private IMLHashItem _Properties;
        internal IMLImportDataSource _Database;
        private int import_id;
        private string name;
        private string section;
        private string update_mode;
        private string plugin_id;
        private string schedule_freq;
        private int schedule_interval;
        private DateTime schedule_time;
        private DateTime last_run_dtm;
        private bool wake_up;
        private bool run_missed;

        #endregion

        #region Constructors

        public MLImport()
        {
            this.import_id = 0;
            this._Database = null;
            this._Library = null;
            this._Properties = new MLHashItem() as IMLHashItem;
            this.name = "New Import";
            this.section = string.Empty;
            this.update_mode = "update";
            this.plugin_id = string.Empty;
            this.schedule_freq = "never";
            this.schedule_interval = 0;
            this.schedule_time = DateTime.ParseExact("00:00", "HH:mm", null);
            this.wake_up = true;
            this.run_missed = true;
        }

        #endregion

        #region Properties

        #region Read Only Properties

        public bool HasSchedule
        {
            get { return ScheduleFrequency != "never"; }
        }

        public bool MissedLastRun
        {
            get { return DateTime.Now > ScheduleTime; }
        }

        public string ScheduleDescription
        {
            //TODO Create description string from other properties
            get { return ""; }
        }

        #endregion

        #region Read/Write Properties

        public int ID
        {
            get {return import_id;}
            set { import_id = value; }
        }
        
        public DateTime LastRun
        {
            get { return last_run_dtm; }
            set { last_run_dtm = value; }
        }
        
        public string Mode
        {
            get { return update_mode; }
            set { update_mode = value; }
        }
        
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public string PluginID
        {
            get { return plugin_id; }
            set 
            { 
                plugin_id = value;
                _Properties.Clear();
            }
        }

        public IMLHashItem PluginProperties
        {
            get { return _Properties; }
            set { _Properties = value; }
        }
        
        public bool RunMissed
        {
            get { return run_missed; }
            set { run_missed = value; }
        }
        
        public string ScheduleFrequency
        {
            get { return schedule_freq; }
            set { schedule_freq = value; }
        }
        
        public int ScheduleInterval
        {
            get { return schedule_interval; }
            set { schedule_interval = value; }
        }
        
        public DateTime ScheduleTime
        {
            get { return schedule_time; }
            set { schedule_time = value; }
        }
        
        public string SectionName
        {
            get { return section; }
            set { section = value; }
        }
        
        public bool WakeUp
        {
            get { return wake_up; }
            set { wake_up = value; }
        }

        #endregion

        #endregion

        #region Methods

        public bool Run(IMLImportProgress Progress, out string ErrorText)
        {
            ErrorText = "";
            IMLSection Section = _Library.FindSection(this.SectionName, false);
            IMLHashItemList pluginlist = ((MediaLibraryClass)_Library).SystemObject.GetInstalledPlugins("import");
            try
            {
                if (Section != null)
                {
                    for (int i = 0; i < pluginlist.Count; i++)
                    {
                        IMLHashItem item = pluginlist[i];
                        MLPluginDescription desc = item["description"] as MLPluginDescription;
                        if (desc.information.plugin_id.ToString() == this.PluginID)
                        {
                            Type plugintype = MLSystem.GetTypeFromPlugin((string)item["path"]);
                            this._Plugin = (IMLImportPlugin)Activator.CreateInstance(plugintype);
                            MLExtentsions.SetProperties(this._Plugin, this.PluginProperties, out ErrorText);
                            this._Plugin.Import(Section, Progress);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                return false;
            }
            return false;
            
        }

        public void Save()
        {
            if (this.ID < 1)
                this._Database.AddNewImport(this);
            else
                this._Database.UpdateImport(this);
        }

        #endregion
    }
}
