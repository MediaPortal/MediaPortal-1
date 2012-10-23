using System;
using System.IO;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;

namespace TvLibrary.Integration.MP1
{
  class PathManager: IPathManager
  {
    /// <summary>
    /// Checks if a path with the specified label is registered.
    /// </summary>
    /// <param name="label">Label to lookup in the registration.</param>
    /// <returns>True, if the specified label exists in our path registration, else false.</returns>
    public bool Exists (string label)
    {
      // TODO: Check if folder exist
      return true;
    }

    protected string GetRootPath()
    {
      return String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    }

    /// <summary>
    /// Registers the specified <paramref name="pathPattern"/> for the specified
    /// <paramref name="label"/>. If the label is already registered, it will be replaced.
    /// </summary>
    /// <param name="label">The lookup label for the new path.</param>
    /// <param name="pathPattern">The path pattern to be registered with the <paramref name="label"/>.
    /// This pattern may contain references to other path registrations.</param>
    public void SetPath (string label, string pathPattern)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Resolves the specified pathPattern, as described in the class documentation.
    /// </summary>
    /// <param name="pathPattern">The path pattern to be resolved.</param>
    /// <returns>The resolved path as a string.</returns>
    /// <exception cref="ArgumentException">When the specified <paramref name="pathPattern"/>
    /// contains labels that are not registered.</exception>
    public string GetPath (string pathPattern)
    {
      string rootPath = GetRootPath();
      if (string.IsNullOrEmpty(pathPattern) || pathPattern == "<TVCORE>")
        return rootPath;

      return Path.Combine(rootPath, pathPattern.Replace("<", "").Replace(">", ""));
    }

    /// <summary>
    /// Removes the path registration for the specified <paramref name="label"/>.
    /// </summary>
    /// <param name="label">The label of the path registration to be removed.</param>
    public void RemovePath (string label)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Loads path values from a paths file.
    /// </summary>
    /// <param name="pathsFile">Name of a file with paths to load. See <c>[App-Root]/Defaults/Paths.xml</c> as example.</param>
    /// <returns><c>true</c>, if the file with the given name exists and could be loaded. Else, <c>false</c> is returned.</returns>
    public bool LoadPaths (string pathsFile)
    {
      throw new NotImplementedException();
    }
  }
}
