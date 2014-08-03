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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Product;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Service
{
  internal class SmarDtvUsbCiConfigService : ISmarDtvUsbCiConfigService
  {
    public ICollection<string> GetProductNames()
    {
      List<string> names = new List<string>();
      ReadOnlyCollection<ISmarDtvUsbCiProduct> products = SmarDtvUsbCiProductBase.GetProductList();
      foreach (ISmarDtvUsbCiProduct p in products)
      {
        names.Add(p.Name);
      }
      return names;
    }

    public SmarDtvUsbCiDriverInstallState GetProductInstallState(string productName)
    {
      ReadOnlyCollection<ISmarDtvUsbCiProduct> products = SmarDtvUsbCiProductBase.GetProductList();
      foreach (ISmarDtvUsbCiProduct p in products)
      {
        if (p.Name.Equals(productName))
        {
          return p.InstallState;
        }
      }
      return SmarDtvUsbCiDriverInstallState.NotInstalled;
    }

    public string GetLinkedTunerForProduct(string productName)
    {
      ReadOnlyCollection<ISmarDtvUsbCiProduct> products = SmarDtvUsbCiProductBase.GetProductList();
      foreach (ISmarDtvUsbCiProduct p in products)
      {
        if (p.Name.Equals(productName))
        {
          return p.LinkedTuner;
        }
      }
      return string.Empty;
    }

    public void LinkTunerToProduct(string productName, string tunerExternalId)
    {
      ReadOnlyCollection<ISmarDtvUsbCiProduct> products = SmarDtvUsbCiProductBase.GetProductList();
      foreach (ISmarDtvUsbCiProduct p in products)
      {
        if (p.Name.Equals(productName))
        {
          p.LinkedTuner = tunerExternalId;
          return;
        }
      }
    }
  }
}