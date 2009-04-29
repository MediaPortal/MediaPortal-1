using System;

using Microsoft.Win32;

namespace SetMerit
{

  static class Program
  {

    const string FiltersKey = @"CLSID\{083863F1-70DE-11d0-BD40-00A0C911CE86}\Instance\";
    const string MeritKey = "FilterData";


    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      try
      {
        if (args.Length == 2)
        {
          if (args[1].Length != 8)
            throw new InvalidOperationException(String.Format("Not a valid merit value \"{0}\"", args[1]));

          // Reverse the merit bytes ...
          byte[] meritData = new byte[4];
          int meritByte = 3;
          for (int index = 0; index < 8; index += 2)
          {
            string byteStr = args[1].Substring(index, 2);

            meritData[meritByte--] = byte.Parse(byteStr, System.Globalization.NumberStyles.HexNumber);
          }

          string filter = FiltersKey + args[0];
          Console.WriteLine("Modifying HKCR\\{0} ...", filter);

          using (RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(filter, true))
          {
            byte[] data = (byte[])regKey.GetValue(MeritKey, null);

            Array.Copy(meritData, 0, data, 4, 4);

            regKey.SetValue(MeritKey, (object)data);
          }

          Console.WriteLine("Success");
        }
        else
        {
          Console.WriteLine("Usage: SetMerit [Filter GUID] [New Merit]");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

  }

}
