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

using System.Threading;

namespace System.Windows.Dispatcher
{
  public sealed class Dispatcher
  {
    #region Constructors

    private Dispatcher()
    {
      _thread = new Thread(new ThreadStart(RunWorker));
      _thread.Name = "Dispatcher";
      _thread.Start();
    }

    #endregion Constructors

    #region Events

    public event EventHandler ShutdownFinished;
    public event EventHandler ShutdownStarted;
    public event DispatcherUnhandledExceptionEventHandler UnhandledException;
    public event DispatcherUnhandledExceptionFilterEventHandler UnhandledExceptionFilter;

    #endregion Events

    #region Methods

    public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method)
    {
      return BeginInvoke(priority, method, null);
    }

    public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method, object arg)
    {
      return BeginInvoke(priority, method, arg, null);
    }

    public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method, object arg,
                                           params object[] args)
    {
      return new DispatcherOperation(priority, this);
    }

    public void BeginInvokeShutdown(DispatcherPriority priority) {}

    private void BeginInvokeShutdownWorker()
    {
      _hasShutdownStarted = true;

      if (ShutdownStarted != null)
      {
        ShutdownStarted(this, EventArgs.Empty);
      }


      _hasShutdownFinished = true;

      if (ShutdownFinished != null)
      {
        ShutdownFinished(this, EventArgs.Empty);
      }
    }

    public bool CheckAccess()
    {
      return _thread == Thread.CurrentThread;
    }

    public DispatcherProcessingDisabled DisableProcessing()
    {
      throw new NotImplementedException();
    }

    public static void ExitAllFrames() {}

    public static Dispatcher FromThread(Thread thread)
    {
      if (thread != _currentDispatcher.Thread)
      {
        return null;
      }

      return _currentDispatcher;
    }

    public object Invoke(DispatcherPriority priority, Delegate method)
    {
      return Invoke(priority, method, null);
    }

    public object Invoke(DispatcherPriority priority, Delegate method, object arg)
    {
      return Invoke(priority, method, arg, null);
    }

    public object Invoke(DispatcherPriority priority, Delegate method, object arg, params object[] args)
    {
      throw new NotImplementedException();
    }

    public object Invoke(DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg)
    {
      throw new NotImplementedException();
    }

    public void InvokeShutdown() {}

    public static void PushFrame(DispatcherFrame frame) {}

    public static void Run()
    {
      new Dispatcher();
    }

    private void RunWorker()
    {
      _currentDispatcher = this;

      for (;;)
      {
        try {}
        catch (Exception e)
        {
          if (UnhandledExceptionFilter != null)
          {
            DispatcherUnhandledExceptionFilterEventArgs args = new DispatcherUnhandledExceptionFilterEventArgs(this, e);

            if (UnhandledException != null && args.RequestCatch == false)
            {
              UnhandledException(this, new DispatcherUnhandledExceptionEventArgs(this, e));
            }
          }
        }
      }

//			_currentDispatcher = null;
    }

    public static void ValidatePriority(DispatcherPriority priority, string parameterName) {}

    public void VerifyAccess()
    {
      if (_thread != Thread.CurrentThread)
      {
        throw new InvalidOperationException();
      }
    }

    #endregion Methods

    public static Dispatcher CurrentDispatcher
    {
      get { return _currentDispatcher; }
    }

    public bool HasShutdownFinished
    {
      get { return _hasShutdownFinished; }
    }

    public bool HasShutdownStarted
    {
      get { return _hasShutdownStarted; }
    }

    public DispatcherHooks Hooks
    {
      get { throw new NotImplementedException(); }
    }

    public Thread Thread
    {
      get { return _thread; }
    }

    #region Fields

    private static Dispatcher _currentDispatcher;
    private bool _hasShutdownFinished;
    private bool _hasShutdownStarted;
    private Thread _thread;

    #endregion Fields
  }
}