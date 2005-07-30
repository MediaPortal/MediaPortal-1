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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using MediaPortal.GUI.Library;

namespace GUIRecipies 
{

	public class RecipiePrinter
	{
		private string cat;
		private bool nextpage;      // Is there a second Page to print?
 		private Font printFont;
		private Font printBFont;
		private Font printHFont;
		private string Snextpage;
		private Recipie recP = new Recipie();

		public RecipiePrinter() : base() 
		{
		}

		// The Click event is raised when the user clicks the Print button.
		public void printRecipie(Recipie rec, string scat, string stit)
		{
			cat=scat;
			recP = rec;
			nextpage = false;
			Snextpage="";
			printFont = new Font("Arial", 11);
			printHFont = new Font("Arial Black",13);
			printBFont = new Font("Arial Black",10);
			
			PrintDocument pd = new PrintDocument();
			pd.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);
			pd.Print();
			if (nextpage==true) 
			{
				PrintDocument pd2 = new PrintDocument();
				pd2.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);
				pd2.Print();
			}
		}

		// The PrintPage event is raised for each page to be printed.
		private void pd_PrintPage(object sender, PrintPageEventArgs ev) 
		{
			float linesPerPage = 0;
			float yPos = 0;
			int count = 0;
			int start = 0;
			int end = 0;
			int mx = 0;
			int len = 80;

			float leftMargin = ev.MarginBounds.Left;
			float topMargin = ev.MarginBounds.Top;
			string contents = recP.ToString();

			// Calculate the number of lines per page.
			linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);

			string[] lines = contents.Split( '\n' );
			string pline = "";
			string lline = "";

			// Create pen.
			Pen blackPen = new Pen(Color.Black, 2);
			ev.Graphics.DrawRectangle(blackPen,ev.MarginBounds.Left-50,ev.MarginBounds.Top-70,ev.MarginBounds.Width+50,21);
			ev.Graphics.DrawRectangle(blackPen,ev.MarginBounds.Left-50,ev.MarginBounds.Top-49,ev.MarginBounds.Width+50,33);
			ev.Graphics.DrawString(GUILocalizeStrings.Get(2053)+": "+cat, printBFont, Brushes.Black,leftMargin-30, ev.MarginBounds.Top-69, new StringFormat());
			ev.Graphics.DrawString(lines[0].Substring(8), printHFont, Brushes.Black,leftMargin-30, ev.MarginBounds.Top-47, new StringFormat());

			// Print each line of the file.
			foreach( string line in lines )
			{   
				if (nextpage==true) 
				{
					if (Snextpage.Length > 1) 
					{
						lline = Snextpage;
						start = 0;
						end = 0;
						mx = len;
						for( int i=0; i < Snextpage.Length; i++) 
						{
							if (lline[i] == ' ') end=i;
							if (i == mx) 
							{
								pline = Snextpage.Substring(start,end-start);
								mx = end + len;
								start = end + 1;
								pline = "            " + pline;
								yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
								ev.Graphics.DrawString(pline, printFont, Brushes.Black,leftMargin, yPos, new StringFormat());
								count++;
								if (count >= linesPerPage) 	break;
							}
						}
						pline = Snextpage.Substring(start);
						pline = "            " + pline;
						yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
						ev.Graphics.DrawString(pline, printFont, Brushes.Black,leftMargin, yPos, new StringFormat());
						count++;
						break;	
					}
				} 
				if (line.Length > len && nextpage==false) 
				{   
					lline=line;
					start = 0;
					end = 0;
					mx=len;
					for( int i=0; i<line.Length; i++) 
					{
						if (lline[i] == ' ') end=i;
						if (i==mx) 
						{
							pline=line.Substring(start,end-start);
							mx=end+len;
							start=end+1;
							pline="            "+pline;
							yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
							ev.Graphics.DrawString(pline, printFont, Brushes.Black,leftMargin, yPos, new StringFormat());
							count++;
							if (count >= linesPerPage) 
							{
								Snextpage=line.Substring(start);
								nextpage=true;
								break;
							}
						}
					}
					if (nextpage==false) 
					{
						pline=line.Substring(start);
						pline="            "+pline;
						yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
						ev.Graphics.DrawString(pline, printFont, Brushes.Black,leftMargin, yPos, new StringFormat());
						count++;
					} 
					else  
					{
						break;
					}
				} 
				else 
				{  
					if (nextpage==false) 
					{
						yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
						ev.Graphics.DrawString(line, printFont, Brushes.Black,leftMargin, yPos, new StringFormat());
						count++;
						if (count >= linesPerPage) 
						{
							nextpage=true;
							break;
						}
					}
				}				
				if (count >= linesPerPage) 
				{
					nextpage=true;
					break;
				}
			}
		}
	}
}
