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
using System.Collections.ObjectModel;
using System.Reflection;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Product
{
  internal abstract class SmarDtvUsbCiProductBase : ISmarDtvUsbCiProduct
  {
    #region variables

    private static ReadOnlyCollection<ISmarDtvUsbCiProduct> _products = null;

    protected string _name = null;
    protected string _deviceNameWdm = null;
    protected string _deviceNameBda = null;
    protected string _dbTunerLinkSettingName = null;
    protected IBaseFilter _filter = null;
    protected DsDevice _device = null;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="SmarDtvUsbCiProductBase"/> class.
    /// </summary>
    /// <param name="name">The product's official name.</param>
    /// <param name="deviceNameWdm">The name of the WDM device exposed by the driver.</param>
    /// <param name="deviceNameBda">The name of the BDA device exposed by the driver.</param>
    /// <param name="dbTunerLinkSettingName">The name of the database setting used to store the tuner link.</param>
    public SmarDtvUsbCiProductBase(string name, string deviceNameWdm, string deviceNameBda, string dbTunerLinkSettingName)
    {
      _name = name;
      _deviceNameWdm = deviceNameWdm;
      _deviceNameBda = deviceNameBda;
      _dbTunerLinkSettingName = dbTunerLinkSettingName;
    }

    /// <summary>
    /// Get the official name of the product.
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
    }

    /// <summary>
    /// Get the driver install state.
    /// </summary>
    public SmarDtvUsbCiDriverInstallState InstallState
    {
      get
      {
        DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
        try
        {
          foreach (DsDevice device in captureDevices)
          {
            if (device.Name != null)
            {
              if (device.Name.Equals(_deviceNameWdm))
              {
                return SmarDtvUsbCiDriverInstallState.WdmDriver;
              }
              else if (device.Name.Equals(_deviceNameBda))
              {
                return SmarDtvUsbCiDriverInstallState.BdaDriver;
              }
            }
          }
          return SmarDtvUsbCiDriverInstallState.NotInstalled;
        }
        finally
        {
          foreach (DsDevice d in captureDevices)
          {
            d.Dispose();
          }
        }
      }
    }

    /// <summary>
    /// Get or set the the tuner that the product is linked to.
    /// </summary>
    /// <value>The tuner's external identifier.</value>
    public string LinkedTuner
    {
      get
      {
        return SettingsManagement.GetValue(_dbTunerLinkSettingName, string.Empty);
      }
      set
      {
        SettingsManagement.SaveValue(_dbTunerLinkSettingName, value);
      }
    }

    /// <summary>
    /// Is the product being used?
    /// </summary>
    public bool IsInUse
    {
      get
      {
        return _device != null;
      }
    }

    /// <summary>
    /// Initialise and take control of the product.
    /// </summary>
    /// <returns>the product's DirectShow filter if successful, otherwise <c>null</c></returns>
    public virtual IBaseFilter Initialise()
    {
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      try
      {
        foreach (DsDevice device in captureDevices)
        {
          if (device.Name != null)
          {
            if (device.Name.Equals(_deviceNameWdm))
            {
              return null;
            }
            else if (device.Name.Equals(_deviceNameBda))
            {
              Guid filterIid = typeof(IBaseFilter).GUID;
              object obj;
              try
              {
                device.Mon.BindToObject(null, null, ref filterIid, out obj);
              }
              catch
              {
                return null;
              }
              _device = device;
              return obj as IBaseFilter;
            }
          }
        }
        return null;
      }
      finally
      {
        foreach (DsDevice d in captureDevices)
        {
          if (d != _device)
          {
            d.Dispose();
          }
        }
      }
    }

    /// <summary>
    /// Deinitialise and release control of the product.
    /// </summary>
    public virtual void Deinitialise()
    {
      if (_device != null)
      {
        _device.Dispose();
        _device = null;
      }
    }

    #region CI interaction

    /// <summary>
    /// Open the CI interface.
    /// </summary>
    /// <param name="callBack">A set of delegates for the driver to invoke when the CI state changes or MMI becomes available.</param>
    /// <returns>an HRESULT indicating whether the interface was successfully opened</returns>
    public abstract int OpenInterface(ref SmarDtvUsbCiCallBack callBack);

    /// <summary>
    /// Open an MMI session with the CAM.
    /// </summary>
    /// <returns>an HRESULT indicating whether the session was successfully opened</returns>
    public abstract int OpenMmiSession();

    /// <summary>
    /// Send an APDU to the CAM.
    /// </summary>
    /// <remarks>
    /// It is not possible send CA PMT APDUs because the underlying session is an MMI session.
    /// </remarks>
    /// <param name="apdu">The APDU.</param>
    /// <param name="apduLength">The length of the APDU in bytes.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully received by the CAM</returns>
    public abstract int SendMmiApdu(int apduLength, byte[] apdu);

    /// <summary>
    /// Send PMT data to the CAM to request a service be descrambled.
    /// </summary>
    /// <param name="pmt">The PMT.</param>
    /// <param name="pmtLength">The length of the PMT in bytes.</param>
    /// <returns>an HRESULT indicating whether the PMT was successfully received by the CAM</returns>
    public abstract int SendPmt(byte[] pmt, short pmtLength);

    /// <summary>
    /// Get version information about the interface, driver and device.
    /// </summary>
    /// <param name="versionInfo">The version information.</param>
    /// <returns>an HRESULT indicating whether the version information was successfully retrieved</returns>
    public abstract int GetVersionInfo(out SmarDtvUsbCiVersionInfo versionInfo);

    #endregion

    /// <summary>
    /// Get a list of the USB CI products that this extension supports and
    /// which are based on the SmarDTV USB CI design.
    /// </summary>
    /// <returns>the product list</returns>
    public static ReadOnlyCollection<ISmarDtvUsbCiProduct> GetProductList()
    {
      // We use a basic cache here.
      if (_products != null)
      {
        return _products;
      }

      List<ISmarDtvUsbCiProduct> products = new List<ISmarDtvUsbCiProduct>(2);
      Assembly a = Assembly.GetExecutingAssembly();
      foreach (Type t in a.GetTypes())
      {
        if (t.IsClass && !t.IsAbstract)
        {
          Type detectorInterface = t.GetInterface(typeof(ISmarDtvUsbCiProduct).Name);
          if (detectorInterface != null)
          {
            products.Add((ISmarDtvUsbCiProduct)Activator.CreateInstance(t));
          }
        }
      }

      _products = new ReadOnlyCollection<ISmarDtvUsbCiProduct>(products);
      return _products;
    }
  }
}