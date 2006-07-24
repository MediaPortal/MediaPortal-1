// FileUtils.cs: A few reusable File processing routines
// Copyright (C) 2005-2006  Michel Otte
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
/*
 * Created by SharpDevelop.
 * User: Michel
 * Date: 27-9-2005
 * Time: 15:58
 * 
 */
using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// A few reusable File processing routines
/// </summary>
public class FileUtils
{
	public static string getCreationTime(string file)
	{
		try {
			FileInfo fi = new FileInfo(file);
			return fi.CreationTime.ToString();
		} catch {
			return string.Empty;
		}
	}
	public static string getHashValue(string file)
	{
		try {
			MD5CryptoServiceProvider md5csp = new MD5CryptoServiceProvider();
			byte[] bHash = md5csp.ComputeHash(
			                                  new FileStream(
			                                                 file,
			                                                 FileMode.Open,
			                                                 FileAccess.Read
			                                                )
			                                 );
				return BitConverter.ToString(bHash);
		} catch {
			return string.Empty;
		}
	}
}
