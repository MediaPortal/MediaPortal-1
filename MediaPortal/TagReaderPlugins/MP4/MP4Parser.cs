/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.IO;
using System.Collections;
using System.Text;

using MediaPortal.TagReader.MP4.MiscUtil.Conversion;

namespace MediaPortal.TagReader.MP4
{
	/// <summary>
	/// 
	/// </summary>
	public class MP4Parser
	{
		static string[] ATOM_CONTAINER_TYPES = 
		{
			"MOOV", "UDTA", "META",
			"ILST", "©NAM", "©CPY", "©DAY", "©DIR",
			"©FMT", "©INF", "©PRD", "©PRF", "©REQ", 
			"©SRC", "©WRT", "©ART", "©ALB", "TRKN",
			"©CMT", "COVR", "DISK", "GNRE", "©GEN"
		};
		static byte[] atomSizeBuf = new byte[4];
		static byte[] atomTypeBuf = new byte[4];
		static byte[] extendedAtomSizeBuf = new byte[8];

		public MP4Parser()
		{
			// 
			// TODO: Add constructor logic here
			//
		}
		public static ParsedAtom[] parseAtoms (string fileName) {
			FileInfo f = new FileInfo(fileName);
			Stream s = f.OpenRead();
			try 
			{
				ParsedAtom[] atoms = parseAtoms (s, 0, s.Length);
				return atoms;
			} 
			finally 
			{
				s.Close();
			}
		}

		public static ParsedAtom findAtom(ParsedAtom[] atoms, string atomPath) 
		{
			string[] components = atomPath.Split('.');
			int i = 0;
			bool found = false;
			ParsedAtom[] children = atoms;
			while (true) 
			{
				foreach(ParsedAtom atom in children) 
				{
					if (atom.Type == components[i]) 
					{
						i++;
						if (i == components.Length) 
						{
							return atom;
						}
						if (atom is ParsedContainerAtom) 
						{
							children = ((ParsedContainerAtom)atom).Children;
						} 
						else 
						{
							return null;
						}
						found = true;
						break;
					}
				}
				if (!found) 
				{
					return null;
				}
				found = false;
			}
		}

		protected static ParsedAtom[] parseAtoms(Stream s, long offset, long stopAt) 
		{
			ArrayList parsedAtomList = new ArrayList();
			while (offset < stopAt)
			{
				if (s.Position != offset) 
				{
					s.Seek(offset, SeekOrigin.Begin);
				}
				int bytesRead = s.Read(atomSizeBuf, 0, atomSizeBuf.Length);
				if (bytesRead < atomSizeBuf.Length) 
				{
					throw new IOException ("couldn't read atom length");
				}
				long atomSize = EndianBitConverter.Big.ToUInt32(atomSizeBuf, 0);
				if (s.Position == s.Length) 
				{
					break;
				}
				bytesRead = s.Read(atomTypeBuf, 0, atomTypeBuf.Length);
				if (bytesRead != atomTypeBuf.Length) 
				{
					throw new IOException ("Couldn't read atom type");
				}
				string atomType = Encoding.Default.GetString(atomTypeBuf).ToUpper();
				if (atomSize == 1) 
				{
					bytesRead = s.Read(extendedAtomSizeBuf, 0,	extendedAtomSizeBuf.Length);
					if (bytesRead != extendedAtomSizeBuf.Length)
					{
						throw new IOException("Couldn't read extended atom size");
					}
					atomSize = EndianBitConverter.Big.ToInt64(extendedAtomSizeBuf, 0);
				}
				if ((atomSize < 0) || ((offset + atomSize) > s.Length)) 
				{
					throw new IOException("atom has invalid size: " +	atomSize + " (" + s.Length + ")");
				}
				ParsedAtom parsedAtom = null;
				if (Array.IndexOf(ATOM_CONTAINER_TYPES, atomType) >= 0) 
				{
					// children run from current point to the end of the atom
					long pos = (atomType == "META") ? s.Position + 4 : s.Position;

					ParsedAtom [] children =
						parseAtoms (s,
						pos,
						offset + atomSize);
					parsedAtom = new ParsedContainerAtom(atomSize, atomType, children);
				} 
				else 
				{
					parsedAtom = AtomFactory.createAtomFor(atomSize, atomType, s);
				}
				// add atom to the list
				parsedAtomList.Add(parsedAtom);
				if (atomSize == 0) 
				{
					offset = s.Length;
				}
				else  
				{
					offset += atomSize;
				}


			}
			return (ParsedAtom[])parsedAtomList.ToArray(typeof(ParsedAtom));
		}
	}
}
