using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ActionType
{
    class RunApplication : IActionType
    {
        private const string Const_APP = "Path to application";
        private const string Const_Params = "Parameters to application";

        public event FileInstalledEventHandler ItemProcessed;

        public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
        {
            return 1;
        }

        public string DisplayName
        {
            get { return "RunApplication"; }
        }

        public string Description
        {
            get { return "Execute the specified application"; }
        }

        public SectionParamCollection GetDefaultParams()
        {
            var Params = new SectionParamCollection();
            Params.Add(new SectionParam(Const_APP, "", ValueTypeEnum.Template,
                                        "Path to the application like \n %Base%\\MediaPortal.exe"));
            return Params;
        }

        public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
        {
            try
            {
                if(!string.IsNullOrEmpty(actionItem.Params[Const_Params].Value))
                {
                    Process.Start(MpeInstaller.TransformInRealPath(actionItem.Params[Const_APP].Value), actionItem.Params[Const_Params].Value);
                }
                else
                {
                    Process.Start(MpeInstaller.TransformInRealPath(actionItem.Params[Const_APP].Value));
                }
            }
            catch (Exception)
            {
                if (ItemProcessed != null)
                    ItemProcessed(this, new InstallEventArgs("Error tos start application"));
                return SectionResponseEnum.Ok;
                }
            if (ItemProcessed != null)
                ItemProcessed(this, new InstallEventArgs("Application start done"));
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
