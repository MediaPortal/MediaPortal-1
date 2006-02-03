// Performs the actions required before testing MediaPortal.
// Copyright (C) 2005  Michel Otte
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
 * Date: 17-9-2005
 * Time: 12:45
 * 
 */

using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace MPTestTool
{
	/// <summary>
	/// Performs actions necessary before doing MediaPortal tests.
	/// </summary>
	public class PreTestActions : ProgressDialog, IActionsCanceler
	{
		private bool actionsContinue = true;
		
		private int totalActions = 3;
		
		private string mplogdir;
		private string logdir;
		
		private static string[] logNames = { "Application", "System" };

		public PreTestActions(string mplogdir, string logdir)
		{
			this.mplogdir = mplogdir;
			this.logdir = logdir;
			base.setCaller(this);
		}
	
		// Callback method for ProgressDialog.cs
		// (described in ActionsCanceler.cs)
		public void ActionCanceled()
		{
			actionsContinue = false;
		}
		// Shows an error dialogbox with given error
		private void Error(string text)
		{
			MessageBox.Show(
			                "An Error occurred:\n\n" + text,
			                "Error",
			                MessageBoxButtons.OK,
			                MessageBoxIcon.Error
			               );
		}
		private void updateProgress(int subActions)
		{
			int actionAmount = 100 / totalActions;
			int subActionAmount = actionAmount / subActions;
			base.setProgress(base.getProgress() + subActionAmount);
		}
		public bool PerformActions()
		{
			if (actionsContinue)
				ClearEventLog();
			if (actionsContinue)
				ClearMPLogDir();
			if (actionsContinue)
				ClearLogDir();
			if (actionsContinue)
				base.Done();
			return actionsContinue;
		}
		private void ClearEventLog()
		{
			base.setAction("Clearing EventLogs...");
			int subActions = logNames.Length;
			try {
				foreach (string strLogName in logNames)
				{
					if (!actionsContinue)
						break;
					EventLog e = 
						new EventLog(strLogName);
					e.Clear();
					updateProgress(subActions);
				}
			} catch (Exception ex)
			{
				actionsContinue = false;
				Error(ex.ToString());
			}
			if (subActions == 0)
				updateProgress(1);
		}
		private void ClearDir(string strDir)
		{
			string[] files = Directory.GetFiles(strDir);
			int subActions = files.Length;
			foreach (string file in files)
			{
				if (!actionsContinue)
					break;
				if (File.Exists(file))
				{
					File.Delete(file);
					updateProgress(subActions);
				}
			}
			if (subActions == 0)
				updateProgress(1);

		}
		private void ClearMPLogDir()
		{
			base.setAction("Clearing MediaPortal log subdirectory...");
			ClearDir(mplogdir);
		}
		private void ClearLogDir()
		{
			base.setAction("Clearing destination log directory...");
			ClearDir(logdir);
		}
	}
}
