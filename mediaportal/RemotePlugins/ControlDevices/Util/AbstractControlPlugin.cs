#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using MediaPortal.Services;
using MediaPortal.ControlDevices;

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

    public void Initialize() 
    {
    }

  }
}
