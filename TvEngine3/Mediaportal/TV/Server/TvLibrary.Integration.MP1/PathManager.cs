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
using System.IO;
using Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Integration.MP1
{
  public class PathManager: IPathManager
  {
    private IDictionary<string, string> _registeredPaths = new Dictionary<string, string>();

    /// <summary>
    /// Checks if a path with the specified label is registered.
    /// </summary>
    /// <param name="label">Label to lookup in the registration.</param>
    /// <returns>True, if the specified label exists in our path registration, else false.</returns>
    public bool Exists(string label)
    {
      return _registeredPaths.ContainsKey(label);
    }

    protected string GetRootPath()
    {
      return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal TV Server");
    }

    /// <summary>
    /// Registers the specified <paramref name="pathPattern"/> for the specified
    /// <paramref name="label"/>. If the label is already registered, it will be replaced.
    /// </summary>
    /// <param name="label">The lookup label for the new path.</param>
    /// <param name="pathPattern">The path pattern to be registered with the <paramref name="label"/>.
    /// This pattern may contain references to other path registrations.</param>
    public void SetPath(string label, string pathPattern)
    {
      _registeredPaths[label] = pathPattern;
    }

    /// <summary>
    /// Resolves the specified pathPattern, as described in the class documentation.
    /// </summary>
    /// <param name="pathPattern">The path pattern to be resolved.</param>
    /// <returns>The resolved path as a string.</returns>
    /// <exception cref="ArgumentException">When the specified <paramref name="pathPattern"/>
    /// contains labels that are not registered.</exception>
    public string GetPath(string pathPattern)
    {
      string rootPath = GetRootPath();
      if (string.IsNullOrEmpty(pathPattern) || pathPattern == "<TVCORE>")
      {
        return rootPath;
      }
      return Path.Combine(rootPath, pathPattern.Replace("<", "").Replace(">", ""));
    }

    /// <summary>
    /// Removes the path registration for the specified <paramref name="label"/>.
    /// </summary>
    /// <param name="label">The label of the path registration to be removed.</param>
    public void RemovePath(string label)
    {
      _registeredPaths.Remove(label);
    }

    /// <summary>
    /// Loads path values from a paths file.
    /// </summary>
    /// <param name="pathsFile">Name of a file with paths to load. See <c>[App-Root]/Defaults/Paths.xml</c> as example.</param>
    /// <returns><c>true</c>, if the file with the given name exists and could be loaded. Else, <c>false</c> is returned.</returns>
    public bool LoadPaths(string pathsFile)
    {
      throw new NotImplementedException();
    }
  }
}
