#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using MediaPortal.Services;

namespace MediaPortal.ControlDevices
{
  public abstract class AbstractControlPlugin
  {
    private string _libraryName = string.Empty;
    protected IControlSettings _settings;
    protected string _dllPath = string.Empty;
    protected ILog _log;

    public AbstractControlPlugin()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    public string LibraryName
    {
      set { _libraryName = value; }
      get { return _libraryName; }
    }

    public void Initialize() {}
  }
}