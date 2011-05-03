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
using MediaPortal.CoreServices;
using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
using NUnit.Framework;

namespace MediaPortal.Tests.Utils.Services
{
  [TestFixture]
  [Category("ServiceProvider")]
  public class ServiceTest
  {
    /// <summary>
    /// Tests whether we are getting an exception when we try to add a service that is already
    /// registered.
    /// </summary>
    [Test]
    [ExpectedException(typeof (ArgumentException),"A service of type MediaPortal.Services.ILogger is already present")]
    public void TestAddDuplicateService1()
    {
      ServiceProvider provider = new ServiceProvider();
      ILogger log1 = new NoLog();
      ILogger log2 = new NoLog();
      provider.Add<ILogger>(log1);
      provider.Add<ILogger>(log2);
    }

 
    private ILogger ServiceRequested(ServiceProvider provider)
    {
      return new NoLog();
    }

  }

 
}