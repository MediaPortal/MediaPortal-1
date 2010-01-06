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
using MediaPortal.GUI.Library;

internal class SlideCache
{
  private enum RelativeIndex
  {
    Prev = 0,
    Curr = 1,
    Next = 2
  }

  private Thread _prefetchingThread;
  private Object _prefetchingThreadLock = new Object();

  private SlidePicture[] _slides = new SlidePicture[3];
  private Object _slidesLock = new Object();

  private string _neededSlideFilePath;
  private RelativeIndex _neededSlideRelativeIndex;

  private SlidePicture NeededSlide
  {
    get { return _slides[(int)_neededSlideRelativeIndex]; }
    set { _slides[(int)_neededSlideRelativeIndex] = value; }
  }

  private SlidePicture PrevSlide
  {
    get { return _slides[(int)RelativeIndex.Prev]; }
    set { _slides[(int)RelativeIndex.Prev] = value; }
  }

  private SlidePicture CurrentSlide
  {
    get { return _slides[(int)RelativeIndex.Curr]; }
    set { _slides[(int)RelativeIndex.Curr] = value; }
  }

  private SlidePicture NextSlide
  {
    get { return _slides[(int)RelativeIndex.Next]; }
    set { _slides[(int)RelativeIndex.Next] = value; }
  }

  public SlidePicture GetCurrentSlide(string slideFilePath)
  {
    // wait for any (needed) prefetching to complete
    lock (_prefetchingThreadLock)
    {
      if (_prefetchingThread != null)
      {
        // only wait for the prefetching if it is for the slide file that we need
        if (_neededSlideFilePath == slideFilePath)
        {
          _prefetchingThread.Priority = ThreadPriority.AboveNormal;
        }
        else
        {
          // uneeded, abort
          _prefetchingThread.Abort();
          _prefetchingThread = null;
        }
      }
    }

    while (_prefetchingThread != null)
    {
      GUIWindowManager.Process();
    }

    lock (_slidesLock)
    {
      // try and use pre-fetched slide if appropriate
      if (NextSlide != null && NextSlide.FilePath == slideFilePath)
      {
        return NextSlide;
      }
      else if (PrevSlide != null && PrevSlide.FilePath == slideFilePath)
      {
        return PrevSlide;
      }
      else if (CurrentSlide != null && CurrentSlide.FilePath == slideFilePath)
      {
        return CurrentSlide;
      }
      else
      {
        // slide is not in cache, so get it now
        CurrentSlide = new SlidePicture(slideFilePath, false);
        return CurrentSlide;
      }
    }
  }

  public void PrefetchNextSlide(string prevPath, string currPath, string nextPath)
  {
    lock (_prefetchingThreadLock)
    {
      // assume that any incomplete prefetching is uneeded, abort
      if (_prefetchingThread != null)
      {
        _prefetchingThread.Abort();
        _prefetchingThread = null;
      }
    }

    lock (_slidesLock)
    {
      // shift slides and determine _neededSlideRelativeIndex
      if (NextSlide != null && NextSlide.FilePath == currPath)
      {
        PrevSlide = CurrentSlide;
        CurrentSlide = NextSlide;
        _neededSlideFilePath = nextPath;
        _neededSlideRelativeIndex = RelativeIndex.Next;
      }
      else if (PrevSlide != null && PrevSlide.FilePath == currPath)
      {
        NextSlide = CurrentSlide;
        CurrentSlide = PrevSlide;
        _neededSlideFilePath = prevPath;
        _neededSlideRelativeIndex = RelativeIndex.Prev;
      }
      else
      {
        // may need all 3, but just get next
        _neededSlideFilePath = nextPath;
        _neededSlideRelativeIndex = RelativeIndex.Next;
      }
    }

    lock (_prefetchingThreadLock)
    {
      _prefetchingThread = new Thread(LoadNextSlideThread);
      _prefetchingThread.IsBackground = true;
      _prefetchingThread.Name = "PicPrefetch";
      //string cacheString = String.Format("cache:{0}|{1}|{2} ",
      //  _slides[0] != null ? "1" : "0",
      //  _slides[1] != null ? "1" : "0",
      //  _slides[2] != null ? "1" : "0");
      //Trace.WriteLine(cacheString + String.Format("prefetching {0} slide {1}", _neededSlideRelativeIndex.ToString("G"), System.IO.Path.GetFileNameWithoutExtension(_neededSlideFilePath)));
      _prefetchingThread.Start();
    }
  }

  /// <summary>
  /// Method to do the work of actually loading the image from file. This method
  /// should only be used by the prefetching thread.
  /// </summary>
  public void LoadNextSlideThread()
  {
    try
    {
      Debug.Assert(Thread.CurrentThread == _prefetchingThread);

      lock (_slidesLock)
      {
        NeededSlide = new SlidePicture(_neededSlideFilePath, false);
      }

      lock (_prefetchingThreadLock)
      {
        _prefetchingThread = null;
      }
    }
    catch (ThreadAbortException)
    {
      // abort is expected when slide changes outpace prefetch, ignore
      // Trace.WriteLine(String.Format("  ...aborted {0} slide {1}", _neededSlideRelativeIndex.ToString("G"), System.IO.Path.GetFileNameWithoutExtension(_neededSlideFilePath)));
    }
    catch (Exception) {}
  }

  public void InvalidateSlide(string slideFilePath)
  {
    lock (_slidesLock)
    {
      for (int i = 0; i < _slides.Length; i++)
      {
        SlidePicture slide = _slides[i];
        if (slide != null && slide.FilePath == slideFilePath)
        {
          _slides[i] = null;
        }
      }
    }

    // Note that we could pre-fetch the invalidated slide, but if the new version
    // of the slide is going to be requested immediately (as with DoRotate) then
    // pre-fetching won't help.
  }
}