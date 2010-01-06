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

namespace System.Windows.Dispatcher
{
  public sealed class DispatcherOperation
  {
    #region Constructors

    internal DispatcherOperation(DispatcherPriority priority, Dispatcher dispatcher)
    {
      _priority = priority;
      _dispatcher = dispatcher;
    }

    #endregion Constructors

    #region Events

    public event EventHandler Aborted;
//		public event EventHandler Completed;

    #endregion Events

    #region Methods

    public bool Abort()
    {
      if (Aborted != null)
      {
        Aborted(this, EventArgs.Empty);
      }

      return true;
    }

    public DispatcherOperationStatus Wait()
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public Dispatcher Dispatcher
    {
      get { return _dispatcher; }
    }

    public DispatcherPriority Priority
    {
      get { return _priority; }
      set { _priority = value; }
    }

    public object Result
    {
      get { return _result; }
    }

    public DispatcherOperationStatus Status
    {
      get { return _status; }
    }

    #endregion Properties

    #region Fields

    private Dispatcher _dispatcher;
    private DispatcherPriority _priority;
    private object _result = null;
    private DispatcherOperationStatus _status = DispatcherOperationStatus.Pending;

    #endregion Fields
  }
}