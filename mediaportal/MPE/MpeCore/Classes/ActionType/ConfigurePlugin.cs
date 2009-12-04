using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;


namespace MpeCore.Classes.ActionType
{
    class ConfigurePlugin : IActionType
    {
        private const string Const_APP = "Path to plugin";
  
        public event FileInstalledEventHandler ItemProcessed;

        public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
        {
            return 1;
        }

        public string DisplayName
        {
            get { return "ConfigurePlugin"; }
        }

        public string Description
        {
            get { return "Execute the specified application"; }
        }

        public SectionParamCollection GetDefaultParams()
        {
            var Params = new SectionParamCollection();
            Params.Add(new SectionParam(Const_APP, "", ValueTypeEnum.Template,
                                        "Path to the plugin like \n %Plugins%\\Windows\\plugin.dll"));
            return Params;
        }

        public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
        {
            try
            {
                if (!packageClass.Silent && File.Exists(MpeInstaller.TransformInRealPath(actionItem.Params[Const_APP].Value)))
                {
                    string assemblyFileName = MpeInstaller.TransformInRealPath(actionItem.Params[Const_APP].Value);
                    AppDomainSetup setup = new AppDomainSetup();
                    setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                    setup.PrivateBinPath = Path.GetDirectoryName(assemblyFileName);
                    setup.ApplicationName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                    setup.ShadowCopyFiles = "true";
                    setup.ShadowCopyDirectories = Path.GetDirectoryName(assemblyFileName);
                    AppDomain appDomain = AppDomain.CreateDomain("pluginDomain", null, setup);

                    PluginLoader remoteExecutor = (PluginLoader)appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(PluginLoader).ToString());
                    remoteExecutor.Load(assemblyFileName);
                    remoteExecutor.Dispose();

                    AppDomain.Unload(appDomain);
                }
            }
            catch (Exception)
            {
                if (ItemProcessed != null)
                    ItemProcessed(this, new InstallEventArgs("Error to configure plugin"));
                return SectionResponseEnum.Ok;
            }
            if (ItemProcessed != null)
                ItemProcessed(this, new InstallEventArgs("Plugin configuration donne"));
            return SectionResponseEnum.Ok;
        }

        public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
        {
            if (!string.IsNullOrEmpty(actionItem.ConditionGroup) && packageClass.Groups[actionItem.ConditionGroup] == null)
                return new ValidationResponse()
                {
                    Message = actionItem.Name + " condition group not found " + actionItem.ConditionGroup,
                    Valid = false
                }; 

            return new ValidationResponse();
        }

        public SectionResponseEnum UnInstall(PackageClass packageClass, UnInstallItem item)
        {
            return SectionResponseEnum.Ok;
        }
    }
}
