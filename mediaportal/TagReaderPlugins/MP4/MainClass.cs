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
