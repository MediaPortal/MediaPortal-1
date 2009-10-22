using System.Diagnostics;
using System;
using System.IO;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;
using IWshRuntimeLibrary;

namespace MpeCore.Classes.ActionType
{
    class CreateShortCut : IActionType
    {

        private const string Const_Loc = "ShortCut location";
        private const string Const_Target = "ShortCut target";
        private const string Const_Description = "Description";
        private const string Const_Icon = "Icon of the shortcut";

        public event FileInstalledEventHandler ItemProcessed;


        public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
        {
            return 1;
        }

        public string DisplayName
        {
            get { return "CreateShortCut"; }
        }

        public string Description
        {
            get { return "Creat a shortcut"; }
        }

        public SectionParamCollection GetDefaultParams()
        {
            var Params = new SectionParamCollection();
            Params.Add(new SectionParam(Const_Loc, "", ValueTypeEnum.Template,
                                       "Location of shortcut"));
            Params.Add(new SectionParam(Const_Target, "", ValueTypeEnum.Template,
                                       "Target of short cut"));
            Params.Add(new SectionParam(Const_Description, "", ValueTypeEnum.String,
                                       "Description tooltip text "));
            Params.Add(new SectionParam(Const_Icon, "", ValueTypeEnum.Template,
                           "Icon of the shortcut, \n if is empty the icon of the target will be used"));

            return Params;
        }

        public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
        {
            if (ItemProcessed != null)
                ItemProcessed(this, new InstallEventArgs("Create ShortCut"));

            try
            {
                WshShellClass wshShell = new WshShellClass();
                // Create the shortcut

                IWshShortcut myShortcut = (IWshShortcut)wshShell.CreateShortcut(actionItem.Params[Const_Loc].GetValueAsPath());
                myShortcut.TargetPath = Path.GetFullPath(actionItem.Params[Const_Target].GetValueAsPath());
                myShortcut.WorkingDirectory = Path.GetDirectoryName(myShortcut.TargetPath);
                myShortcut.Description = actionItem.Params[Const_Description].Value;

                if (!string.IsNullOrEmpty(actionItem.Params[Const_Icon].Value))
                    myShortcut.IconLocation = actionItem.Params[Const_Icon].GetValueAsPath();
                else
                    myShortcut.IconLocation = actionItem.Params[Const_Target].GetValueAsPath();

                myShortcut.Save();

            }
            catch (Exception)
            {
                
            }

            return SectionResponseEnum.Ok;
        }

        public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
        {
            return new ValidationResponse();
        }

    }
}
