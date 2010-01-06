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

#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using MediaPortal.Threading;
using NUnit.Framework;
using ThreadPool = MediaPortal.Threading.ThreadPool;

#endregion

namespace MediaPortal.Tests.Core.Threading
{

  #region TestFixture

  [TestFixture]
  public class ThreadPoolTest
  {
    #region Variables

    private ThreadPool _pool;
    private object _waitHandle = new object();

    #endregion

    #region Tests

    [SetUp]
    public void Init()
    {
      Console.Out.WriteLine("----=<Init>=----");
      _pool = new ThreadPool(2, 5);
      SetupLogging();
    }

    [TearDown]
    public void Dispose()
    {
      Console.Out.WriteLine("----=<Stop>=----");
      _pool.Stop();
      _pool = null;
    }

    [Test]
    public void TestDelayedInit()
    {
      Console.Out.WriteLine("----=<TestDelayedInit>=----");
      Assert.AreEqual(0, _pool.ThreadCount, "Thread count not 0; pool delayed initialization not OK");
      _pool.Stop();
    }

    [Test]
    public void TestImmediateInit()
    {
      Console.Out.WriteLine("----=<TestImmediateInit>=----");
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo();
      tpsi.MinimumThreads = 2;
      tpsi.MaximumThreads = 5;
      tpsi.ThreadIdleTimeout = 1000;
      tpsi.DelayedInit = false;
      _pool = new ThreadPool(tpsi);
      SetupLogging();
      Assert.AreEqual(2, _pool.ThreadCount, "Thread count not 2; pool immediate initialization not OK");
      _pool.Stop();
    }

    [Test]
    public void TestMaxThreads()
    {
      Console.Out.WriteLine("----=<TestMaxThreads>=----");
      for (int i = 0; i < 10; i++)
      {
        _pool.Add(new DoWorkHandler(delegate() { Thread.Sleep(300); }));
        Thread.Sleep(10);
      }
      // System.Threading.Thread.Sleep(600);
      Assert.AreEqual(5, _pool.BusyThreadCount, "Maximum number of 5 threads exceeded");
      _pool.Stop();
    }

    [Test]
    public void TestDropBackToMinThreads()
    {
      Console.Out.WriteLine("----=<TestDropBackToMinThreads>=----");
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo(2, 10, 100);
      _pool = new ThreadPool(tpsi);
      SetupLogging();
      for (int i = 0; i < 10; i++)
      {
        _pool.Add(new DoWorkHandler(delegate() { Thread.Sleep(300); }), "TestDropBackToMinThreads" + i);
        Thread.Sleep(10);
      }
      Thread.Sleep(600);
      Assert.AreEqual(0, _pool.BusyThreadCount);
      Assert.AreEqual(10, _pool.WorkItemsProcessed);
      Assert.AreEqual(2, _pool.ThreadCount, "Pool did not drop back down to 2 threads");
      _pool.Stop();
    }

    private Work _testStrongTypeCallbackWork;
    private List<int> _testStrongTypeCallbackResult;

    [Test]
    public void TestStrongTypeCallback()
    {
      Console.Out.WriteLine("----=<TestStrongTypeCallback>=----");
      Work w = _testStrongTypeCallbackWork = new Work();
      w.ThreadPriority = ThreadPriority.Normal;
      w.WorkLoad = new DoWorkHandler(delegate()
                                       {
                                         _testStrongTypeCallbackWork.State = WorkState.INPROGRESS;
                                         List<int> result = new List<int>();
                                         for (int i = 0; i < 10; i++)
                                         {
                                           result.Add(i);
                                         }
                                         _testStrongTypeCallbackWork.EventArgs.SetResult<List<int>>(result);
                                         _testStrongTypeCallbackWork.State = WorkState.FINISHED;
                                         _testStrongTypeCallbackWork.WorkCompleted(_testStrongTypeCallbackWork.EventArgs);
                                       });
      w.WorkCompleted = new WorkEventHandler(delegate(WorkEventArgs args)
                                               {
                                                 _testStrongTypeCallbackResult = args.GetResult<List<int>>();
                                                 lock (_waitHandle)
                                                   Monitor.Pulse(_waitHandle);
                                               });

      lock (_waitHandle)
      {
        _pool.Add(w);
        Monitor.Wait(_waitHandle);
      }

      if (_testStrongTypeCallbackResult != null)
      {
        if (_testStrongTypeCallbackResult.Count != 10)
        {
          Assert.Fail("received {0} results instead of 10", _testStrongTypeCallbackResult.Count);
        }
        else
        {
          for (int i = 0; i < _testStrongTypeCallbackResult.Count; i++)
          {
            Assert.AreEqual(i, _testStrongTypeCallbackResult[i]);
          }
        }
      }
      else
      {
        Assert.Fail("no results were received");
      }
    }

    [Test]
    public void TestAddWorkNullReference()
    {
      Console.Out.WriteLine("----=<TestAddWorkNullReference>=----");
      IWork w = null;
      try
      {
        _pool.Add(w);
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException e)
      {
        Assert.AreEqual(e.ParamName, "work");
      }
    }

    [Test]
    public void TestAddWorkInvalidState()
    {
      Console.Out.WriteLine("----=<TestAddWorkInvalidState>=----");
      Work w = new Work();
      w.State = WorkState.INPROGRESS;
      try
      {
        _pool.Add(w);
        Assert.Fail("InvalidOperationException not thrown");
      }
      catch (InvalidOperationException) {}
    }

    [Test]
    public void TestAddWorkWhileStopped()
    {
      Console.Out.WriteLine("----=<TestAddWorkWhileStopped>=----");
      Work w = new Work(new DoWorkHandler(delegate() { Thread.Sleep(300); }));
      _pool.Stop();
      try
      {
        _pool.Add(w);
        Assert.Fail("InvalidOperationException not thrown");
      }
      catch (InvalidOperationException) {}
    }

    [Test]
    public void TestThreadCount()
    {
      Console.Out.WriteLine("----=<TestThreadCount>=----");
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo();
      tpsi.MinimumThreads = 15;
      tpsi.DelayedInit = false;
      _pool = new ThreadPool(tpsi);
      Assert.AreEqual(15, _pool.ThreadCount);
    }

    [Test]
    public void TestWorkItemsProcessed()
    {
      Console.Out.WriteLine("----=<TestWorkItemsProcessed>=----");
      for (int i = 0; i < 10; i++)
      {
        _pool.Add(new DoWorkHandler(delegate() { Thread.Sleep(300); }));
        Thread.Sleep(10);
      }
      while (_pool.BusyThreadCount > 0)
      {
        Thread.Sleep(100);
      }
      Assert.AreEqual(10, _pool.WorkItemsProcessed);
    }

    [Test]
    public void TestWorkQueueLengthBusyCountAndWorkCancellation()
    {
      DoWorkHandler workHandler = new DoWorkHandler(delegate() { Thread.Sleep(300); });
      Console.Out.WriteLine("----=<TestWorkItemsProcessed>=----");
      List<IWork> workList = new List<IWork>();
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo(1, 1, 10);
      _pool = new ThreadPool(tpsi);
      SetupLogging();

      _pool.Add(workHandler);
      Thread.Sleep(10);
      Assert.AreEqual(1, _pool.BusyThreadCount);
      for (int i = 0; i < 10; i++)
      {
        workList.Add(_pool.Add(workHandler));
      }
      Assert.AreEqual(10, _pool.QueueLength);

      foreach (IWork work in workList)
      {
        work.State = WorkState.CANCELED;
      }
      _pool.MinimumThreads = _pool.MaximumThreads = 10;
      _pool.MinimumThreads = 0;

      while (_pool.BusyThreadCount > 0)
      {
        Thread.Sleep(100);
      }

      foreach (IWork work in workList)
      {
        Assert.AreEqual(WorkState.CANCELED, work.State);
      }
      Assert.AreEqual(0, _pool.QueueLength);
      Assert.AreEqual(0, _pool.BusyThreadCount);
    }

    [Test]
    public void TestWorkWithException()
    {
      IWork work =
        _pool.Add(new DoWorkHandler(delegate() { throw new ArgumentOutOfRangeException("test", 0, "testmessage"); }));
      while (work.State < WorkState.FINISHED)
      {
        Thread.Sleep(100);
      }
      Assert.AreEqual(WorkState.ERROR, work.State, "WorkState is not ERROR");
      Assert.IsTrue(work.Exception is ArgumentOutOfRangeException);
      ArgumentOutOfRangeException e = work.Exception as ArgumentOutOfRangeException;
      Assert.AreEqual("test", e.ParamName, "Expected \"test\" as ParamName");
      Assert.AreEqual(0, (int)e.ActualValue, "Expected \"0\" as ActualValue");
    }

    private int TestIntervalWorkRuns = 0;

    [Test]
    public void TestIntervalWork()
    {
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo(2, 10, 1000);
      _pool = new ThreadPool(tpsi);
      SetupLogging();
      IntervalWork iWork = new IntervalWork(new DoWorkHandler(delegate() { TestIntervalWorkRuns++; }),
                                            new TimeSpan(0, 0, 1));
      iWork.Description = "TestIntervalWork";
      _pool.AddIntervalWork(iWork, true);
      while (TestIntervalWorkRuns < 2)
      {
        Thread.Sleep(100);
      }
      _pool.RemoveIntervalWork(iWork);
      Assert.AreEqual(2, _pool.WorkItemsProcessed);
    }

    #endregion

    #region Test helper methods

    private void SetupLogging()
    {
      // only enable in case of trouble... ;-)
      // _pool.DebugLog += new LoggerDelegate(Console.Out.WriteLine);
      // // _pool.ErrorLog += new LoggerDelegate(Console.Error.WriteLine);
      // _pool.ErrorLog += new LoggerDelegate(Console.Out.WriteLine);
      // _pool.WarnLog += new LoggerDelegate(Console.Out.WriteLine);
      // _pool.InfoLog += new LoggerDelegate(Console.Out.WriteLine);
    }

    #endregion
  }

  #endregion
}