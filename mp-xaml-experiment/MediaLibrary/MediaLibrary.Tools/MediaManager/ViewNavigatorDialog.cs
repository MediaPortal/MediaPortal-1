using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaLibrary;

namespace MediaManager
{
    public partial class ViewNavigatorDialog : Form
    {
        IMLViewNavigator Navigator;

        public ViewNavigatorDialog(IMLViewNavigator Navigator)
        {
            this.Navigator = Navigator;
            InitializeComponent();
        }

        private void ViewNavigatorDialog_Load(object sender, EventArgs e)
        {
            if (Navigator.AtViews)
                BackButton.Enabled = false;
            RefreshList();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (Navigator.Back())
                RefreshList();
        }

        private void AdvanceButton_Click(object sender, EventArgs e)
        {
            if(ItemsListView.SelectedIndices.Count > 0)
                if (Navigator.Select(ItemsListView.SelectedIndices[0]))
                {
                    RefreshList();
                    BackButton.Enabled = true;
                }
        }

        private void ItemsListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ItemsListView_ItemActivate(object sender, EventArgs e)
        {
            AdvanceButton_Click(null, null);
        }

        private void RefreshList()
        {
            ItemsListView.Clear();
            for (int i = 0; i < Navigator.Count; i++)
                ItemsListView.Items.Add(Navigator.Choices(i));
        }


    }
}