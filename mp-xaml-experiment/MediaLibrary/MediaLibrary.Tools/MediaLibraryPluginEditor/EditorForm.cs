using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using MediaLibrary;
using MediaLibrary.Configuration;
using ICSharpCode.SharpZipLib.Zip;

namespace PluginDescriptionEditor
{
    public partial class EditorForm : Form
    {
        
        bool descchanged = false;
        string filename = string.Empty;
        string folder = Directory.GetCurrentDirectory();
        MLPluginDescription desc = null;
        
        
        public EditorForm(string[] args)
        {
            InitializeComponent();

            //this.pictureBox1.Image = Image.FromFile(".\\resources\\images\\default\\SplashScreenImage");
            this.pictureBox1.ImageLocation = ".\\resources\\images\\default\\SplashScreenImage";

            if (args != null && args.Length == 1)
            {
                if( File.Exists( Path.GetFullPath( args[0] ) ) )
                {                   
                    filename = Path.GetFileName( Path.GetFullPath( args[0] ));
                    folder = Path.GetDirectoryName(Path.GetFullPath(args[0]));

                    if ( filename.EndsWith( "mlpd") )
                    {
                        // its a plugin description
                        desc = MLPluginDescription.Deserialize(filename);
                    }
                }
            }

            this.saveFileDialog1.InitialDirectory = folder;
            this.openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            
            BindForm();
            descchanged = false;
            FormText();
        }

        #region form and file sync

        // Updates the title of the form
        void FormText()
        {
            string s = string.Empty;

            if( descchanged == true )
                s = "*";
            this.Text = "Media Library Plugin Description Editor " + Path.GetFileName(filename) +" " + s; 
        }

        void BindForm()
        {
            if (desc == null)
                desc = new MLPluginDescription();

            string[] sa = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);
            foreach (string s in sa)
            {
                this.MainFileComboBox.Items.Add(Path.GetFileName(s));
            } 
            this.pluginDescriptionauthorBindingSource.DataSource = desc.author;
            this.pluginDescriptiondocumentationBindingSource.DataSource = desc.documentation;
            this.pluginDescriptioninformationBindingSource.DataSource = desc.information;
            this.pluginDescriptioninstallationBindingSource.DataSource = desc.installation;


            this.pluginDescriptioninstallfileBindingSource.DataSource = null;
            this.InstallActionsListView.Items.Clear();
            foreach (PluginDescription_install_file i in desc.installation.install_file)
            {
                ListViewItem l = new ListViewItem(i.file);
                l.SubItems.Add(i.action);

                this.InstallActionsListView.Items.Add(l);
            }
        }
        #endregion

        #region menu items

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // we discard changes
            if (descchanged)
            {
                DialogResult r;

                r = MessageBox.Show("You have unsaved changes. Save current configuration?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (r == DialogResult.Cancel)
                {
                    return;
                }
                if (r == DialogResult.Yes)
                {
                    // save
                    saveToolStripMenuItem_Click(null, null);
                }
            }
            // and create an "empty" PluginDescription
            desc = new MLPluginDescription();            
            filename = string.Empty;
            folder = Directory.GetCurrentDirectory();
            BindForm();
            descchanged = false;
            FormText();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // we discard changes
            if (descchanged)
            {
                DialogResult r;

                r = MessageBox.Show("You have unsaved changes. Save current configuration?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (r == DialogResult.Cancel)
                {
                    return; 
                }
                if (r == DialogResult.Yes)
                {
                    // save
                    saveToolStripMenuItem_Click(null, null);
                }
            }

            // and load 
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // then deserialize the file
                try
                {
                    filename = openFileDialog1.FileName;
                    folder = Path.GetDirectoryName(filename);
                    desc = MLPluginDescription.Deserialize(filename);
                    BindForm();
                    descchanged = false;                    
                    FormText();                    
                }
                catch
                {
                    MessageBox.Show("The file could not be read.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // if invalid filename, we gosub Save As ...
            if( filename == string.Empty )
            {
                saveAsToolStripMenuItem_Click(null, null);                
                return;
            }
            // we serialize the data to file.
            //try
            {
                MLPluginDescription.Serialize(desc, filename);
                descchanged = false;
                FormText();
            }
            //catch
            {
            //    MessageBox.Show("The file could not be written.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = this.saveFileDialog1.FileName;
                folder = Path.GetDirectoryName(filename);
                this.MainFileComboBox.Items.Clear();
                string[] sa = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);
                foreach (string s in sa)
                {
                    this.MainFileComboBox.Items.Add(Path.GetFileName(s));
                } 
                saveToolStripMenuItem_Click(null, null);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (descchanged)
            {
                DialogResult r;

                r = MessageBox.Show("You have unsaved changes. Save current configuration?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (r == DialogResult.Cancel)
                {
                    return;
                }
                if (r == DialogResult.Yes)
                {
                    // save
                    saveToolStripMenuItem_Click(null, null);
                }
            }
            // and exit the application
            Application.Exit();
        }

        /// <summary>
        /// Shows the AboutBox
        /// </summary>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 form = new AboutBox1();
            form.ShowDialog();
        }

        #endregion

        #region form interaction 

        private void GenerateIDButton_Click(object sender, EventArgs e)
        {
            this.PluginIdTextBox.Text = Guid.NewGuid().ToString();            
        }
        
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            descchanged = true;
            FormText();
        }

        private void MultiPluginCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TextBox_TextChanged(null, null);
            tabPage2.Enabled = !MultiPluginCheckBox.Checked;
            tabPage3.Enabled = !MultiPluginCheckBox.Checked;
            tabPage4.Enabled = !MultiPluginCheckBox.Checked;
            tabPage5.Enabled = !MultiPluginCheckBox.Checked;
        }

        private void AddInstallActionButton_Click(object sender, EventArgs e)
        {
            ListViewItem l = new ListViewItem("<file>");
            PluginDescription_install_file i = new PluginDescription_install_file();
            l.SubItems.Add("<file>");
            i.file = "<file>";
            l.SubItems.Add("<action>");
            i.action = "<action>";
           
            desc.installation.install_file.Add(i);
            InstallActionsListView.Items.Add(l);
            descchanged = true;
            FormText();
        }

        private void RemoveInstallActionButton_Click(object sender, EventArgs e)
        {
            if (InstallActionsListView.SelectedItems.Count > 0)
            {
                desc.installation.install_file.RemoveAt(InstallActionsListView.SelectedIndices[0]);
                InstallActionsListView.Items.RemoveAt(InstallActionsListView.SelectedIndices[0]);
                
                descchanged = true;
                FormText();
            }
        }

        private void InstallActionsListView_SubItemClicked(object sender, ListViewEx.SubItemEventArgs e)
        {
            this.pluginDescriptioninstallfileBindingSource.DataSource = desc.installation.install_file[e.Item.Index];
            if (e.SubItem == 0)            // file
            {
                InstallFileComboBox.Items.Clear();
                string[] sa = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);
                foreach (string s in sa)
                {
                    InstallFileComboBox.Items.Add(Path.GetFileName(s));
                }
                
                InstallActionsListView.StartEditing(InstallFileComboBox, e.Item, e.SubItem);
            }
            else if (e.SubItem == 1)         // action
            {
                InstallActionsListView.StartEditing(InstallActionComboBox, e.Item, e.SubItem);
            }
            descchanged = true;
            FormText();
        }

        #endregion

        #region zipping and unzipping

        private void CreatePackageButton_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem_Click(null, null);

            string tempfile = Path.GetTempFileName();
            string zipFileName = Path.GetFileNameWithoutExtension(filename) + ".mlpp";
            string sourceDirectory = Path.GetDirectoryName(filename);

            FastZip fastZip = new FastZip();
            try
            {
                fastZip.CreateZip(tempfile, sourceDirectory, true, "");
                File.Move(tempfile, zipFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            MessageBox.Show("Package created" , "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

    
    
    }


}