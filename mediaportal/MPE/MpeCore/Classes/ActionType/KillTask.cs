using System.Diagnostics;
using System;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;


namespace MpeCore.Classes.ActionType
{
    class KillTask : IActionType
    {
        private const string Const_MESSAGE = "Task name";
        public event FileInstalledEventHandler ItemProcessed;

        public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
        {
            return 1;
        }

        public string DisplayName
        {
            get { return "KillTask"; }
        }

        public string Description
        {
            get { return "Kill a task with specified name"; }
        }

        public SectionParamCollection GetDefaultParams()
        {
            var Params = new SectionParamCollection();
            Params.Add(new SectionParam(Const_MESSAGE, "", ValueTypeEnum.String,
                                       "Task name to kill"));
            return Params;
        }

        public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
        {
            if (ItemProcessed != null)
                ItemProcessed(this, new InstallEventArgs("Kill Task"));
            Process[] prs = Process.GetProcesses();
            foreach (Process pr in prs)
            {
                if (pr.ProcessName.Equals(actionItem.Params[Const_MESSAGE].Value, StringComparison.InvariantCultureIgnoreCase))
                    pr.Kill();
            }
            return SectionResponseEnum.Ok;
        }

        public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
        {
            return new ValidationResponse();
        }
    }
}
