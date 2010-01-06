using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
  public enum CommandEnum
  {
    Install,
    Uninstall
  }

  public class QueueCommand
  {
    public QueueCommand()
    {
      Date = DateTime.Now;
      CommandEnum = CommandEnum.Install;
    }

    public QueueCommand(PackageClass packageClass, CommandEnum oper)
    {
      Date = DateTime.Now;
      CommandEnum = oper;
      TargetId = packageClass.GeneralInfo.Id;
      TargetVersion = packageClass.GeneralInfo.Version;
    }

    public CommandEnum CommandEnum { get; set; }
    public string TargetId { get; set; }
    public VersionInfo TargetVersion { get; set; }
    public DateTime Date { get; set; }
  }
}