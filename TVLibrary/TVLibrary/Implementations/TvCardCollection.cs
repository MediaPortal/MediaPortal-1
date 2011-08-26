#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.RadioWebStream;
using DirectShowLib;
using DirectShowLib.BDA;
using Microsoft.Win32;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations
{
  /// <summary>
  /// this class will enumerate all TV devices
  /// and create the correct ITVCards
  /// </summary>
  public class TvCardCollection
  {
    private readonly List<ITVCard> _cards;
    private readonly IEpgEvents _epgEvents;

    /// <summary>
    /// ctor
    /// </summary>
    public TvCardCollection(IEpgEvents epgEvents)
    {
      Log.Log.WriteFile("----------------------------");
      _epgEvents = epgEvents;
      // Logic here to delay detection of cards
      // Ideally this should occur after standby event.
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("delayCardDetect", "0");
      int delayDetect = Convert.ToInt32(setting.Value);
      if (delayDetect >= 1)
      {
        Log.Log.WriteFile("Detecting Cards in {0} seconds", delayDetect);
        System.Threading.Thread.Sleep(delayDetect * 1000);
      }
      Log.Log.WriteFile("Detecting Cards");
      _cards = new List<ITVCard>();
      DetectCards();
    }

    private static bool ConnectFilter(IFilterGraph2 graphBuilder, IBaseFilter networkFilter, IBaseFilter tunerFilter)
    {
      IPin pinOut = DsFindPin.ByDirection(networkFilter, PinDirection.Output, 0);
      IPin pinIn = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      int hr = graphBuilder.Connect(pinOut, pinIn);
      return (hr == 0);
    }

    /// <summary>
    /// Enumerate all tvcard devices and add them to the list
    /// </summary>
    private void DetectCards()
    {
      ITunerCap _providerType;
      bool genericNP = false;
      //SkyStar 2 & IP Streaming
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      for (int i = 0; i < devices.Length; ++i)
      {
        if (String.Compare(devices[i].Name, "B2C2 MPEG-2 Source", true) == 0)
        {
          Log.Log.WriteFile("Detected SkyStar 2 card");
          TvCardDvbSS2 card = new TvCardDvbSS2(_epgEvents, devices[i]);
          _cards.Add(card);
          //break;  maybe more than one B2C2 card ?
        }
        else if (String.Compare(devices[i].Name, "Elecard NWSource-Plus", true) == 0)
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          Setting setting;
          setting = layer.GetSetting("iptvCardCount", "1");
          int iptvCardCount = Convert.ToInt32(setting.Value);
          for (int cardNum = 0; cardNum < iptvCardCount; cardNum++)
          {
            Log.Log.WriteFile("Detected IP TV Card " + cardNum);
            TvCardDVBIP card = new TvCardDVBIPElecard(_epgEvents, devices[i], cardNum);
            _cards.Add(card);
          }
        }
        else if (String.Compare(devices[i].Name, "MediaPortal IPTV Source Filter", true) == 0)
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          Setting setting;
          setting = layer.GetSetting("iptvCardCount", "1");
          int iptvCardCount = Convert.ToInt32(setting.Value);
          for (int cardNum = 0; cardNum < iptvCardCount; cardNum++)
          {
            Log.Log.WriteFile("Detected IP TV Card " + cardNum);
            TvCardDVBIP card = new TvCardDVBIPBuiltIn(_epgEvents, devices[i], cardNum);
            _cards.Add(card);
          }
        }
      }
      //Hauppauge HD PVR & Colossus
      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      for (int i = 0; i < devices.Length; ++i)
      {
        if (devices[i].Name == null)
        {
          continue;
        }
        if (devices[i].Name.Equals("Hauppauge HD PVR Crossbar"))
        {
          Log.Log.WriteFile("Detected Hauppauge HD PVR");
          TvCardHDPVR card = new TvCardHDPVR(devices[i]);
          _cards.Add(card);
        }
        else if (devices[i].Name.Contains("Hauppauge Colossus Crossbar"))
        {
          Log.Log.WriteFile("Detected Hauppauge Colossus");
          TvCardHDPVR card = new TvCardHDPVR(devices[i]);
          _cards.Add(card);
        }
      }
      //BDA TV devices
      devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      if (devices.Length > 0)
      {
        IFilterGraph2 graphBuilder = (IFilterGraph2)new FilterGraph();
        DsROTEntry rotEntry = new DsROTEntry(graphBuilder);

        Guid networkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
        if (FilterGraphTools.IsThisComObjectInstalled(networkProviderClsId))
        {
          handleInternalNetworkProviderFilter(devices, graphBuilder, networkProviderClsId, rotEntry);
        }
        else
        {
          ITuningSpace tuningSpace = null;
          ILocator locator = null;

          //DVBT
          IBaseFilter networkDVBT = null;
          try
          {
            networkProviderClsId = typeof (DVBTNetworkProvider).GUID;
            networkDVBT = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId,
                                                              "DVBT Network Provider");
            tuningSpace = (ITuningSpace)new DVBTuningSpace();
            tuningSpace.put_UniqueName("DVBT TuningSpace");
            tuningSpace.put_FriendlyName("DVBT TuningSpace");
            tuningSpace.put__NetworkType(typeof (DVBTNetworkProvider).GUID);
            ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Terrestrial);
            locator = (ILocator)new DVBTLocator();
            locator.put_CarrierFrequency(-1);
            locator.put_InnerFEC(FECMethod.MethodNotSet);
            locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
            locator.put_Modulation(ModulationType.ModNotSet);
            locator.put_OuterFEC(FECMethod.MethodNotSet);
            locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
            locator.put_SymbolRate(-1);
            tuningSpace.put_DefaultLocator(locator);
            ((ITuner)networkDVBT).put_TuningSpace(tuningSpace);
          }
          catch (Exception ex)
          {
            Log.Log.Error("DVBT card detection error: {0}", ex.ToString());
          }

          //DVBS
          networkProviderClsId = typeof (DVBSNetworkProvider).GUID;
          IBaseFilter networkDVBS = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId,
                                                                        "DVBS Network Provider");
          tuningSpace = (ITuningSpace)new DVBSTuningSpace();
          tuningSpace.put_UniqueName("DVBS TuningSpace");
          tuningSpace.put_FriendlyName("DVBS TuningSpace");
          tuningSpace.put__NetworkType(typeof (DVBSNetworkProvider).GUID);
          ((IDVBSTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Satellite);
          locator = (ILocator)new DVBTLocator();
          locator.put_CarrierFrequency(-1);
          locator.put_InnerFEC(FECMethod.MethodNotSet);
          locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
          locator.put_Modulation(ModulationType.ModNotSet);
          locator.put_OuterFEC(FECMethod.MethodNotSet);
          locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
          locator.put_SymbolRate(-1);
          tuningSpace.put_DefaultLocator(locator);
          ((ITuner)networkDVBS).put_TuningSpace(tuningSpace);

          //ATSC
          networkProviderClsId = typeof (ATSCNetworkProvider).GUID;
          IBaseFilter networkATSC = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId,
                                                                        "ATSC Network Provider");
          tuningSpace = (ITuningSpace)new ATSCTuningSpace();
          tuningSpace.put_UniqueName("ATSC TuningSpace");
          tuningSpace.put_FriendlyName("ATSC TuningSpace");
          ((IATSCTuningSpace)tuningSpace).put_MaxChannel(10000);
          ((IATSCTuningSpace)tuningSpace).put_MaxMinorChannel(10000);
          ((IATSCTuningSpace)tuningSpace).put_MinChannel(0);
          ((IATSCTuningSpace)tuningSpace).put_MinMinorChannel(0);
          ((IATSCTuningSpace)tuningSpace).put_MinPhysicalChannel(0);
          ((IATSCTuningSpace)tuningSpace).put_InputType(TunerInputType.Antenna);
          locator = (IATSCLocator)new ATSCLocator();
          locator.put_CarrierFrequency(-1);
          locator.put_InnerFEC(FECMethod.MethodNotSet);
          locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
          locator.put_Modulation(ModulationType.ModNotSet);
          locator.put_OuterFEC(FECMethod.MethodNotSet);
          locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
          locator.put_SymbolRate(-1);
          locator.put_CarrierFrequency(-1);
          ((IATSCLocator)locator).put_PhysicalChannel(-1);
          ((IATSCLocator)locator).put_TSID(-1);
          tuningSpace.put_DefaultLocator(locator);
          ((ITuner)networkATSC).put_TuningSpace(tuningSpace);

          //DVBC
          networkProviderClsId = typeof (DVBCNetworkProvider).GUID;
          IBaseFilter networkDVBC = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId,
                                                                        "DVBC Network Provider");
          tuningSpace = (ITuningSpace)new DVBTuningSpace();
          tuningSpace.put_UniqueName("DVBC TuningSpace");
          tuningSpace.put_FriendlyName("DVBC TuningSpace");
          tuningSpace.put__NetworkType(typeof (DVBCNetworkProvider).GUID);
          ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Cable);
          locator = (ILocator)new DVBCLocator();
          locator.put_CarrierFrequency(-1);
          locator.put_InnerFEC(FECMethod.MethodNotSet);
          locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
          locator.put_Modulation(ModulationType.ModNotSet);
          locator.put_OuterFEC(FECMethod.MethodNotSet);
          locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
          locator.put_SymbolRate(-1);
          tuningSpace.put_DefaultLocator(locator);
          ((ITuner)networkDVBC).put_TuningSpace(tuningSpace);

          //MS Network Provider - MCE Roll-up 2 or better
          networkProviderClsId = typeof (NetworkProvider).GUID;
          // First test if the Generic Network Provider is available (only on MCE 2005 + Update Rollup 2)
          if (FilterGraphTools.IsThisComObjectInstalled(networkProviderClsId))
          {
            genericNP = true;
          }
          for (int i = 0; i < devices.Length; i++)
          {
            bool connected = false;
            bool isCablePreferred = false;
            string name = devices[i].Name ?? "unknown";
            name = name.ToLowerInvariant();
            Log.Log.WriteFile("Found card:{0}", name);
            //silicondust work-around for dvb type detection issue. generic provider would always use dvb-t
            if (name.Contains("silicondust hdhomerun tuner"))
            {
              isCablePreferred = CheckHDHomerunCablePrefered(name);
              Log.Log.WriteFile("silicondust hdhomerun detected - prefer cable mode: {0}", isCablePreferred);
            }
            IBaseFilter tmp;
            try
            {
              graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, name, out tmp);
            }
            catch (InvalidComObjectException)
            {
              //ignore bad card
              Log.Log.WriteFile("cannot add filter {0} to graph", devices[i].Name);
              continue;
            }
            //Use the Microsoft Network Provider method first but only if available
            if (genericNP)
            {
              IBaseFilter networkDVB = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId,
                                                                           "Microsoft Network Provider");
              if (ConnectFilter(graphBuilder, networkDVB, tmp))
              {
                Log.Log.WriteFile("Detected DVB card:{0}", name);
                // determine the DVB card supported GUIDs here!
                _providerType = networkDVB as ITunerCap;
                int ulcNetworkTypesMax = 5;
                int pulcNetworkTypes;
                Guid[] pguidNetworkTypes = new Guid[ulcNetworkTypesMax];
                int hr = _providerType.get_SupportedNetworkTypes(ulcNetworkTypesMax, out pulcNetworkTypes,
                                                                 pguidNetworkTypes);
                for (int n = 0; n < pulcNetworkTypes; n++)
                {
                  Log.Log.Debug("Detecting type by MSNP {0}: {1}", n, pguidNetworkTypes[n]);
                  //test the first found guid to determine the DVB card type
                  if (pguidNetworkTypes[n] == (typeof (DVBTNetworkProvider).GUID) && !isCablePreferred)
                  {
                    Log.Log.WriteFile("Detected DVB-T* card:{0}", name);
                    TvCardDVBT dvbtCard = new TvCardDVBT(_epgEvents, devices[i]);
                    _cards.Add(dvbtCard);
                    connected = true;
                  }
                  else if (pguidNetworkTypes[n] == (typeof (DVBSNetworkProvider).GUID) && !isCablePreferred)
                  {
                    Log.Log.WriteFile("Detected DVB-S* card:{0}", name);
                    TvCardDVBS dvbsCard = new TvCardDVBS(_epgEvents, devices[i]);
                    _cards.Add(dvbsCard);
                    connected = true;
                  }
                  else if (pguidNetworkTypes[n] == (typeof (DVBCNetworkProvider).GUID))
                  {
                    Log.Log.WriteFile("Detected DVB-C* card:{0}", name);
                    TvCardDVBC dvbcCard = new TvCardDVBC(_epgEvents, devices[i]);
                    _cards.Add(dvbcCard);
                    connected = true;
                  }
                  else if (pguidNetworkTypes[n] == (typeof (ATSCNetworkProvider).GUID))
                  {
                    Log.Log.WriteFile("Detected ATSC* card:{0}", name);
                    TvCardATSC dvbsCard = new TvCardATSC(_epgEvents, devices[i]);
                    _cards.Add(dvbsCard);
                    connected = true;
                  }
                  if (connected) break; // already found one, no need to continue
                }
                graphBuilder.RemoveFilter(tmp);
                Release.ComObject("tmp filter", tmp);
              }
              else
              {
                Log.Log.WriteFile("Not connected with generic MS Network Provider, using the original method");
                connected = false;
              }
              graphBuilder.RemoveFilter(networkDVB);
              Release.ComObject("ms provider", networkDVB);
            }
            if (!genericNP || !connected)
            {
              if (ConnectFilter(graphBuilder, networkDVBT, tmp))
              {
                Log.Log.WriteFile("Detected DVB-T card:{0}", name);
                TvCardDVBT dvbtCard = new TvCardDVBT(_epgEvents, devices[i]);
                _cards.Add(dvbtCard);
              }
              else if (ConnectFilter(graphBuilder, networkDVBC, tmp))
              {
                Log.Log.WriteFile("Detected DVB-C card:{0}", name);
                TvCardDVBC dvbcCard = new TvCardDVBC(_epgEvents, devices[i]);
                _cards.Add(dvbcCard);
              }
              else if (ConnectFilter(graphBuilder, networkDVBS, tmp))
              {
                Log.Log.WriteFile("Detected DVB-S card:{0}", name);
                TvCardDVBS dvbsCard = new TvCardDVBS(_epgEvents, devices[i]);
                _cards.Add(dvbsCard);
              }
              else if (ConnectFilter(graphBuilder, networkATSC, tmp))
              {
                Log.Log.WriteFile("Detected ATSC card:{0}", name);
                TvCardATSC dvbsCard = new TvCardATSC(_epgEvents, devices[i]);
                _cards.Add(dvbsCard);
              }
              graphBuilder.RemoveFilter(tmp);
              Release.ComObject("tmp filter", tmp);
            }
          }
          FilterGraphTools.RemoveAllFilters(graphBuilder);
          Release.ComObject("dvbc provider", networkDVBC);
          Release.ComObject("atsc provider", networkATSC);
          Release.ComObject("dvbs provider", networkDVBS);
          Release.ComObject("dvbt provider", networkDVBT);
          rotEntry.Dispose();
          Release.ComObject("graph builder", graphBuilder);
        }
      }
      //Analogue TV devices
      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSTVTuner);
      for (int i = 0; i < devices.Length; i++)
      {
        string name = devices[i].Name ?? "unknown";
        name = name.ToLowerInvariant();
        Log.Log.WriteFile("Detected analog card:{0}", name);
        TvCardAnalog analogCard = new TvCardAnalog(devices[i]);
        _cards.Add(analogCard);
      }
      _cards.Add(new RadioWebStreamCard());
    }

    private void handleInternalNetworkProviderFilter(DsDevice[] devices, IFilterGraph2 graphBuilder,
                                                     Guid networkProviderClsId, DsROTEntry rotEntry)
    {
      IDvbNetworkProvider interfaceNetworkProvider;
      TuningType tuningTypes;
      for (int i = 0; i < devices.Length; i++)
      {
        bool isCablePreferred = false;
        string name = devices[i].Name ?? "unknown";
        name = name.ToLowerInvariant();
        Log.Log.WriteFile("Found card:{0}", name);
        //silicondust work-around for dvb type detection issue. generic provider would always use dvb-t
        if (name.Contains("silicondust hdhomerun tuner"))
        {
          isCablePreferred = CheckHDHomerunCablePrefered(name);
          Log.Log.WriteFile("silicondust hdhomerun detected - prefer cable mode: {0}", isCablePreferred);
        }
        IBaseFilter tmp;
        graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, name, out tmp);
        //Use the Microsoft Network Provider method first but only if available
        IBaseFilter networkDVB = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId,
                                                                     "MediaPortal Network Provider");
        interfaceNetworkProvider = (IDvbNetworkProvider)networkDVB;
        string hash = GetHash(devices[i].DevicePath);
        interfaceNetworkProvider.ConfigureLogging(GetFileName(devices[i].DevicePath), hash, LogLevelOption.Debug);
        if (ConnectFilter(graphBuilder, networkDVB, tmp))
        {
          Log.Log.WriteFile("Detected DVB card:{0}- Hash: {1}", name, hash);
          interfaceNetworkProvider.GetAvailableTuningTypes(out tuningTypes);
          Log.Log.WriteFile("TuningTypes: " + tuningTypes);
          // determine the DVB card supported GUIDs here!
          if ((tuningTypes & TuningType.DvbT) != 0 && !isCablePreferred)
          {
            Log.Log.WriteFile("Detected DVB-T* card:{0}", name);
            TvCardDVBT dvbtCard = new TvCardDVBT(_epgEvents, devices[i]);
            _cards.Add(dvbtCard);
          }
          if ((tuningTypes & TuningType.DvbS) != 0 && !isCablePreferred)
          {
            Log.Log.WriteFile("Detected DVB-S* card:{0}", name);
            TvCardDVBS dvbsCard = new TvCardDVBS(_epgEvents, devices[i]);
            _cards.Add(dvbsCard);
          }
          if ((tuningTypes & TuningType.DvbC) != 0)
          {
            Log.Log.WriteFile("Detected DVB-C* card:{0}", name);
            TvCardDVBC dvbcCard = new TvCardDVBC(_epgEvents, devices[i]);
            _cards.Add(dvbcCard);
          }
          if ((tuningTypes & TuningType.Atsc) != 0 && !isCablePreferred)
          {
            Log.Log.WriteFile("Detected ATSC* card:{0}", name);
            TvCardATSC dvbsCard = new TvCardATSC(_epgEvents, devices[i]);
            _cards.Add(dvbsCard);
          }
        }
        graphBuilder.RemoveFilter(tmp);
        Release.ComObject("tmp filter", tmp);
        graphBuilder.RemoveFilter(networkDVB);
        Release.ComObject("ms provider", networkDVB);
      }
      FilterGraphTools.RemoveAllFilters(graphBuilder);
      rotEntry.Dispose();
      Release.ComObject("graph builder", graphBuilder);
    }

    /// <summary>
    /// Generates the file and pathname of the log file
    /// </summary>
    /// <param name="devicePath">Device Path of the card</param>
    /// <returns>Complete filename of the configuration file</returns>
    public static String GetFileName(string devicePath)
    {
      string hash = GetHash(devicePath);
      String pathName = PathManager.GetDataPath;
      String fileName = String.Format(@"{0}\Log\NetworkProvider-{1}.log", pathName, hash);
      Log.Log.WriteFile("NetworkProvider logfilename: " + fileName);
      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      return fileName;
    }

    public static string GetHash(string value)
    {
      byte[] data = Encoding.ASCII.GetBytes(value);
      byte[] hashData = new SHA1Managed().ComputeHash(data);

      string hash = string.Empty;

      foreach (byte b in hashData)
      {
        hash += b.ToString("X2");
      }
      return hash;
    }

    /// <summary>
    /// returns the list of cards present...
    /// </summary>
    public List<ITVCard> Cards
    {
      get { return _cards; }
    }

    #region Hardware specific functions

    private bool CheckHDHomerunCablePrefered(String cardName)
    {
      try
      {
        // tuner name is referenced in registry: silicondust hdhomerun tuner 1210551e-0
        //          HKEY_LOCAL_MACHINE\SOFTWARE\Silicondust\HDHomeRun\Tuners\1210551E-0
        String tunerName = cardName.Replace("silicondust hdhomerun tuner ", "");
        using (
          RegistryKey registryKey =
            Registry.LocalMachine.OpenSubKey(String.Format(@"SOFTWARE\Silicondust\HDHomeRun\Tuners\{0}", tunerName)))
        {
          if (registryKey != null)
          {
            String source = registryKey.GetValue("Source").ToString();
            return source == "Digital Cable";
          }
        }
      }
      catch {}
      return false;
    }

    #endregion
  }
}