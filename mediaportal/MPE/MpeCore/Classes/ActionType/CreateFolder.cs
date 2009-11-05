using System.Diagnostics;
using System;
using System.IO;
using System.Windows.Forms;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ActionType
{
    class CreateFolder : IActionType
    {
        private const string Const_Loc = "Folder location";
        private const string Const_Question = "Question on UnInstall";
        private const string Const_Remove = "Remove on Uninstall";

        public event FileInstalledEventHandler ItemProcessed;
        public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
        {
            return 1;
        }

        public string DisplayName
        {
            get { return "CreateFolder"; }
        }

        public string Description
        {
            get { return "Creat a folder"; }
        }

        public SectionParamCollection GetDefaultParams()
        {
            var Params = new SectionParamCollection();
            Params.Add(new SectionParam(Const_Loc, "", ValueTypeEnum.Template,
                                       "Location of folder"));
            Params.Add(new SectionParam(Const_Remove, "", ValueTypeEnum.Bool,
                           "Remove on UnInstall"));

            Params.Add(new SectionParam(Const_Question, "", ValueTypeEnum.String,
                                       "Quetion asked from user when UnInstall.\n With Yes response the folder will be deleted\n If empty no question will be asked and no folder will be deleted"));
            return Params;
        }

        public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
        {
            try
            {
                if (ItemProcessed != null)
                    ItemProcessed(this, new InstallEventArgs("Create Folder"));
                if (!Directory.Exists(actionItem.Params[Const_Loc].GetValueAsPath()))
                    Directory.CreateDirectory(actionItem.Params[Const_Loc].GetValueAsPath());
                UnInstallItem unInstallItem = new UnInstallItem();
                unInstallItem.ActionType = DisplayName;
                unInstallItem.ActionParam = new SectionParamCollection(actionItem.Params);
                unInstallItem.ActionParam[Const_Loc].Value = actionItem.Params[Const_Loc].GetValueAsPath();
                packageClass.UnInstallInfo.Items.Add(unInstallItem);
            }catch
            {
                
            }
            return SectionResponseEnum.Ok;
        }

        public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
        {
            return new ValidationResponse();
        }

        public SectionResponseEnum UnInstall(PackageClass packageClass, UnInstallItem item)
        {
            if (!item.ActionParam[Const_Remove].GetValueAsBool())
                return SectionResponseEnum.Ok;
            if (!string.IsNullOrEmpty(item.ActionParam[Const_Question].Value))
            {
                if (!packageClass.Silent && MessageBox.Show(item.ActionParam[Const_Question].Value, "Question ", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        Directory.Delete(item.ActionParam[Const_Loc].Value, true);
                    }
                    catch (Exception)
                    {
                    }
                }

            }
            else
            {
                try
                {
                    Directory.Delete(item.ActionParam[Const_Loc].Value, true);
                }
                catch (Exception)
                {
                }
            }
            return SectionResponseEnum.Ok;
        }
    }
}

