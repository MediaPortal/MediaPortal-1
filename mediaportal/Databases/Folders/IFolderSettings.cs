using System;
using System.Collections.Generic;
using System.Text;

namespace Databases.Folders
{
  public interface IFolderSettings
  {
    void DeleteFolderSetting(string path, string Key);
    void AddFolderSetting(string path, string Key, Type type, object Value);
    void GetFolderSetting(string path, string Key, Type type, out object Value);
    void Dispose();
  }
}
