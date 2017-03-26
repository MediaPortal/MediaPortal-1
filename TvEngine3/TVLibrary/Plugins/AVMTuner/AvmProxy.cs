using System;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.Common.UPnP;
using UPnP.Infrastructure.CP.DeviceTree;

namespace UPnPDeviceSpy
{
  public class AvmProxy : UPnPServiceProxyBase
  {
    public AvmProxy(CpService serviceStub)
      : base(serviceStub, "AVM Proxy")
    {

    }

    public string GetChannelListHD()
    {
      try
      {
        CpAction action = GetAction("X_AVM-DE_GetChannelList_m3u_HD");
        IList<object> inParameters = new List<object>();
        IList<object> outParameters = action.InvokeAction(inParameters);
        return (string)outParameters[0];
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public string GetChannelListSD()
    {
      try
      {
        CpAction action = GetAction("X_AVM-DE_GetChannelList_m3u_SD");
        IList<object> inParameters = new List<object>();
        IList<object> outParameters = action.InvokeAction(inParameters);
        return (string)outParameters[0];
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public string GetChannelListRadio()
    {
      try
      {
        CpAction action = GetAction("X_AVM-DE_GetChannelList_m3u_Radio");
        IList<object> inParameters = new List<object>();
        IList<object> outParameters = action.InvokeAction(inParameters);
        return (string)outParameters[0];
      }
      catch (Exception ex)
      {
        throw;
      }
    }
    public byte GetNumberOfTuners()
    {
      try
      {
        CpAction action = GetAction("X_AVM-DE_GetNumberOfTuners");
        IList<object> inParameters = new List<object>();
        IList<object> outParameters = action.InvokeAction(inParameters);
        return (byte)outParameters[0];
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public bool GetTunerInfos(byte tunerNumber, out bool isUsed, out bool hasLock, out double signalPower, out double snr, out string channelName, out byte clientCount, out string ipAddresses)
    {
      try
      {
        CpAction action = GetAction("X_AVM-DE_GetTunerInfo");
        IList<object> inParameters = new List<object> { tunerNumber };
        IList<object> outParameters = action.InvokeAction(inParameters);
        if (outParameters.Count != 7)
          throw new ArgumentException();
        isUsed = (bool)outParameters[0];
        hasLock = (bool)outParameters[1];
        signalPower = double.Parse((string)outParameters[2], CultureInfo.InvariantCulture);
        snr = double.Parse((string)outParameters[3], CultureInfo.InvariantCulture);
        channelName = (string)outParameters[4];
        clientCount = (byte)outParameters[5];
        ipAddresses = (string)outParameters[6];
        return true;
      }
      catch (Exception ex)
      {
        throw;
      }
    }
  }
}
