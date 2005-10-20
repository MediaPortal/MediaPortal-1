#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

namespace System.Threading
{
	public class BackgroundWorker
	{
		#region Events

		public event				DoWorkEventHandler DoWork;
		public event				ProgressChangedEventHandler ProgressChanged;
		public event				RunWorkerCompletedEventHandler RunWorkerCompleted;

		#endregion Events

		#region Methods

		public void CancelAsync()
		{
			lock(this)
				_isCancelPending = true;
		}

		void InvokeDelegate(Delegate handler, object[] args)
		{
			ISynchronizeInvoke synchronizer = (ISynchronizeInvoke)handler.Target;

			if(synchronizer == null)
			{
				handler.DynamicInvoke(args);
				return;
			}

			if(synchronizer.InvokeRequired == false)
			{
				handler.DynamicInvoke(args);
				return;
			}

			synchronizer.Invoke(handler, args);
		}
		
		void ReportCompletion(IAsyncResult asyncResult)
		{
			System.Runtime.Remoting.Messaging.AsyncResult ar = (System.Runtime.Remoting.Messaging.AsyncResult)asyncResult;

			DoWorkEventHandler del;

			del  = (DoWorkEventHandler)ar.AsyncDelegate;

			DoWorkEventArgs doWorkArgs = (DoWorkEventArgs)ar.AsyncState;

			object result = null;

			Exception error = null;

			try
			{
				del.EndInvoke(asyncResult);
				result = doWorkArgs.Result;
			}
			catch(Exception exception)
			{
				error = exception;
			}

			object[] args = new object[] { this, new RunWorkerCompletedEventArgs(result, error, doWorkArgs.Cancel) };

			foreach(Delegate handler in RunWorkerCompleted.GetInvocationList())
				InvokeDelegate(handler, args);
		}

		public void ReportProgress(int percent)
		{
			if(_isProgressReported == false)
				throw new InvalidOperationException("Background worker does not report its progress");

			object[] args = new object[] { this, new ProgressChangedEventArgs(percent) };

			foreach(Delegate handler in ProgressChanged.GetInvocationList())
				InvokeDelegate(handler, args);
		}

		public void RunWorkerAsync()
		{
			RunWorkerAsync(null);
		}

		public void RunWorkerAsync(object argument)
		{
			if(DoWork == null)
				return;
				
			_isCancelPending = false;

			DoWorkEventArgs args = new DoWorkEventArgs(argument);
			DoWork.BeginInvoke(this, args, new AsyncCallback(ReportCompletion), args);
		}

		#endregion Methods

		#region Properties

		public bool WorkerSupportsCancellation
		{
			get { lock(this) return _isCancellationSupported; } 
			set { lock(this) _isCancellationSupported = value; } 
		}

		public bool WorkerReportsProgress
		{
			get { lock(this) return _isProgressReported; }
			set { lock(this) _isProgressReported = value; }
		}

		public bool CancellationPending
		{
			get { lock(this) return _isCancelPending; }
		}

		#endregion Properties

		#region Fields

		bool						_isCancelPending = false;
		bool						_isProgressReported = false;
		bool						_isCancellationSupported = false;

		#endregion Fields
	}
}