using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace Common.GUIPlugins
{
  public class RemovableDrivesHandler
  {
    static private List<GUIListItem> _removableDrives = new List<GUIListItem>();

    private static void AddRemovableDrives(List<GUIListItem> Drives)
    {
      _removableDrives.AddRange(Drives);
    }

    public static void FilterDrives(ref List<GUIListItem> Itemlist)
    {
      List<int> indexList = new List<int>();

      foreach (GUIListItem item in Itemlist)
      {
        foreach (GUIListItem subItem in _removableDrives)
        {
          if (item.Path.Length <= 3 && subItem.Path.StartsWith(item.Path))
          { 
            indexList.Add((Itemlist.FindIndex(item2 => item2.Path == item.Path)));
            break;
          }
        }
      }
      indexList.Reverse();
      foreach (int i in indexList)
      {
        Itemlist.RemoveAt(i);
      }
    }

    public static void ListRemovableDrives(List<GUIListItem> allItems)
    {
      List<GUIListItem> removableDrives = new List<GUIListItem>();
      foreach (var item in allItems)
      {
        if ((Utils.IsUsbHdd(item.Path) || Utils.IsRemovableUsbDisk(item.Path) || Utils.IsRemovable(item.Path)) && item.Path.Length > 3)
        {
          if (removableDrives.Find(item2 => item2.Path == item.Path) == null)
          {
            removableDrives.Add(item);
          }
        }
      }
      AddRemovableDrives(removableDrives);
    }
  }
}
