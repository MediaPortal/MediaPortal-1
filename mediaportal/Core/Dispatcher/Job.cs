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

using System;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;

namespace MediaPortal.Dispatcher
{
  public sealed class Job
  {
    #region Events

    public event DoWorkEventHandler DoWork;
    public event ProgressChangedEventHandler ProgressChanged;
    public event RunWorkerCompletedEventHandler RunWorkerCompleted;

    #endregion Events

    #region Methods

    public void Cancel()
    {
      lock (this)
        _isCancelPending = true;
    }

    public void Dispatch()
    {
      Dispatch(DateTime.Now);
    }

    public void Dispatch(int ticks)
    {
      Dispatch(DateTime.Now + TimeSpan.FromMilliseconds(ticks));
    }

    public void Dispatch(TimeSpan timespan)
    {
      Dispatch(DateTime.Now + timespan);
    }

    public void Dispatch(DateTime dateTime)
    {
      if (DoWork == null)
      {
        return;
      }

      lock (this)
        _dateTime = dateTime;

      JobDispatcher.Dispatch(this);
    }

    private void InvokeDelegate(Delegate handler, object[] e)
    {
      ISynchronizeInvoke synchronizer = (ISynchronizeInvoke) handler.Target;

      if (synchronizer == null)
      {
        handler.DynamicInvoke(e);
        return;
      }

      if (synchronizer.InvokeRequired == false)
      {
        handler.DynamicInvoke(e);
        return;
      }

      synchronizer.Invoke(handler, e);
    }

    private void InvokeDelegate(Delegate[] handlers, object[] e)
    {
      foreach (Delegate handler in handlers)
      {
        InvokeDelegate(handler, e);
      }
    }

    private void ReportCompletion(IAsyncResult asyncResult)
    {
      AsyncResult ar = (AsyncResult) asyncResult;

      DoWorkEventHandler handler = (DoWorkEventHandler) ar.AsyncDelegate;
      DoWorkEventArgs args = (DoWorkEventArgs) ar.AsyncState;

      object result = null;

      Exception error = null;

      try
      {
        handler.EndInvoke(asyncResult);
        result = args.Result;
      }
      catch (Exception exception)
      {
        error = exception;
      }

      if (RunWorkerCompleted != null)
      {
        InvokeDelegate(RunWorkerCompleted.GetInvocationList(),
                       new object[] {this, new RunWorkerCompletedEventArgs(result, error, args.Cancel)});
      }
    }

    public void ReportProgress(int percent)
    {
      if (_isProgressReported == false)
      {
        throw new InvalidOperationException("Job does not report its progress");
      }

      object[] e = new object[] {this, new ProgressChangedEventArgs(percent, null)};

      foreach (Delegate handler in ProgressChanged.GetInvocationList())
      {
        InvokeDelegate(handler, e);
      }
    }

    internal void Run()
    {
      if (DoWork == null)
      {
        return;
      }

      _isCancelPending = false;

      DoWorkEventArgs args = new DoWorkEventArgs(_argument);
      DoWork.BeginInvoke(this, args, new AsyncCallback(ReportCompletion), args);
    }

    #endregion Methods

    #region Properties

    public bool IsCancelPending
    {
      get { lock (this) return _isCancelPending; }
    }

    public object Argument
    {
      get { lock (this) return _argument; }
      set { lock (this) _argument = value; }
    }

    public JobFlags Flags
    {
      get { lock (this) return _flags; }
      set { lock (this) _flags = value; }
    }

    public bool IsReady
    {
      get { lock (this) return _isCancelPending == false && DateTime.Compare(_dateTime, DateTime.Now) <= 0; }
    }

    public string Name
    {
      get { lock (this) return _name; }
      set { lock (this) _name = value; }
    }

    public DateTime Next
    {
      get { lock (this) return _dateTime; }
    }

    public JobPriority Priority
    {
      get { lock (this) return _priority; }
      set { lock (this) _priority = value; }
    }

    public bool JobSupportsCancellation
    {
      get { lock (this) return _isCancellationSupported; }
      set { lock (this) _isCancellationSupported = value; }
    }

    public bool JobReportsProgress
    {
      get { lock (this) return _isProgressReported; }
      set { lock (this) _isProgressReported = value; }
    }

    #endregion Properties

    #region Fields

    private DateTime _dateTime = DateTime.Now;
    private JobFlags _flags;
    private bool _isCancelPending = false;
    private bool _isProgressReported = false;
    private bool _isCancellationSupported = false;
    private string _name = string.Empty;
    private JobPriority _priority = JobPriority.Lowest;
    private object _argument;

    #endregion Fields
  }
}