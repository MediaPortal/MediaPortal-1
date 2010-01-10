#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TvDatabase;
using TvLibrary.Log;

namespace TvService
{
  internal class TsCopier
  {
    private Int64 _posStart;
    private string _fileStart;
    private Int64 _posEnd;
    private string _fileEnd;
    private string _recording;

    public TsCopier(Int64 posStart, string fileStart, Int64 posEnd, string fileEnd, string recording)
    {
      _posStart = posStart;
      _fileStart = fileStart;
      _posEnd = posEnd;
      _fileEnd = fileEnd;
      _recording = recording;
      Log.Info("TsCopier: dtor() pos1: {0}, file1: {1}, pos2: {2}, file2: {3}, rec: {4}", posStart, fileStart, posEnd,
               fileEnd, recording);
    }

    public void DoCopy()
    {
      try
      {
        string baseTs = Path.GetDirectoryName(_fileStart) + "\\" +
                        Path.GetFileNameWithoutExtension(_fileStart).Substring(0, 19);
        Log.Info("TsCopier: baseTs: {0}", baseTs);
        int idCurrent = Int32.Parse(Path.GetFileNameWithoutExtension(_fileStart).Remove(0, 19));
        int idStart = idCurrent;
        int idStop = Int32.Parse(Path.GetFileNameWithoutExtension(_fileEnd).Remove(0, 19));
        TvBusinessLayer layer = new TvBusinessLayer();
        decimal maxFiles = Convert.ToDecimal(layer.GetSetting("timeshiftMaxFiles", "20").Value);
        Log.Info("TsCopier: baseTs={0} idCurrent={1} idStop={2} maxFiles={3}", baseTs, idCurrent, idStop, maxFiles);
        Directory.CreateDirectory(Path.GetDirectoryName(_recording) + "\\tsbuffers");
        int cycles = 1;
        if (idStop > idStart)
          cycles = (idStop - idStart) + 1;
        else if (idStop < idStart)
          cycles = (int)(maxFiles - idStart) + 1 + idStop;
        for (int i = idStart; i <= cycles; i++)
        {
          string currentSourceBuffer = baseTs + idCurrent.ToString() + ".ts";
          string targetTs = Path.GetDirectoryName(_recording) + "\\tsbuffers\\" + Path.GetFileName(currentSourceBuffer);
          Log.Info("TsCopier: Copying - source: {0}, target: {1}", currentSourceBuffer, targetTs);
          FileStream reader = new FileStream(currentSourceBuffer, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
          FileStream writer = new FileStream(targetTs, FileMode.CreateNew, FileAccess.Write);

          reader.Seek(_posStart, SeekOrigin.Begin);
          byte[] buf = new byte[1024];
          int bytesRead = reader.Read(buf, 0, 1024);
          while (bytesRead > 0)
          {
            if (reader.Position > _posEnd && currentSourceBuffer == _fileEnd)
              bytesRead -= (int)(reader.Position - _posEnd);
            if (bytesRead <= 0)
              break;
            writer.Write(buf, 0, bytesRead);
            bytesRead = reader.Read(buf, 0, 1024);
          }
          writer.Flush();
          writer.Close();
          writer.Dispose();
          writer = null;
          reader.Close();
          reader.Dispose();
          reader = null;
          Log.Info("TsCopier: copying done.");
          idCurrent++;
          if (idCurrent > maxFiles)
            idCurrent = 1;
        }
        Log.Info("TsCopier: processed all timeshift buffer files for recording.");
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
  }
}