using System;
using MediaLibrary;
using MediaLibrary.Settings;
using MediaLibrary.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace MediaManager
{
    public partial class MediaManager : Form
    {

        MediaManagerSystem MySystem;
        
        MLPluginPropertyControlCollection DbPluginControls;

        public MediaManager()
        {
            InitializeComponent();
            MySystem = new MediaManagerSystem(Properties.Settings.Default.MediaLibraryConfigPath);
            DbPluginControls = new MLPluginPropertyControlCollection("database",splitContainer1.Panel2, MySystem.Library);
            MySystem.LoadDbPluginsListView(DbPluginsListView);
            for (int i = 0; i < MySystem.Library.SectionCount; i++)
            {
                SectionsListView.Items.Add(MySystem.Library.Sections(i));
            }
        }

        #region File Events

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        #endregion

        #region Media Library Events

        #region Section Events
        private void NewSectionToolStripButton_Click(object sender, EventArgs e)
        {
            string SectionName = InputBox.Show("Add New Section", "Enter a name for the new section");
            if (!string.IsNullOrEmpty(SectionName))
            {
                MySystem.CurrentSection = MySystem.Library.FindSection(SectionName, true);
                MySystem.RefreshSections(SectionsListView);
            }
        }

        private void DeleteSectionToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                MySystem.Library.DeleteSection(SectionsListView.SelectedItems[0].Text);
                SectionsListView.SelectedItems[0].Remove();
                SectionsListView_SelectedIndexChanged(null, null);
            }
        }

        private void SectionsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ItemsDataGridView.DataSource = null;
            ImportsListView.Items.Clear();
            ViewsListView.Items.Clear();
            iMLViewStepBindingSource.Clear();
            MySystem.CurrentSection = null;
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                MySystem.CurrentSection = MySystem.Library.FindSection(SectionsListView.SelectedItems[0].Text, false);
                MySystem.RefreshItems(ItemsDataGridView);
                MySystem.RefreshImports(ImportsListView);
                MySystem.RefreshViews(ViewsListView);
            }
        }
        #endregion

        #region Item Events
        private void SaveItemsButton_Click(object sender, EventArgs e)
        {
            IMLDataSet dataSet = ItemsDataGridView.Tag as IMLDataSet;
            dataSet.SaveChanges();
            SectionsListView_SelectedIndexChanged(null, null);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            for (int i = ItemsDataGridView.SelectedRows.Count - 1; i >= 0; i--)
                ItemsDataGridView.Rows.RemoveAt(ItemsDataGridView.SelectedRows[i].Index);
        }

        private void DeleteAllButton_Click(object sender, EventArgs e)
        {
            MySystem.CurrentSection.DeleteAllItems();
            SectionsListView_SelectedIndexChanged(null, null);
        }
        #endregion

        #region Import Events
        private void NewImportToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                IMLImports Imports = MySystem.Library.GetImports();
                IMLImport Import = Imports.AddNewImport();
                Import.SectionName = SectionsListView.SelectedItems[0].Text;
                ImportEditor IEForm = new ImportEditor(Import, MySystem.Library);
                IEForm.ShowDialog();
                MySystem.RefreshImports(ImportsListView);
            }
        }

        private void DeleteImportToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                if (ImportsListView.SelectedIndices.Count > 0)
                {
                    IMLImports Imports = MySystem.Library.GetImports();
                    IMLImport Import = Imports.FindImport(Convert.ToInt32(ImportsListView.SelectedItems[0].Text));
                    Imports.DeleteImport(Import);
                    MySystem.RefreshImports(ImportsListView);
                }
            }
        }

        private void EditImportToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                if (ImportsListView.SelectedIndices.Count > 0)
                {
                    IMLImports Imports = MySystem.Library.GetImports();
                    IMLImport Import = Imports.FindImport(Convert.ToInt32(ImportsListView.SelectedItems[0].Text));
                    ImportEditor IEForm = new ImportEditor(Import, MySystem.Library);
                    IEForm.ShowDialog();
                    MySystem.RefreshImports(ImportsListView);
                }
            }
        }

        private void RunImportToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                if (ImportsListView.SelectedIndices.Count > 0)
                {
                    IMLImports Imports = MySystem.Library.GetImports();
                    IMLImport Import = Imports.FindImport(Convert.ToInt32(ImportsListView.SelectedItems[0].Text));
                    ImportProgress ProgressForm = new ImportProgress();
                    ProgressForm.Run(Import, new ImportProgressClass(ProgressForm) as IMLImportProgress);
                    SectionsListView_SelectedIndexChanged(null, null);
                }
            }
        }

        private void ImportsListView_ItemActivate(object sender, EventArgs e)
        {
            EditImportToolStripButton_Click(sender, e);
        }

        #endregion

        #region View Events

        private void NewViewToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                string ViewName = InputBox.Show("New View", "Enter a name for the new view");
                if (ViewName != null && ViewName != string.Empty)
                {
                    IMLView View = MySystem.CurrentSection.AddNewView(ViewName);
                    ListViewItem lvi = new ListViewItem(View.Name);
                    lvi.Tag = View;
                    lvi = ViewsListView.Items.Add(lvi);
                    lvi.Selected = true;
                    ViewsListView_SelectedIndexChanged(null, null);
                }
            }
        }

        private void DeleteViewToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                if (ViewsListView.SelectedIndices.Count > 0)
                {
                    IMLView View = ViewsListView.SelectedItems[0].Tag as IMLView;
                    MySystem.CurrentSection.DeleteView(View);
                    ViewsListView.SelectedItems[0].Remove();
                    ViewsListView_SelectedIndexChanged(null, null);
                }
            }
        }

        private void MoveViewUpToolStripButton_Click(object sender, EventArgs e)
        {

        }

        private void MoveViewDownToolStripButton_Click(object sender, EventArgs e)
        {

        }

        private void ViewsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ViewsListView.SelectedIndices.Count > 0)
            {
                ViewStepDataGridView.DataSource = iMLViewStepBindingSource;
                MySystem.FillGroupTagComboBox(groupTagDataGridViewComboBoxColumn);
                MySystem.FillSortTagComboBox(sortTagDataGridViewComboBoxColumn);
                IMLView View = ViewsListView.SelectedItems[0].Tag as IMLView;
                MySystem.RefreshViewSteps(View, ViewStepDataGridView);
                ViewFilterText.Text = View.Filter;
            }
        }

        private void ViewsListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (ViewsListView.SelectedItems.Count > 0)
            {
                IMLView View = ViewsListView.SelectedItems[0].Tag as IMLView;
                View.Filter = ViewFilterText.Text;
            }
        }

        private void ViewFilterText_TextChanged(object sender, EventArgs e)
        {
            if (ViewsListView.SelectedItems.Count > 0)
            {
                IMLView View = ViewsListView.SelectedItems[0].Tag as IMLView;
                View.Filter = ViewFilterText.Text;
            }
        }

        #endregion

        #region ViewStep Events

        private void NewViewStepToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                if (ViewsListView.SelectedIndices.Count > 0)
                {
                    IMLView View = ViewsListView.SelectedItems[0].Tag as IMLView;
                    IMLViewStep Step = View.AddNewStep("");
                    int num = iMLViewStepBindingSource.Add(Step);
                    ViewStepDataGridView.Rows[num].Cells["RowCount"].Value = num + 1;
                }
            }
        }

        private void DeleteViewStepToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                if (ViewsListView.SelectedIndices.Count > 0)
                {
                    if (ViewStepDataGridView.SelectedRows.Count > 0)
                    {
                        IMLView View = ViewsListView.SelectedItems[0].Tag as IMLView;


                        for (int i = ViewStepDataGridView.SelectedRows.Count - 1; i >= 0; i--)
                        {
                            IMLViewStep Step = iMLViewStepBindingSource[ViewStepDataGridView.SelectedRows[i].Index] as IMLViewStep;
                            iMLViewStepBindingSource.RemoveAt(ViewStepDataGridView.SelectedRows[i].Index);
                            View.DeleteStep(Step);
                        }
                    }
                }
            }
        }

        private void MoveViewStepUpToolStripButton_Click(object sender, EventArgs e)
        {

        }

        private void MoveViewStepDownToolStripButton_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #endregion

        #region Database Plugins Events

        private void DbPluginsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ErrorText;
            //Save the properties back to the HashItem
            if (DbPluginControls.SaveProperties())
            {   //Make Sure the values for the plug-in are valid
                if (!DbPluginControls.Plugin.SetProperties(DbPluginControls.PluginPropertyValues, out ErrorText))
                {
                    MessageBox.Show(ErrorText);
                    DbPluginsListView.SelectedIndices.Clear();
                    DbPluginsListView.SelectedIndices.Add(DbPluginsListView.Items.IndexOf(MySystem.CurrentDbPluginListViewItem));

                    return;
                }
            }

            if (DbPluginsListView.SelectedItems.Count <= 0)
                return;
            MySystem.CurrentDbPluginListViewItem = DbPluginsListView.SelectedItems[0];
            MLPluginConfiguration PluginConfig = (MLPluginConfiguration)MySystem.CurrentDbPluginListViewItem.Tag;
            DbPluginControls.LoadPlugin((Guid)PluginConfig.description["plugin_id"]);
            MLPluginProperties PropCollection = new MLPluginProperties();
            DbPluginControls.Plugin.GetProperties(PropCollection);
            DbPluginControls.LoadProperties(PropCollection, PluginConfig.plugin_properties);
        }

        #endregion

        private void TestViewsToolStripButton_Click(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedIndices.Count > 0)
            {
                MySystem.CurrentSection.RefreshViews();
                ViewNavigatorDialog ViewNav = new ViewNavigatorDialog(MySystem.CurrentSection.GetViewNavigator());
                ViewNav.ShowDialog();
                MySystem.RefreshImports(ImportsListView);
            }
            

        }

    }
}