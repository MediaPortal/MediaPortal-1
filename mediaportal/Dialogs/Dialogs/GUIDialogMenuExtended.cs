using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;

namespace Dialogs.Dialogs
{

  public class GUIDialogMenuExtended : GUIDialogMenu
  {
    [SkinControl(6)]
    protected GUIListControl subListView = null;    
    public override int GetID { get { return (int)Window.WINDOW_DIALOG_MENU_EXTENDED; } set { } }

    public bool IsAvailable { get; protected set; }

    public override bool Init()
    {      
      //Window.WINDOW_DIALOG_MENU
      const string dialogmenuextendedXml = @"\DialogMenuExtended.xml";
      IsAvailable = File.Exists(GUIGraphicsContext.Skin + dialogmenuextendedXml);
      // 1. MP doesn't actually load the skin yet when calling Load, because SupportsDelayedLoad is true by default
      // 2. returning false from Init won't prevent the Window from being added to the list of Windows, because return value is never evaluated
      // 3. use our IsAvailable Property to find out if the skin (File Exists check) is available
      return Load(GUIGraphicsContext.Skin + dialogmenuextendedXml);
    }

    private IDictionary<GUIListItem, List<GUIListItem>> _list = new Dictionary<GUIListItem, List<GUIListItem>>();

    public void Add(GUIListItem pItem, List<GUIListItem> subItems)
    {
      _list[pItem] = subItems;
      base.Add(pItem);
    }

    public override bool OnMessage(GUIMessage message)
    {
      bool result = base.OnMessage(message);

      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED && message.TargetWindowId == (int)Window.WINDOW_DIALOG_MENU_EXTENDED && message.SenderControlId == 3)
      {        
        subListView.Clear();
        if (listView.SelectedListItem != null)
        {
          List<GUIListItem> subitems;
          if (_list.TryGetValue(listView.SelectedListItem, out subitems))
          {
            foreach (GUIListItem subitem in subitems)
            {
              subListView.Add(subitem);
            }            
          }
        }
      }
      return result;
    }

    public override void Dispose()
    {
      subListView.SafeDispose();
      base.Dispose();
    }
  }
}
