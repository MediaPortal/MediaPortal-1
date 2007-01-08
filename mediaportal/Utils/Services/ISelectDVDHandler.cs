using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Services
{
  /// <summary>
  /// Interface for SelectDVDHandler class.
  /// </summary>
  public interface ISelectDVDHandler
  {
    string ShowSelectDVDDialog(int parentId);
  }
}