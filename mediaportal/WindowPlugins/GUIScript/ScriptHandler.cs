/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

#region Usings
using System;
using System.IO;
using System.Collections;
using System.Management;
#endregion

namespace MediaPortal.GUI.GUIScript
{
	/// <summary>
	/// Summary description for ScriptHandler.
	/// </summary>
	public class ScriptHandler
	{
		
		private struct script 
		{
			public string name;
			public string button;
			public MPScript mpScript;
		}
		private static MPScript gscript=new MPScript();
		private static ArrayList	scripts = new ArrayList();

		public ScriptHandler()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <summary>
		/// sets a global bool var
		/// </summary>
		public void SetGlobalVar(string key,object val)
		{
			gscript.setGlobalVar(key,val);
		}

		/// <summary>
		/// start a script with Button Name
		/// </summary>
		public void StartScript(string name)		// starts script "name"
		{
			foreach (script scr in scripts) 
			{
				if (scr.button==name) 
				{
					bool b=scr.mpScript.RunScript();
					break;
				}
			}
		}

		/// <summary>
		/// start a script with Script Name
		/// </summary>
		public void StartScriptName(string name)		// starts script "name"
		{
			foreach (script scr in scripts) 
			{
				if (scr.name==name) 
				{
					bool b=scr.mpScript.RunScript();
					break;
				}
			}
		}

		/// <summary>
		/// load a script in memory
		/// </summary>
		public string LoadScript(string name) // loads a script in memory
		{
			string btnTxt="";
			if (scripts.Count>0) 
			{
				bool found=false;
				foreach (script scr in scripts)   // is script already here? 
				{
					if (scr.name==name) 
					{
						found=true;
						btnTxt=scr.button;
						break;
					}
				}
				if (found==false) 
				{
					script sc = new script();
					sc.name=name;
					MPScript mpScript = new MPScript();
					sc.button=mpScript.GetScript(name);
					sc.mpScript=mpScript;
					scripts.Add(sc);
					btnTxt=sc.button;
				} 
			} 
			else 
			{
				script sc = new script();
				sc.name=name;
				MPScript mpScript = new MPScript();
				btnTxt=mpScript.GetScript(name);
				sc.button=btnTxt;
				sc.mpScript=mpScript;
				scripts.Add(sc);
			}
			return btnTxt;
		}

	}
}
