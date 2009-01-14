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
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace MediaPortal.Dispatcher
{
  public sealed class JobDispatcher
  {
    #region Methods

    private void Clear()
    {
      _jobs.Clear();
    }

    private void Dispatch()
    {
      _isRunning = true;

      try
      {
        Job job = null;

        for (;;)
        {
          lock (_dispatcher)
          {
            if (_isPaused || _jobs.Count == 0)
            {
              Monitor.Wait(_dispatcher);
            }

            if (_isRunning == false)
            {
              break;
            }

            job = (Job) _jobs.Peek();

            if (job.IsReady == false)
            {
              Monitor.Wait(_dispatcher, job.Next.Subtract(DateTime.Now));
            }

            if (_isRunning == false)
            {
              break;
            }

            if (job.IsReady == false)
            {
              continue;
            }

            _jobs.Dequeue();
          }

          job.Run();
          job = null;
        }
      }
      catch (ThreadInterruptedException)
      {
        Trace.WriteLine("JobDispatcher.Dispatch: ThreadInterruptException");
      }
      catch (Exception e)
      {
        Trace.WriteLine("JobDispatcher.Dispatch: {0}", e.Message);
      }

      _isRunning = false;
    }

    internal static void Dispatch(Job job)
    {
      lock (_dispatcher)
      {
        _jobs.Enqueue(job);

        Monitor.Pulse(_dispatcher);
      }
    }

    public static void Init()
    {
      lock (_dispatcher)
      {
        if (_isRunning)
        {
          throw new InvalidOperationException("Dispatcher is already initialized.");
        }

        _dispatcherThread = new Thread(new ThreadStart(_dispatcher.Dispatch));
        _dispatcherThread.IsBackground = true;
        _dispatcherThread.Priority = ThreadPriority.AboveNormal;
        _dispatcherThread.Name = "Dispatcher";
        _dispatcherThread.Start();

        // wait for the dispatcher thread to get going
        while (_isRunning == false)
        {
          Thread.Sleep(100);
        }
      }
    }

    public static void Pause()
    {
      lock (_dispatcher)
      {
        if (_isRunning == false)
        {
          throw new InvalidOperationException("Dispatcher is not running.");
        }

        if (_isPaused)
        {
          throw new InvalidOperationException("Dispatcher is already paused.");
        }

        _isPaused = true;
      }
    }

    public static void Resume()
    {
      lock (_dispatcher)
      {
        if (_isRunning == false)
        {
          throw new InvalidOperationException("Dispatcher is not running.");
        }

        if (_isPaused == false)
        {
          throw new InvalidOperationException("Dispatcher is already running.");
        }

        _isPaused = false;

        Monitor.Pulse(_dispatcher);
      }
    }

    public static void Term()
    {
      lock (_dispatcher)
      {
        //if(_isRunning == false)
        //	throw new InvalidOperationException("Dispatcher is already shutdown.");

        _isRunning = false;
        _dispatcher.Clear();

        Monitor.Pulse(_dispatcher);

        _dispatcherThread = null;
      }
    }

    #endregion Methods

    #region Fields

    private static JobDispatcher _dispatcher = new JobDispatcher();
    private static Thread _dispatcherThread;
    private static volatile bool _isRunning;
    private static volatile bool _isPaused;
    private static PriorityQueue _jobs = new PriorityQueue(new JobComparer());

    #endregion Fields
  }
}