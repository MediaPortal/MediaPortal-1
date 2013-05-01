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
using System.Collections.Generic;
using System.Threading;

namespace TvThumbnails
{
  public class ProcessingQueue : IDisposable
  {
    public delegate void DoWork(string filename);

    public event DoWork OnDoWork;

    private readonly EventWaitHandle _wh = new AutoResetEvent(false);

    private readonly Thread _worker;

    private readonly Queue<string> _tasks = new Queue<string>();

    private readonly object _lock = new object();

    public ProcessingQueue(DoWork callback)
    {
      OnDoWork += callback;

      _worker = new Thread(Work);
      _worker.IsBackground = true;
      _worker.Start();
    }

    public void EnqueueTask(List<string> tasks)
    {
      lock (_lock)
      {
        if (tasks != null)
        {
          foreach (string task in tasks)
          {
            _tasks.Enqueue(task);
          }
        }
        else
        {
          _tasks.Enqueue(null);
        }
      }

      _wh.Set();
    }

    void Work()
    {
      while (true)
      {
        string task = null;

        lock (_lock)
        {
          if (_tasks.Count > 0)
          {
            task = _tasks.Dequeue();

            if (task == null)
            {
              return;
            }
          }
        }

        if (task != null)
        {
          if (OnDoWork != null)
          {
            OnDoWork.Invoke(task);
          }
        }
        else
        {
          _wh.WaitOne();
        }
      }
    }

    public void Dispose()
    {
      EnqueueTask(null);
      _worker.Join();
      _wh.Close();
    }
  }
}