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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Helper
{
  /// <summary>
  /// <see cref="FilterLoader"/> is a helper class to load <see cref="IBaseFilter"/>s from any .dll, for which is no registration of the filter needed.
  /// </summary>
  public static class FilterLoader
  {
    /// <summary>
    /// Loads an COM .dll and creates an instance of the given Interface with IID <paramref name="interfaceId"/>.
    /// </summary>
    /// <param name="dllName">Filename of a .dll or .ax component</param>
    /// <param name="interfaceId">Interface to create an object instance for</param>
    /// <param name="useAssemblyRelativeLocation">Combine the given file name to a full path</param>
    /// <returns>Instance or <c>null</c></returns>
    public static IBaseFilter LoadFilterFromDll(string dllName, Guid interfaceId, bool useAssemblyRelativeLocation = false)
    {
      //Get a classFactory for our classID
      string dllPath = useAssemblyRelativeLocation ? PathManager.BuildAssemblyRelativePath(dllName) : dllName;
      IClassFactory classFactory = ComHelper.GetClassFactory(dllPath, interfaceId);
      if (classFactory == null)
        return null;

      //And create an IFilter instance using that class factory
      Guid baseFilterGuid = typeof(IBaseFilter).GUID;
      Object obj;
      classFactory.CreateInstance(null, ref baseFilterGuid, out obj);
      return (obj as IBaseFilter);
    }
  }
}
