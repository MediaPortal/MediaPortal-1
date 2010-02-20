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

using System;
using System.Diagnostics;
using System.Threading;

namespace MediaPortal.Util
{
  /// <summary>
  /// Provides a lock object to prevent a program from being launched multiple times.
  /// </summary>
  public class ProcessLock : IDisposable
  {
    #region Fields

    private string name;
    private Mutex mutex;
    private bool isCreated;
    private bool isDisposed;

    #endregion

    #region Instance Management

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessLock"/> class.
    /// </summary>
    /// <param name="_name">Name of the process lock</param>
    public ProcessLock(string _name)
    {
      name = _name;
      TryLock();
    }

    /// <summary>
    /// Disposes the process lock resources.
    /// </summary>
    public void Dispose()
    {
      if (mutex != null)
      {
        if (isCreated)
          Unlock();
        else
        {
          mutex.Close();
          mutex = null;
        }
      }

      isDisposed = true;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Indicates whether a process for the given name does already exist.
    /// </summary>
    public bool AlreadyExists
    {
      get { return !isCreated; }
    }

    /// <summary>
    /// Indicates whether the process lock is disposed.
    /// </summary>
    public bool Disposed
    {
      get { return isDisposed; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Try to get the process lock again.
    /// </summary>
    public bool TryLock()
    {
      if (isDisposed)
        throw new ObjectDisposedException("ProcessLock");

      if (isCreated)
        throw new InvalidOperationException();

      if (mutex != null)
        mutex.Close();
      mutex = new Mutex(true, name, out isCreated);

      return isCreated;
    }

    /// <summary>
    /// Releases the process lock.
    /// </summary>
    public void Unlock()
    {
      if (isDisposed)
        throw new ObjectDisposedException("ProcessLock");

      if (!isCreated)
        throw new InvalidOperationException();

      Debug.Assert(mutex != null);

      mutex.ReleaseMutex();
      mutex = null;
      isCreated = false;
    }

    #endregion
  }
}