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
using System.Collections.Generic;
using System.Text;

using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Epg
{
  class EPGLanguage
  {
    private string _title;
    private string _description;
    private string _language;
    public EPGLanguage(string language, string title, string description)
    {
      _title = title;
      _description = description;
      _language = language;
    }
    public string Language
    {
      get { return _language; }
    }
    public string Title
    {
      get { return _title; }
    }
    public string Description
    {
      get { return _description; }
    }
  }
}
