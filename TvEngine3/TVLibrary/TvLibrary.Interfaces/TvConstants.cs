using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Class for definition of globally used constants
  /// </summary>
  public class TvConstants
  {
    /// <summary>
    /// all constant Tv channel group names
    /// </summary>
    public static class TvGroupNames
    {
      /// <summary>
      /// Name of group where all (new) channels are stored
      /// </summary>
      public static string AllChannels = "All Channels";
      /// <summary>
      /// Name of group where all analog channels are stored
      /// </summary>
      public static string Analog = "Analog";
      /// <summary>
      /// Name of group where all DVB-T channels are stored
      /// </summary>
      public static string DVBT = "Digital terrestrial";
      /// <summary>
      /// Name of group where all DVB-C channels are stored
      /// </summary>
      public static string DVBC = "Digital cable";
    }

    /// <summary>
    /// all constant Radio channel group names
    /// </summary>
    public static class RadioGroupNames
    {
      /// <summary>
      /// Name of group where all (new) channels are stored
      /// </summary>
      public static string AllChannels = "All Channels";
      /// <summary>
      /// Name of group where all analog channels are stored
      /// </summary>
      public static string Analog = "Analog";
      /// <summary>
      /// Name of group where all DVB-T channels are stored
      /// </summary>
      public static string DVBT = "Digital terrestrial";
      /// <summary>
      /// Name of group where all DVB-C channels are stored
      /// </summary>
      public static string DVBC = "Digital cable";
    }
  }
}
