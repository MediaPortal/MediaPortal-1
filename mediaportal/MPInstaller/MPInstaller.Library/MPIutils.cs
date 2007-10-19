using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using System.Reflection;
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
    public class MPIutils
    {
        public MPIutils()
        {
        }

        public static void LoadPlugins(string pluginFile)
        {
            if (!File.Exists(pluginFile))
            {
                MessageBox.Show("File not found "+pluginFile);
                return;
            }
            try
            {
                Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

                if (pluginAssembly != null)
                {
                    Type[] exportedTypes = pluginAssembly.GetExportedTypes();

                    foreach (Type type in exportedTypes)
                    {
                        if (type.IsAbstract)
                        {
                            continue;
                        }
                        if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
                        {
                            try
                            {
                                //
                                // Create instance of the current type
                                //
                                object pluginObject = Activator.CreateInstance(type);
                                ISetupForm pluginForm = pluginObject as ISetupForm;

                                if (pluginForm != null)
                                {
                                    if (pluginForm.HasSetup())
                                        pluginForm.ShowPlugin();
                                    //ItemTag tag = new ItemTag();
                                    //tag.SetupForm = pluginForm;
                                    //tag.DLLName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                                    //tag.windowId = pluginForm.GetWindowId();
                                    //loadedPlugins.Add(tag);
                                }
                            }
                            catch (Exception setupFormException)
                            {
                                MessageBox.Show(string.Format("Exception in plugin SetupForm loading : {0} ", setupFormException.Message));

                            }
                        }
                    }
                }
            }
            catch (Exception unknownException)
            {
                MessageBox.Show("Exception in plugin loading :{0}", unknownException.Message);
            }
        }
    
       
        public static void StartApp(string file)
        {
            Process app = new Process();
            app.StartInfo.FileName = file;
            app.StartInfo.Arguments = "";
            app.Start();
        }
    }
}
