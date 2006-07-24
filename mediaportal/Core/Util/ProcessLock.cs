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
      get
      {
        return !isCreated;
      }
    }

    /// <summary>
    /// Indicates whether the process lock is disposed.
    /// </summary>
    public bool Disposed
    {
      get
      {
        return isDisposed;
      }
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