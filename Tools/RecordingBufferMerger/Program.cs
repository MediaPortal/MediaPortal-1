using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RecordingBufferMerger
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("ERROR: i need the recording as the first parameter");
        Console.WriteLine("");
        Console.WriteLine("usage: RecordingBufferMerger.exe <recording file> /MOVE_BUFFERS");
        Console.WriteLine("       /MOVE_BUFFERS is optional. If set it deletes the buffers from the buffers dir.");
        return;
      }
      if (!File.Exists(args[0]))
      {
        Console.WriteLine("ERROR: recording file not found");
        return;
      }
      bool doMove = false;
      if (args.Length == 2)
      {
        if (args[1].ToLower() == "/move_buffers")
          doMove = true;
      }
      string recording=args[0];
      string recDir=Path.GetDirectoryName(recording);
      string buffersDir = recDir + "\\buffers";
      if (!Directory.Exists(buffersDir))
      {
        Console.WriteLine("ERROR: this recording has no timeshift buffers");
        return;
      }
      DirectoryInfo dinfo=new DirectoryInfo(buffersDir);
      FileInfo[] allFiles = dinfo.GetFiles("*.ts");
      int counter = 1;
      Array.Sort(allFiles, new FileInfoComparer());
      foreach (FileInfo fi in allFiles)
      {
        if (doMove)
          File.Move(fi.FullName, recDir + "\\" + Path.GetFileNameWithoutExtension(recording) + "_Part" + counter.ToString() + ".ts");
        else
          File.Copy(fi.FullName, recDir + "\\" + Path.GetFileNameWithoutExtension(recording) + "_Part" + counter.ToString() + ".ts");
        counter++;
      }
      File.Move(recording, recDir + "\\" + Path.GetFileNameWithoutExtension(recording) + "_Part" + counter.ToString() + ".ts");
    }
  }

  public class FileInfoComparer : System.Collections.IComparer
  {
    public int Compare(Object obj1, Object obj2)
    {
      FileInfo fi1 = (FileInfo)obj1;
      FileInfo fi2 = (FileInfo)obj2;
      return DateTime.Compare(fi1.LastWriteTime, fi2.LastWriteTime);
    }
  }
}
