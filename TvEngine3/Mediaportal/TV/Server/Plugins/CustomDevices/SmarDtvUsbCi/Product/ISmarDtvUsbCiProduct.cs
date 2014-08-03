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

using DirectShowLib;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Product
{
  internal interface ISmarDtvUsbCiProduct
  {
    /// <summary>
    /// Get the official name of the product.
    /// </summary>
    string Name
    {
      get;
    }

    /// <summary>
    /// Get the driver install state.
    /// </summary>
    SmarDtvUsbCiDriverInstallState InstallState
    {
      get;
    }

    /// <summary>
    /// Get or set the the tuner that the product is linked to.
    /// </summary>
    /// <value>The tuner's external identifier.</value>
    string LinkedTuner
    {
      get;
      set;
    }

    /// <summary>
    /// Is the product being used?
    /// </summary>
    /// <remarks>
    /// Only one instance of each product can be connected to each PC (driver
    /// limitation), and that instance can only be used with one tuner at any
    /// given time.
    /// </remarks>
    bool IsInUse
    {
      get;
    }

    /// <summary>
    /// Initialise and take control of the product.
    /// </summary>
    /// <returns>the product's DirectShow filter if successful, otherwise <c>null</c></returns>
    IBaseFilter Initialise();

    /// <summary>
    /// Deinitialise and release control of the product.
    /// </summary>
    void Deinitialise();

    #region CI interaction

    /// <summary>
    /// Open the CI interface.
    /// </summary>
    /// <param name="callBack">A set of delegates for the driver to invoke when the CI state changes or MMI becomes available.</param>
    /// <returns>an HRESULT indicating whether the interface was successfully opened</returns>
    int OpenInterface(ref SmarDtvUsbCiCallBack callBack);

    /// <summary>
    /// Open an MMI session with the CAM.
    /// </summary>
    /// <returns>an HRESULT indicating whether the session was successfully opened</returns>
    int OpenMmiSession();

    /// <summary>
    /// Send an APDU to the CAM.
    /// </summary>
    /// <remarks>
    /// It is not possible send CA PMT APDUs because the underlying session is an MMI session.
    /// </remarks>
    /// <param name="apdu">The APDU.</param>
    /// <param name="apduLength">The length of the APDU in bytes.</param>
    /// <returns>an HRESULT indicating whether the APDU was successfully received by the CAM</returns>
    int SendMmiApdu(int apduLength, byte[] apdu);

    /// <summary>
    /// Send PMT data to the CAM to request a service be descrambled.
    /// </summary>
    /// <remarks>
    /// It is not possible to descramble more than one program at a time.
    /// </remarks>
    /// <param name="pmt">The PMT.</param>
    /// <param name="pmtLength">The length of the PMT in bytes.</param>
    /// <returns>an HRESULT indicating whether the PMT was successfully received by the CAM</returns>
    int SendPmt(byte[] pmt, short pmtLength);

    /// <summary>
    /// Get version information about the interface, driver and device.
    /// </summary>
    /// <param name="versionInfo">The version information.</param>
    /// <returns>an HRESULT indicating whether the version information was successfully retrieved</returns>
    int GetVersionInfo(out SmarDtvUsbCiVersionInfo versionInfo);

    #endregion
  }
}