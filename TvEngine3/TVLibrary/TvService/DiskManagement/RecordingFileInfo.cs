using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
using TvDatabase;

namespace TvService
{
  public class RecordingFileInfo : IComparable<RecordingFileInfo>
  {
    public string filename;
    public FileInfo info;
    public Recording record;
    #region IComparable Members

    public int CompareTo(RecordingFileInfo fi)
    {
      if (info.CreationTime < fi.info.CreationTime) return -1;
      if (info.CreationTime > fi.info.CreationTime) return 1;
      return 0;
    }

    #endregion
  }
}
