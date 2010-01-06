#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace AMS.Profile
{
  /// <summary>
  ///   Types of changes that may be made to a Profile object. </summary>
  /// <remarks>
  ///   A variable of this type is passed inside the ProfileChangedArgs object 
  ///   for the <see cref="Profile.Changing" /> and <see cref="Profile.Changed" /> events </remarks>
  /// <seealso cref="ProfileChangedArgs" />
  public enum ProfileChangeType
  {
    /// <summary> 
    ///   The change refers to the <see cref="Profile.Name" /> property. </summary>		
    /// <remarks> 
    ///   <see cref="ProfileChangedArgs.Value" /> will contain the new name. </remarks>
    Name,

    /// <summary> 
    ///   The change refers to the <see cref="Profile.ReadOnly" /> property. </summary>		
    /// <remarks> 
    ///   <see cref="ProfileChangedArgs.Value" /> will be true. </remarks>
    ReadOnly,

    /// <summary> 
    ///   The change refers to the <see cref="Profile.SetValue" /> method. </summary>		
    /// <remarks> 
    ///   <see cref="ProfileChangedArgs.Section" />,  <see cref="ProfileChangedArgs.Entry" />, 
    ///   and <see cref="ProfileChangedArgs.Value" /> will be set to the same values passed 
    ///   to the SetValue method. </remarks>
    SetValue,

    /// <summary> 
    ///   The change refers to the <see cref="Profile.RemoveEntry" /> method. </summary>		
    /// <remarks> 
    ///   <see cref="ProfileChangedArgs.Section" /> and <see cref="ProfileChangedArgs.Entry" /> 
    ///   will be set to the same values passed to the RemoveEntry method. </remarks>
    RemoveEntry,

    /// <summary> 
    ///   The change refers to the <see cref="Profile.RemoveSection" /> method. </summary>		
    /// <remarks> 
    ///   <see cref="ProfileChangedArgs.Section" /> will contain the name of the section passed to the RemoveSection method. </remarks>
    RemoveSection,

    /// <summary> 
    ///   The change refers to method or property specific to the Profile class. </summary>		
    /// <remarks> 
    ///   <see cref="ProfileChangedArgs.Entry" /> will contain the name of the  method or property.
    ///   <see cref="ProfileChangedArgs.Value" /> will contain the new value. </remarks>
    Other
  }

  /// <summary>
  ///   EventArgs class to be passed as the second parameter of a <see cref="Profile.Changed" /> event handler. </summary>
  /// <remarks>
  ///   This class provides all the information relevant to the change made to the Profile.
  ///   It is also used as a convenient base class for the ProfileChangingArgs class which is passed 
  ///   as the second parameter of the <see cref="Profile.Changing" /> event handler. </remarks>
  /// <seealso cref="ProfileChangingArgs" />
  public class ProfileChangedArgs : EventArgs
  {
    // Fields
    private readonly ProfileChangeType m_changeType;
    private readonly string m_section;
    private readonly string m_entry;
    private readonly object m_value;

    /// <summary>
    ///   Initializes a new instance of the ProfileChangedArgs class by initializing all of its properties. </summary>
    /// <param name="changeType">
    ///   The type of change made to the profile. </param>
    /// <param name="section">
    ///   The name of the section involved in the change or null. </param>
    /// <param name="entry">
    ///   The name of the entry involved in the change, or if changeType is set to Other, the name of the method/property that was changed. </param>
    /// <param name="value">
    ///   The new value for the entry or method/property, based on the value of changeType. </param>
    /// <seealso cref="ProfileChangeType" />
    public ProfileChangedArgs(ProfileChangeType changeType, string section, string entry, object value)
    {
      m_changeType = changeType;
      m_section = section;
      m_entry = entry;
      m_value = value;
    }

    /// <summary>
    ///   Gets the type of change that raised the event. </summary>
    public ProfileChangeType ChangeType
    {
      get { return m_changeType; }
    }

    /// <summary>
    ///   Gets the name of the section involved in the change, or null if not applicable. </summary>
    public string Section
    {
      get { return m_section; }
    }

    /// <summary>
    ///   Gets the name of the entry involved in the change, or null if not applicable. </summary>
    /// <remarks> 
    ///   If <see cref="ChangeType" /> is set to Other, this property holds the name of the 
    ///   method/property that was changed. </remarks>
    public string Entry
    {
      get { return m_entry; }
    }

    /// <summary>
    ///   Gets the new value for the entry or method/property, based on the value of <see cref="ChangeType" />. </summary>
    public object Value
    {
      get { return m_value; }
    }
  }

  /// <summary>
  ///   EventArgs class to be passed as the second parameter of a <see cref="Profile.Changing" /> event handler. </summary>
  /// <remarks>
  ///   This class provides all the information relevant to the change about to be made to the Profile.
  ///   Besides the properties of ProfileChangedArgs, it adds the Cancel property which allows the 
  ///   event handler to prevent the change from happening. </remarks>
  /// <seealso cref="ProfileChangedArgs" />
  public class ProfileChangingArgs : ProfileChangedArgs
  {
    private bool m_cancel;

    /// <summary>
    ///   Initializes a new instance of the ProfileChangingArgs class by initializing all of its properties. </summary>
    /// <param name="changeType">
    ///   The type of change to be made to the profile. </param>
    /// <param name="section">
    ///   The name of the section involved in the change or null. </param>
    /// <param name="entry">
    ///   The name of the entry involved in the change, or if changeType is set to Other, the name of the method/property that was changed. </param>
    /// <param name="value">
    ///   The new value for the entry or method/property, based on the value of changeType. </param>
    /// <seealso cref="ProfileChangeType" />
    public ProfileChangingArgs(ProfileChangeType changeType, string section, string entry, object value) :
      base(changeType, section, entry, value) {}

    /// <summary>
    ///   Gets or sets whether the change about to the made should be canceled or not. </summary>
    /// <remarks> 
    ///   By default this property is set to false, meaning that the change is allowed.  </remarks>
    public bool Cancel
    {
      get { return m_cancel; }
      set { m_cancel = value; }
    }
  }

  /// <summary>
  ///   Definition of the <see cref="Profile.Changing" /> event handler. </summary>
  /// <remarks>
  ///   This definition complies with the .NET Framework's standard for event handlers.
  ///   The sender is always set to the Profile object that raised the event. </remarks>
  public delegate void ProfileChangingHandler(object sender, ProfileChangingArgs e);

  /// <summary>
  ///   Definition of the <see cref="Profile.Changed" /> event handler. </summary>
  /// <remarks>
  ///   This definition complies with the .NET Framework's standard for event handlers.
  ///   The sender is always set to the Profile object that raised the event. </remarks>
  public delegate void ProfileChangedHandler(object sender, ProfileChangedArgs e);
}