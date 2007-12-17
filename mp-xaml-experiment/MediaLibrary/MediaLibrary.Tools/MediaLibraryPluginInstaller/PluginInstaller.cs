using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MediaLibrary;
using MediaLibrary.Configuration;
using ICSharpCode.SharpZipLib.Zip;

namespace PluginInstaller
{
    public partial class PluginInstaller : Form
    {
        IMediaLibrary MediaLibrary;
        public PluginInstaller(string[] args)
        {
            string filename;
            
            InitializeComponent();
            MediaLibrary = new MediaLibraryClass(Properties.Settings.Default.MediaLibraryConfigPath);
            
            //get all installed plugins
            GetInstalledPlugins();

            if (args != null && args.Length == 1)
            {
                if (File.Exists(Path.GetFullPath(args[0])))
                {
                    filename = Path.GetFullPath(args[0]);
                    
                    if (filename.EndsWith(".mlpp"))
                    {
                        // its a package
                        InstallPackage(filename);
                        Application.Exit();
                    }
                }
            }
        }

        #region Events
        
        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void UninstallButton_Click(object sender, EventArgs e)
        {
            if (PluginListView.SelectedIndices.Count > 0)
            {
                MLPluginDescription desc;
                desc = (MLPluginDescription)PluginListView.SelectedItems[0].Tag;

                UninstallPlugin(desc);

                GetInstalledPlugins();
            }
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                InstallPackage(openFileDialog1.FileName);
            }
        }

        #endregion

        private void GetInstalledPlugins()
        {
            string[] sa;
            string path;
            ListViewItem lvi;
            MLPluginDescription desc;

            PluginListView.Items.Clear();

            path = MediaLibrary.SystemObject.GetPluginsDirectory(null);
            sa = Directory.GetFiles(path, "*.mlpd", SearchOption.AllDirectories);
            foreach (string s in sa)
            {
                desc = MLPluginDescription.Deserialize(s);
                lvi = new ListViewItem(desc.information.plugin_name);
                lvi.Tag = desc;
                PluginListView.Items.Add(lvi);
            }

            PluginListView.Sort();
            PluginListView.Columns[0].Width = -1;
        }

        void UninstallPlugin(MLPluginDescription desc)
        {
            string pluginFolder = GetPluginDirectory(desc);
            if (Directory.Exists(pluginFolder))
            {
                // unregister com dlls.
                foreach (PluginDescription_install_file mpdif in desc.installation.install_file)
                {
                    //Register
                    if (mpdif.action == "Register DLL")
                    {
                        string[] dllpath;

                        dllpath = Directory.GetFiles(pluginFolder, mpdif.file, SearchOption.AllDirectories);

                        if (dllpath.Length > 0)
                        {
                            System.Diagnostics.Process.Start("regsrv32.exe", "/u /s " + dllpath[0]);
                        }
                    }
                }

                // delete plugin folder
                Directory.Delete(pluginFolder, true);
            }
        }

        private void InstallPackage( string filename )
        {
            MLPluginDescription desc;
            string tempfolder = GetTempDirectory();
            string zipFileName = Path.GetFullPath(filename);
            string sourceDirectory = Path.GetDirectoryName(filename);

            FastZip fastZip = new FastZip();
            try
            {
                fastZip.ExtractZip(zipFileName, tempfolder, null);
                // we find all included plugin descriptions
                string[] pluginFiles = Directory.GetFiles(tempfolder, "*.mlpd", SearchOption.AllDirectories);
                // for each mlpd
                foreach (string DescPath in pluginFiles)
                {
                    desc = MLPluginDescription.Deserialize(DescPath);
                    //uninstall previous plugin and delete the folder
                    UninstallPlugin(desc);

                    if (desc.installation.is_multi_package == false)  // the multipack package is no plugin
                    {
                        // get the plugin folder path
                        string pluginFolder = GetPluginDirectory(desc);
                        if (!string.IsNullOrEmpty(pluginFolder))  //only extract valid plugins
                        {
                            // copy temp folder to plugin folder
                            Shell32.ShellClass sc = new Shell32.ShellClass();
                            Shell32.Folder SrcFlder = sc.NameSpace(Path.GetDirectoryName(DescPath));    
                            Shell32.Folder DestFlder = sc.NameSpace(pluginFolder);
                            Shell32.FolderItems items = SrcFlder.Items();
                            DestFlder.CopyHere(items, 20);
                            


                            // now we do the install-actions
                            foreach (PluginDescription_install_file mpdif in desc.installation.install_file)
                            {
                                //Assembly
                                CopyAssembly(pluginFolder, mpdif);
                                //Register
                                RegisterDll(pluginFolder, mpdif);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            GetInstalledPlugins();
            MessageBox.Show("Package installed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        string GetPluginDirectory(MLPluginDescription desc)
        {
            switch (desc.information.plugin_type)
            {
                case "import":
                case "database":
                    return Path.GetFullPath(MediaLibrary.SystemObject.GetPluginsDirectory(desc.information.plugin_type + "\\" + desc.installation.destination_folder));
                    break;

            }
            return null;
        }

        public string GetTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
            return path;
        }

        private void CopyAssembly(string pluginFolder, PluginDescription_install_file mpdif)
        {
            if (mpdif.action == "assembly")
            {
                string[] dllpath;

                dllpath = Directory.GetFiles(pluginFolder, mpdif.file, SearchOption.AllDirectories);

                if (dllpath.Length > 0)
                {
                    string file = MediaLibrary.SystemObject.GetRootDirectory("Assemblies") + "\\" + mpdif.file;
                    File.Delete(file);
                    File.Move(dllpath[0], file);
                }
            }
        }

        private void RegisterDll(string pluginFolder, PluginDescription_install_file mpdif)
        {
            if (mpdif.action == "register")
            {
                string[] dllpath;

                dllpath = Directory.GetFiles(pluginFolder, mpdif.file, SearchOption.AllDirectories);

                if (dllpath.Length > 0)
                {
                    System.Diagnostics.Process.Start("regsrv32.exe", "/s " + dllpath[0]);
                    // regsrv32 dllpath[0]
                }
            }
        }

    }
}