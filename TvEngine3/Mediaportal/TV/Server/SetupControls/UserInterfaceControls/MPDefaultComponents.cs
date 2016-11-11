#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupControls.UserInterfaceControls
{
  public class MPCheckedListBox : CheckedListBox { }
  public class MPColumnHeader : ColumnHeader
  {
    public MPColumnHeader() : base() { }
    public MPColumnHeader(int imageIndex) : base(imageIndex) { }
    public MPColumnHeader(string imageKey) : base(imageKey) { }
  }
  public class MPContextMenuStrip : ContextMenuStrip
  {
    public MPContextMenuStrip() : base() { }
    public MPContextMenuStrip(System.ComponentModel.IContainer container) : base(container) { }
  }
  public class MPDataGridViewButtonColumn : DataGridViewButtonColumn { }
  public class MPDataGridViewCheckBoxColumn : DataGridViewCheckBoxColumn
  {
    public MPDataGridViewCheckBoxColumn() : base() { }
    public MPDataGridViewCheckBoxColumn(bool threeState) : base(threeState) { }
  }
  public class MPDataGridViewComboBoxColumn : DataGridViewComboBoxColumn { }
  public class MPDataGridViewTextBoxColumn : DataGridViewTextBoxColumn { }
  public class MPDateTimePicker : DateTimePicker { }
  public class MPHScrollBar : HScrollBar { }
  public class MPLabel : Label { }
  public class MPLinkLabel : LinkLabel { }
  public class MPListBox : ListBox { }
  public class MPMaskedTextBox : MaskedTextBox
  {
    public MPMaskedTextBox() : base() { }
    public MPMaskedTextBox(System.ComponentModel.MaskedTextProvider maskedTextProvider) : base(maskedTextProvider) { }
    public MPMaskedTextBox(string mask) : base(mask) { }
  }
  public class MPPanel : Panel { }
  public class MPPictureBox : PictureBox { }
  public class MPProgressBar : ProgressBar { }
  public class MPTableLayoutPanel : TableLayoutPanel { }
  public class MPToolStrip : ToolStrip
  {
    public MPToolStrip() { }
    public MPToolStrip(params ToolStripItem[] items) : base(items) { }
  }
  public class MPToolStripButton : ToolStripButton
  {
    public MPToolStripButton() : base() { }
    public MPToolStripButton(Image image) : base(image) { }
    public MPToolStripButton(string text) : base(text) { }
    public MPToolStripButton(string text, Image image) : base(text, image) { }
    public MPToolStripButton(string text, Image image, EventHandler onClick) : base(text, image, onClick) { }
    public MPToolStripButton(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) { }
  }
  public class MPToolStripMenuItem : ToolStripMenuItem
  {
    public MPToolStripMenuItem() : base() { }
    public MPToolStripMenuItem(Image image) : base(image) { }
    public MPToolStripMenuItem(string text) : base(text) { }
    public MPToolStripMenuItem(string text, Image image) : base(text, image) { }
    public MPToolStripMenuItem(string text, Image image, EventHandler onClick) : base(text, image, onClick) { }
    public MPToolStripMenuItem(string text, Image image, params ToolStripItem[] dropDownItems) : base(text, image, dropDownItems) { }
    public MPToolStripMenuItem(string text, Image image, EventHandler onClick, Keys shortcutKeys) : base(text, image, onClick, shortcutKeys) { }
    public MPToolStripMenuItem(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) { }
  }
  public class MPToolStripSeparator : ToolStripSeparator { }
  public class MPToolTip : ToolTip
  {
    public MPToolTip() : base() { }
    public MPToolTip(System.ComponentModel.IContainer container) : base(container) { }
  }
  public class MPTreeView : TreeView { }
}