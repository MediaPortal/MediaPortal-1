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

namespace Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces
{
  /// <summary>
  /// <see cref="IIntegrationProvider"/> provides required information about the hosting enviromment and features for the TvCore. 
  /// Each hosting environment needs to provide an integration provider to allow use of different folders or logging providers. 
  /// This way we can use the TvCore inside MP1-TvServer or in MP2-Server as plugin.
  /// </summary>
  public interface IIntegrationProvider
  {
    /// <summary>
    /// Gets the <see cref="IPathManager"/> instance.
    /// </summary>
    IPathManager PathManager { get; }

    /// <summary>
    /// Gets the <see cref="ILogger"/> instance.
    /// </summary>
    ILogger Logger { get; }
  }
}
