#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using DirectShowLib;
using MediaPortal.GUI.Library;

namespace MediaPortal.TV.Database
{

  #region TVNotify class

  [Serializable()]
  public class TVNotify
  {
    public int ID = -1;
    public TVProgram Program = null;
  }

  #endregion

  #region Special Channels structure

  public struct SpecialChannelsStruct
  {
    public string Name;
    public long Frequency;
    public int Number;

    public SpecialChannelsStruct(string name, long frequency, int number)
    {
      Name = name;
      Frequency = frequency;
      Number = number;
    }
  }

  #endregion

  #region enums

  public enum ExternalInputs : int
  {
    svhs = 250000,
    cvbs1 = 250001,
    cvbs2 = 250002,
    rgb = 250003
  }

  #endregion

  #region TVGroup class

  [Serializable()]
  public class TVGroup
  {
    public int ID;
    public string GroupName;
    public int Sort;
    public int Pincode;
    private List<TVChannel> tvChannels = null;

    public override string ToString()
    {
      return GroupName;
    }

    public List<TVChannel> TvChannels
    {
      get
      {
        if (tvChannels == null)
        {
          tvChannels = new List<TVChannel>();
          TVDatabase.GetTVChannelsForGroup(ID, tvChannels);
        }
        return tvChannels;
      }
    }
  }

  #endregion

  #region TVChannel class

  /// <summary>
  /// Class which holds all information about a tv channel
  /// </summary>
  [Serializable()]
  public class TVChannel
  {
    #region special channels

    public static SpecialChannelsStruct[] SpecialChannels =
      {
        new SpecialChannelsStruct("K2", 48250000L, 2),
        new SpecialChannelsStruct("K3", 55250000L, 3),
        new SpecialChannelsStruct("K4", 62250000L, 4),
        new SpecialChannelsStruct("S1", 105250000L, 1),
        new SpecialChannelsStruct("S2", 112250000L, 2),
        new SpecialChannelsStruct("S3", 119250000L, 3),
        new SpecialChannelsStruct("S4", 126250000L, 4),
        new SpecialChannelsStruct("S5", 133250000L, 5),
        new SpecialChannelsStruct("S6", 140250000L, 6),
        new SpecialChannelsStruct("S7", 147250000L, 7),
        new SpecialChannelsStruct("S8", 154250000L, 8),
        new SpecialChannelsStruct("S9", 161250000L, 9),
        new SpecialChannelsStruct("S10", 168250000L, 10),
        new SpecialChannelsStruct("K5", 175250000L, 5),
        new SpecialChannelsStruct("K6", 182250000L, 6),
        new SpecialChannelsStruct("K7", 189250000L, 7),
        new SpecialChannelsStruct("K8", 196250000L, 8),
        new SpecialChannelsStruct("K9", 203250000L, 9),
        new SpecialChannelsStruct("K10", 210250000L, 10),
        new SpecialChannelsStruct("K11", 217250000L, 11),
        new SpecialChannelsStruct("K12", 224250000L, 12),
        new SpecialChannelsStruct("S11", 231250000L, 11),
        new SpecialChannelsStruct("S12", 238250000L, 12),
        new SpecialChannelsStruct("S13", 245250000L, 13),
        new SpecialChannelsStruct("S14", 252250000L, 14),
        new SpecialChannelsStruct("S15", 259250000L, 15),
        new SpecialChannelsStruct("S16", 266250000L, 16),
        new SpecialChannelsStruct("S17", 273250000L, 17),
        new SpecialChannelsStruct("S18", 280250000L, 18),
        new SpecialChannelsStruct("S19", 287250000L, 19),
        new SpecialChannelsStruct("S20", 294250000L, 20),
        new SpecialChannelsStruct("S21", 303250000L, 21),
        new SpecialChannelsStruct("S22", 311250000L, 22),
        new SpecialChannelsStruct("S23", 319250000L, 23),
        new SpecialChannelsStruct("S24", 327250000L, 24),
        new SpecialChannelsStruct("S25", 335250000L, 25),
        new SpecialChannelsStruct("S26", 343250000L, 26),
        new SpecialChannelsStruct("S27", 351250000L, 27),
        new SpecialChannelsStruct("S28", 359250000L, 28),
        new SpecialChannelsStruct("S29", 367250000L, 29),
        new SpecialChannelsStruct("S30", 375250000L, 30),
        new SpecialChannelsStruct("S31", 383250000L, 31),
        new SpecialChannelsStruct("S32", 391250000L, 32),
        new SpecialChannelsStruct("S33", 399250000L, 33),
        new SpecialChannelsStruct("S34", 407250000L, 34),
        new SpecialChannelsStruct("S35", 415250000L, 35),
        new SpecialChannelsStruct("S36", 423250000L, 36),
        new SpecialChannelsStruct("S37", 431250000L, 37),
        new SpecialChannelsStruct("S38", 439250000L, 38),
        new SpecialChannelsStruct("S39", 447250000L, 39),
        new SpecialChannelsStruct("S40", 455250000L, 40),
        new SpecialChannelsStruct("S41", 463250000L, 41),
        new SpecialChannelsStruct("K21", 471250000L, 21),
        new SpecialChannelsStruct("K22", 479250000L, 22),
        new SpecialChannelsStruct("K23", 487250000L, 23),
        new SpecialChannelsStruct("K24", 495250000L, 24),
        new SpecialChannelsStruct("K25", 503250000L, 25),
        new SpecialChannelsStruct("K26", 511250000L, 26),
        new SpecialChannelsStruct("K27", 519250000L, 27),
        new SpecialChannelsStruct("K28", 527250000L, 28),
        new SpecialChannelsStruct("K29", 535250000L, 29),
        new SpecialChannelsStruct("K30", 543250000L, 30),
        new SpecialChannelsStruct("K31", 551250000L, 31),
        new SpecialChannelsStruct("K32", 559250000L, 32),
        new SpecialChannelsStruct("K33", 567250000L, 33),
        new SpecialChannelsStruct("K34", 575250000L, 34),
        new SpecialChannelsStruct("K35", 583250000L, 35),
        new SpecialChannelsStruct("K36", 591250000L, 36),
        new SpecialChannelsStruct("K37", 599250000L, 37),
        new SpecialChannelsStruct("K38", 607250000L, 38),
        new SpecialChannelsStruct("K39", 615250000L, 39),
        new SpecialChannelsStruct("K40", 623250000L, 40),
        new SpecialChannelsStruct("K41", 631250000L, 41),
        new SpecialChannelsStruct("K42", 639250000L, 42),
        new SpecialChannelsStruct("K43", 647250000L, 43),
        new SpecialChannelsStruct("K44", 655250000L, 44),
        new SpecialChannelsStruct("K45", 663250000L, 45),
        new SpecialChannelsStruct("K46", 671250000L, 46),
        new SpecialChannelsStruct("K47", 679250000L, 47),
        new SpecialChannelsStruct("K48", 687250000L, 48),
        new SpecialChannelsStruct("K49", 695250000L, 49),
        new SpecialChannelsStruct("K50", 703250000L, 50),
        new SpecialChannelsStruct("K51", 711250000L, 51),
        new SpecialChannelsStruct("K52", 719250000L, 52),
        new SpecialChannelsStruct("K53", 727250000L, 53),
        new SpecialChannelsStruct("K54", 735250000L, 54),
        new SpecialChannelsStruct("K55", 743250000L, 55),
        new SpecialChannelsStruct("K56", 751250000L, 56),
        new SpecialChannelsStruct("K57", 759250000L, 57),
        new SpecialChannelsStruct("K58", 767250000L, 58),
        new SpecialChannelsStruct("K59", 775250000L, 59),
        new SpecialChannelsStruct("K60", 783250000L, 60),
        new SpecialChannelsStruct("K61", 791250000L, 61),
        new SpecialChannelsStruct("K62", 799250000L, 62),
        new SpecialChannelsStruct("K63", 807250000L, 63),
        new SpecialChannelsStruct("K64", 815250000L, 64),
        new SpecialChannelsStruct("K65", 823250000L, 65),
        new SpecialChannelsStruct("K66", 831250000L, 66),
        new SpecialChannelsStruct("K67", 839250000L, 67),
        new SpecialChannelsStruct("K68", 847250000L, 68),
        new SpecialChannelsStruct("K69", 855250000L, 69),
      };

    #endregion

    #region variables

    private int _epgHours = 1;
    private bool _autoGrabEpg = true;
    private string _channelName;
    private int _channelNumber;
    private int _channelId;
    private long _channelFrequency;
    private string _externalXmlTvId = string.Empty;
    private bool _isExternalChannel = false;
    private string _externalTuneCommand = string.Empty;
    private bool _isVisibleInTvGuide = true;
    private int _country = -1;
    private string _providerName = string.Empty;
    private bool _isScrambled = false;
    private int _sortIndex = -1;
    private TVProgram _currentProgram = null;
    private TVProgram _previousProgram = null;
    private TVProgram _nextProgram = null;
    private DateTime _lastTimeEpgGrabbed = DateTime.MinValue;

    private AnalogVideoStandard _TVStandard;

    #endregion

    #region ctor/dtor

    public TVChannel()
    {
    }

    public TVChannel(string channelname)
    {
      _channelName = channelname;
    }

    public TVChannel Clone()
    {
      TVChannel newChan = new TVChannel();
      newChan.Name = _channelName;
      newChan.Number = _channelNumber;
      newChan.ID = _channelId;
      newChan.Frequency = _channelFrequency;
      newChan.XMLId = _externalXmlTvId;
      newChan.External = _isExternalChannel;
      newChan.ExternalTunerChannel = _externalTuneCommand;
      newChan.VisibleInGuide = _isVisibleInTvGuide;
      newChan.Country = _country;
      newChan.ProviderName = _providerName;
      newChan.Scrambled = _isScrambled;
      newChan.Sort = _sortIndex;
      newChan.EpgHours = _epgHours;
      newChan.AutoGrabEpg = _autoGrabEpg;
      newChan.LastDateTimeEpgGrabbed = LastDateTimeEpgGrabbed;
      return newChan;
    }

    #endregion

    public override bool Equals(object obj)
    {
      if ((obj as TVChannel) == null)
      {
        return false;
      }
      TVChannel chan = (TVChannel) obj;

      if (chan.ID != ID)
      {
        return false;
      }
      if (chan.Frequency != Frequency)
      {
        return false;
      }
      if (chan.Number != Number)
      {
        return false;
      }
      if (chan.Name != Name)
      {
        return false;
      }
      if (chan.XMLId != XMLId)
      {
        return false;
      }
      if (chan.External != External)
      {
        return false;
      }
      if (chan.ExternalTunerChannel != ExternalTunerChannel)
      {
        return false;
      }
      if (chan.VisibleInGuide != VisibleInGuide)
      {
        return false;
      }
      if (chan.Country != Country)
      {
        return false;
      }
      if (chan.ProviderName != ProviderName)
      {
        return false;
      }
      if (chan.Scrambled != Scrambled)
      {
        return false;
      }
      if (chan.Sort != Sort)
      {
        return false;
      }
      if (chan.EpgHours != EpgHours)
      {
        return false;
      }
      if (chan.AutoGrabEpg != AutoGrabEpg)
      {
        return false;
      }
      if (chan.LastDateTimeEpgGrabbed.Year != LastDateTimeEpgGrabbed.Year)
      {
        return false;
      }
      if (chan.LastDateTimeEpgGrabbed.Month != LastDateTimeEpgGrabbed.Month)
      {
        return false;
      }
      if (chan.LastDateTimeEpgGrabbed.Day != LastDateTimeEpgGrabbed.Day)
      {
        return false;
      }
      if (chan.LastDateTimeEpgGrabbed.Hour != LastDateTimeEpgGrabbed.Hour)
      {
        return false;
      }
      if (chan.LastDateTimeEpgGrabbed.Minute != LastDateTimeEpgGrabbed.Minute)
      {
        return false;
      }
      return true;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode() ^ ID.GetHashCode() ^ Frequency.GetHashCode() ^
             Number.GetHashCode() ^ Name.GetHashCode() ^ XMLId.GetHashCode() ^
             External.GetHashCode() ^ ExternalTunerChannel.GetHashCode() ^
             VisibleInGuide.GetHashCode() ^ Country.GetHashCode() ^
             ProviderName.GetHashCode() ^ Scrambled.GetHashCode() ^ Sort.GetHashCode() ^
             EpgHours.GetHashCode() ^ AutoGrabEpg.GetHashCode() ^ LastDateTimeEpgGrabbed.GetHashCode();
    }

    #region properties

    public DateTime LastDateTimeEpgGrabbed
    {
      get { return _lastTimeEpgGrabbed; }
      set { _lastTimeEpgGrabbed = value; }
    }

    /// <summary> 
    /// Property to indicate if this channel is scrambled or not
    /// </summary>
    public bool Scrambled
    {
      get { return _isScrambled; }
      set { _isScrambled = value; }
    }

    /// <summary> 
    /// Property to indicate if this is an internal or external (USB-UIRT) channel
    /// </summary>
    public bool External
    {
      get { return _isExternalChannel; }
      set { _isExternalChannel = value; }
    }

    public int Sort
    {
      get { return _sortIndex; }
      set { _sortIndex = value; }
    }

    /// <summary>
    /// Property to specify the TV standard
    /// </summary>
    public AnalogVideoStandard TVStandard
    {
      get { return _TVStandard; }
      set { _TVStandard = value; }
    }

    /// <summary>
    /// Property that indicates if this channel should be visible in the EPG or not.
    /// </summary>
    public bool VisibleInGuide
    {
      get { return _isVisibleInTvGuide; }
      set { _isVisibleInTvGuide = value; }
    }

    /// <summary> 
    /// Property to get/set the external tuner channel
    /// </summary>
    public string ExternalTunerChannel
    {
      get { return _externalTuneCommand; }
      set
      {
        _externalTuneCommand = value;
        if (_externalTuneCommand.Equals(Strings.Unknown))
        {
          _externalTuneCommand = "";
        }
      }
    }

    /// <summary> 
    /// Property to get/set the ID the tv channel has in the XMLTV file
    /// </summary>
    public string XMLId
    {
      get { return _externalXmlTvId; }
      set { _externalXmlTvId = value; }
    }

    /// <summary>
    /// Property to get/set the ID the tvchannel has in the tv database
    /// </summary>
    public int ID
    {
      get { return _channelId; }
      set { _channelId = value; }
    }

    /// <summary>
    /// Property to get/set the name of the tvchannel
    /// </summary>
    public string Name
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    /// <summary>
    /// Property to get/set the name of the tvchannel
    /// </summary>
    public string ProviderName
    {
      get { return _providerName; }
      set { _providerName = value; }
    }

    /// <summary>
    /// Property to get/set the the tvchannel number
    /// </summary>
    public int Number
    {
      get { return _channelNumber; }
      set { _channelNumber = value; }
    }


    /// <summary>
    /// Property to get/set the the tvchannel country
    /// </summary>
    public int Country
    {
      get { return _country; }
      set { _country = value; }
    }

    /// <summary>
    /// Property to get/set the the frequency of the tvchannel (0=use default)
    /// </summary>
    public long Frequency
    {
      get { return _channelFrequency; }
      set { _channelFrequency = value; }
    }

    //return the current running program for this channel
    public TVProgram CurrentProgram
    {
      get
      {
        DateTime dtNow = DateTime.Now;
        long lNow = Util.Utils.datetolong(dtNow);
        if (_currentProgram != null)
        {
          if (_currentProgram.Start <= lNow && _currentProgram.End >= lNow)
          {
            return _currentProgram;
          }
          _currentProgram = null;
        }

        Update();
        return _currentProgram;
      }
    }

    public bool AutoGrabEpg
    {
      get { return _autoGrabEpg; }
      set { _autoGrabEpg = value; }
    }

    public int EpgHours
    {
      get { return _epgHours; }
      set { _epgHours = value; }
    }

    public bool IsDigital
    {
      get { return TVDatabase.IsDigitalChannel(this); }
    }

    #endregion

    #region methods

    private void Update()
    {
      DateTime dt = DateTime.Now;
      _previousProgram = null;
      _currentProgram = null;
      _nextProgram = null;
      long lNow = Util.Utils.datetolong(dt);
      List<TVProgram> progs = new List<TVProgram>();
      long starttime = Util.Utils.datetolong(dt.AddDays(-2));
      long endtime = Util.Utils.datetolong(dt.AddDays(2));
      TVDatabase.GetProgramsPerChannel(Name, starttime, endtime, ref progs);
      for (int i = 0; i < progs.Count; ++i)
      {
        TVProgram prog = progs[i];
        if (prog.Start <= lNow && prog.End >= lNow)
        {
          _currentProgram = prog;
          if (i - 1 >= 0)
          {
            _previousProgram = progs[i - 1];
          }
          if (i + 1 < progs.Count)
          {
            _nextProgram = progs[i + 1];
          }
          break;
        }
      }
    }

    public TVProgram GetProgramAt(DateTime dt)
    {
      long lNow = Util.Utils.datetolong(DateTime.Now);

      if (_currentProgram == null)
      {
        Update();
      }

      if (_currentProgram != null)
      {
        if (_currentProgram.End < lNow)
        {
          Update();
        }
      }

      lNow = Util.Utils.datetolong(dt);
      if (_previousProgram != null)
      {
        if (_previousProgram.Start <= lNow && _previousProgram.End >= lNow)
        {
          return _previousProgram;
        }
      }
      if (_nextProgram != null)
      {
        if (_nextProgram.Start <= lNow && _nextProgram.End >= lNow)
        {
          return _nextProgram;
        }
      }
      if (_currentProgram != null)
      {
        if (_currentProgram.Start <= lNow && _currentProgram.End >= lNow)
        {
          return _currentProgram;
        }
      }

      List<TVProgram> progs = new List<TVProgram>();
      long starttime = Util.Utils.datetolong(dt.AddDays(-2));
      long endtime = Util.Utils.datetolong(dt.AddDays(2));
      TVDatabase.GetProgramsPerChannel(Name, starttime, endtime, ref progs);
      foreach (TVProgram prog in progs)
      {
        if (prog.Start <= lNow && prog.End >= lNow)
        {
          return prog;
        }
      }
      return null;
    }

    #endregion
  }

  #endregion
}