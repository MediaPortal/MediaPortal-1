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

namespace System.Windows.Input
{
  public sealed class ApplicationCommands
  {
    #region Constructors

    static ApplicationCommands()
    {
      Open = new UICommand("Open", typeof (ApplicationCommands));
      Close = new UICommand("Close", typeof (ApplicationCommands));
      ContextMenu = new UICommand("ContextMenu", typeof (ApplicationCommands));
      Copy = new UICommand("Copy", typeof (ApplicationCommands));
      CorrectionList = new UICommand("CorrectionList", typeof (ApplicationCommands));
      Cut = new UICommand("Cut", typeof (ApplicationCommands));
      Delete = new UICommand("Delete", typeof (ApplicationCommands));
      Find = new UICommand("Find", typeof (ApplicationCommands));
      Help = new UICommand("Help", typeof (ApplicationCommands));
      New = new UICommand("New", typeof (ApplicationCommands));
      Open = new UICommand("Open", typeof (ApplicationCommands));
      Paste = new UICommand("Paste", typeof (ApplicationCommands));
      Print = new UICommand("Print", typeof (ApplicationCommands));
      PrintPreview = new UICommand("PrintPreview", typeof (ApplicationCommands));
      Properties = new UICommand("Properies", typeof (ApplicationCommands));
      Redo = new UICommand("Redo", typeof (ApplicationCommands));
      Replace = new UICommand("Replace", typeof (ApplicationCommands));
      Save = new UICommand("Save", typeof (ApplicationCommands));
      SaveAs = new UICommand("SaveAs", typeof (ApplicationCommands));
      SelectAll = new UICommand("SelectAll", typeof (ApplicationCommands));
      Stop = new UICommand("Stop", typeof (ApplicationCommands));
      Undo = new UICommand("Undo", typeof (ApplicationCommands));
    }

    private ApplicationCommands()
    {
    }

    #endregion Constructors

    #region Fields

    public static readonly UICommand Close;
    public static readonly UICommand ContextMenu;
    public static readonly UICommand Copy;
    public static readonly UICommand CorrectionList;
    public static readonly UICommand Cut;
    public static readonly UICommand Delete;
    public static readonly UICommand Find;
    public static readonly UICommand Help;
    public static readonly UICommand New;
    public static readonly UICommand Open;
    public static readonly UICommand Paste;
    public static readonly UICommand Print;
    public static readonly UICommand PrintPreview;
    public static readonly UICommand Properties;
    public static readonly UICommand Redo;
    public static readonly UICommand Replace;
    public static readonly UICommand Save;
    public static readonly UICommand SaveAs;
    public static readonly UICommand SelectAll;
    public static readonly UICommand Stop;
    public static readonly UICommand Undo;

    #endregion Fields
  }
}