using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Player;

namespace TvPluginTests.IPlayerStubs
{
  ///<summary>
  /// stub class for iplayer interface, faking an audiostream with the following specs
  /// streams : 5
  /// stream1 : mpeg1, lang: dan.
  /// stream2 : ac3, lang: dan.
  /// stream3 : mpeg1, lang: eng.
  /// stream4 : ac3, lang: eng.
  /// stream4 : mpeg1, lang: deu.  
  ///</summary>
  public class PlayerMultipleAudioStreams : IPlayer
  {
    #region consts

    private const string AUDIO_AC3 = "AC3";
    private const string AUDIO_AC3_PLUS = "AC3plus";
    private const string AUDIO_MPEG1 = "Mpeg1";
    private const string AUDIO_MPEG2 = "Mpeg2";
    private const string AUDIO_AAC = "AAC";
    private const string AUDIO_LATMAAC = "LATMAAC";
    private const string AUDIO_UNKNOWN = "UNKNOWN";

    private const string LANG_DAN = "dan";
    private const string LANG_ENG = "eng";
    private const string LANG_DEU = "deu";
    private const string LANG_UNKNOWN = "unknown";

    #endregion

    public override int AudioStreams
    {
      get { return 5; }
    }

    public override string AudioType(int iStream)
    {
      switch (iStream)
      {
        case 0:
          return AUDIO_MPEG1;
          break;
        case 1:
          return AUDIO_AC3; 
          break;
        case 2:
          return AUDIO_MPEG1;
          break;
        case 3:
          return AUDIO_AC3;
        case 4:
          return AUDIO_MPEG1;
          break;
      }
      return AUDIO_UNKNOWN;
    }

    public override string AudioLanguage(int iStream)
    {
      switch (iStream)
      {
        case 0:
          return LANG_DAN;
          break;
        case 1:
          return LANG_DAN;
          break;
        case 2:
          return LANG_ENG;
          break;
        case 3:
          return LANG_ENG;
        case 4:
          return LANG_DEU;
          break;
      }
      return LANG_UNKNOWN;
    }

    public override eAudioDualMonoMode GetAudioDualMonoMode()
    {
      return eAudioDualMonoMode.UNSUPPORTED;
    }
  }
}
