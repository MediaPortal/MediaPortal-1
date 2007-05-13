using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;

namespace PersonalTVGuide
{
  public interface IWishList : IList<IWishItem>
  {
    int SelectedItem
    {
      get; set;
    }
    void UpDate();
    void InsertTVProgs(ref GUIListControl lcProgramList, DateTime start, DateTime stop);
  }
}