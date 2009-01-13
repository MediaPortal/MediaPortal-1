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

#region usings

using System.Threading;

#endregion

namespace MediaPortal.TV.Recording
{
  public class CardCommand
  {
    private bool _isFinished = false;
    private bool _isSucceeded = false;
    private string _errorMessage = "";
    private AutoResetEvent _event = null;

    public CardCommand()
    {
    }

    public AutoResetEvent Event
    {
      get { return _event; }
      set { _event = value; }
    }

    public virtual void Execute(CommandProcessor handler)
    {
    }

    public void WaitOne()
    {
      _event.WaitOne();
    }

    public void WaitOne(int millisecondsTimeout)
    {
      _event.WaitOne(millisecondsTimeout, true);
    }

    public bool Finished
    {
      get { return _isFinished; }

      set
      {
        _isFinished = value;
        if (_isFinished)
        {
          if (_event != null)
          {
            _event.Set();
          }
        }
      }
    }

    public bool Succeeded
    {
      get { return _isSucceeded; }
      set { _isSucceeded = value; }
    }

    public string ErrorMessage
    {
      get { return _errorMessage; }
      set { _errorMessage = value; }
    }
  }
}