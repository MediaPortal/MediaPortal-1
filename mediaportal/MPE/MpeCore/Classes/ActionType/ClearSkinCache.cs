using System;
using System.IO;
using System.Windows.Forms;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;
namespace MpeCore.Classes.ActionType
{
    class ClearSkinCache : IActionType
    {
        public event FileInstalledEventHandler ItemProcessed;

        public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
        {
            return 2;
        }

        public string DisplayName
        {
            get { return "ClearSkinCache"; }
        }

        public string Description
        {
            get { return "Delete MediaPortal Skin cache folder"; }
        }

        public SectionParamCollection GetDefaultParams()
        {
            return new SectionParamCollection(); ;
        }

        public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
        {
            if (ItemProcessed != null)
                ItemProcessed(this, new InstallEventArgs("Clear skin cache"));
            try
            {
                Directory.Delete(MpeInstaller.TransformInRealPath("%Cache%"), true);
                Directory.CreateDirectory(MpeInstaller.TransformInRealPath("%Cache%"));

            }
            catch (Exception)
            {
                if (ItemProcessed != null)
                    ItemProcessed(this, new InstallEventArgs("Error to clear skin cache"));
                return SectionResponseEnum.Ok;                
            }
            if (ItemProcessed != null)
                ItemProcessed(this, new InstallEventArgs("Clear skin cache done"));
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
