using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaLibrary;
using MediaLibrary.Settings;
using MediaLibrary.Configuration;

namespace MediaManager
{
    class MLPluginPropertyControlCollection
    {
        private string PluginType;
        private Panel TargetPanel;
        private IMediaLibrary Library;

        private List<MLPluginPropertyControl> PluginControls;
        private IMLPlugin _Plugin;
        private Guid PluginId;
        private IMLHashItem _PluginPropertyValues;
        private IMLPluginProperties PluginProperties;

       

        public IMLPlugin Plugin
        {
            get { return _Plugin; }
        }
        public IMLHashItem PluginPropertyValues
        {
            get { return _PluginPropertyValues; }
        }
       

        public MLPluginPropertyControlCollection(string PluginType, Panel TargetPanel, IMediaLibrary Library)
        {
            this.PluginType = PluginType;
            this.Library = Library;
            this.TargetPanel = TargetPanel;
            this.PluginControls = new List<MLPluginPropertyControl>();
        }

        public void Clear()
        {
            this._Plugin = null;
            this.PluginControls.Clear();
            this.PluginProperties = null;
            this._PluginPropertyValues = null;
            this.PluginId = Guid.Empty;
            this.TargetPanel.Controls.Clear();
        }

        public bool LoadPlugin(Guid PluginId)
        {
            Clear();

            IMLHashItemList plugins = Library.SystemObject.GetInstalledPlugins(PluginType);
            foreach (IMLHashItem plugin in plugins)
            {
                MLPluginDescription desc = plugin["description"] as MLPluginDescription;
                if (desc.information.plugin_id == PluginId)
                {
                    Type plugintype = MLSystem.GetTypeFromPlugin((string)plugin["path"]);
                    this._Plugin = (IMLPlugin)Activator.CreateInstance(plugintype);
                    return Plugin != null;
                }
            }
            return false;
        }

        public void LoadProperties(IMLPluginProperties PluginProperties, IMLHashItem PluginPropertyValues)
        {
            this.PluginProperties = PluginProperties;
            this._PluginPropertyValues = PluginPropertyValues;

            this.TargetPanel.Controls.Clear();

            // if there's no property, we fake one
            if (PluginProperties.Count == 0)
            {
                IMLPluginProperty property = PluginProperties.AddNew(MLPluginProperty.NoPropName);
                property.Caption = "This plugin has no properties.";
                property.DataType = "label";
            }

            //copy properties over
            foreach (string key in PluginPropertyValues.Keys)
                if (PluginProperties.Contains(key))
                    ((MLPluginProperty)PluginProperties[key]).Value = PluginPropertyValues[key];

            foreach (MLPluginProperty Property in PluginProperties)
            {
                MLPluginPropertyControl control = new MLPluginPropertyControl(Property);
                control.Name = Property.Name;
                control.Plugin = this.Plugin;
                PluginControls.Add(control);
                AddControl(TargetPanel, control);
            }
        }

        public bool SaveProperties()
        {
            if (this.Plugin != null)
            {
                PluginPropertyValues.Clear();
                foreach (MLPluginPropertyControl control in PluginControls)
                    if (control.Name != MLPluginProperty.NoPropName) // There might be the "no props" label
                        PluginPropertyValues[control.Key] = control.Value;
                return true;
            }
            return false;
        }

        #region private void AddControl(Panel tabPage, MLPluginPropertyControl ppControl)
        /// <summary>
        /// Adds a MLPluginPropertyControl to the lower end of a panel.
        /// </summary>
        /// <param name="tabPage">The panel to add the control to.</param>
        /// <param name="ppControl">The control to add.</param>
        private void AddControl(Panel tabPage, MLPluginPropertyControl ppControl)
        {
            ppControl.Height = (int)tabPage.Font.Size * 3;
            ppControl.Top = tabPage.Controls.Count * (int)tabPage.Font.Size * 3;
            ppControl.Left = 0;
            ppControl.Width = tabPage.ClientRectangle.Width;
            if (tabPage.Controls.Count % 2 == 1)
                ppControl.BackColor = Color.White;
            else
                ppControl.BackColor = Color.FromArgb(128, Color.Cyan);


            tabPage.Controls.Add(ppControl);
        }
        #endregion
    }


}
