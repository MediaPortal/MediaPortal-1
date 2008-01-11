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
using System.Data;
                       

namespace AMS.Profile
{	
	/// <summary>
	///   Base interface for all profile classes in this namespace.
	///   It represents a read-only profile. </summary>
	/// <seealso cref="IProfile" />
	/// <seealso cref="Profile" />
	public interface IReadOnlyProfile : ICloneable
	{
		/// <summary>
		///   Gets the name associated with the profile. </summary>
		/// <remarks>
		///   This should be the name of the file where the data is stored, or something equivalent. </remarks>
		string Name
		{
			get; 
		}

		/// <summary>
		///   Retrieves the value of an entry inside a section. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry with the value. </param>
		/// <param name="entry">
		///   The name of the entry where the value is stored. </param>
		/// <returns>
		///   The return value should be the entry's value, or null if the entry does not exist. </returns>
		/// <seealso cref="HasEntry" />
		object GetValue(string section, string entry);
		
		/// <summary>
		///   Retrieves the value of an entry inside a section, or a default value if the entry does not exist. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry with the value. </param>
		/// <param name="entry">
		///   The name of the entry where the value is stored. </param>
		/// <param name="defaultValue">
		///   The value to return if the entry (or section) does not exist. </param>
		/// <returns>
		///   The return value should be the entry's value converted to a string, or the given default value if the entry does not exist. </returns>
		/// <seealso cref="HasEntry" />
		string GetValue(string section, string entry, string defaultValue);
		
		/// <summary>
		///   Retrieves the value of an entry inside a section, or a default value if the entry does not exist. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry with the value. </param>
		/// <param name="entry">
		///   The name of the entry where the value is stored. </param>
		/// <param name="defaultValue">
		///   The value to return if the entry (or section) does not exist. </param>
		/// <returns>
		///   The return value should be the entry's value converted to an integer.  If the value
		///   cannot be converted, the return value should be 0.  If the entry does not exist, the
		///   given default value should be returned. </returns>
		/// <seealso cref="HasEntry" />
		int GetValue(string section, string entry, int defaultValue);

		/// <summary>
		///   Retrieves the value of an entry inside a section, or a default value if the entry does not exist. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry with the value. </param>
		/// <param name="entry">
		///   The name of the entry where the value is stored. </param>
		/// <param name="defaultValue">
		///   The value to return if the entry (or section) does not exist. </param>
		/// <returns>
		///   The return value should be the entry's value converted to a double.  If the value
		///   cannot be converted, the return value should be 0.  If the entry does not exist, the
		///   given default value should be returned. </returns>
		/// <seealso cref="HasEntry" />
		double GetValue(string section, string entry, double defaultValue);

		/// <summary>
		///   Retrieves the value of an entry inside a section, or a default value if the entry does not exist. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry with the value. </param>
		/// <param name="entry">
		///   The name of the entry where the value is stored. </param>
		/// <param name="defaultValue">
		///   The value to return if the entry (or section) does not exist. </param>
		/// <returns>
		///   The return value should be the entry's value converted to a bool.  If the value
		///   cannot be converted, the return value should be <c>false</c>.  If the entry does not exist, the
		///   given default value should be returned. </returns>
		/// <remarks>
		///   Note: Boolean values are stored as "True" or "False". </remarks>
		/// <seealso cref="HasEntry" />
		bool GetValue(string section, string entry, bool defaultValue);

		/// <summary>
		///   Determines if an entry exists inside a section. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry. </param>
		/// <param name="entry">
		///   The name of the entry to be checked for existence. </param>
		/// <returns>
		///   If the entry exists inside the section, the return value should be true; otherwise false. </returns>
		/// <seealso cref="HasSection" />
		/// <seealso cref="GetEntryNames" />
		bool HasEntry(string section, string entry);

		/// <summary>
		///   Determines if a section exists. </summary>
		/// <param name="section">
		///   The name of the section to be checked for existence. </param>
		/// <returns>
		///   If the section exists, the return value should be true; otherwise false. </returns>
		/// <seealso cref="HasEntry" />
		/// <seealso cref="GetSectionNames" />
		bool HasSection(string section);

		/// <summary>
		///   Retrieves the names of all the entries inside a section. </summary>
		/// <param name="section">
		///   The name of the section holding the entries. </param>
		/// <returns>
		///   If the section exists, the return value should be an array with the names of its entries; 
		///   otherwise it should be null. </returns>
		/// <seealso cref="HasEntry" />
		/// <seealso cref="GetSectionNames" />
		string[] GetEntryNames(string section);

		/// <summary>
		///   Retrieves the names of all the sections. </summary>
		/// <returns>
		///   The return value should be an array with the names of all the sections. </returns>
		/// <seealso cref="HasSection" />
		/// <seealso cref="GetEntryNames" />
		string[] GetSectionNames();

		/// <summary>
		///   Retrieves a DataSet object containing every section, entry, and value in the profile. </summary>
		/// <returns>
		///   If the profile exists, the return value should be a DataSet object representing the profile; otherwise it's null. </returns>
		/// <remarks>
		///   The returned DataSet should be named using the <see cref="Name" /> property.  
		///   It should contain one table for each section, and each entry should be represented by a column inside the table.
		///   Each table should contain only one row where the values will be stored corresponding to each column (entry). 
		///   <para>
		///   This method serves as a convenient way to extract the profile's data to this generic medium known as the DataSet.  
		///   This allows it to be moved to many different places, including a different type of profile object 
		///   (eg., INI to XML conversion). </para>
		/// </remarks>
		DataSet GetDataSet();
	}

	/// <summary>
	///   Interface implemented by all profile classes in this namespace.
	///   It represents a normal profile. </summary>
	/// <remarks>
	///   This interface takes the members of IReadOnlyProfile (its base interface) and adds
	///   to it the rest of the members, which allow modifications to the profile.  
	///   Altogether, this represents a complete profile object. </remarks>
	/// <seealso cref="IReadOnlyProfile" />
	/// <seealso cref="Profile" />
	public interface IProfile : IReadOnlyProfile
	{
		/// <summary>
		///   Gets or sets the name associated with the profile. </summary>
		/// <remarks>
		///   This should be the name of the file where the data is stored, or something equivalent.
		///   When setting this property, the <see cref="ReadOnly" /> property should be checked and if true, an InvalidOperationException should be thrown.
		///   The <see cref="Changing" /> and <see cref="Changed" /> events should be raised before and after this property is changed. </remarks>
		/// <seealso cref="DefaultName" />
		new string Name
		{
			get; 
			set;
		}

		/// <summary>
		///   Gets the name associated with the profile by default. </summary>
		/// <remarks>
		///   This is used to set the default Name of the profile and it is typically based on 
		///   the name of the executable plus some extension. </remarks>
		/// <seealso cref="Name" />
		string DefaultName
		{
			get;
		}

		/// <summary>
		///   Gets or sets whether the profile is read-only or not. </summary>
		/// <remarks>
		///   A read-only profile should not allow any operations that alter sections,
		///   entries, or values, such as <see cref="SetValue" /> or <see cref="RemoveEntry" />.  
		///   Once a profile has been marked read-only, it should be allowed to go back; 
		///   attempting to do so should cause an InvalidOperationException to be thrown.
		///   The <see cref="Changing" /> and <see cref="Changed" /> events should be raised before 
		///   and after this property is changed. </remarks>
		/// <seealso cref="CloneReadOnly" />
		/// <seealso cref="IReadOnlyProfile" />
		bool ReadOnly
		{
			get; 
			set;
		}		
	
		/// <summary>
		///   Sets the value for an entry inside a section. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry. </param>
		/// <param name="entry">
		///   The name of the entry where the value will be set. </param>
		/// <param name="value">
		///   The value to set. If it's null, the entry should be removed. </param>
		/// <remarks>
		///   This method should check the <see cref="ReadOnly" /> property and throw an InvalidOperationException if it's true.
		///   It should also raise the <see cref="Changing" /> and <see cref="Changed" /> events before and after the value is set. </remarks>
		/// <seealso cref="IReadOnlyProfile.GetValue" />
		void SetValue(string section, string entry, object value);
		
		/// <summary>
		///   Removes an entry from a section. </summary>
		/// <param name="section">
		///   The name of the section that holds the entry. </param>
		/// <param name="entry">
		///   The name of the entry to remove. </param>
		/// <remarks>
		///   This method should check the <see cref="ReadOnly" /> property and throw an InvalidOperationException if it's true.
		///   It should also raise the <see cref="Changing" /> and <see cref="Changed" /> events before and after the entry is removed. </remarks>
		/// <seealso cref="RemoveSection" />
		void RemoveEntry(string section, string entry);

		/// <summary>
		///   Removes a section. </summary>
		/// <param name="section">
		///   The name of the section to remove. </param>
		/// <remarks>
		///   This method should check the <see cref="ReadOnly" /> property and throw an InvalidOperationException if it's true.
		///   It should also raise the <see cref="Changing" /> and <see cref="Changed" /> events before and after the section is removed. </remarks>
		/// <seealso cref="RemoveEntry" />
		void RemoveSection(string section);
		
		/// <summary>
		///   Writes the data of every table from a DataSet into this profile. </summary>
		/// <param name="ds">
		///   The DataSet object containing the data to be set. </param>
		/// <remarks>
		///   Each table in the DataSet should be used to represent a section of the profile.  
		///   Each column of each table should represent an entry.  And for each column, the corresponding value
		///   of the first row is the value that should be passed to <see cref="SetValue" />.  
		///   <para>
		///   This method serves as a convenient way to take any data inside a generic DataSet and 
		///   write it to any of the available profiles. </para></remarks>
		/// <seealso cref="IReadOnlyProfile.GetDataSet" />
		void SetDataSet(DataSet ds);
		
		/// <summary>
		///   Creates a copy of itself and makes it read-only. </summary>
		/// <returns>
		///   The return value should be a copy of itself as an IReadOnlyProfile object. </returns>
		/// <remarks>
		///   This method is meant as a convenient way to pass a read-only copy of the profile to methods 
		///   that are not allowed to modify it. </remarks>
		/// <seealso cref="ReadOnly" />
		IReadOnlyProfile CloneReadOnly();
		
		/// <summary>
		///   Event that should be raised just before the profile is to be changed to allow the change to be canceled. </summary>
		/// <seealso cref="Changed" />
		event ProfileChangingHandler Changing;

		/// <summary>
		///   Event that should be raised right after the profile has been changed. </summary>
		/// <seealso cref="Changing" />
		event ProfileChangedHandler Changed;				
	}
}

