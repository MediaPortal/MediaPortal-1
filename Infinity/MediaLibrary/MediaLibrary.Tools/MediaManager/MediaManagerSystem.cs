using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaLibrary;
using MediaLibrary.Configuration;

namespace MediaManager
{
    public class DisplayValue
    {
        private string m_Display;
        private string m_Value;
        public DisplayValue(string Display, string Value)
        {
            m_Display = Display;
            m_Value = Value;
        }
        public string Display
        {
            get { return m_Display; }
        }
        public string Value
        {
            get { return m_Value; }
        }
    }

    class MediaManagerSystem
    {
        private IMediaLibrary _Library;
        private IMLSection _CurrentSection;
        private ListViewItem _CurrentDbPluginListViewItem;

        public MediaManagerSystem(string ConfigPath)
        {
            _Library = new MediaLibraryClass(ConfigPath);
            _CurrentSection = null;
        }

        

        public IMediaLibrary Library
        {
            get { return _Library; }
            set { _Library = value; }
        }

        public IMLSection CurrentSection
        {
            get { return _CurrentSection; }
            set { _CurrentSection = value; }
        }

        public ListViewItem CurrentDbPluginListViewItem
        {
            get { return _CurrentDbPluginListViewItem; }
            set { _CurrentDbPluginListViewItem = value; }
        }

        public void RefreshSections(ListView SectionsListView)
        {
            SectionsListView.Clear();
            for (int i = 0; i < Library.SectionCount; i++)
            {
                SectionsListView.Items.Add(Library.Sections(i));
            }
            ListViewItem lvi = SectionsListView.FindItemWithText(CurrentSection.Name);
            lvi.Selected = true;
        }
        public void RefreshItems(DataGridView ItemsDataGridView)
        {
            IMLDataSet Dataset = CurrentSection.GetDataSet();
            ItemsDataGridView.DataSource = Dataset.Dataset.Tables[0];
            ItemsDataGridView.Tag = Dataset;
            if (ItemsDataGridView.Columns.Count > 0)
            {
                ItemsDataGridView.Columns[0].ReadOnly = true;
                ItemsDataGridView.Columns[0].Width = 30;
            }
        }

        public void RefreshImports(ListView ImportsListView)
        {        
            ImportsListView.Items.Clear();

            IMLImports Imports = Library.GetImports();
            IMLHashItemList pluginlist = Library.SystemObject.GetInstalledPlugins("import");
            for (int i = 0; i < Imports.Count; i++)
            {
                if (Imports.Imports(i).SectionName == CurrentSection.Name)
                {
                    for (int j = 0; j < pluginlist.Count; j++)
                    {
                        IMLHashItem plugin = pluginlist[j];
                        MLPluginDescription desc = plugin["description"] as MLPluginDescription;
                        if (desc.information.plugin_id.ToString() == Imports.Imports(i).PluginID)
                        {
                            string[] str = new string[5];
                            str[0] = Convert.ToString(Imports.Imports(i).ID);
                            str[1] = Imports.Imports(i).Name;
                            str[2] = desc.information.plugin_name;
                            str[3] = Imports.Imports(i).Mode;
                            str[4] = Imports.Imports(i).ScheduleTime.ToString();
                            ListViewItem lvi = new ListViewItem(str);
                            lvi.Tag = plugin;
                            ImportsListView.Items.Add(lvi);
                        }
                    }
                }
            }
        }

        public void RefreshViews(ListView ViewsListView)
        {
            ViewsListView.Items.Clear();
            for (int i = 0; i < CurrentSection.ViewCount; i++)
            {
                IMLView View = CurrentSection.Views(i);
                ListViewItem lvi = new ListViewItem(View.Name);
                lvi.Tag = View;
                ViewsListView.Items.Add(lvi);
            }
        }

        public void RefreshViewSteps(IMLView View, DataGridView ViewStepDataGridView)
        {
            (ViewStepDataGridView.DataSource as BindingSource).Clear();
            for (int i = 0; i < View.Count; i++)
            {
                (ViewStepDataGridView.DataSource as BindingSource).Add(View.Steps(i));
                ViewStepDataGridView.Rows[i].Cells["RowCount"].Value = i + 1;
            }
        }

        public void FillGroupTagComboBox(DataGridViewComboBoxColumn groupTagBox)
        {
            string[] tags = CurrentSection.GetTagNames();
            ArrayList tagList = new ArrayList();
            tagList.Add(new DisplayValue("<name>", ""));
            if(tags != null)
                foreach(string tag in tags)
                    tagList.Add(new DisplayValue(tag, tag));
            groupTagBox.DataSource = tagList;
            groupTagBox.DisplayMember = "Display";
            groupTagBox.ValueMember = "Value";
        }

        public void FillSortTagComboBox(DataGridViewComboBoxColumn sortTagBox)
        {
            string[] tags = CurrentSection.GetTagNames();
            sortTagBox.Items.Clear();
            sortTagBox.Items.Add("");
            if (tags != null)
                sortTagBox.Items.AddRange(tags);
        }

        public void LoadDbPluginsListView(ListView DbPluginsListView)
        {
            IMLHashItemList plugins = Library.SystemObject.GetInstalledPlugins("database");
            plugins.Sort("name", true);

            foreach (IMLHashItem plugin in plugins)
            {
                bool hasConfig = false;
                MLPluginDescription desc = (MLPluginDescription)plugin["description"];
                foreach (MLPluginConfiguration pc in ((MLSystem)Library.SystemObject).Configuration.Plugins)
                {
                    if ((Guid)pc.description["plugin_id"] == desc.information.plugin_id)
                    {
                        // add item to listview
                        ListViewItem lvi = DbPluginsListView.Items.Add(desc.information.plugin_name);
                        lvi.Tag = pc;
                        lvi.Checked = (bool)pc.description["enabled"];
                        hasConfig = true;
                        break;
                    }
                }
                // we add new plugins with no configuration
                if(!hasConfig)
                {
                    MLPluginConfiguration pc = new MLPluginConfiguration();
                    pc.description["plugin_id"] = desc.information.plugin_id;
                    pc.description["enabled"] = false;

                    ListViewItem lvi = DbPluginsListView.Items.Add(desc.information.plugin_name);
                    lvi.Tag = pc;
                    lvi.Checked = false;
                }
            }
            
        }
    }
}
