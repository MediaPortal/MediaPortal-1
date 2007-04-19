using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaManager
{
    public partial class FolderlistDialog : Form
    {
        List<string> _Folderlist;
        
        public FolderlistDialog()
        {
            InitializeComponent();
            _Folderlist = new List<string>();
        }

        public string[] Folderlist
        {
            get
            {
                return _Folderlist.ToArray();
            }
            set
            {
                _Folderlist = new List<string>(value);
                FoldersListView.Clear();
                foreach (string folder in _Folderlist)
                    FoldersListView.Items.Add(folder);
            }
        }

        private void CanclButton_Click(object sender, EventArgs e)
        {     
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (FoldersListView.SelectedIndices.Count > 0)
            {
                int index = FoldersListView.SelectedIndices[0];
                FoldersListView.Items.RemoveAt(index);
                _Folderlist.RemoveAt(index);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                FoldersListView.Items.Add(folderBrowserDialog1.SelectedPath);
                _Folderlist.Add(folderBrowserDialog1.SelectedPath);
            }
        }
    }
}