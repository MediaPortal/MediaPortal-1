

using MpeCore.Classes;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;

namespace MpeCore.Interfaces
{
    public interface IActionType
    {
        event FileInstalledEventHandler ItemProcessed;

        int ItemsCount(PackageClass packageClass, ActionItem actionItem);
        
        string DisplayName { get; }

        string Description { get; }
        
        SectionParamCollection GetDefaultParams();

        SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem);
        
        ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem);

        SectionResponseEnum UnInstall(UnInstallItem item);
    }
}