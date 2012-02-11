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


using System;
using System.Collections.Generic;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvDatabase;
using DirectShowLib.BDA;

namespace TvLibrary.Implementations.Helper.Providers
{

  /// <summary>
  /// Helper class for provider specific functionality
  /// </summary>
  public class SkyUK
    : Provider
  {
    #region Fields

    /// <summary>
    /// Singleton instance
    /// </summary>
    private static readonly SkyUK _instance = new SkyUK();

    private const uint INTERNAL_ID = 0x01;

    private const int SKY_UK_DEFAULT_TRANSPONDER_FREQUENCY = 11778000;
    private const string SKY_UK_DEFAULT_TRANSPONDER_POLARISATION = "Vertical";
    private const int SKY_UK_DEFAULT_TRANSPONDER_SYMBOL_RATE = 27500;
    private const string SKY_UK_DEFAULT_TRANSPONDER_INNER_FEC = "2/3";
    private const int SKY_UK_DEFAULT_NETWORK_ID = 0x2;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the internal id for Sky UK
    /// </summary>
    public uint InternalId
    {
      get
      {
        return INTERNAL_ID;
      }
    }

    /// <summary>
    /// Gets the instance
    /// </summary>
    public static SkyUK Instance
    {
      get
      {
        return _instance;
      }
    }

    /// <summary>
    /// Gets/Sets if the EPG should be grabbed for Sky UK
    /// </summary>
    public bool EPGGrabbingEnabled
    {
      get
      {
        return GetSetting<bool>("SkyUKGrabEPG", false);
      }
      set
      {
        SetSetting("SkyUKGrabEPG", value);
      }
    }

    /// <summary>
    /// Gets/Sets the last epg grab time
    /// </summary>
    public DateTime LastEPGGrabTime
    {
      get
      {
        DateTime lastGrabTime = DateTime.MinValue;

        DateTime.TryParse(GetSetting<string>("SkyUKLastEPGGrabTime", DateTime.MinValue.ToString()), out lastGrabTime);

        return lastGrabTime;
      }
      set
      {
        SetSetting("SkyUKLastEPGGrabTime", value.ToString());
      }
    }

    /// <summary>
    /// Gets/Sets the default transponder frequency
    /// </summary>
    public int DefaultTransponderFrequency
    {
      get
      {
        return GetSetting<int>("SkyUKDefaultTransponderFrequency", SKY_UK_DEFAULT_TRANSPONDER_FREQUENCY);
      }
      set
      {
        SetSetting("SkyUKDefaultTransponderFrequency", value);
      }
    }

    /// <summary>
    /// Gets/Sets the default transponder polarisation
    /// </summary>
    public string DefaultTransponderPolarisationString
    {
      get
      {
        return GetSetting<string>("SkyUKDefaultTransponderPolarisation", SKY_UK_DEFAULT_TRANSPONDER_POLARISATION);
      }
      set
      {
        SetSetting("SkyUKDefaultTransponderPolarisation", value);
      }
    }

    public Polarisation DefaultTransponderPolarisation
    {
      get
      {
        switch (DefaultTransponderPolarisationString)
        {
          case "Horizontal":
            return Polarisation.LinearH;
          case "Vertical":
            return Polarisation.LinearV;
          case "Circular Left":
            return Polarisation.CircularL;
          case "Circular Right":
            return Polarisation.CircularR;
        }

        return Polarisation.LinearV;
      }
      set
      {
        switch (value)
        {
          case Polarisation.LinearH:
            DefaultTransponderPolarisationString = "Horizontal";
            break;
          case Polarisation.LinearV:
            DefaultTransponderPolarisationString = "Vertical";
            break;
          case Polarisation.CircularL:
            DefaultTransponderPolarisationString = "Circular Left";
            break;
          case Polarisation.CircularR:
            DefaultTransponderPolarisationString = "Circular Right";
            break;

          default:
            DefaultTransponderPolarisationString = SKY_UK_DEFAULT_TRANSPONDER_POLARISATION;
            break;
        }
      }
    }

    /// <summary>
    /// Gets/Sets the default transponder symbol rate
    /// </summary>
    public int DefaultTransponderSymbolRate
    {
      get
      {
        return GetSetting<int>("SkyUKDefaultTransponderSymbolRate", SKY_UK_DEFAULT_TRANSPONDER_SYMBOL_RATE);
      }
      set
      {
        SetSetting("SkyUKDefaultTransponderSymbolRate", value);
      }
    }

    /// <summary>
    /// Gets/Sets the default transponder inner FEC
    /// </summary>
    public string DefaultTransponderInnerFECString
    {
      get
      {
        return GetSetting<string>("SkyUKDefaultTransponderInnerFEC", SKY_UK_DEFAULT_TRANSPONDER_INNER_FEC);
      }
      set
      {
        SetSetting("SkyUKDefaultTransponderInnerFEC", value);
      }
    }

    /// <summary>
    /// Gets the default transponder inner FEC
    /// </summary>
    public BinaryConvolutionCodeRate DefaultTransponderInnerFEC
    {
      get
      {
        switch (DefaultTransponderInnerFECString)
        {
          case "1/2":
            return BinaryConvolutionCodeRate.Rate1_2;
          case "1/3":
            return BinaryConvolutionCodeRate.Rate1_3;
          case "1/4":
            return BinaryConvolutionCodeRate.Rate1_4;
          case "2/3":
            return BinaryConvolutionCodeRate.Rate2_3;
          case "2/5":
            return BinaryConvolutionCodeRate.Rate2_5;
          case "3/4":
            return BinaryConvolutionCodeRate.Rate3_4;
          case "3/5":
            return BinaryConvolutionCodeRate.Rate3_5;
          case "4/5":
            return BinaryConvolutionCodeRate.Rate4_5;
          case "5/11":
            return BinaryConvolutionCodeRate.Rate5_11;
          case "5/6":
            return BinaryConvolutionCodeRate.Rate5_6;
          case "7/8":
            return BinaryConvolutionCodeRate.Rate7_8;
          case "8/9":
            return BinaryConvolutionCodeRate.Rate8_9;
          case "9/10":
            return BinaryConvolutionCodeRate.Rate9_10;
        }

        return BinaryConvolutionCodeRate.Rate2_3;
      }
      set
      {
        switch (value)
        {
          case BinaryConvolutionCodeRate.Rate1_2:
            DefaultTransponderInnerFECString = "1/2";
            break;
          case BinaryConvolutionCodeRate.Rate1_3:
            DefaultTransponderInnerFECString = "1/3";
            break;
          case BinaryConvolutionCodeRate.Rate1_4:
            DefaultTransponderInnerFECString = "1/4";
            break;
          case BinaryConvolutionCodeRate.Rate2_3:
            DefaultTransponderInnerFECString = "2/3";
            break;
          case BinaryConvolutionCodeRate.Rate2_5:
            DefaultTransponderInnerFECString = "2/5";
            break;
          case BinaryConvolutionCodeRate.Rate3_4:
            DefaultTransponderInnerFECString = "3/4";
            break;
          case BinaryConvolutionCodeRate.Rate3_5:
            DefaultTransponderInnerFECString = "3/5";
            break;
          case BinaryConvolutionCodeRate.Rate4_5:
            DefaultTransponderInnerFECString = "4/5";
            break;
          case BinaryConvolutionCodeRate.Rate5_11:
            DefaultTransponderInnerFECString = "5/11";
            break;
          case BinaryConvolutionCodeRate.Rate5_6:
            DefaultTransponderInnerFECString = "5/6";
            break;
          case BinaryConvolutionCodeRate.Rate6_7:
            DefaultTransponderInnerFECString = "6/7";
            break;
          case BinaryConvolutionCodeRate.Rate7_8:
            DefaultTransponderInnerFECString = "7/8";
            break;
          case BinaryConvolutionCodeRate.Rate8_9:
            DefaultTransponderInnerFECString = "8/9";
            break;
          case BinaryConvolutionCodeRate.Rate9_10:
            DefaultTransponderInnerFECString = "9/10";
            break;

          default:
            DefaultTransponderInnerFECString = "2/3";
            break;
        }
      }
    }

    /// <summary>
    /// Gets/Sets the default transponder symbol rate
    /// </summary>
    public int NetworkId
    {
      get
      {
        return GetSetting<int>("SkyUKNetworkId", SKY_UK_DEFAULT_NETWORK_ID);
      }
      set
      {
        SetSetting("SkyUKNetworkId", value);
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Resets the default transponder parameters
    /// </summary>
    public void ResetDefaultTransponderParameters()
    {
      DefaultTransponderFrequency = SKY_UK_DEFAULT_TRANSPONDER_FREQUENCY;
      DefaultTransponderPolarisationString = SKY_UK_DEFAULT_TRANSPONDER_POLARISATION;
      DefaultTransponderSymbolRate = SKY_UK_DEFAULT_TRANSPONDER_SYMBOL_RATE;
      DefaultTransponderInnerFECString = SKY_UK_DEFAULT_TRANSPONDER_INNER_FEC;
      NetworkId = SKY_UK_DEFAULT_NETWORK_ID;
    }

    #endregion

  }
}
