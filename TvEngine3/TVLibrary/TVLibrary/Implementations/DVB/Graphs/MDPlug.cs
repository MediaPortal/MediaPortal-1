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
//#define FORM

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using DirectShowLib;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// base class for MDPlug Array
  /// </summary>
  public class MDPlugs
  {
    #region variables
    private MDPlug[] _mDPlugsArray;
    private int _instanceNumber;
    private String _cardFolder;

    #endregion

    #region ctor
    /// <summary>
    /// MDPlugs public static Creator to test condition before create an instance
    /// </summary>
    public static MDPlugs Create(DsDevice captureDevice)
    {
      string CardFolder;
      int InstanceNumber;
      Log.Log.Info("mdplugs: Create");

      if (IsMDApiEnabled(captureDevice, out CardFolder, out InstanceNumber))
      {
        MDPlugs ret = new MDPlugs(CardFolder, InstanceNumber);
        return ret;
      }
      return null;
    }
    /// <summary>
    /// private MDPlug Creator
    /// </summary>
    private MDPlugs(string CardFolder, int InstanceNumber)
    {
      _instanceNumber = InstanceNumber;
      _cardFolder = CardFolder;
      Log.Log.Info("mdplugs: InstanceNumber(s) = {0}", _instanceNumber);
    }

    /// <summary>
    /// Lazy initializing plugins
    /// </summary>
    private MDPlug[] getPlugins()
    {
      if (_mDPlugsArray == null)
      {
        _mDPlugsArray = new MDPlug[_instanceNumber];
        for (int iplg = 0; iplg < _instanceNumber; iplg++)
          _mDPlugsArray[iplg] = MDPlug.Create(_cardFolder + iplg);
      }
      return _mDPlugsArray;
    }

    /// <summary>
    /// private MDPlug Destructor
    /// </summary>
    ~MDPlugs()
    {
      Close();
    }
    #endregion

    #region static method

    static bool IsMDApiEnabled(DsDevice captureDevice, out string CardFolder, out int InstanceNumber)
    {
      CardFolder = "";
      InstanceNumber = 0;
      if (Directory.Exists("MDPLUGINS") == false)
        return false;
      Log.Log.Info("mdplugs: MDPLUGINS exist");

      bool useMDAPI = false;
      try
      {
        string DisplayMoniker;
        captureDevice.Mon.GetDisplayName(null, null, out DisplayMoniker);
        CardFolder = captureDevice.Name;
        string xmlFile = AppDomain.CurrentDomain.BaseDirectory + "MDPLUGINS\\MDAPICards.xml";
        if (!File.Exists(xmlFile))
        {
          XmlDocument doc = new XmlDocument();
          XmlNode rootNode = doc.CreateElement("cards");
          XmlNode nodeCard = doc.CreateElement("card");
          XmlAttribute attr = doc.CreateAttribute("DevicePath");
          attr.InnerText = DisplayMoniker;
          nodeCard.Attributes.Append(attr);
          attr = doc.CreateAttribute("Name");
          attr.InnerText = captureDevice.Name;
          nodeCard.Attributes.Append(attr);
          attr = doc.CreateAttribute("EnableMdapi");
          attr.InnerText = "1";
          //Fix null pointer problem on first run.
          InstanceNumber = 1;
          nodeCard.Attributes.Append(attr);
          rootNode.AppendChild(nodeCard);
          doc.AppendChild(rootNode);
          doc.Save(xmlFile);
          useMDAPI = true;
        }
        else
        {
          bool cardFound = false;
          XmlDocument doc = new XmlDocument();
          doc.Load(xmlFile);
          XmlNodeList cardList = doc.SelectNodes("/cards/card");
          if (cardList != null)
            foreach (XmlNode nodeCard in cardList)
            {
              if (nodeCard.Attributes["DevicePath"].Value == DisplayMoniker)
              {
                if (nodeCard.Attributes["EnableMdapi"].Value == "yes")
                {
                  InstanceNumber = 1;
                  nodeCard.Attributes["EnableMdapi"].Value = "1";
                  doc.Save(xmlFile);
                }
                if (nodeCard.Attributes["EnableMdapi"].Value == "no")
                {
                  InstanceNumber = 0;
                  nodeCard.Attributes["EnableMdapi"].Value = "0";
                  doc.Save(xmlFile);
                }
                InstanceNumber = Convert.ToInt32(nodeCard.Attributes["EnableMdapi"].Value);
                if (InstanceNumber > 0)
                  useMDAPI = true;
                CardFolder = nodeCard.Attributes["Name"].Value;
                cardFound = true;
                break;
              }
            }
          if (!cardFound)
          {
            XmlNode nodeNewCard = doc.CreateElement("card");
            XmlAttribute attr = doc.CreateAttribute("DevicePath");
            attr.InnerText = DisplayMoniker;
            nodeNewCard.Attributes.Append(attr);
            attr = doc.CreateAttribute("Name");
            attr.InnerText = captureDevice.Name;
            nodeNewCard.Attributes.Append(attr);
            attr = doc.CreateAttribute("EnableMdapi");
            attr.InnerText = "1";
            //Fix null pointer problem on first run.
            InstanceNumber = 1;
            nodeNewCard.Attributes.Append(attr);
            XmlNode rootNode = doc.SelectSingleNode("/cards");
            rootNode.AppendChild(nodeNewCard);
            doc.Save(xmlFile);
            useMDAPI = true;
          }
        }
      } catch (Exception ex)
      {
        Log.Log.Error("mdplugs: error - useMDAPI = {0}", ex);
      }
      Log.Log.Info("mdplugs: useMDAPI = {0}");
      return useMDAPI;
    }
    #endregion

    #region public method

    /// <summary>
    /// Method release all the mdapi filters in ordinary fashion
    /// </summary>
    public void Close()
    {
      if (_mDPlugsArray != null)
      {
        for (int iplg = 0; iplg < _instanceNumber; iplg++)
        {
          if (_mDPlugsArray[iplg] != null)
          {
            _mDPlugsArray[iplg].Close();
            _mDPlugsArray[iplg] = null;
          }
        }
        _mDPlugsArray = null;
      }
    }

    /// <summary>
    /// Connect all mdapifilters between [inftee main] and [TIF MPEG2 Demultiplexer]
    /// </summary>
    public void Connectmdapifilter(IFilterGraph2 graphBuilder, ref IBaseFilter infTeeMain, ref IBaseFilter infTeeSecond, ref IBaseFilter filterMpeg2DemuxTif)
    {
      MDPlug[] _mDPlugs = getPlugins();
      int iplg;
      int hr;
      string filtername;

      //capture -> maintee -> mdapi(n)-> secondtee -> demux
      for (iplg = 0; iplg < _instanceNumber; iplg++)
      {
        filtername = "mdapifilter" + iplg;
        Log.Log.Info("mdplugs: add {0}", filtername);

        hr = graphBuilder.AddFilter(_mDPlugs[iplg].mdapiFilter, filtername);
        if (hr != 0)
        {
          Log.Log.Error("mdplugs:Add {0} returns:0x{1:X}", filtername, hr);
          throw new TvException("Unable to add " + filtername);
        }
      }
      iplg = 0;
      filtername = "mdapifilter" + iplg;

      Log.Log.Info("mdplugs: connect maintee->{0}", filtername);
      IPin mainTeeOut = DsFindPin.ByDirection(infTeeMain, PinDirection.Output, 0);
      IPin mdApiInFirst = DsFindPin.ByDirection(_mDPlugs[iplg].mdapiFilter, PinDirection.Input, 0);
      hr = graphBuilder.Connect(mainTeeOut, mdApiInFirst);
      Release.ComObject("maintee pin0", mainTeeOut);
      Release.ComObject("mdapifilter0 pinin", mdApiInFirst);
      if (hr != 0)
      {
        Log.Log.Info("unable to connect maintee->{0}", filtername);
      }
      if (_instanceNumber > 1)
      {
        for (iplg = 0; iplg < _instanceNumber - 1; iplg++)
        {
          filtername = "mdapifilter" + (iplg + 1);
          Log.Log.Info("mdplugs: add {0}", filtername);
          graphBuilder.AddFilter(_mDPlugs[iplg + 1].mdapiFilter, filtername);

          Log.Log.Info("mdplugs: connect mdapifilter{0}->mdapifilter{1}", iplg, iplg + 1);
          IPin mdApiOutPrev = DsFindPin.ByDirection(_mDPlugs[iplg].mdapiFilter, PinDirection.Output, 0);
          IPin mdApiInNext = DsFindPin.ByDirection(_mDPlugs[iplg + 1].mdapiFilter, PinDirection.Input, 0);
          hr = graphBuilder.Connect(mdApiOutPrev, mdApiInNext);
          Release.ComObject("mdApiPrev pinout", mdApiOutPrev);
          Release.ComObject("mdApiNext pinin", mdApiInNext);
          if (hr != 0)
          {
            Log.Log.Info("unable to connect mdapifilter{0}->mdapifilter{1]", iplg, iplg + 1);
          }
        }
      }
      Log.Log.Info("mdplugs: connect {0}->2nd tee", filtername);
      IPin mdApiOutLast = DsFindPin.ByDirection(_mDPlugs[iplg].mdapiFilter, PinDirection.Output, 0);
      IPin secondTeeIn = DsFindPin.ByDirection(infTeeSecond, PinDirection.Input, 0);
      hr = graphBuilder.Connect(mdApiOutLast, secondTeeIn);
      Release.ComObject("mdApiLast pinout", mdApiOutLast);
      Release.ComObject("secondTee pinin", secondTeeIn);
      if (hr != 0)
      {
        Log.Log.Info("unable to connect {0}->2nd tee", filtername);
      }
      Log.Log.WriteFile("mdplugs:  Render [inftee2]->[demux]");
      IPin secondTeeOut = DsFindPin.ByDirection(infTeeSecond, PinDirection.Output, 0);
      IPin demuxPinIn = DsFindPin.ByDirection(filterMpeg2DemuxTif, PinDirection.Input, 0);
      hr = graphBuilder.Connect(secondTeeOut, demuxPinIn);
      Release.ComObject("secondTee pin0", secondTeeOut);
      Release.ComObject("tifdemux pinin", demuxPinIn);
      if (hr != 0)
      {
        Log.Log.Info("unable to connect [inftee2]->[demux]");
      }
    }

    /// <summary>
    /// Remove all mdapifilters between [inftee main] and [TIF MPEG2 Demultiplexer]
    /// </summary>
    public void Disconnectmdapifilter(IFilterGraph2 graphBuilder, ref IBaseFilter infTeeMain, ref IBaseFilter infTeeSecond, ref IBaseFilter filterMpeg2DemuxTif)
    {
      int iplg;

      MDPlug[] plugins = getPlugins();
      Log.Log.Info("mdplugs: disconnect and reconnecting pins");

      IPin mainTeeOut = DsFindPin.ByDirection(infTeeMain, PinDirection.Output, 0);
      IPin mpegDemuxIn = DsFindPin.ByDirection(filterMpeg2DemuxTif, PinDirection.Input, 0);

      IPin teeSecoundIn = DsFindPin.ByDirection(infTeeSecond, PinDirection.Input, 0);
      IPin teeSecoundOut = DsFindPin.ByDirection(infTeeSecond, PinDirection.Output, 0);

      Log.Log.Info("Disconnecting main pin");
      graphBuilder.Disconnect(mainTeeOut);
      Log.Log.Info("Disconnecting mpeg demux pin");
      graphBuilder.Disconnect(mpegDemuxIn);
      Log.Log.Info("Disconnecting secound tee");
      graphBuilder.Disconnect(teeSecoundIn);
      graphBuilder.Disconnect(teeSecoundOut);

      Log.Log.Info("Reconnecting MainTee to MpegDemux");
      graphBuilder.Connect(mainTeeOut, mpegDemuxIn);
      Release.ComObject("Releasing maintee out", mainTeeOut);
      Release.ComObject("Releasing mpegdemux pinin", mpegDemuxIn);
      Release.ComObject("Releasing teeSecound pinin", teeSecoundIn);
      Release.ComObject("Releasing teeSecound  pinOut", teeSecoundOut);

      //Last Part Remove Filters from Graph.
      //capture -> maintee -> mdapi(n)-> secondtee -> demux
      for (iplg = 0; iplg < _instanceNumber; iplg++)
      {
        string filtername = "mdapifilter" + iplg;
        Log.Log.Info("mdplugs: remove {0}", filtername);

        IPin mdOut = DsFindPin.ByDirection(plugins[iplg].mdapiFilter, PinDirection.Output, 0);
        IPin mdIn = DsFindPin.ByDirection(plugins[iplg].mdapiFilter, PinDirection.Input, 0);
        graphBuilder.Disconnect(mdOut);
        graphBuilder.Disconnect(mdIn);
        Release.ComObject("Releasing  pinout", mdOut);
        Release.ComObject("Releasing  pinin", mdIn);

        int hr = graphBuilder.RemoveFilter(plugins[iplg].mdapiFilter);
        if (hr != 0)
        {
          Log.Log.Error("mdplugs:Remove {0} returns:0x{1:X}", filtername, hr);
          throw new TvException("Unable to remove " + filtername);
        }

        Log.Log.Info("Filter should now be totally removed from graph hr :{0}", hr);
      }

      Log.Log.Info("Graph should be clean of MDAPI Filters..");
    }

    /// <summary>
    /// Frees the given channel
    /// </summary>
    /// <param name="channelName">Channel name to be freed</param>
    public void FreeChannel(string channelName)
    {
      Log.Log.Info("mdplug: FreeChannel {0}", channelName);
      MDPlug[] plugins = getPlugins();
      foreach (MDPlug plugin in plugins)
      {
        if (plugin.IsDecodingChannel(channelName))
        {
          plugin.FreeDecodingChannel(false);
          return;
        }
      }
    }

    /// <summary>
    /// Frees all channels
    /// </summary>
    public void FreeAllChannels()
    {
      Log.Log.Info("mdplug: FreeAllChannels");
      MDPlug[] plugins = getPlugins();
      foreach (MDPlug plugin in plugins)
      {
        plugin.FreeDecodingChannel(true);
      }
    }

    /// <summary>
    /// Sends the current channel to the mdapifilter
    /// </summary>    
    public void SetChannel(int SubCh, IChannel currentChannel, ChannelInfo channelInfo)
    {
      MDPlug[] plugins = getPlugins();

      if (plugins == null || plugins.Length == 0)
      {
        Log.Log.Info("mdplug: SetChannel no MD plugins has been defined for channel {0} subch {1}.", currentChannel.Name, SubCh);
        return;
      }

      foreach (MDPlug plugin in plugins)
      {
        if (plugin.IsDecodingChannel(currentChannel))
        {
          // Increment usage counter of this slot.
          plugin.IncrementDecodingCounter();
          Log.Log.Info("mdplug: SetChannel already decoding channel {0} subch {1}.", currentChannel.Name, SubCh);
          return;
        }
      }

      int slotNumber;
      MDPlug freePlugin = FindFreeSlot(plugins, currentChannel, out slotNumber);
      if (freePlugin == null)
      {
        Log.Log.Info("mdplug: SetChannel no free slots available for channel {0} (try increase limit).", currentChannel.Name);
        return;
      }

      Log.Log.Info("mdplug: SetChannel nr of currently decoding channels {0}.", CalculateSlotsInUse(plugins));
      int idx = 1;
      foreach (MDPlug plugin in plugins)
      {
        IChannel chan = plugin.GetDecodingChannel();
        if (chan != null)
        {
          Log.Log.Info("  slot[" + idx + "] {0}", chan.Name);
        }
        idx++;
      }

      Log.Log.Info("mdplug: SetChannel starting decryption on channel '{0}' using plugin slot {1} of {2} avail.", currentChannel.Name, slotNumber, plugins.Length);

      freePlugin.SetChannel(currentChannel, channelInfo);
    }

    private MDPlug FindFreeSlot(MDPlug[] plugins, IChannel currentChannel, out int slotNumber)
    {
      slotNumber = 1;
      foreach (MDPlug plugin in plugins)
      {
        if (plugin.GetDecodingChannel() == null)
        {
          return plugin;
      }
        slotNumber++;
      }
      return null;
    }

    private int CalculateSlotsInUse(MDPlug[] plugins)
      {
      int slotsInUse = 0;
      foreach (MDPlug plugin in plugins)
      {
        if (plugin.GetDecodingChannel() != null)
        {
          slotsInUse++;
        }
      }
      return slotsInUse;
    }

    #endregion
  }
  /// <summary>
  /// base class for Agarwal's mdapifilter interface
  /// </summary>
  public class MDPlug
  {
    #region constants

    [ComImport, Guid("72E6DB8F-9F33-4D1C-A37C-DE8148C0BE74")]
    private class MDAPIFilter { };
    #endregion

    #region interfaces

    /// <summary>
    /// IChangeChannel interface
    /// </summary>
    [ComVisible(true), ComImport,
    Guid("C3F5AA0D-C475-401B-8FC9-E33FB749CD85"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IChangeChannel
    {
      /// <summary>
      /// Get the file name of media file.
      /// </summary>
      /// <param name="frequency">Frequency</param>
      /// <param name="bandwidth">Bandwith</param>
      /// <param name="polarity">Polarity</param>
      /// <param name="videopid">Video PID</param>
      /// <param name="audiopid">Audio PID</param>
      /// <param name="ecmpid">ECM PID</param>
      /// <param name="caid">CAID</param>
      /// <param name="providerid">Provider ID</param>
      /// <returns></returns>
      /// <remarks>fn should point to a buffer allocated to at least the length of MAX_PATH (=260)</remarks>
      [PreserveSig]
      int ChangeChannel(int frequency, int bandwidth, int polarity, int videopid, int audiopid, int ecmpid, int caid, int providerid);
      /// <summary>
      /// Get the file name of media file.
      /// </summary>
      /// <param name="tp82">The file name buffer.</param>
      /// <returns></returns>
      /// <remarks>fn should point to a buffer allocated to at least the length of MAX_PATH (=260)</remarks>
      int ChangeChannelTP82([In] IntPtr tp82);
      //      int ChangeChannelTP82_Ex([In] IntPtr tp82, [In] IntPtr tPids2Dec);
      ///<summary>
      /// Sets the plugin directory
      ///</summary>
      ///<param name="dir">Directory</param>
      ///<returns></returns>
      int SetPluginsDirectory([In, MarshalAs(UnmanagedType.LPWStr)] string dir);
    }

    /// <summary>
    /// IChangeChannel_Ex interface
    /// </summary>
    [ComVisible(true), ComImport,
    Guid("E98B70EE-F5A1-4f46-B8B8-A1324BA92F5F"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IChangeChannel_Ex
    {
      /// <summary>
      /// Get the file name of media file.
      /// </summary>
      /// <param name="tp82">The file name buffer.</param>
      /// <param name="tPids2Dec">tPids2Dec</param>
      /// <returns></returns>
      /// <remarks>fn should point to a buffer allocated to at least the length of MAX_PATH (=260)</remarks>
      [PreserveSig]
      int ChangeChannelTP82_Ex([In] IntPtr tp82, [In] IntPtr tPids2Dec);
    }
    #endregion

    #region structs

    /// <summary>
    /// CY System 82 struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CA_System82
    {
      /// <summary>
      /// CY TYP
      /// </summary>
      public ushort CA_Typ;
      /// <summary>
      /// ECM
      /// </summary>
      public ushort ECM;
      /// <summary>
      /// EMM
      /// </summary>
      public ushort EMM;
      /// <summary>
      /// Provider Id
      /// </summary>
      public uint Provider_Id;
    }

    /// <summary>
    /// TProgram 82 struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TProgram82
    {
      /// <summary>
      /// Name
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public byte[] Name;   // to simulate c++ char Name[30]
      /// <summary>
      /// Provider
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public byte[] Provider;
      /// <summary>
      /// Country
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public byte[] Country;
      /// <summary>
      /// Frequence
      /// </summary>
      public uint Freq;
      /// <summary>
      /// PType
      /// </summary>
      public byte PType;
      /// <summary>
      /// Voltage
      /// </summary>
      public byte Voltage;
      /// <summary>
      /// AFC
      /// </summary>
      public byte Afc;
      /// <summary>
      /// DiSEqC
      /// </summary>
      public byte DiSEqC;
      /// <summary>
      /// Symbolrate
      /// </summary>
      public uint Symbolrate;
      /// <summary>
      /// QAM
      /// </summary>
      public byte Qam;
      /// <summary>
      /// FEC
      /// </summary>
      public byte Fec;
      /// <summary>
      /// Norm
      /// </summary>
      public byte Norm;
      /// <summary>
      /// TP Id
      /// </summary>
      public ushort Tp_id;
      /// <summary>
      /// Video PID
      /// </summary>
      public ushort Video_pid;
      /// <summary>
      /// Audio PID
      /// </summary>
      public ushort Audio_pid;
      /// <summary>
      /// Teletext PID
      /// </summary>
      public ushort TeleText_pid;          // Teletext PID 
      /// <summary>
      /// PMT PID
      /// </summary>
      public ushort PMT_pid;
      /// <summary>
      /// PCR PID
      /// </summary>
      public ushort PCR_pid;
      /// <summary>
      /// ECM PID
      /// </summary>
      public ushort ECM_PID;
      /// <summary>
      /// SID PID
      /// </summary>
      public ushort SID_pid;
      /// <summary>
      /// AC3 PID
      /// </summary>
      public ushort AC3_pid;
      /// <summary>
      /// TV Type
      /// </summary>
      public byte TVType;           //  == 00 PAL ; 11 == NTSC    
      /// <summary>
      /// Service type
      /// </summary>
      public byte ServiceTyp;
      /// <summary>
      /// CA ID
      /// </summary>
      public byte CA_ID;
      /// <summary>
      /// Temp Audio
      /// </summary>
      public ushort Temp_Audio;
      /// <summary>
      /// Filter numbers
      /// </summary>
      public ushort FilterNr;
      /// <summary>
      /// Filters
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public byte[] Filters;  // to simulate struct PIDFilters Filters[MAX_PID_IDS];
      /// <summary>
      /// CA Number
      /// </summary>
      public ushort CA_Nr;
      /// <summary>
      /// CA System82
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      public CA_System82[] CA_System82;  // to simulate struct TCA_System CA_System[MAX_CA_SYSTEMS];
      /// <summary>
      /// CA Country
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
      public byte[] CA_Country;
      /// <summary>
      /// Marker
      /// </summary>
      public byte Marker;
      /// <summary>
      /// Link TP
      /// </summary>
      public ushort Link_TP;
      /// <summary>
      /// Link SID
      /// </summary>
      public ushort Link_SID;
      /// <summary>
      /// PDynamic
      /// </summary>
      public byte PDynamic;
      /// <summary>
      /// Extern Buffer
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] Extern_Buffer;
    }

    /// <summary>
    /// TPids2Dec struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TPids2Dec
    {
      /// <summary>
      /// PIDs
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
      public ushort[] Pids;
      /// <summary>
      /// Number of pids
      /// </summary>
      public ushort nbPids;
    }
    #endregion

    #region variables
    TProgram82 _mDPlugTProg82;
    TPids2Dec _mPids2Dec;

    internal IBaseFilter mdapiFilter;
    IChangeChannel _changeChannel;
    IChangeChannel_Ex _changeChannel_Ex;
    private IChannel _decodingChannel;
    private int _decodingCounter;
    #endregion

    #region ctor
    /// <summary>
    /// MDapi public static Creator to test condition before create an instance
    /// </summary>
    public static MDPlug Create(string CardFolder)
    {
      MDPlug ret = new MDPlug(CardFolder);
      return ret;
    }
    /// <summary>
    /// private MDPlug Creator
    /// </summary>
    private MDPlug(string CardFolder)
    {
      _mDPlugTProg82.CA_Country = new byte[5];
      _mDPlugTProg82.CA_System82 = new CA_System82[32];
      _mDPlugTProg82.Country = new byte[30];
      _mDPlugTProg82.Extern_Buffer = new byte[16];
      _mDPlugTProg82.Filters = new byte[256];
      _mDPlugTProg82.Name = new byte[30];
      _mDPlugTProg82.Provider = new byte[30];

      _mPids2Dec.Pids = new ushort[63];
      _mPids2Dec.nbPids = 0;

      try
      {
        mdapiFilter = (IBaseFilter)new MDAPIFilter();
        _changeChannel = (IChangeChannel)mdapiFilter;
      } catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      // Test Ex capabilities
      try
      {
        _changeChannel_Ex = (IChangeChannel_Ex)mdapiFilter;
        _changeChannel.SetPluginsDirectory(CardFolder);
        Log.Log.Info("mdplug: This MDAPIfilter accept Extend capabilities");
        Log.Log.Info("mdplug: The mdplugin folder for this instance is : MDPlugins\\{0}", CardFolder);
      } catch (Exception)
      {
        //Log.Log.Write(ex);
        Log.Log.Info("mdplug: This MDAPIfilter doesnt have Extend capabilities. We will use standard");
        _changeChannel_Ex = null;
      }
    }
    /// <summary>
    /// private MDPlug Creator
    /// </summary>
    ~MDPlug()
    {
      Close();
    }
    #endregion

    #region public method

    public IChannel GetDecodingChannel()
    {
      return _decodingChannel;
    }

    public bool IsDecodingChannel(IChannel channel)
    {
      return _decodingChannel != null
        && _decodingChannel.Name == channel.Name;
    }

    public bool IsDecodingChannel(string channelName)
    {
        return _decodingChannel != null
          && _decodingChannel.Name == channelName;
    }

    public void IncrementDecodingCounter()
    {
      if (_decodingChannel != null)
      {
        _decodingCounter++;
        Log.Log.Info("mdplug: usage counter for channel '{0}' is {1}", _decodingChannel.Name, _decodingCounter);
      }
    }

    public void FreeDecodingChannel(bool forceFree)
    {
      if (_decodingChannel != null)
      {
        _decodingCounter--;
        if (forceFree
            || _decodingCounter <= 0)
        {
          _decodingChannel = null;
          _decodingCounter = 0;
        }
        else
        {
          Log.Log.Info("mdplug: usage counter for channel '{0}' is still {1}", _decodingChannel.Name, _decodingCounter);
        }
      }
    }

    /// <summary>
    /// Method release the mdapi filter in ordinary fashion
    /// </summary>
    public void Close()
    {
      if (mdapiFilter != null)
      {
        Marshal.FinalReleaseComObject(mdapiFilter);
      }
      mdapiFilter = null;
      _changeChannel = null;
      _changeChannel_Ex = null;
    }

    /// <summary>
    /// Sends the current channel to the mdapi filter
    /// </summary>
    public void SetChannel(IChannel currentChannel, ChannelInfo channelInfo)
    {
      int Index;
      int end_Index;
      //is mdapi installed?
      if (mdapiFilter == null)
        return; //nop, then return

      //did we already receive the pmt?
      if (channelInfo == null)
        return; //nop, then return
      if (channelInfo.caPMT == null)
        return;
      DVBBaseChannel dvbChannel = currentChannel as DVBBaseChannel;
      if (dvbChannel == null) //not a DVB channel??
        return;

      //set channel name
      if (dvbChannel.Name != null)
      {
        end_Index = _mDPlugTProg82.Name.GetLength(0) - 1;

        if (dvbChannel.Name.Length < end_Index)
        {
          end_Index = dvbChannel.Name.Length;
        }
        for (Index = 0; Index < end_Index; ++Index)
        {
          _mDPlugTProg82.Name[Index] = (byte)dvbChannel.Name[Index];
        }
      }
      else
        end_Index = 0;
      _mDPlugTProg82.Name[end_Index] = 0;

      //set provide name
      if (dvbChannel.Provider != null)
      {
        end_Index = _mDPlugTProg82.Provider.GetLength(0) - 1;
        if (dvbChannel.Provider.Length < end_Index)
          end_Index = dvbChannel.Provider.Length;
        for (Index = 0; Index < end_Index; ++Index)
        {
          _mDPlugTProg82.Provider[Index] = (byte)dvbChannel.Provider[Index];
        }
      }
      else
        end_Index = 0;
      _mDPlugTProg82.Provider[end_Index] = 0;

      //public byte[] Country;
      _mDPlugTProg82.Freq = (uint)dvbChannel.Frequency;
      //public byte PType = (byte);
      _mDPlugTProg82.Afc = 68;
      //_mDPlugTProg82.DiSEqC = (byte)dvbChannel.DisEqc;
      //_mDPlugTProg82.Symbolrate = (uint)dvbChannel.SymbolRate;
      //public byte Qam;

      _mDPlugTProg82.Fec = 0;
      //public byte Norm;
      _mDPlugTProg82.Tp_id = (ushort)dvbChannel.TransportId;
      _mDPlugTProg82.SID_pid = (ushort)dvbChannel.ServiceId;
      _mDPlugTProg82.PMT_pid = (ushort)dvbChannel.PmtPid;
      _mDPlugTProg82.PCR_pid = (ushort)dvbChannel.PcrPid;
      _mDPlugTProg82.Video_pid = 0;
      _mDPlugTProg82.Audio_pid = 0;
      _mDPlugTProg82.TeleText_pid = 0;
      _mDPlugTProg82.AC3_pid = 0;
      _mPids2Dec.nbPids = 0;
      foreach (PidInfo pid in channelInfo.pids)
      {
        if (pid.isVideo)
          if (_mDPlugTProg82.Video_pid == 0) //keep the first one
            _mDPlugTProg82.Video_pid = (ushort)pid.pid;

        if (pid.isAudio)
          if (_mDPlugTProg82.Audio_pid == 0) //keep the first one
            _mDPlugTProg82.Audio_pid = (ushort)pid.pid;

        if (pid.isTeletext)
          if (_mDPlugTProg82.TeleText_pid == 0) //keep the first one
            _mDPlugTProg82.TeleText_pid = (ushort)pid.pid;

        if (pid.isAC3Audio)
          if (_mDPlugTProg82.AC3_pid == 0) //keep the first one
            _mDPlugTProg82.AC3_pid = (ushort)pid.pid;

        _mPids2Dec.Pids[_mPids2Dec.nbPids++] = (ushort)pid.pid;
      }
      _mDPlugTProg82.ServiceTyp = currentChannel.IsTv ? (byte)1 : (byte)2;
      //public byte TVType;           //  == 00 PAL ; 11 == NTSC    
      //public ushort Temp_Audio;
      _mDPlugTProg82.FilterNr = 0; //to test
      //public byte[] Filters;  // to simulate struct PIDFilters Filters[MAX_PID_IDS];
      //public byte[] CA_Country;
      //public byte Marker;
      //public ushort Link_TP;
      //public ushort Link_SID;
      _mDPlugTProg82.PDynamic = 0; //to test
      //public byte[] Extern_Buffer;
      if (channelInfo.caPMT != null)
      {
        //get all EMM's (from CAT (pid 0x1))
        List<ECMEMM> emmList = channelInfo.caPMT.GetEMM();
        //13.05.08 GEMX: pmt is not parsed correctly
        //               This is just a quick fix. The pmt parsing has to be reworked
        //               according to the changes made for TsWriter (PmtParser.cpp - bool CPmtParser::DecodePmt(...)
        //if (emmList.Count <= 0) return;
        for (int i = 0; i < emmList.Count; ++i)
        {
          Log.Log.Info("EMM #{0} CA:0x{1:X} EMM:0x{2:X} ID:0x{3:X}",
                i, emmList[i].CaId, emmList[i].Pid, emmList[i].ProviderId);
        }

        //get all ECM's for this service
        List<ECMEMM> ecmList = channelInfo.caPMT.GetECM();
        for (int i = 0; i < ecmList.Count; ++i)
        {
          Log.Log.Info("ECM #{0} CA:0x{1:X} ECM:0x{2:X} ID:0x{3:X}",
                i, ecmList[i].CaId, ecmList[i].Pid, ecmList[i].ProviderId);
        }


        //Clearing OLD Values...
        for (int y = 0; y < _mDPlugTProg82.CA_System82.Length; y++)
        {

          _mDPlugTProg82.CA_System82[y].CA_Typ = 0;
          _mDPlugTProg82.CA_System82[y].ECM = 0;
          _mDPlugTProg82.CA_System82[y].EMM = 0;
          _mDPlugTProg82.CA_System82[y].Provider_Id = 0;

        }


        _mDPlugTProg82.CA_Nr = (ushort)ecmList.Count;
        int count = 0;
        for (int x = 0; x < ecmList.Count; ++x)
        {
          _mDPlugTProg82.CA_System82[x].CA_Typ = (ushort)ecmList[x].CaId;
          _mDPlugTProg82.CA_System82[x].ECM = (ushort)ecmList[x].Pid;
          _mDPlugTProg82.CA_System82[x].EMM = 0;
          _mDPlugTProg82.CA_System82[x].Provider_Id = (uint)ecmList[x].ProviderId;
          count++;
        }


        for (int i = 0; i < emmList.Count; ++i)
        {
          bool found = false;
          for (int j = 0; j < count; ++j)
          {
            if (emmList[i].ProviderId == _mDPlugTProg82.CA_System82[j].Provider_Id && emmList[i].CaId == _mDPlugTProg82.CA_System82[j].CA_Typ)
            {
              found = true;
              _mDPlugTProg82.CA_System82[j].EMM = (ushort)emmList[i].Pid;
              break;
            }
          }
          if (!found)
          {
            _mDPlugTProg82.CA_System82[count].CA_Typ = (ushort)emmList[i].CaId;
            _mDPlugTProg82.CA_System82[count].ECM = 0;
            _mDPlugTProg82.CA_System82[count].EMM = (ushort)emmList[i].Pid;
            _mDPlugTProg82.CA_System82[count].Provider_Id = (uint)emmList[i].ProviderId;
            count++;
          }
        }




        _mDPlugTProg82.CA_ID = 0;
        _mDPlugTProg82.CA_Nr = (ushort)count;
        if (count == 0)
        {
          _mDPlugTProg82.ECM_PID = 0;
        }
        else
        {
          _mDPlugTProg82.ECM_PID = _mDPlugTProg82.CA_System82[0].ECM;
        }
        //find preferred ECM from preferred MDAPIProvID.xml file and pointing CA_ID on the right CA_System82 row
        //first search in channel list for individual match, else search for provider ID match else search for CA_Typ match
        string xmlFile = AppDomain.CurrentDomain.BaseDirectory + "MDPLUGINS\\MDAPIProvID.xml";
        if (File.Exists(xmlFile))
        {
          try
          {
            bool providfound = false;
            bool channelfound = false;
            bool catypfound = false;
            int i;
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFile);

            XmlNode mainNode = doc.SelectSingleNode("/mdapi");
            bool filloutXMLFile;
            if (!((XmlElement)mainNode).HasAttribute("fillout"))
            {
              XmlAttribute fillout = doc.CreateAttribute("fillout");
              fillout.Value = "" + false;
              mainNode.Attributes.Append(fillout);
              doc.Save(xmlFile);
            }

            Boolean.TryParse(mainNode.Attributes["fillout"].Value, out filloutXMLFile);

            Log.Log.Info("mdplug: MDAPIProvID.xml Filling out MDAPIProvID {0} ", filloutXMLFile);

            XmlNodeList channelList = doc.SelectNodes("/mdapi/channels/channel");
            string Tp_id = String.Format("{0:D}", _mDPlugTProg82.Tp_id);
            string SID_pid = String.Format("{0:D}", _mDPlugTProg82.SID_pid);
            string PMT_pid = String.Format("{0:D}", _mDPlugTProg82.PMT_pid);

            if (channelList != null)
              foreach (XmlNode nodechannel in channelList)
              {
                if (nodechannel.Attributes["tp_id"].Value == Tp_id
                    && nodechannel.Attributes["sid"].Value == SID_pid
                    && nodechannel.Attributes["pmt_pid"].Value == PMT_pid)
                {
                  for (i = 0; i < count; ++i)
                  {
                    string ECM_pid = String.Format("{0:D}", _mDPlugTProg82.CA_System82[i].ECM);
                    if (nodechannel.Attributes["ecm_pid"].Value == ECM_pid)
                    {
                      _mDPlugTProg82.CA_ID = (byte)i;
                      _mDPlugTProg82.ECM_PID = _mDPlugTProg82.CA_System82[i].ECM;

                      if (((XmlElement)nodechannel).HasAttribute("emm_pid"))
                      {
                        _mDPlugTProg82.CA_System82[i].EMM = UInt16.Parse(((XmlElement)nodechannel).GetAttribute("emm_pid"));
                      }

                      channelfound = true;
                      break;
                    }
                  }
                  if (channelfound)
                    break;
                }
              }
            if (!channelfound)
            {
              XmlNodeList providList = doc.SelectNodes("/mdapi/providers/provider");
              if (providList != null)
                foreach (XmlNode nodeprovid in providList)
                {
                  for (i = 0; i < count; ++i)
                  {
                    string Provider_Id = String.Format("{0:D}", _mDPlugTProg82.CA_System82[i].Provider_Id);
                    if (nodeprovid.Attributes["ID"].Value == Provider_Id)
                    {
                      _mDPlugTProg82.CA_ID = (byte)i;
                      _mDPlugTProg82.ECM_PID = _mDPlugTProg82.CA_System82[i].ECM;
                      providfound = true;
                      break;
                    }
                  }
                  if (providfound)
                    break;
                }
            }

            if (!channelfound && !providfound)
            {
              XmlNodeList catypList = doc.SelectNodes("/mdapi/CA_Types/CA_Type");
              if (catypList != null)
                foreach (XmlNode nodecatyp in catypList)
                {
                  for (i = 0; i < count; ++i)
                  {
                    string CAtyp_Id = String.Format("{0:D}", _mDPlugTProg82.CA_System82[i].CA_Typ);
                    if (nodecatyp.Attributes["ID"].Value == CAtyp_Id)
                    {
                      _mDPlugTProg82.CA_ID = (byte)i;
                      _mDPlugTProg82.ECM_PID = _mDPlugTProg82.CA_System82[i].ECM;
                      catypfound = true;
                      break;
                    }
                  }
                  if (catypfound)
                    break;
                }
            }
            Log.Log.Info("mdplug: MDAPIProvID.xml mode used = Channel:{0} Provider:{1} Ca_typ:{2}",
                    channelfound,
                    providfound,
                    catypfound);


            if (!channelfound)
            {
              try
              {
                Log.Log.Info("mdapi: Attempting to add entry to MDAPIProvID.xml");

                XmlNode node;
                if (filloutXMLFile && ecmList.Count > 0)
                {
                  node = doc.SelectSingleNode("/mdapi/channels");

                  String comment = "";

                  XmlNode cnode = doc.CreateElement("channel");

                  comment += "Channel Name : " + dvbChannel.Name + " ";

                  XmlAttribute tpid = doc.CreateAttribute("tp_id");
                  tpid.Value = "" + dvbChannel.TransportId;
                  cnode.Attributes.Append(tpid);
                  XmlAttribute sid = doc.CreateAttribute("sid");
                  sid.Value = "" + dvbChannel.ServiceId;
                  cnode.Attributes.Append(sid);
                  XmlAttribute pmt_pid = doc.CreateAttribute("pmt_pid");
                  pmt_pid.Value = "" + dvbChannel.PmtPid;
                  cnode.Attributes.Append(pmt_pid);
                  //CA_ID
                  //_mDPlugTProg82.CA_System82[0].ECM;
                  XmlAttribute ecm_pid = doc.CreateAttribute("ecm_pid");
                  ecm_pid.Value = "" + _mDPlugTProg82.ECM_PID;
                  cnode.Attributes.Append(ecm_pid);

                  String possibleValues = "";
                  for (int x = 0; x < ecmList.Count; ++x)
                  {
                    if (x != 0)
                      possibleValues += ", ";
                    possibleValues += "" + ecmList[x].Pid;
                    //possibleValues += "(" + ecmList[x].CaId + ")";
                    // ecmList[x].CaId;
                    // ecmList[x].Pid;
                    // ecmList[x].ProviderId;
                  }

                  comment += "Possible ECM values ( " + possibleValues + " )";


                  node.AppendChild(doc.CreateComment(comment));
                  node.AppendChild(cnode);

                  node = doc.SelectSingleNode("/mdapi/providers");

                  if (node != null)
                  {


                    for (int x = 0; x < ecmList.Count; ++x)
                    {
                      bool found = false;
                      XmlNodeList providList = doc.SelectNodes("/mdapi/providers/provider");
                      if (providList != null)
                        foreach (XmlNode nodeprovid in providList)
                        {
                          String value = nodeprovid.Attributes["ID"].Value;
                          if (Int32.Parse(value).CompareTo(ecmList[x].ProviderId) == 0)
                          {
                            found = true;
                            break;
                          }
                        }

                      if (!found && ecmList[x].ProviderId != 0)
                      {

                        XmlNode d = doc.CreateElement("provider");
                        XmlAttribute r = doc.CreateAttribute("ID");
                        r.Value = "" + ecmList[x].ProviderId;
                        d.Attributes.Append(r);
                        node.AppendChild(d);
                      }


                    }


                  }


                  node = doc.SelectSingleNode("/mdapi/CA_Types");
                  if (node != null)
                  {
                    for (int x = 0; x < ecmList.Count; ++x)
                    {
                      bool found = false;
                      XmlNodeList providList = doc.SelectNodes("/mdapi/CA_Types/CA_Type");
                      if (providList != null)
                        foreach (XmlNode nodeprovid in providList)
                        {
                          String value = nodeprovid.Attributes["ID"].Value;
                          if (Int32.Parse(value).CompareTo(ecmList[x].CaId) == 0)
                          {
                            found = true;
                            break;
                          }
                        }
                      if (!found)
                      {

                        XmlNode d = doc.CreateElement("CA_Type");
                        XmlAttribute r = doc.CreateAttribute("ID");
                        r.Value = "" + ecmList[x].CaId;
                        d.Attributes.Append(r);
                        node.AppendChild(d);
                      }

                    }

                  }

                  doc.Save(xmlFile);

                }

              } catch (Exception g)
              {
                Log.Log.Write(g);
              }

            }


          } catch (Exception e)
          {
            Log.Log.Write(e);
          }
        }

        Log.Log.Info("mdplug: tp_id:{0}(0x{1:X}) sid:{2}(0x{3:X}) pmt_id:{4}(0x{5:X})",
                _mDPlugTProg82.Tp_id,
                _mDPlugTProg82.Tp_id,
                _mDPlugTProg82.SID_pid,
                _mDPlugTProg82.SID_pid,
                _mDPlugTProg82.PMT_pid,
                _mDPlugTProg82.PMT_pid);

        for (int i = 0; i < count; ++i)
        {
          Log.Log.Info("mdplug: #{0} CA_typ:{1}(0x{2:X}) ECM:{3}(0x{4:X}) EMM:0x{5:X} provider:{6}(0x{7:X})",
                  i,
                  _mDPlugTProg82.CA_System82[i].CA_Typ,
                  _mDPlugTProg82.CA_System82[i].CA_Typ,
                  _mDPlugTProg82.CA_System82[i].ECM,
                  _mDPlugTProg82.CA_System82[i].ECM,
                  _mDPlugTProg82.CA_System82[i].EMM,
                  _mDPlugTProg82.CA_System82[i].Provider_Id,
                  _mDPlugTProg82.CA_System82[i].Provider_Id);
        }
      }
      //ca types:
      //0xb00 : conax
      //0x100 : seca
      //0x500 : Viaccess
      //0x622 : irdeto
      //0x1801: Nagravision
      IntPtr lparam = Marshal.AllocHGlobal(Marshal.SizeOf(_mDPlugTProg82));
      Marshal.StructureToPtr(_mDPlugTProg82, lparam, true);
      IntPtr lparam2 = Marshal.AllocHGlobal(Marshal.SizeOf(_mPids2Dec));
      Marshal.StructureToPtr(_mPids2Dec, lparam2, true);
      try
      {
        if (_changeChannel_Ex != null)
          _changeChannel_Ex.ChangeChannelTP82_Ex(lparam, lparam2);
        else
          _changeChannel.ChangeChannelTP82(lparam);

        _decodingChannel = currentChannel;
        _decodingCounter = 1;

        Log.Log.Info("mdplug: Send channel change to MDAPI filter Ca_Id:{0} CA_Nr:{1} ECM_PID:{2}(0x{3:X})",
           _mDPlugTProg82.CA_ID,
           _mDPlugTProg82.CA_Nr,
           _mDPlugTProg82.ECM_PID,
           _mDPlugTProg82.ECM_PID);
      } catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      Marshal.FreeHGlobal(lparam);
      Marshal.FreeHGlobal(lparam2);
    }
    #endregion
  }
}
