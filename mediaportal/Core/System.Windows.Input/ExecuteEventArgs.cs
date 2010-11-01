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

namespace System.Windows.Input
{
  public class ExecuteEventArgs : EventArgs
  {
    #region Constructors

    public ExecuteEventArgs(ICommand command)
    {
      _command = command;
    }

    #endregion Constructors

    #region Properties

    public ICommand Command
    {
      get { return _command; }
    }

    public object Data
    {
      get { return _data; }
      set { _data = value; }
    }

    public object Source
    {
      get { return _source; }
    }

    #endregion Properties

    #region Fields

    private ICommand _command;
    private object _data;
    private object _source = null;

    #endregion Fields
  }
}