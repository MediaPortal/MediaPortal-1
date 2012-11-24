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

using Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Integration.MP1
{
  public class MP1IntegrationProvider: IIntegrationProvider
  {
    protected readonly IPathManager _pathManager = new PathManager();
    protected ILogger _logger;

    public MP1IntegrationProvider()
    {
      _logger = new Logger();
    }


    public IPathManager PathManager
    {
      get { return _pathManager; }
    }

    public ILogger Logger
    {
      get { return _logger; }
    }
  }
}
