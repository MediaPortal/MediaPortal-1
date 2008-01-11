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
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>Class for getting Windows Performance Counter values</summary>
  /// <author>JoeDalton</author>
  [Serializable]
  public class PerformanceCounter : Value
  {
    private string categoryName;
    private string counterName;
    private string instanceName;
    private string format;
    private System.Diagnostics.PerformanceCounter counter;

    public PerformanceCounter()
    {
    }

    public PerformanceCounter(string categoryName, string counterName, string instanceName)
    {
      this.categoryName = categoryName;
      this.counterName = counterName;
      this.instanceName = instanceName;
    }

    [XmlAttribute("CategoryName")]
    public string CategoryName
    {
      get { return categoryName; }
      set
      {
        categoryName = value;
        Initialize();
      }
    }

    [XmlAttribute("CounterName")]
    public string CounterName
    {
      get { return counterName; }
      set
      {
        counterName = value;
        Initialize();
      }
    }

    [XmlAttribute("InstanceName")]
    public string InstanceName
    {
      get { return instanceName; }
      set
      {
        instanceName = value;
        Initialize();
      }
    }

    [XmlAttribute("Format")]
    public string Format
    {
      get { return format; }
      set { format = value; }
    }

    protected override string DoEvaluate()
    {
      if (counter == null)
      {
        return "";
      }
      float result = counter.NextValue();
      if (format == null)
      {
        return result.ToString();
      }
      return result.ToString(format);
    }

    private void Initialize()
    {
      if (categoryName == null || counterName == null || instanceName == null)
      {
        return;
      }
      if (counter != null)
      {
        counter.Dispose();
      }
      counter = new System.Diagnostics.PerformanceCounter(categoryName, counterName, instanceName);
    }
  }
}