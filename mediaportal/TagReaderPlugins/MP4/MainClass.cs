/* 
 *	Copyright (C) 2005 Team MediaPortal
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

namespace MediaPortal.TagReader.MP4
{
	/// <summary>
	/// Summary description for MainClass.
	/// </summary>
	class MainClass
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// ParsedAtom[] atoms = MP4Parser.parseAtoms("");
			if (args.Length < 1) 
			{
				printUsage();
				return;
			}
			try 
			{
				ParsedAtom[] atoms = MP4Parser.parseAtoms(args[0]);
				printAtom(MP4Parser.findAtom(atoms, "MOOV.UDTA.META.ILST"), "");

			} 
			catch (Exception e) 
			{
				Console.Error.WriteLine("Error process mp4 file: {0}", e.Message);
			}
		}
		protected static void printAtomTree (ParsedAtom[] atomTree, String indent) 
		{
			foreach (ParsedAtom atom in atomTree) 
			{
				printAtom(atom, indent);
			}
		}
		protected static void printAtom(ParsedAtom atom, String indent) 
		{
			Console.Write(indent);
			Console.WriteLine(atom.ToString());

			if (atom is ParsedContainerAtom) 
			{
				ParsedAtom[] children = ((ParsedContainerAtom) atom).Children;
				printAtomTree (children, indent + "  ");
			}
		}
		protected static void printUsage() 
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("\tMP4Parser fileName");
		}

	}
}
