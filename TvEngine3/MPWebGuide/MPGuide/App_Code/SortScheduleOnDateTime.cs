using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using TvLibrary;
using TvDatabase;
/// <summary>
/// Summary description for SortScheduleOnDateTime
/// </summary>
public class SortScheduleOnDateTime : IComparer<Schedule>
{
  bool _sortAscending=true;
  public SortScheduleOnDateTime(bool sortAscending)
  {
    _sortAscending = sortAscending;
  }
  #region IComparer<Schedule> Members
  public int Compare(Schedule x, Schedule y)
  {
    if (_sortAscending)
      return (x.StartTime.CompareTo(y.StartTime) );

    return (y.StartTime.CompareTo(x.StartTime));
  }

  #endregion
}
