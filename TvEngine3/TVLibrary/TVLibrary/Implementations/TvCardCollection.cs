/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System;
using System.Collections.Generic;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.RadioWebStream;
using DirectShowLib;
using DirectShowLib.BDA;

namespace TvLibrary.Implementations
{
  /// <summary>
  /// this class will enumerate all TV devices
  /// and create the correct ITVCards
  /// </summary>
  public class TvCardCollection
  {
    readonly List<ITVCard> _cards;
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

    static bool ConnectFilter(IFilterGraph2 graphBuilder, IBaseFilter networkFilter, IBaseFilter tunerFilter)
    {
      IPin pinOut = DsFindPin.ByDirection(networkFilter, PinDirection.Output, 0);
      IPin pinIn = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      int hr = graphBuilder.Connect(pinOut, pinIn);
      return (hr == 0);

    }
    /// <summary>
    /// Enumerate all tvcard devices and add them to the list
    /// </summary>
    void DetectCards()
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
      //Hauppauge HD PVR
      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      for (int i = 0; i < devices.Length; ++i)
      {
        if (String.Compare(devices[i].Name, "Hauppauge HD PVR Crossbar", true) == 0)
        {
          Log.Log.WriteFile("Detected Hauppauge HD PVR");
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

        Guid networkProviderClsId = Guid.Empty;
        ITuningSpace tuningSpace = null;
        ILocator locator = null;

        //DVBT
        IBaseFilter networkDVBT = null;
        try
        {
            networkProviderClsId = typeof(DVBTNetworkProvider).GUID;
            networkDVBT = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId, "DVBT Network Provider");
            tuningSpace = (ITuningSpace)new DVBTuningSpace();
            tuningSpace.put_UniqueName("DVBT TuningSpace");
            tuningSpace.put_FriendlyName("DVBT TuningSpace");
            tuningSpace.put__NetworkType(typeof(DVBTNetworkProvider).GUID);
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
        networkProviderClsId = typeof(DVBSNetworkProvider).GUID;
        IBaseFilter networkDVBS = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId, "DVBS Network Provider");
        tuningSpace = (ITuningSpace)new DVBSTuningSpace();
        tuningSpace.put_UniqueName("DVBS TuningSpace");
        tuningSpace.put_FriendlyName("DVBS TuningSpace");
        tuningSpace.put__NetworkType(typeof(DVBSNetworkProvider).GUID);
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
        networkProviderClsId = typeof(ATSCNetworkProvider).GUID;
        IBaseFilter networkATSC = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId, "ATSC Network Provider");
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
        networkProviderClsId = typeof(DVBCNetworkProvider).GUID;
        IBaseFilter networkDVBC = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId, "DVBC Network Provider");
        tuningSpace = (ITuningSpace)new DVBTuningSpace();
        tuningSpace.put_UniqueName("DVBC TuningSpace");
        tuningSpace.put_FriendlyName("DVBC TuningSpace");
        tuningSpace.put__NetworkType(typeof(DVBCNetworkProvider).GUID);
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
        networkProviderClsId = typeof(NetworkProvider).GUID;
        // First test if the Generic Network Provider is available (only on MCE 2005 + Update Rollup 2)
        if (FilterGraphTools.IsThisComObjectInstalled(networkProviderClsId))
        {
          genericNP = true;
        }
        for (int i = 0; i < devices.Length; i++)
        {
          bool connected = false;
          string name = devices[i].Name ?? "unknown";
          name = name.ToLowerInvariant();
          Log.Log.WriteFile("Found card:{0}", name);
          //silicondust work-around for dvb type detection issue.
          if (name.Contains("silicondust hdhomerun tuner"))
          {
            Log.Log.WriteFile("silicondust hdhomerun detected - using old detection method");
            //use the old manual method
            genericNP = false;
          }
          IBaseFilter tmp;
          graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, name, out tmp);
          //Use the Microsoft Network Provider method first but only if available
          if (genericNP)
          {
            IBaseFilter networkDVB = FilterGraphTools.AddFilterFromClsid(graphBuilder, networkProviderClsId, "Microsoft Network Provider");
            if (ConnectFilter(graphBuilder, networkDVB, tmp))
            {
              Log.Log.WriteFile("Detected DVB card:{0}", name);
              // determine the DVB card supported GUIDs here!
              _providerType = networkDVB as ITunerCap;
              int ulcNetworkTypesMax = 5;
              int pulcNetworkTypes;
              Guid[] pguidNetworkTypes = new Guid[ulcNetworkTypesMax];
              int hr = _providerType.get_SupportedNetworkTypes(ulcNetworkTypesMax, out pulcNetworkTypes, pguidNetworkTypes);
              for (int n = 0; n < pulcNetworkTypes; n++)
              {
                Log.Log.Debug("Detecting type by MSNP {0}: {1}", n, pguidNetworkTypes[n]);
                //test the first found guid to determine the DVB card type
                if (pguidNetworkTypes[n] == (typeof(DVBTNetworkProvider).GUID))
                {
                  Log.Log.WriteFile("Detected DVB-T* card:{0}", name);
                  TvCardDVBT dvbtCard = new TvCardDVBT(_epgEvents, devices[i]);
                  _cards.Add(dvbtCard);
                  connected = true;
                }
                else if (pguidNetworkTypes[n] == (typeof(DVBSNetworkProvider).GUID))
                {
                  Log.Log.WriteFile("Detected DVB-S* card:{0}", name);
                  TvCardDVBS dvbsCard = new TvCardDVBS(_epgEvents, devices[i]);
                  _cards.Add(dvbsCard);
                  connected = true;
                }
                else if (pguidNetworkTypes[n] == (typeof(DVBCNetworkProvider).GUID))
                {
                  Log.Log.WriteFile("Detected DVB-C* card:{0}", name);
                  TvCardDVBC dvbcCard = new TvCardDVBC(_epgEvents, devices[i]);
                  _cards.Add(dvbcCard);
                  connected = true;
                }
                else if (pguidNetworkTypes[n] == (typeof(ATSCNetworkProvider).GUID))
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

    /// <summary>
    /// returns the list of cards present...
    /// </summary>
    public List<ITVCard> Cards
    {
      get
      {
        return _cards;
      }
    }
  }
}
