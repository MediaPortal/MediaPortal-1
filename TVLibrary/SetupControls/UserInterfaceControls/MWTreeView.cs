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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using MWCommon;

/// <summary>
///	Mikael Wiberg 2003
///		mikwib@hotmail.com (usual HoTMaiL spam filters)
///		mick@ar.com.au (heavy spam filters on, harldy anything gets through, START the subject with C# and it will probably go through)
///		md5mw@mdstud.chalmers.se (heavy spam filters on, harldy anything gets through, START the subject with C# and it will probably go through)
///	
///	Feel free to use this code as you wish, as long as you do not take credit for it yourself.
///	If it is used in commercial projects or applications please mention my name.
///	Feel free to donate any amount of money if this code makes you happy ;)
///	Use this code at your own risk. If your machine blows up while using it - don't blame me.
/// </summary>
namespace MWControls
{
  /// <summary>
  /// A TreeView Control that allows multiple TreeNodes to be selected at once.
  /// 
  /// Note that the MWTreeView is only using fully managed code, no interop or Win32 stuff. All code will abide by this and therefore some
  ///		things are very hard to do.
  ///	Any code suggestions using Win32 stuff or other is however of interest to me, but will never make it into the MWTreeView. If I receive
  ///		code suggestions that are good enough and also make a real difference I might create another version of the MWTreeView, so don't
  ///		hesitate to make such suggestions.
  ///	Also note though that I will use the Paint methods of the original TreeView Control - I will NOT draw the whole MWTreeView myself. If
  ///		that was what I wanted I would have created one from scratch, probably using less code than this one, and with more(!)
  ///		functionality.
  /// 
  /// There are additional keyboard commands implemented for selecting/deselecting TreeNodes.
  /// Note that in order to keep the feature of scrolling the MWTreeView (inherited from MS TreeView) when the Control key is pressed
  ///		while clicking the Up/Down etc keys, selecting multiple TreeNodes using the keyboard is done by holding down the Shift key.
  ///	Holding down the Alt key selects/deselects whole branches of TreeNodes.
  ///	OnKeyDown is ('heavily') overridden and allows much more keyboard functinality than the original MS TreeView Control:
  ///		F2			Start LabelEdit for SelNode.
  ///		Space		Checking/Unchecking TreeNodes if the CheckBoxes property is set to true. Try holding down Control and/or Alt.
  ///		Down		Moving SelNode. Try holding down Shift and/or Alt.
  ///		Up			Moving SelNode. Try holding down Shift and/or Alt.
  ///		Right		Moving SelNode or expanding SelNode's branch. Try holding down Shift and/or Alt.
  ///		Left		Moving SelNode or collapsing SelNode's branch. Try holding down Shift and/or Alt.
  ///		Home		Moving SelNode. Try holding down Shift and/or Alt.
  ///		End			Moving SelNode. Try holding down Shift and/or Alt.
  ///		PageUp		Moving SelNode. Try holding down Shift and/or Alt.
  ///		PageDown	Moving SelNode. Try holding down Shift and/or Alt.
  ///		Control-A	Selecting all TreeNodes. Try holding down Shift and/or Alt.
  ///		Escape		Deselecting all TreeNodes. Try holding down Shift.
  ///		Control-Q	Collapse all TreeNodes. Try holding down Shift and/or Alt.
  ///		Control-E	Expand all TreeNodes. Try holding down Shift and/or Alt.
  ///	
  ///	Overriding the OnBeforeSelect EventHandler is the key to having a TreeView with multi select working properly (some other people
  ///		have tried, quite unsuccessfully in my view, to create multi-select TreeViews without overriding this EventHandler).
  ///	The important call in the overridden OnBeforeSelect EventHandler is 'e.Cancel = true;'. This means that the MS TreeView
  ///		SelectedNode property is not used. Instead the new SelNode property is used.
  /// 
  /// The MultiSelect property allows you to select TreeNodes in some various ways.
  ///		Classic is the same as the normal TreeView.
  ///		NoMulti works the same way as a normal MS TreeView when it comes to selecting TreeNodes - only one can be selected at a time.
  ///			You do get all the other benefits of the MWTreeView even with NoMulti - mouse handling and keyboard handling etc.
  ///			Note that the SelectedImageIndex DOES work when using NoMulti.
  ///		Multi means that any TreeNodes - no restrictions - can be selected.
  ///		MultiSameBranchAndLevel means that the TreeNodes you select all have to be in the same branch AND on the same TreeNodeLevel.
  ///		MultiSameBranch means that the TreeNodes you select all have to be in the same branch, but CAN be on different TreeNodeLevels.
  ///		MultiSameLevel means that the TreeNodes you select all have to be on the same TreeNodeLevel, but CAN be in different branches.
  ///		MultiPathToParents means that the TreeNodes you select must all be in the same branch AND on the same TreeNodeLevel. All the direct
  ///			Parent TreeNodes also get selected forming a sort of path to the selected TreeNodes.
  ///		MultiPathToParent means that the TreeNodes you select must all be on the same TreeNodeLevel on a per root branch basis. All the
  ///			direct Parent TreeNodes also get selected forming sort of paths to the selected TreeNodes.
  ///		SinglePathToParent means that the TreeNode you select will also select the direct Parent TreeNodes forming a sort of path to the
  ///			selected TreeNode.
  ///		SinglePathToParents means that the TreeNodes you select will also select the direct Parent TreeNodes forming sort of paths to the
  ///			selected TreeNodes. This is done on a per root branch basis.
  ///	You might wonder why anyone would want some of the options available in the MultiSelect property. Multi is pretty obvious and so is
  ///		NoMulti. But the other ones... Well MultiSameBranch is the reason why I created this Control in the first place - because I
  ///		needed this kind of functionality. So there you go.
  ///	
  ///	The AllowRubberbandSelect property lets you select TreeNodes using a rubberband method just like in many Windows applications - I
  ///		haven't seen this in a TreeView before though (too hard? nah).
  ///	The Rubberband selection works a bit differently than it does in e.g. the 'files panel' in Windows Explorer (which is a ListView not a
  ///		TreeView). A TreeNode is selected as soon as the rubberband 'hits' the TreeNode vertically. I.e. even if the rubberband does not
  ///		hit the Text of the TreeNode but e.g. a bit to the right of the Text, the TreeNode is selected. This is done because I do not have
  ///		access to the ScrollBars of the TreeView and thus I cannot find out if the TreeView has been scrolled to the left a bit. If I did
  ///		know this, I could easily measure the TreeNode Text and indentation (including CheckBox and Image) and find out if I hit the Text
  ///		or not.
  ///	Note that if the control key is pressed the rubberband method turns into a sort of painting/masking method. This means that any
  ///		TreeNodes selected from the rubberband do not become unselected if the rubberband is no longer covering them. Windows Explorer
  ///		toggles the entries in the 'files panel', but I don't personally like this - so I did not implement it like that intentionally.
  ///	The Rubberband painting is done using ControlPaint.DrawReversibleFrame and can handle that the Control is scrolled vertically while
  ///		painting it. Scrolling horizontally will not move the rubberband though.
  /// 
  /// The AllowBlankNodeText property allows you to set whether the Text of TreeNodes can be blank or not. If this property is set to
  ///		false and there is an attempt to set the Text to string.Empty the old Text is used instead.
  ///	
  ///	The AllowNoSelNode property allows you to decide whether no TreeNode can be selected in the MWTreeView. If there is no TreeNode
  ///		selected and this property is set to false a TreeNode is selected.
  ///	
  ///	If the ScrollToSelNode property is set to true the MWTreeView scrolls so that the SelNode is visible if possible.
  ///	
  ///	FullRowSelect has been implemented. This feature didn't seem to be 'turned on' in the MS TreeView.
  ///	Because MS did such a 'good' job of hiding the ScrollBars (Controls, methods for positions etc) I couldn't find out if the Control
  ///		is scrolled (if anyone has any ideas how to do this in managed, non-interop etc code, PLEASE let me know). This made it a lot
  ///		harder for me to implement TreeNode selection if the click is done on something other than the Text or Image.
  /// 
  /// HotTracking has been fully implemented.
  /// 
  /// A focus rectangle is now painted around the last selected TreeNode.
  /// Note that the focus rectangle is not painted when TreeNode have been selected using the rubberband selection. When the OnMouseUp
  ///		EventHandler starts working properly (.NET Framework v1.1) I might add support for this (see
  ///		http://www.gotdotnet.com/team/changeinfo/Backwards1.0to1.1/default.aspx#00000057).
  ///	
  ///	If the LabelEditRegEx Property is set to a regular expression this has to be satisfied in order to change the TreeNode Text.
  ///	Note that the LabelEditRegEx Property only applies to changes done through the GUI - NOT in code. This is intentional.
  ///	
  ///	If the DisallowLabelEditRegEx Property is set to a regular expression this must NOT be satisfied in order to be able to change the
  ///		TreeNode Text. I.e. If you have a TreeNode whose Text currently satisfies the regular expression in the DisallowLabelEditRegEx
  ///		Property, this TreeNode's Text CANNOT be changed.
  ///	Note that the DisallowLabelEditRegEx Property only applies to attempted changes done through the GUI - NOT in code. This is intentional.
  /// 
  /// If the SelectNodeRegEx Property is set to a regular expression this must be satisfied in order to be able to select the TreeNode.
  /// 
  /// If the CheckNodeRegEx Property is set to a regular expression this must be satisfied in order to be able to check the TreeNode.
  /// 
  /// 
  /// 
  /// Is there any other functionality you would like to see in the MWTreeView? Please tell me and I'll see what I can do.
  /// 
  /// </summary>
  public class MWTreeView : TreeView
  {
    #region Variables

    #region Property Variables

    /// <summary>
    /// Decides the multi select characteristics of an MWTreeView Control.
    /// </summary>
    private TreeViewMultiSelect tvmsMultiSelect = TreeViewMultiSelect.Multi;

    /// <summary>
    /// True if multiple TreeNodes can be checked at once or false otherwise (true is standard for MS TreeView).
    /// </summary>
    private bool bAllowMultiCheck = true;

    /// <summary>
    /// HashTable containing the Selected TreeNodes wrapped in MWTreeNodeWrapper objects as values and the TreeNode.GetHashCodes as keys.
    /// </summary>
    private Hashtable htSelNodes = new Hashtable();

    /// <summary>
    /// HashTable containing the Checked TreeNodes as values and the TreeNode.GetHashCodes as keys.
    /// </summary>
    private Hashtable htCheckedNodes = new Hashtable();

    /// <summary>
    /// Last Selected TreeNode or null if no TreeNode is selected or if Last Selected TreeNode was deselected.
    /// </summary>
    private TreeNode tnSelNode;

    /// <summary>
    /// True if scrolling is done so the SelNode (Last Selected Tree Node) is always displayed or false otherwise.
    /// </summary>
    private bool bScrollToSelNode = true;

    /// <summary>
    /// True if TreeNodes can be blank, i.e. contain no Text.
    /// </summary>
    private bool bAllowBlankNodeText;

    /// <summary>
    /// True if no TreeNode has to be selected or false otherwise (false is standard for MS TreeView).
    /// Note that if using a MultiSelect of TreeViewMultiSelect.NoMulti this property is ignored.
    /// </summary>
    private bool bAllowNoSelNode = true;

    /// <summary>
    /// True if TreeNodes can be selected by a rubberband method or false otherwise.
    /// </summary>
    private bool bAllowRubberbandSelect = true;

    /// <summary>
    /// Regular expression that has to be satisfied before the Text of a TreeNode can be changed.
    /// </summary>
    private string strLabelEditRegEx = string.Empty;

    /// <summary>
    /// Regular expression that cannot be satisfied if the Text of a TreeNode should be able to be changed.
    /// </summary>
    private string strDisallowLabelEditRegEx = string.Empty;

    /// <summary>
    /// Regular expression that has to be satisfied before a TreeNode can be selected.
    /// Note that the AllowNoSelNode property is ignored if this property is used
    /// </summary>
    private string strSelectNodeRegEx = string.Empty;

    /// <summary>
    /// Regular expression that has to be satisfied before a TreeNode can be checked.
    /// </summary>
    private string strCheckNodeRegEx = string.Empty;

    #endregion Property Variables



    #region Help Variables

    /// <summary>
    /// This variable is set to true if the keyboard was used for checking/unchecking a TreeNode (or group of TreeNodes).
    /// </summary>
    private bool bKeyCheck;

    /// <summary>
    /// This variable is set to true if the mouse was used for checking/unchecking a TreeNode (or group of TreeNodes).
    /// </summary>
    private bool bMouseCheck;

    /// <summary>
    /// This variable is set to true if the AllowMultiCheck property was set to false.
    /// </summary>
    private bool bAllowMultiCheckChanged;

    /// <summary>
    /// This variable is set to true if the TreeNodes should be forced to be checked/unchecked.
    /// </summary>
    private bool bForceCheckNode;

    /// <summary>
    /// When this MWTreeView is selected or has focus this variable is set to true.
    /// When the mouse is used to click inside this MWTreeView this variable is checked to see if the TreeNodes should be repainted.
    ///		This is necessary since the MouseDown event happens before the OnGotFocus and OnEnter events.
    /// </summary>
    private bool bActive;

    /// <summary>
    /// True if a Label is allowed to be edited.
    /// Note that LabelEdit has to be true as well.
    /// </summary>
    private bool bLabelEditAllowed;

    /// <summary>
    /// TreeNode that was in the coordinates of OnMouseDown.
    /// Used in conjunction with the FullRowSelect property.
    /// Note that this TreeNode is treated differently than the tnMouseDown TreeNode and they must therefore be separate variables.
    /// </summary>
    private TreeNode tnFullRowSelect;

    /// <summary>
    /// The selected state of the TreeNode that was in the coordinates of OnMouseDown.
    /// Used in conjunction with the FullRowSelect property.
    /// </summary>
    private bool bFullRowSelectNodeSelected;

    /// <summary>
    /// The checked state of the TreeNode that was in the coordinates of OnMouseDown.
    /// Used in conjunction with the FullRowSelect property.
    /// </summary>
    private bool bFullRowSelectNodeChecked;

    /// <summary>
    /// The expanded state of the TreeNode that was in the coordinates of OnMouseDown.
    /// Used in conjunction with the FullRowSelect property.
    /// </summary>
    private bool bFullRowSelectNodeExpanded;

    /// <summary>
    /// TreeNode that was in the coordinates of the OnMouseDown.
    /// Used in conjunction with the AllowRubberbandSelect property.
    /// Note that this TreeNode is treated differently than the tnFullRowSelect TreeNode and they must therefore be separate variables.
    /// </summary>
    private TreeNode tnMouseDown;

    /// <summary>
    /// Point in the coordinates of the OnMouseDown.
    /// Used in conjunction with the AllowRubberbandSelect property.
    /// Note that this variable changes if the Control is scrolled vertically while a rubberband is painted.
    /// Also see variable ptMouseDownOrig.
    /// </summary>
    private Point ptMouseDown = new Point(0, 0);

    /// <summary>
    /// Point in the coordinates of the OnMouseDown.
    /// Used in conjunction with the AllowRubberbandSelect property.
    /// Note that this variable does NOT change if the Control is scrolled vertically while a rubberband is painted - this is the original
    ///		position from the OnMouseDown EventHandler and therefore does not change until the next OnMouseDown call.
    /// Also see variable ptMouseDown.
    /// </summary>
    private Point ptMouseDownOrig = new Point(0, 0);

    /// <summary>
    /// Point in the coordinates of the OnMouseDown transformed to screen coordinates.
    /// Used in conjunction with the AllowRubberbandSelect property.
    /// </summary>
    private Point ptMouseDownScreen = new Point(0, 0);

    /// <summary>
    /// Point in the coordinates of the mouse position in the OnMouseMove EventHandler transformed to screen coordinates.
    /// Used in conjunction with the AllowRubberbandSelect property.
    /// </summary>
    private Point ptMouseMoveScreen = new Point(0, 0);

    /// <summary>
    /// True if a rubberband has been painted (and therefore must be cleared) or false otherwise.
    /// Used in conjunction with the AllowRubberbandSelect property.
    /// </summary>
    private bool bRubberbandHasBeenPainted;

    /// <summary>
    /// True if the 'next' TreeNode that enters the OnBeforeSelect EventHandler should be treated as a 'proper' selected TreeNode as far as
    ///		the original TreeView Control is concerned. I.e. Hottracking works and the focus rectangle around the TreeNode is also
    ///		displayed.
    ///	If false the above does not happen.
    /// </summary>
    private bool bPaintFocusRectAndHottracking;

    #endregion Help Variables



    #region Component Designer generated Variables

    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    #endregion Component Designer generated Variables

    #endregion Variables



    #region Constructor & Dispose

    /// <summary>
    /// Standard constructor.
    /// </summary>
    public MWTreeView()
    {
      //Set a few ControlStyles (note that not all are used/necessary, AllPaintingInWmPaint, DoubleBuffer and UserPaint are though).

      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.DoubleBuffer, true);
      SetStyle(ControlStyles.ResizeRedraw, true);
      SetStyle(ControlStyles.Selectable, true);
      SetStyle(ControlStyles.SupportsTransparentBackColor, true);
      //SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);

      InitializeComponent();
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }

      base.Dispose(disposing);
    }

    #endregion Constructor & Dispose



    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
    }

    #endregion Component Designer generated code



    #region Overridden Properties

    /// <summary>
    /// Standard HideSelection property that also handles the activation and deactivation of selected TreeNodes.
    /// </summary>
    public new bool HideSelection
    {
      get
      {
        return base.HideSelection;
      }
      set
      {
        base.HideSelection = value;

        ActivateOrDeactivateSelNodes();
      }
    }

    #endregion Overridden Properties



    #region Overridden EventHandlers

    #region Focus (OnGotFocus & OnLostFocus) & Activation (OnEnter & OnLeave)

    /// <summary>
    /// Standard OnGotFocus EventHandler.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    protected override void OnGotFocus(EventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        bLabelEditAllowed = false;

        ActivateSelNodes();
      }

      base.OnGotFocus(e);
    }

    /// <summary>
    /// Standard OnLostFocus EventHandler.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    protected override void OnLostFocus(EventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        bLabelEditAllowed = false;

        bActive = false;

        DeactivateSelNodes();
      }

      base.OnLostFocus(e);
    }

    /// <summary>
    /// Standard OnEnter EventHandler.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    protected override void OnEnter(EventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        bLabelEditAllowed = false;

        ActivateSelNodes();
      }

      base.OnEnter(e);
    }

    /// <summary>
    /// Standard OnLeave EventHandler.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    protected override void OnLeave(EventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        bLabelEditAllowed = false;

        bActive = false;

        DeactivateSelNodes();
      }

      base.OnLeave(e);
    }

    #endregion Focus (OnGotFocus & OnLostFocus) & Activation (OnEnter & OnLeave)



    #region Label Edit (OnBeforeLabelEdit & OnAfterLabelEdit)

    /// <summary>
    /// Standard OnBeforeLabelEdit EventHandler.
    /// </summary>
    /// <param name="e">Standard NodeLabelEditEventArgs object.</param>
    protected override void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        if (IsDisallowLabelEditRegExSatisfied(e.Node.Text))
        {
          e.CancelEdit = true;
        }
      }

      base.OnBeforeLabelEdit(e);
    }

    /// <summary>
    /// Standard OnAfterLabelEdit EventHandler.
    /// </summary>
    /// <param name="e">Standard NodeLabelEditEventArgs object.</param>
    protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        bLabelEditAllowed = false;

        if ((AllowBlankNodeText && e.Label == string.Empty) ||
          ((!string.IsNullOrEmpty(e.Label)) &&
          IsLabelEditRegExSatisfied(e.Label)))
        {
          base.OnAfterLabelEdit(e);
        }
        else
        {
          e.CancelEdit = true;
        }
      }
      else
      {
        base.OnAfterLabelEdit(e);
      }
    }

    #endregion Label Edit (OnBeforeLabelEdit & OnAfterLabelEdit)



    #region Selection (OnBeforeSelect & BeforeSelectMethod)

    /// <summary>
    /// Standard OnBeforeSelect EventHandler.
    /// </summary>
    /// <param name="e">Standard TreeViewCancelEventArgs object.</param>
    protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        if (!bPaintFocusRectAndHottracking)
        {
          if (e.Action != TreeViewAction.Unknown && e.Action != TreeViewAction.ByKeyboard && MultiSelect != TreeViewMultiSelect.NoMulti)
          {
            BeforeSelectMethod(e.Node);
          }

          if (MultiSelect != TreeViewMultiSelect.NoMulti)
          {
            e.Cancel = true;

            if (!IsTreeNodeSelected(e.Node))
            {
              EnsureSelNodeNotNull();
            }
          }
        }
      }

      base.OnBeforeSelect(e);
    }

    /// <summary>
    /// Main method used when selecting/deselecting TreeNodes using the mouse.
    /// </summary>
    /// <param name="tnMain">Node to be selected/deselected.</param>
    private void BeforeSelectMethod(TreeNode tnMain)
    {
      if ((ModifierKeys & Keys.Control) != 0 && (ModifierKeys & Keys.Shift) == 0)
      {
        if ((ModifierKeys & Keys.Alt) != 0)
        {
          if (IsTreeNodeSelected(tnMain))
          {
            DeselectBranch(tnMain, true, true);
          }
          else
          {
            SelectBranch(tnMain, true, true, true);
          }
        }
        else
        {
          ToggleNode(tnMain, true);
        }
      }
      else
      {
        if ((ModifierKeys & Keys.Shift) != 0)
        {
          if ((ModifierKeys & Keys.Control) == 0)
          {
            ClearSelNodes();
          }

          TreeNode tn = SelNode;

          SelectNode(SelNode, false);
          int iSelNodeTreeNodeLevel = GetTreeNodeLevel(SelNode);

          bool bPrevious = false;
          TreeNode tnTemp = SelNode;
          while (tnTemp != null && tnTemp.PrevVisibleNode != null && tnTemp != tnMain)
          {
            if (tnTemp.PrevVisibleNode == tnMain)
            {
              bPrevious = true;
              break;
            }
            tnTemp = tnTemp.PrevVisibleNode;
          }

          if (bPrevious)
          {
            while (tn != null && tn.PrevVisibleNode != null && tn != tnMain)
            {
              if (MultiSelect == TreeViewMultiSelect.MultiSameLevel)
              {
                if (iSelNodeTreeNodeLevel == GetTreeNodeLevel(tn.PrevVisibleNode))
                {
                  SelectNode(tn.PrevVisibleNode, false);
                }
              }
              else if (MultiSelect == TreeViewMultiSelect.MultiPathToParent ||
                MultiSelect == TreeViewMultiSelect.MultiPathToParents ||
                MultiSelect == TreeViewMultiSelect.SinglePathToParent ||
                MultiSelect == TreeViewMultiSelect.SinglePathToParents)
              {
                SelectNode(tn.PrevVisibleNode, false);
              }
              else
              {
                if ((ModifierKeys & Keys.Alt) != 0)
                {
                  SelectBranch(tn.PrevVisibleNode, true, false, false);
                }
                else
                {
                  SelectNode(tn.PrevVisibleNode, false);
                }
              }

              tn = tn.PrevVisibleNode;
            }
          }
          else
          {
            while (tn != null && tn.NextVisibleNode != null && tn != tnMain)
            {
              if (MultiSelect == TreeViewMultiSelect.MultiSameLevel)
              {
                if (iSelNodeTreeNodeLevel == GetTreeNodeLevel(tn.NextVisibleNode))
                {
                  SelectNode(tn.NextVisibleNode, false);
                }
              }
              else if (MultiSelect == TreeViewMultiSelect.MultiPathToParent ||
                MultiSelect == TreeViewMultiSelect.MultiPathToParents ||
                MultiSelect == TreeViewMultiSelect.SinglePathToParent ||
                MultiSelect == TreeViewMultiSelect.SinglePathToParents)
              {
                SelectNode(tn.NextVisibleNode, false);
              }
              else
              {
                if ((ModifierKeys & Keys.Alt) != 0)
                {
                  SelectBranch(tn.NextVisibleNode, true, false, false);
                }
                else
                {
                  SelectNode(tn.NextVisibleNode, false);
                }

                tn = tn.NextVisibleNode;
              }
            }
          }

          if (MultiSelect == TreeViewMultiSelect.MultiSameLevel)
          {
            if (iSelNodeTreeNodeLevel == GetTreeNodeLevel(tnMain))
            {
              SelectNode(tnMain, true);
            }
          }
          else
          {
            SelectNode(tnMain, true);
          }
        }
        else
        {
          ClearSelNodes();

          if ((ModifierKeys & Keys.Alt) != 0 && MultiSelect != TreeViewMultiSelect.MultiSameLevel)
          {
            SelectBranch(tnMain, true, true, true);
          }
          else
          {
            SelectNode(tnMain, true);
          }
        }
      }
    }

    #endregion Selection (OnBeforeSelect & BeforeSelectMethod)



    #region Checking (OnBeforeCheck & OnBeforeCheckMethod)

    /// <summary>
    /// Standard OnBeforeCheck EventHandler.
    /// </summary>
    /// <param name="e">Standard TreeViewCancelEventArgs object.</param>
    protected override void OnBeforeCheck(TreeViewCancelEventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        if (e.Action == TreeViewAction.Unknown && !bKeyCheck && !bMouseCheck && !bAllowMultiCheckChanged && !bForceCheckNode)
        {
          e.Cancel = true;
        }

        if (e.Action == TreeViewAction.ByMouse)
        {
          bMouseCheck = true;

          BeforeCheckMethod(e.Node, e.Action != TreeViewAction.ByKeyboard);

          bLabelEditAllowed = false;

          if (AllowMultiCheck)
          {
            if (IsTreeNodeChecked(e.Node))
            {
              //Opposite, since the TreeNode hasn't been unchecked yet.
              if (e.Node.Checked)
              {
                CheckedNodes.Remove(e.Node.GetHashCode());
              }
            }
            else
            {
              if (CheckedNodes == null)
              {
                CheckedNodes = new Hashtable();
              }

              //Opposite, since the TreeNode hasn't been checked yet.
              if (!e.Node.Checked && IsCheckNodeRegExSatisfied(e.Node.Text))
              {
                CheckedNodes.Add(e.Node.GetHashCode(), e.Node);
              }
            }
          }
          else
          {
            //Opposite, since the TreeNode hasn't been unchecked yet.
            if (e.Node.Checked)
            {
              CheckedNodes.Remove(e.Node.GetHashCode());
            }
            else if (IsCheckNodeRegExSatisfied(e.Node.Text))
            {
              if (CheckedNodes == null)
              {
                CheckedNodes = new Hashtable();
              }

              ClearCheckedNodes();

              CheckedNodes.Add(e.Node.GetHashCode(), e.Node);
            }
          }

          bMouseCheck = false;
        }

        if (!bMouseCheck)
        {
          if (!IsCheckNodeRegExSatisfied(e.Node.Text) && !e.Node.Checked)
          {
            e.Cancel = true;
          }

          base.OnBeforeCheck(e);
        }
      }
      else
      {
        base.OnBeforeCheck(e);
      }
    }

    /// <summary>
    /// Main method used when checking/unchecking TreeNodes using the mouse.
    /// </summary>
    /// <param name="tn">Node to be checked/unchecked.</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    private void BeforeCheckMethod(TreeNode tn, bool bUpdate)
    {
      if ((ModifierKeys & Keys.Shift) != 0 ||
        (ModifierKeys & Keys.Control) != 0 ||
        (ModifierKeys & Keys.Alt) != 0)
      {
        bAllowMultiCheckChanged = true;

        if (tn.Parent != null)
        {
          if (!((ModifierKeys & Keys.Shift) != 0 && (ModifierKeys & Keys.Control) != 0) &&
            (ModifierKeys & Keys.Alt) == 0)
          {
            ToggleCheckNode(tn.Parent, bUpdate);
          }

          if ((ModifierKeys & Keys.Control) != 0)
          {
            ToggleCheckBranch(tn.Parent, true, tn, bUpdate);
          }
          else if ((ModifierKeys & Keys.Shift) != 0 && (ModifierKeys & Keys.Alt) != 0)
          {
            ToggleCheckBranch(tn.Parent, false, tn, bUpdate);
            ToggleCheckBranch(tn, true, bUpdate);
          }
          else if ((ModifierKeys & Keys.Shift) != 0)
          {
            ToggleCheckBranch(tn.Parent, false, tn, bUpdate);
          }
          else if ((ModifierKeys & Keys.Alt) != 0)
          {
            ToggleCheckBranch(tn, true, bUpdate);
          }
        }
        else
        {
          if ((ModifierKeys & Keys.Control) != 0)
          {
            foreach (TreeNode tn2 in Nodes)
            {
              if (tn != tn2)
              {
                ToggleCheckNode(tn2, bUpdate);
              }

              ToggleCheckBranch(tn2, true, tn, bUpdate);
            }
          }
          else if ((ModifierKeys & Keys.Shift) != 0 && (ModifierKeys & Keys.Alt) != 0)
          {
            foreach (TreeNode tn2 in Nodes)
            {
              if (tn != tn2)
              {
                ToggleCheckNode(tn2, bUpdate);
              }
            }

            ToggleCheckBranch(tn, true, bUpdate);
          }
          else if ((ModifierKeys & Keys.Shift) != 0)
          {
            foreach (TreeNode tn2 in Nodes)
            {
              if (tn != tn2)
              {
                ToggleCheckNode(tn2, bUpdate);
              }
            }
          }
          else if ((ModifierKeys & Keys.Alt) != 0)
          {
            ToggleCheckBranch(tn, true, bUpdate);
          }
        }

        bAllowMultiCheckChanged = false;
      }
    }

    #endregion Checking (OnBeforeCheck & OnBeforeCheckMethod)



    #region Keys (OnKeyDown)

    /// <summary>
    /// Standard OnKeyDown EventHandler.
    /// </summary>
    /// <param name="e">Standard KeyEventArgs object.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        bool bThisSelNodeIsNull = false;

        switch (e.KeyCode)
        {
          #region F2 Key

          case Keys.F2:
            if (LabelEdit)
            {
              if (SelNode != null)
              {
                SelNode.BeginEdit();
              }
              else if (SelNodes.Count > 0)
              {
                foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
                {
                  mwtnw.Node.BeginEdit();
                  break;
                }
              }
            }

            break;

          #endregion F2 Key



          #region Space Key

          case Keys.Space:
            if (CheckBoxes)
            {
              bKeyCheck = true;

              if (((e.Modifiers & Keys.Shift) != 0 || (e.Modifiers & Keys.Control) != 0) && AllowMultiCheck)
              {
                if (SelectedNode != null)
                {
                  if (SelectedNode.Parent != null)
                  {
                    if (!((e.Modifiers & Keys.Shift) != 0 && (e.Modifiers & Keys.Control) != 0))
                    {
                      ToggleCheckNode(SelectedNode.Parent, true);
                    }

                    if ((e.Modifiers & Keys.Control) != 0)
                    {
                      ToggleCheckBranch(SelectedNode.Parent, true, SelectedNode, true);
                      ToggleCheckNode(SelectedNode, false);
                    }
                    else
                    {
                      ToggleCheckBranch(SelectedNode.Parent, false, SelectedNode, true);
                      ToggleCheckNode(SelectedNode, false);
                    }
                  }
                  else
                  {
                    if ((e.Modifiers & Keys.Control) != 0)
                    {
                      foreach (TreeNode tn in Nodes)
                      {
                        if (tn != SelectedNode)
                        {
                          ToggleCheckNode(tn, true);
                        }

                        ToggleCheckBranch(tn, true, tn, true);
                      }
                    }
                    else
                    {
                      foreach (TreeNode tn in Nodes)
                      {
                        if (tn != SelectedNode)
                        {
                          ToggleCheckNode(tn, true);
                        }
                      }
                    }
                  }
                }
              }
              else if (SelectedNode != null)
              {
                ToggleCheckNode(SelectedNode, false);
              }

              bKeyCheck = false;
            }

            break;

          #endregion Space Key



          #region Down Key

          case Keys.Down:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (SelNode != null && SelNode.NextVisibleNode != null && (e.Modifiers & Keys.Control) == 0)
              {
                TreeNode tnTemp = SelNode;

                if ((e.Modifiers & Keys.Shift) == 0)
                {
                  ClearSelNodes();
                }

                if ((e.Modifiers & Keys.Alt) == 0)
                {
                  SelectNode(tnTemp.NextVisibleNode, true);
                }
                else
                {
                  TreeNode tnTempNextVisibleNode = tnTemp.NextVisibleNode;
                  if ((e.Modifiers & Keys.Shift) != 0)
                  {
                    SelectBranch(tnTemp, true, false, true);
                  }
                  SelectBranch(tnTempNextVisibleNode, true, false, true);
                }
              }
            }

            break;

          #endregion Down Key



          #region Up Key

          case Keys.Up:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (SelNode != null && SelNode.PrevVisibleNode != null && (e.Modifiers & Keys.Control) == 0)
              {
                TreeNode tnTemp = SelNode;

                if ((e.Modifiers & Keys.Shift) == 0)
                {
                  ClearSelNodes();
                }

                if ((e.Modifiers & Keys.Alt) == 0)
                {
                  SelectNode(tnTemp.PrevVisibleNode, true);
                }
                else
                {
                  TreeNode tnTempPrevVisibleNode = tnTemp.PrevVisibleNode;
                  if ((e.Modifiers & Keys.Shift) != 0)
                  {
                    SelectBranch(tnTemp, true, false, true);
                  }
                  SelectBranch(tnTempPrevVisibleNode, true, true, true);
                }
              }
            }

            break;

          #endregion Up Key



          #region Right Key

          case Keys.Right:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (SelNode != null && (e.Modifiers & Keys.Control) == 0)
              {
                if (!SelNode.IsExpanded)
                {
                  SelNode.Expand();

                  if ((e.Modifiers & Keys.Shift) == 0)
                  {
                    ClearSelNodes();
                  }

                  SelectNode(SelNode, true);
                }
                else
                {
                  if ((e.Modifiers & Keys.Shift) == 0)
                  {
                    ClearSelNodes();
                  }

                  SelectNode(SelNode.FirstNode, true);
                }
              }
            }

            break;

          #endregion Right Key



          #region Left Key

          case Keys.Left:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (SelNode != null && (e.Modifiers & Keys.Control) == 0)
              {
                if (SelNode.IsExpanded)
                {
                  SelNode.Collapse();

                  if ((e.Modifiers & Keys.Shift) == 0)
                  {
                    ClearSelNodes();
                  }

                  SelectNode(SelNode, true);
                }
                else
                {
                  if (SelNode.Parent != null)
                  {
                    if ((e.Modifiers & Keys.Shift) == 0)
                    {
                      ClearSelNodes();
                    }

                    SelectNode(SelNode.Parent, true);
                  }
                }
              }
            }

            break;

          #endregion Left Key



          #region Home Key

          case Keys.Home:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (SelNode != null && (e.Modifiers & Keys.Control) == 0)
              {
                if ((e.Modifiers & Keys.Shift) == 0 && (e.Modifiers & Keys.Alt) == 0)
                {
                  ClearSelNodes();

                  SelectNode(Nodes[0], true);
                }
                else
                {
                  if ((e.Modifiers & Keys.Alt) == 0)
                  {
                    TreeNode tn = SelNode;
                    while (tn != null && tn.PrevVisibleNode != null)
                    {
                      tn = tn.PrevVisibleNode;
                      SelectNode(tn, true);
                    }
                  }
                  else
                  {
                    TreeNode tn = SelNode;

                    if ((e.Modifiers & Keys.Shift) != 0)
                    {
                      SelectBranch(tn, true, true, true);
                    }

                    while (tn != null && tn.PrevVisibleNode != null)
                    {
                      tn = tn.PrevVisibleNode;
                      SelectBranch(tn, true, true, true);
                    }
                  }
                }
              }
            }

            break;

          #endregion Home Key



          #region End Key

          case Keys.End:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (SelNode != null && (e.Modifiers & Keys.Control) == 0)
              {
                if ((e.Modifiers & Keys.Shift) == 0 && (e.Modifiers & Keys.Alt) == 0)
                {
                  ClearSelNodes();

                  TreeNode tn = SelNode;
                  while (tn != null && tn.NextVisibleNode != null)
                  {
                    tn = tn.NextVisibleNode;
                  }
                  SelectNode(tn, true);
                }
                else
                {
                  if ((e.Modifiers & Keys.Alt) == 0)
                  {
                    TreeNode tn = SelNode;
                    while (tn != null && tn.NextVisibleNode != null)
                    {
                      tn = tn.NextVisibleNode;
                      SelectNode(tn, true);
                    }
                  }
                  else
                  {
                    TreeNode tn = SelNode;

                    if ((e.Modifiers & Keys.Shift) != 0)
                    {
                      SelectBranch(tn, true, false, true);
                    }

                    while (tn != null && tn.NextVisibleNode != null)
                    {
                      tn = tn.NextVisibleNode;
                      SelectBranch(tn, true, false, true);
                    }
                  }
                }
              }

              if ((e.Modifiers & Keys.Control) == 0)
              {
                e.Handled = true;
              }
            }

            break;

          #endregion End Key



          #region PageUp Key

          case Keys.PageUp:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (SelNode != null && (e.Modifiers & Keys.Control) == 0)
              {
                TreeNode tnMax = GetNodeAt(0, 0);
                TreeNode tnTemp = SelNode;

                if ((e.Modifiers & Keys.Shift) == 0)
                {
                  ClearSelNodes();
                  DeselectNode(SelNode, true);
                  SelNode = null;

                  if ((e.Modifiers & Keys.Alt) == 0)
                  {
                    bThisSelNodeIsNull = true;
                  }
                }

                if (tnMax != SelNode && !bThisSelNodeIsNull)
                {
                  if (tnMax != null)
                  {
                    TreeNode tn = tnTemp;

                    if ((e.Modifiers & Keys.Alt) != 0 && (e.Modifiers & Keys.Shift) != 0)
                    {
                      SelectBranch(tn, true, true, true);
                    }
                    else
                    {
                      SelectNode(tn, true);
                    }

                    while (tn != null && tn.PrevVisibleNode != null && tn != tnMax)
                    {
                      tn = tn.PrevVisibleNode;

                      if ((e.Modifiers & Keys.Alt) != 0)
                      {
                        SelectBranch(tn, true, true, true);
                      }
                      else if ((e.Modifiers & Keys.Shift) != 0)
                      {
                        SelectNode(tn, true);
                      }
                    }

                    if ((e.Modifiers & Keys.Alt) != 0)
                    {
                      SelectBranch(tnMax, true, true, true);
                    }
                    else
                    {
                      SelectNode(tnMax, true);
                    }
                  }
                }
                else
                {
                  TreeNode tn = tnTemp;
                  int iMax = ClientRectangle.Height / ItemHeight;
                  for (int i = 0; i < iMax; i++)
                  {
                    if (tn.PrevVisibleNode != null)
                    {
                      if ((e.Modifiers & Keys.Alt) != 0)
                      {
                        SelectBranch(tn, true, true, true);
                      }
                      else if ((e.Modifiers & Keys.Shift) != 0)
                      {
                        SelectNode(tn, true);
                      }
                      tn = tn.PrevVisibleNode;
                    }
                    else
                    {
                      break;
                    }
                  }

                  if ((e.Modifiers & Keys.Alt) != 0)
                  {
                    SelectBranch(tnMax, true, true, true);
                  }
                  else
                  {
                    SelectNode(tn, true);
                  }
                }
              }
            }

            break;

          #endregion PageUp Key



          #region PageDown Key

          case Keys.PageDown:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (SelNode != null && (e.Modifiers & Keys.Control) == 0)
              {
                TreeNode tnMax = GetNodeAt(0, ClientRectangle.Height - ItemHeight);
                TreeNode tnTemp = SelNode;

                if ((e.Modifiers & Keys.Shift) == 0)
                {
                  ClearSelNodes();
                  DeselectNode(SelNode, true);
                  SelNode = null;

                  if ((e.Modifiers & Keys.Alt) == 0)
                  {
                    bThisSelNodeIsNull = true;
                  }
                }

                if (tnMax == null)
                {
                  SelectNode(tnMax, true);

                  tnMax = Nodes[Nodes.Count - 1];
                  while (tnMax != null && tnMax.NextVisibleNode != null)
                  {
                    tnMax = tnMax.NextVisibleNode;
                  }
                }

                if (tnMax != SelNode && !bThisSelNodeIsNull)
                {
                  if (tnMax != null)
                  {
                    TreeNode tn = tnTemp;

                    if ((e.Modifiers & Keys.Alt) != 0 && (e.Modifiers & Keys.Shift) != 0)
                    {
                      SelectBranch(tn, true, false, true);
                    }
                    else
                    {
                      SelectNode(tn, true);
                    }

                    while (tn != null && tn.NextVisibleNode != null && tn != tnMax)
                    {
                      tn = tn.NextVisibleNode;

                      if ((e.Modifiers & Keys.Alt) != 0)
                      {
                        SelectBranch(tn, true, false, true);
                      }
                      else if ((e.Modifiers & Keys.Shift) != 0)
                      {
                        SelectNode(tn, true);
                      }
                    }

                    if ((e.Modifiers & Keys.Alt) != 0)
                    {
                      SelectBranch(tn, true, false, true);
                    }
                    else
                    {
                      SelectNode(tnMax, true);
                    }
                  }
                }
                else
                {
                  TreeNode tn = tnTemp;
                  int iMax = ClientRectangle.Height / ItemHeight;
                  for (int i = 0; i < iMax; i++)
                  {
                    if (tn.NextVisibleNode != null)
                    {
                      if ((e.Modifiers & Keys.Alt) != 0)
                      {
                        SelectBranch(tn, true, false, true);
                      }
                      else if ((e.Modifiers & Keys.Shift) != 0)
                      {
                        SelectNode(tn, true);
                      }
                      tn = tn.NextVisibleNode;
                    }
                    else
                    {
                      break;
                    }
                  }

                  if ((e.Modifiers & Keys.Alt) != 0)
                  {
                    SelectBranch(tn, true, false, true);
                  }
                  else
                  {
                    SelectNode(tn, true);
                  }
                }
              }

              if ((e.Modifiers & Keys.Control) == 0)
              {
                e.Handled = true;
              }
            }

            break;

          #endregion PageDown Key



          #region A Key

          case Keys.A:
            if (MultiSelect != TreeViewMultiSelect.NoMulti && (e.Modifiers & Keys.Control) != 0)
            {
              switch (MultiSelect)
              {
                case TreeViewMultiSelect.Multi:
                  if ((e.Modifiers & Keys.Alt) != 0 && (e.Modifiers & Keys.Shift) != 0 && SelNode != null)
                  {
                    if (SelNode.Parent != null)
                    {
                      SelectBranch(SelNode, true, true, false);

                      SelectAllParentNodes(SelNode, false);
                    }
                    else
                    {
                      SelectBranch(SelNode, true, true, false);
                    }
                  }
                  else if ((e.Modifiers & Keys.Alt) != 0 && SelNode != null)
                  {
                    if (SelNode.Parent != null)
                    {
                      TreeNode tnTemp = SelNode;

                      foreach (TreeNode tn in SelNode.Parent.Nodes)
                      {
                        SelectBranch(tn, true, true, true);
                      }

                      SelectNode(tnTemp, true);
                    }
                    else
                    {
                      TreeNode tnTemp = SelNode;

                      foreach (TreeNode tn in Nodes)
                      {
                        SelectBranch(tn, true, true, true);
                      }

                      SelectNode(tnTemp, true);
                    }
                  }
                  else if ((e.Modifiers & Keys.Shift) != 0 && SelNode != null)
                  {
                    if (SelNode.Parent != null)
                    {
                      TreeNode tnTemp = SelNode;

                      foreach (TreeNode tn in SelNode.Parent.Nodes)
                      {
                        SelectNode(tn, true);
                      }

                      SelectNode(tnTemp, true);
                    }
                    else
                    {
                      TreeNode tnTemp = SelNode;

                      foreach (TreeNode tn in Nodes)
                      {
                        SelectNode(tn, true);
                      }

                      SelectNode(tnTemp, true);
                    }
                  }
                  else
                  {
                    ClearSelNodes();

                    SelectAllNodes();
                  }
                  break;

                case TreeViewMultiSelect.MultiSameBranchAndLevel:
                  if (SelNode.Parent != null)
                  {
                    foreach (TreeNode tn in SelNode.Parent.Nodes)
                    {
                      SelectNode(tn, false);
                    }
                  }
                  break;

                case TreeViewMultiSelect.MultiSameBranch:
                  if (SelNode.Parent != null)
                  {
                    if ((e.Modifiers & Keys.Shift) != 0 && (e.Modifiers & Keys.Alt) != 0)
                    {
                      SelectBranch(SelNode, true, true, false);

                      SelectAllParentNodes(SelNode, false);
                    }
                    else if ((e.Modifiers & Keys.Shift) != 0 || (e.Modifiers & Keys.Alt) != 0)
                    {
                      if ((e.Modifiers & Keys.Shift) != 0)
                      {
                        foreach (TreeNode tn in SelNode.Parent.Nodes)
                        {
                          SelectNode(tn, false);
                        }
                      }
                      else if ((e.Modifiers & Keys.Alt) != 0)
                      {
                        foreach (TreeNode tn in SelNode.Parent.Nodes)
                        {
                          SelectBranch(tn, true, true, false);
                        }
                      }
                    }
                    else
                    {
                      SelectBranch(GetTreeNodeGrandParent(SelNode), true, true, false);
                    }
                  }
                  else
                  {
                    if ((e.Modifiers & Keys.Shift) == 0 || ((e.Modifiers & Keys.Shift) != 0 && (e.Modifiers & Keys.Alt) != 0))
                    {
                      SelectBranch(SelNode, true, true, false);
                    }
                  }
                  break;

                case TreeViewMultiSelect.MultiSameLevel:
                  if (SelNode.Parent != null)
                  {
                    if ((e.Modifiers & Keys.Shift) != 0)
                    {
                      foreach (TreeNode tn in SelNode.Parent.Nodes)
                      {
                        SelectNode(tn, false);
                      }
                    }
                    else
                    {
                      SelectAllNodes(GetTreeNodeLevel(SelNode), false);
                    }
                  }
                  else
                  {
                    foreach (TreeNode tn in Nodes)
                    {
                      SelectNode(tn, false);
                    }
                  }
                  break;

                case TreeViewMultiSelect.MultiPathToParents:
                case TreeViewMultiSelect.MultiPathToParent:
                  if (SelNode != null)
                  {
                    SelectAllParentNodes(SelNode, false);

                    if (SelNode.Parent != null)
                    {
                      foreach (TreeNode tn in SelNode.Parent.Nodes)
                      {
                        SelectNode(tn, false);
                      }
                    }
                    else if (MultiSelect == TreeViewMultiSelect.MultiPathToParents)
                    {
                      foreach (TreeNode tn in Nodes)
                      {
                        SelectNode(tn, false);
                      }
                    }
                  }
                  else
                  {
                    if (Nodes.Count > 0)
                    {
                      SelectBranch(Nodes[0], true, true, false);
                    }
                  }
                  break;

                case TreeViewMultiSelect.SinglePathToParent:
                  break;

              }
            }

            break;

          #endregion A Key



          #region Escape Key

          case Keys.Escape:
            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if ((e.Modifiers & Keys.Shift) != 0 && SelNode != null)
              {
                if (SelNode != null)
                {
                  if (SelNode.Parent != null)
                  {
                    DeselectBranch(SelNode.Parent, true, true);
                  }
                  else
                  {
                    foreach (TreeNode tn in Nodes)
                    {
                      DeselectBranch(tn, true, true);
                    }
                  }
                }
              }
              else
              {
                SelNode = null;
                ClearSelNodes();
              }
            }

            break;

          #endregion Escape Key



          #region Q Key

          case Keys.Q:
            if ((e.Modifiers & Keys.Control) != 0)
            {
              if (MultiSelect != TreeViewMultiSelect.NoMulti)
              {
                if ((e.Modifiers & Keys.Alt) != 0 && SelNode != null)
                {
                  SelNode.Collapse();
                }
                else if ((e.Modifiers & Keys.Shift) != 0 && SelNode != null)
                {
                  SelNode.Collapse();
                }
                else
                {
                  CollapseAll();
                }
              }
              else
              {
                CollapseAll();
              }
            }

            break;

          #endregion Q Key



          #region E Key

          case Keys.E:
            if ((e.Modifiers & Keys.Control) != 0)
            {
              if (MultiSelect != TreeViewMultiSelect.NoMulti)
              {
                if ((e.Modifiers & Keys.Alt) != 0 && SelNode != null)
                {
                  SelNode.ExpandAll();
                }
                else if ((e.Modifiers & Keys.Shift) != 0 && SelNode != null)
                {
                  SelNode.Expand();
                }
                else
                {
                  ExpandAll();
                }
              }
              else
              {
                ExpandAll();
              }
            }

            break;

          #endregion E Key

        }

        if (MultiSelect != TreeViewMultiSelect.NoMulti)
        {
          EnsureSelNodeNotNull();
        }
      }

      base.OnKeyDown(e);
    }

    #endregion Keys (OnKeyDown)



    #region Mouse (OnMouseDown, OnMouseUp, OnMouseMove (& MoveRubberbandStart) & OnMouseLeave)

    #region OnMouseDown

    /// <summary>
    /// Standard OnMouseDown EventHandler.
    /// </summary>
    /// <param name="e">Standard MouseEventArgs object.</param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        if (MultiSelect != TreeViewMultiSelect.NoMulti)
        {
          SelectedNode = null;
        }

        if (!bActive)
        {
          Focus();
          ActivateSelNodes();
          bActive = true;
        }

        tnMouseDown = GetNodeAt(e.X, e.Y);
        ptMouseDown = new Point(e.X, e.Y);
        ptMouseDownOrig = new Point(e.X, e.Y);

        if (tnMouseDown == null && Nodes.Count > 0)
        {
          tnMouseDown = Nodes[Nodes.Count - 1];

          while (tnMouseDown.NextVisibleNode != null)
          {
            tnMouseDown = tnMouseDown.NextVisibleNode;
          }
        }

        TreeNode tn = GetNodeAt(e.X, e.Y);
        TreeNode tnSel = SelNode;

        if (tn != null)
        {
          if (MultiSelect != TreeViewMultiSelect.NoMulti)
          {
            HighlightNode(tn);
          }

          if (e.Button == MouseButtons.Left)
          {
            if (FullRowSelect)
            {
              tnFullRowSelect = tn;
              bFullRowSelectNodeSelected = IsTreeNodeSelected(tn);
              bFullRowSelectNodeChecked = tn.Checked;
              bFullRowSelectNodeExpanded = tn.IsExpanded;
            }
          }

          base.OnMouseDown(e);
        }
        else
        {
          bLabelEditAllowed = false;

          base.OnMouseDown(e);
        }

        if (tnSel != SelNode)
        {
          bLabelEditAllowed = false;
        }

        if (MultiSelect != TreeViewMultiSelect.NoMulti)
        {
          SelectedNode = SelNode;
        }
      }
      else
      {
        base.OnMouseDown(e);
      }
    }

    #endregion OnMouseDown



    #region OnMouseUp

    /// <summary>
    /// Standard OnMouseUp EventHandler.
    /// </summary>
    /// <param name="e">Standard MouseEventArgs object.</param>
    protected override void OnMouseUp(MouseEventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        TreeNode tn = GetNodeAt(e.X, e.Y);

        if (tn != null)
        {
          if (!IsTreeNodeSelected(tnMouseDown) || MultiSelect == TreeViewMultiSelect.NoMulti)
          {
            LowlightNode(tnMouseDown);
          }

          if (tn == SelNode && bLabelEditAllowed)
          {
            if (LabelEdit)
            {
              tn.BeginEdit();
            }
          }
        }
        else
        {
          bLabelEditAllowed = false;
        }

        if (tnFullRowSelect != null)
        {
          if (FullRowSelect &&
            bFullRowSelectNodeSelected == IsTreeNodeSelected(tnFullRowSelect) &&
            bFullRowSelectNodeChecked == tnFullRowSelect.Checked &&
            bFullRowSelectNodeExpanded == tnFullRowSelect.IsExpanded)
          {
            BeforeSelectMethod(tnFullRowSelect);
          }

          tnFullRowSelect = null;
        }
      }

      base.OnMouseUp(e);
    }

    #endregion OnMouseUp



    #region OnMouseMove & MoveRubberbandStart

    /// <summary>
    /// Standard OnMouseMove EventHandler.
    /// </summary>
    /// <param name="e">Standard MouseEventArgs object.</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        int iMouseDownTreeNodeLevel = GetTreeNodeLevel(tnMouseDown);

        bool bStartRubberbandPainting = Math.Abs(ptMouseDown.Y - e.Y) > ItemHeight;

        TreeNode tn = GetNodeAt(e.X, e.Y);

        if (MultiSelect != TreeViewMultiSelect.NoMulti && HotTracking && IsTreeNodeSelected(tn))
        {
          bPaintFocusRectAndHottracking = true;
          SelectedNode = tn;
          bPaintFocusRectAndHottracking = false;
        }

        if (AllowRubberbandSelect)
        {
          if (e.Button == MouseButtons.Left &&
            MultiSelect != TreeViewMultiSelect.NoMulti &&
            (bStartRubberbandPainting || bRubberbandHasBeenPainted || (tn != tnMouseDown && tn != null)) &&
            IsMouseMoveSelect)
          {
            if (tnMouseDown != null && tn != null)
            {
              TreeNode tnTemp = tnMouseDown;

              if (tnTemp != tn)
              {
                bool bPrevious = false;

                while (tnTemp != null && tnTemp.PrevVisibleNode != null && tnTemp != tn)
                {
                  if (tnTemp.PrevVisibleNode == tn)
                  {
                    bPrevious = true;
                    break;
                  }

                  tnTemp = tnTemp.PrevVisibleNode;
                }

                tnTemp = tnMouseDown;

                if ((ModifierKeys & Keys.Control) == 0)
                {
                  if (bPrevious)
                  {
                    ClearSelNodes(tn, tnMouseDown);
                  }
                  else
                  {
                    ClearSelNodes(tnMouseDown, tn);
                  }
                }

                if ((ModifierKeys & Keys.Alt) != 0 &&
                  MultiSelect != TreeViewMultiSelect.MultiSameLevel &&
                  MultiSelect != TreeViewMultiSelect.MultiPathToParent &&
                  MultiSelect != TreeViewMultiSelect.MultiPathToParents &&
                  MultiSelect != TreeViewMultiSelect.SinglePathToParent &&
                  MultiSelect != TreeViewMultiSelect.SinglePathToParents)
                {
                  SelectBranch(tnMouseDown, true, true, true);
                }
                else
                {
                  SelectNode(tnMouseDown, true);
                }

                if (bPrevious)
                {
                  while (tnTemp.PrevVisibleNode != null && tnTemp != tn)
                  {
                    if (MultiSelect == TreeViewMultiSelect.MultiSameLevel)
                    {
                      if (iMouseDownTreeNodeLevel == GetTreeNodeLevel(tnTemp.PrevVisibleNode))
                      {
                        SelectNode(tnTemp.PrevVisibleNode, true);
                      }
                    }
                    else if (MultiSelect == TreeViewMultiSelect.MultiPathToParent ||
                      MultiSelect == TreeViewMultiSelect.MultiPathToParents ||
                      MultiSelect == TreeViewMultiSelect.SinglePathToParent ||
                      MultiSelect == TreeViewMultiSelect.SinglePathToParents)
                    {
                      SelectNode(tnTemp.PrevVisibleNode, true);
                    }
                    else
                    {
                      if ((ModifierKeys & Keys.Alt) != 0)
                      {
                        SelectBranch(tnTemp.PrevVisibleNode, true, false, true);
                      }
                      else
                      {
                        SelectNode(tnTemp.PrevVisibleNode, true);
                      }
                    }

                    tnTemp = tnTemp.PrevVisibleNode;
                  }
                }
                else
                {
                  while (tnTemp.NextVisibleNode != null && tnTemp != tn)
                  {
                    if (MultiSelect == TreeViewMultiSelect.MultiSameLevel)
                    {
                      if (iMouseDownTreeNodeLevel == GetTreeNodeLevel(tnTemp.NextVisibleNode))
                      {
                        SelectNode(tnTemp.NextVisibleNode, true);
                      }
                    }
                    else if (MultiSelect == TreeViewMultiSelect.MultiPathToParent ||
                      MultiSelect == TreeViewMultiSelect.MultiPathToParents ||
                      MultiSelect == TreeViewMultiSelect.SinglePathToParent ||
                      MultiSelect == TreeViewMultiSelect.SinglePathToParents)
                    {
                      SelectNode(tnTemp.NextVisibleNode, true);
                    }
                    else
                    {
                      if ((ModifierKeys & Keys.Alt) != 0)
                      {
                        SelectBranch(tnTemp.NextVisibleNode, true, true, true);
                      }
                      else
                      {
                        SelectNode(tnTemp.NextVisibleNode, true);
                      }
                    }

                    tnTemp = tnTemp.NextVisibleNode;
                  }
                }
              }
              else
              {
                if ((ModifierKeys & Keys.Control) == 0)
                {
                  ClearSelNodes();
                }
              }

              if (MultiSelect == TreeViewMultiSelect.MultiSameLevel)
              {
                if (iMouseDownTreeNodeLevel == GetTreeNodeLevel(tn))
                {
                  SelectNode(tn, true);
                }
              }
              else if (MultiSelect == TreeViewMultiSelect.MultiPathToParent ||
                MultiSelect == TreeViewMultiSelect.MultiPathToParents ||
                MultiSelect == TreeViewMultiSelect.SinglePathToParent ||
                MultiSelect == TreeViewMultiSelect.SinglePathToParents)
              {
                SelectNode(tn, true);
              }
              else
              {
                if ((ModifierKeys & Keys.Alt) != 0)
                {
                  SelectBranch(tn, true, true, true);
                }
                else
                {
                  SelectNode(tn, true);
                }
              }
            }

            MoveRubberbandStart();

            Point ptMouseMoveClient = PointToClient(ptMouseMoveScreen);

            //The next line is not used because if the Control is scrolled horizontally while the rubberband is painted it is not cleared properly.
            //Rectangle rctInvalidate1 = new Rectangle(ptMouseDown.X, ptMouseDown.Y, ptMouseMoveClient.X - ptMouseDown.X, ptMouseMoveClient.Y - ptMouseDown.Y);
            Rectangle rctInvalidate1 = new Rectangle(ClientRectangle.Left, ptMouseDown.Y, ClientRectangle.Width, ptMouseMoveClient.Y - ptMouseDown.Y);

            ptMouseDownScreen = PointToScreen(ptMouseDown);
            ptMouseMoveScreen = PointToScreen(new Point(e.X, e.Y));

            Rectangle rctSelection = new Rectangle(ptMouseDownScreen.X, ptMouseDownScreen.Y, ptMouseMoveScreen.X - ptMouseDownScreen.X, ptMouseMoveScreen.Y - ptMouseDownScreen.Y);

            //The next line is not used because if the Control is scrolled horizontally while the rubberband is painted it is not cleared properly.
            //Rectangle rctInvalidate2 = new Rectangle(ptMouseDown.X, ptMouseDown.Y, e.X - ptMouseDown.X, e.Y - ptMouseDown.Y);
            Rectangle rctInvalidate2 = new Rectangle(ClientRectangle.Left, ptMouseDown.Y, ClientRectangle.Width, e.Y - ptMouseDown.Y);

            //Two Rectangles need to be Invalidated because the new Rectangle could be bigger than the old one or smaller.
            Invalidate(rctInvalidate1, false);
            Invalidate(rctInvalidate2, false);
            Update();

            ControlPaint.DrawReversibleFrame(rctSelection, BackColor, FrameStyle.Dashed);
            bRubberbandHasBeenPainted = true;

          }
          else if (e.Button == MouseButtons.Left && FullRowSelect)
          {
            if ((ModifierKeys & Keys.Control) == 0)
            {
              ClearSelNodes(tnMouseDown);
            }

            if (MultiSelect == TreeViewMultiSelect.MultiSameLevel)
            {
              if (iMouseDownTreeNodeLevel == GetTreeNodeLevel(tnMouseDown))
              {
                SelectNode(tnMouseDown, true);
              }
            }
            else if (MultiSelect == TreeViewMultiSelect.MultiPathToParent ||
              MultiSelect == TreeViewMultiSelect.MultiPathToParents ||
              MultiSelect == TreeViewMultiSelect.SinglePathToParent ||
              MultiSelect == TreeViewMultiSelect.SinglePathToParents)
            {
              SelectNode(tnMouseDown, true);
            }
            else
            {
              if ((ModifierKeys & Keys.Alt) != 0)
              {
                SelectBranch(tnMouseDown, true, true, true);
              }
              else
              {
                SelectNode(tnMouseDown, true);
              }
            }
          }
          else if (bRubberbandHasBeenPainted)
          {
            ControlPaint.DrawReversibleFrame(new Rectangle(ptMouseDownScreen.X, ptMouseDownScreen.Y, ptMouseMoveScreen.X - ptMouseDownScreen.X, ptMouseMoveScreen.Y - ptMouseDownScreen.Y), BackColor, FrameStyle.Dashed);
            bRubberbandHasBeenPainted = false;
          }
        }
        else if (bRubberbandHasBeenPainted)
        {
          ControlPaint.DrawReversibleFrame(new Rectangle(ptMouseDownScreen.X, ptMouseDownScreen.Y, ptMouseMoveScreen.X - ptMouseDownScreen.X, ptMouseMoveScreen.Y - ptMouseDownScreen.Y), BackColor, FrameStyle.Dashed);
          bRubberbandHasBeenPainted = false;
        }
      }

      base.OnMouseMove(e);
    }

    /// <summary>
    /// If the Control is scrolled (up or down) while the rubberband is being painted the starting point (vertically) of the rubberband has
    ///		to stay on the same TreeNode, not the same Y-coordinate.
    ///	Try this by dragging a selection so that the rubberband becomes visible and then without releasing the left mouse button scroll up
    ///		or down using the (vertical) mouse wheel.
    /// </summary>
    private void MoveRubberbandStart()
    {
      CreateGraphics();

      ptMouseDown = ptMouseDownOrig;
      TreeNode tnOldMouseDown = GetNodeAt(ptMouseDown);

      bool bPrev = false;
      int iY = 0;

      if (tnOldMouseDown != tnMouseDown)
      {
        while (tnOldMouseDown != null && tnOldMouseDown.PrevVisibleNode != null)
        {
          iY += ItemHeight;

          if (tnOldMouseDown.PrevVisibleNode == tnMouseDown)
          {
            bPrev = true;
            ptMouseDown = new Point(ptMouseDown.X, Math.Max(ptMouseDown.Y - iY, 0));
            break;
          }

          tnOldMouseDown = tnOldMouseDown.PrevVisibleNode;
        }

        if (!bPrev)
        {
          tnOldMouseDown = GetNodeAt(ptMouseDown);
          iY = 0;

          while (tnOldMouseDown != null && tnOldMouseDown.NextVisibleNode != null)
          {
            iY += ItemHeight;

            if (tnOldMouseDown.NextVisibleNode == tnMouseDown)
            {
              ptMouseDown = new Point(ptMouseDown.X, Math.Min(ptMouseDown.Y + iY, ClientRectangle.Height));
              break;
            }

            tnOldMouseDown = tnOldMouseDown.NextVisibleNode;
          }
        }
      }
    }

    #endregion OnMouseMove & MoveRubberbandStart



    #region OnMouseLeave

    /// <summary>
    /// Standard OnMouseLeave EventHandler.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    protected override void OnMouseLeave(EventArgs e)
    {
      if (MultiSelect != TreeViewMultiSelect.Classic)
      {
        if (bRubberbandHasBeenPainted)
        {
          ControlPaint.DrawReversibleFrame(new Rectangle(ptMouseDownScreen.X, ptMouseDownScreen.Y, ptMouseMoveScreen.X - ptMouseDownScreen.X, ptMouseMoveScreen.Y - ptMouseDownScreen.Y), BackColor, FrameStyle.Dashed);
          bRubberbandHasBeenPainted = false;
        }
      }

      base.OnMouseLeave(e);
    }

    #endregion OnMouseLeave

    #endregion Mouse (OnMouseDown, OnMouseUp, OnMouseMove (& MoveRubberbandStart) & OnMouseLeave)

    #endregion Overridden EventHandlers



    #region Properties and their EventHandlers

    #region MultiSelect

    /// <summary>
    /// Decides the multi select characteristics of an MWTreeView Control.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Decides the multi select characteristics of an MWTreeView Control."),
    DefaultValue(TreeViewMultiSelect.Multi)
    ]
    public TreeViewMultiSelect MultiSelect
    {
      get
      {
        return tvmsMultiSelect;
      }
      set
      {
        if (tvmsMultiSelect != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(tvmsMultiSelect, value);
          OnBeforeMultiSelectChanged(e);

          if (!e.Cancel)
          {
            if (value == TreeViewMultiSelect.Classic)
            {
              tvmsMultiSelect = value;

              SelectedNode = SelNode;

              SelNodeInt = null;

              ClearSelNodes();
            }
            else if (value == TreeViewMultiSelect.NoMulti)
            {
              tvmsMultiSelect = value;

              SelectedNode = SelNode;

              SelNodeInt = null;

              ClearSelNodes();
            }
            else if (tvmsMultiSelect == TreeViewMultiSelect.NoMulti)
            {
              tvmsMultiSelect = value;

              SelectNode(SelectedNode, true);

              SelectedNode = null;
            }
            else if (value == TreeViewMultiSelect.MultiSameBranchAndLevel && value != tvmsMultiSelect)
            {
              tvmsMultiSelect = value;

              TreeNode tnGrandParent = GetTreeNodeGrandParent(SelNode);
              int iLevel = GetTreeNodeLevel(SelNode);

              Hashtable ht = new Hashtable();

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                if (tnGrandParent == GetTreeNodeGrandParent(mwtnw.Node) && iLevel == GetTreeNodeLevel(mwtnw.Node))
                {
                  ht.Add(mwtnw.Node.GetHashCode(), mwtnw);
                }
              }

              SelNodes = ht;
            }
            else if (value == TreeViewMultiSelect.MultiSameBranch && value != tvmsMultiSelect)
            {
              tvmsMultiSelect = value;

              TreeNode tnGrandParent = GetTreeNodeGrandParent(SelNode);

              Hashtable ht = new Hashtable();

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                if (tnGrandParent == GetTreeNodeGrandParent(mwtnw.Node))
                {
                  ht.Add(mwtnw.Node.GetHashCode(), mwtnw);
                }
              }

              SelNodes = ht;
            }
            else if (value == TreeViewMultiSelect.MultiSameLevel && value != tvmsMultiSelect)
            {
              tvmsMultiSelect = value;

              int iLevel = GetTreeNodeLevel(SelNode);

              Hashtable ht = new Hashtable();

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                if (iLevel == GetTreeNodeLevel(mwtnw.Node))
                {
                  ht.Add(mwtnw.Node.GetHashCode(), mwtnw);
                }
              }

              SelNodes = ht;
            }
            else if (value == TreeViewMultiSelect.MultiPathToParents && value != tvmsMultiSelect ||
              value == TreeViewMultiSelect.SinglePathToParents && value != tvmsMultiSelect)
            {
              tvmsMultiSelect = value;

              ArrayList alTreeNodeGrandParent = new ArrayList();
              ArrayList alTreeNodeLevel = new ArrayList();

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                TreeNode tnGrandParent = GetTreeNodeGrandParent(mwtnw.Node);

                int iIndex = alTreeNodeGrandParent.IndexOf(tnGrandParent);

                if (iIndex == -1)
                {
                  alTreeNodeGrandParent.Add(tnGrandParent);
                  alTreeNodeLevel.Add(GetTreeNodeLevel(mwtnw.Node));
                }
                else
                {
                  alTreeNodeLevel[iIndex] = Math.Max((int)alTreeNodeLevel[iIndex], GetTreeNodeLevel(mwtnw.Node));
                }
              }

              Hashtable ht = new Hashtable();

              for (int i = 0; i < alTreeNodeGrandParent.Count; i++)
              {
                foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
                {
                  if ((int)alTreeNodeLevel[i] == GetTreeNodeLevel(mwtnw.Node) &&
                    alTreeNodeGrandParent[i] == GetTreeNodeGrandParent(mwtnw.Node) &&
                    !ht.Contains(mwtnw.Node.GetHashCode()))
                  {
                    ht.Add(mwtnw.Node.GetHashCode(), mwtnw);

                    if (value == TreeViewMultiSelect.SinglePathToParents)
                    {
                      break;
                    }
                  }
                }
              }

              SelNodes = ht;

              ArrayList al = new ArrayList();

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                al.Add(mwtnw.Node);
              }

              for (int i = 0; i < al.Count; i++)
              {
                SelectNode(al[i] as TreeNode, false);
              }
            }
            else if (value == TreeViewMultiSelect.MultiPathToParent && value != tvmsMultiSelect)
            {
              tvmsMultiSelect = value;

              TreeNode tnGrandParent = GetTreeNodeGrandParent(SelNode);

              int iSelNodeLevel = GetTreeNodeLevel(SelNode);

              Hashtable ht = new Hashtable();

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                if (tnGrandParent == GetTreeNodeGrandParent(mwtnw.Node) && iSelNodeLevel == GetTreeNodeLevel(mwtnw.Node))
                {
                  ht.Add(mwtnw.Node.GetHashCode(), mwtnw);
                }
              }

              SelNodes = ht;

              ArrayList al = new ArrayList();

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                al.Add(mwtnw.Node);
              }

              for (int i = 0; i < al.Count; i++)
              {
                SelectNode(al[i] as TreeNode, true);
              }
            }
            else if (value == TreeViewMultiSelect.SinglePathToParent && value != tvmsMultiSelect)
            {
              tvmsMultiSelect = value;

              TreeNode tn = SelNode;

              ClearSelNodes();

              SelectNode(tn, true);
            }
            else
            {
              tvmsMultiSelect = value;
            }

            if (!IsTreeNodeSelected(SelNode))
            {
              SelNode = null;

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                SelectNode(mwtnw.Node, true);
                break;
              }
            }

            OnAfterMultiSelectChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the MultiSelect property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the MultiSelect property changes.")
    ]
    public event EventHandler BeforeMultiSelectChanged;

    /// <summary>
    /// Raises the BeforeMultiSelectChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeMultiSelectChanged(MWCancelEventArgs e)
    {
      if (BeforeMultiSelectChanged != null)
      {
        BeforeMultiSelectChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the MultiSelect property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the MultiSelect property has changed.")
    ]
    public event EventHandler AfterMultiSelectChanged;

    /// <summary>
    /// Raises the AfterMultiSelectChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterMultiSelectChanged(EventArgs e)
    {
      if (AfterMultiSelectChanged != null)
      {
        AfterMultiSelectChanged(this, e);
      }
    }

    #endregion MultiSelect



    #region AllowMultiCheck

    /// <summary>
    /// True if multiple TreeNodes can be checked at once or false otherwise (true is standard for MS TreeView).
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("True if multiple TreeNodes can be checked at once or false otherwise (true is standard for MS TreeView)."),
    DefaultValue(true)
    ]
    public bool AllowMultiCheck
    {
      get
      {
        return bAllowMultiCheck;
      }
      set
      {
        if (bAllowMultiCheck != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(bAllowMultiCheck, value);
          OnBeforeAllowMultiCheckChanged(e);

          if (!e.Cancel)
          {
            if (!value)
            {
              bAllowMultiCheckChanged = true;

              TreeNode tn = null;

              if (CheckedNodes != null)
              {
                foreach (TreeNode tn2 in CheckedNodes.Values)
                {
                  tn = tn2;
                  break;
                }
              }

              ClearCheckedNodes();

              if (tn != null)
              {
                CheckNode(tn, true);
              }

              bAllowMultiCheckChanged = false;
            }

            bAllowMultiCheck = value;

            OnAfterAllowMultiCheckChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the AllowMultiCheck property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the AllowMultiCheck property changes.")
    ]
    public event EventHandler BeforeAllowMultiCheckChanged;

    /// <summary>
    /// Raises the BeforeAllowMultiCheckChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeAllowMultiCheckChanged(MWCancelEventArgs e)
    {
      if (BeforeAllowMultiCheckChanged != null)
      {
        BeforeAllowMultiCheckChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the AllowMultiCheck property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the AllowMultiCheck property has changed.")
    ]
    public event EventHandler AfterAllowMultiCheckChanged;

    /// <summary>
    /// Raises the AfterAllowMultiCheckChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterAllowMultiCheckChanged(EventArgs e)
    {
      if (AfterAllowMultiCheckChanged != null)
      {
        AfterAllowMultiCheckChanged(this, e);
      }
    }

    #endregion AllowMultiCheck



    #region AllowNoSelNode

    /// <summary>
    /// True if no TreeNode has to be selected or false otherwise (false is standard for MS TreeView).
    /// Note that if using a MultiSelect of TreeViewMultiSelect.NoMulti this property is ignored.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("True if no TreeNode has to be selected or false otherwise (false is standard for MS TreeView).\nNote that if using a MultiSelect of TreeViewMultiSelect.NoMulti this property is ignored."),
    DefaultValue(true)
    ]
    public bool AllowNoSelNode
    {
      get
      {
        return bAllowNoSelNode;
      }
      set
      {
        if (bAllowNoSelNode != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(bAllowNoSelNode, value);
          OnBeforeAllowNoSelNodeChanged(e);

          if (!e.Cancel)
          {
            bAllowNoSelNode = value;

            if (MultiSelect != TreeViewMultiSelect.NoMulti)
            {
              if (!bAllowNoSelNode)
              {
                bActive = false;
                EnsureSelNodeNotNull();
              }
            }

            OnAfterAllowNoSelNodeChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the AllowNoSelNode property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the AllowNoSelNode property changes.")
    ]
    public event EventHandler BeforeAllowNoSelNodeChanged;

    /// <summary>
    /// Raises the BeforeAllowNoSelNodeChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeAllowNoSelNodeChanged(MWCancelEventArgs e)
    {
      if (BeforeAllowNoSelNodeChanged != null)
      {
        BeforeAllowNoSelNodeChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the AllowNoSelNode property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the AllowNoSelNode property has changed.")
    ]
    public event EventHandler AfterAllowNoSelNodeChanged;

    /// <summary>
    /// Raises the AfterAllowNoSelNodeChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterAllowNoSelNodeChanged(EventArgs e)
    {
      if (AfterAllowNoSelNodeChanged != null)
      {
        AfterAllowNoSelNodeChanged(this, e);
      }
    }

    #endregion AllowNoSelNode



    #region AllowBlankNodeText

    /// <summary>
    /// True if TreeNodes can be blank, i.e. contain no Text.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("True if TreeNodes can be blank, i.e. contain no Text."),
    DefaultValue(false)
    ]
    public bool AllowBlankNodeText
    {
      get
      {
        return bAllowBlankNodeText;
      }
      set
      {
        if (bAllowBlankNodeText != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(bAllowBlankNodeText, value);
          OnBeforeAllowBlankNodeTextChanged(e);

          if (!e.Cancel)
          {
            bAllowBlankNodeText = value;

            OnAfterAllowBlankNodeTextChanged(new EventArgs());
          }
        }
      }
    }


    /// <summary>
    /// Occurs before the AllowBlankNodeText property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the AllowBlankNodeText property changes.")
    ]
    public event EventHandler BeforeAllowBlankNodeTextChanged;

    /// <summary>
    /// Raises the BeforeAllowBlankNodeTextChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeAllowBlankNodeTextChanged(MWCancelEventArgs e)
    {
      if (BeforeAllowBlankNodeTextChanged != null)
      {
        BeforeAllowBlankNodeTextChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the AllowBlankNodeText property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the AllowBlankNodeText property has changed.")
    ]
    public event EventHandler AfterAllowBlankNodeTextChanged;

    /// <summary>
    /// Raises the AfterAllowBlankNodeTextChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterAllowBlankNodeTextChanged(EventArgs e)
    {
      if (AfterAllowBlankNodeTextChanged != null)
      {
        AfterAllowBlankNodeTextChanged(this, e);
      }
    }

    #endregion AllowBlankNodeText



    #region SelNodes

    /// <summary>
    /// HashTable containing the Selected TreeNodes wrapped in MWTreeNodeWrapper objects as values and the TreeNode.GetHashCodes as keys.
    /// </summary>
    [
    Browsable(false),
    Category("Behavior"),
    Description("HashTable containing the Selected TreeNodes wrapped in MWTreeNodeWrapper objects as values and the TreeNode.GetHashCodes as keys."),
    DefaultValue(null)
    ]
    public Hashtable SelNodes
    {
      get
      {
        return htSelNodes;
      }
      set
      {
        if (htSelNodes != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(htSelNodes, value);
          OnBeforeSelNodesChanged(e);

          if (!e.Cancel)
          {
            if (value == null)
            {
              SelNode = null;

              if (SelNodes == null)
              {
                htSelNodes = new Hashtable();
              }

              if (SelNodes != null)
                foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
                {
                  MWTreeNodeWrapper.Deselect(mwtnw);
                  LowlightNode(mwtnw.Node);
                }

              htSelNodes.Clear();
            }
            else
            {
              if (SelNodes == null)
              {
                htSelNodes = new Hashtable();
              }

              foreach (MWTreeNodeWrapper mwtnw in htSelNodes.Values)
              {
                if (!value.Contains(mwtnw.Node.GetHashCode()))
                {
                  mwtnw.Reset();
                  LowlightNode(mwtnw.Node);
                }
              }
            }

            htSelNodes = new Hashtable();

            if (value != null)
            {
              foreach (MWTreeNodeWrapper mwtnw in value.Values)
              {
                mwtnw.Reset();
                htSelNodes.Add(mwtnw.Node.GetHashCode(), new MWTreeNodeWrapper(mwtnw.Node));
              }
            }

            if (value != null)
              if (SelNode != null && !value.Contains(SelNode.GetHashCode()))
              {
                SelNode = null;
              }

            ActivateOrDeactivateSelNodes();

            EnsureAllSelectedNodesAreAllowed();

            OnAfterSelNodesChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the SelNodes property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the SelNodes property changes.")
    ]
    public event EventHandler BeforeSelNodesChanged;

    /// <summary>
    /// Raises the BeforeSelNodesChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeSelNodesChanged(MWCancelEventArgs e)
    {
      if (BeforeSelNodesChanged != null)
      {
        BeforeSelNodesChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the SelNodes property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the SelNodes property has changed.")
    ]
    public event EventHandler AfterSelNodesChanged;

    /// <summary>
    /// Raises the AfterSelNodesChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterSelNodesChanged(EventArgs e)
    {
      if (AfterSelNodesChanged != null)
      {
        AfterSelNodesChanged(this, e);
      }
    }

    #endregion SelNodes



    #region SelNode (See SelNodeInt for EventHandlers)

    /// <summary>
    /// Last Selected TreeNode or null if no TreeNode is selected or if Last Selected TreeNode was deselected.
    /// </summary>
    [
    Browsable(false),
    Category("Behavior"),
    Description("Last Selected TreeNode or null if no TreeNode is selected or if Last Selected TreeNode was deselected."),
    DefaultValue(null)
    ]
    public TreeNode SelNode
    {
      get
      {
        return tnSelNode;
      }
      set
      {
        if (value != null)
        {
          SelectNode(tnSelNode, true);
        }
        else
        {
          DeselectNode(tnSelNode, true);
        }
      }
    }

    #endregion SelNode (See SelNodeInt for EventHandlers)



    #region SelNodeInt

    /// <summary>
    /// Selected TreeNode or null if no TreeNode is selected.
    /// Note that this is an internal property only. EventHandlers are attached to this property though because in order to change the
    ///		tnSelNode variable this property has to do it.
    /// </summary>
    [
    Browsable(false),
    Category("Behavior"),
    Description("Last Selected TreeNode or null if no TreeNode is selected or if Last Selected TreeNode was deselected.\nFor internal use only."),
    DefaultValue(null)
    ]
    private TreeNode SelNodeInt
    {
      get
      {
        return tnSelNode;
      }
      set
      {
        if (tnSelNode != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(tnSelNode, value);
          OnBeforeSelNodeChanged(e);

          if (!e.Cancel)
          {
            if (tnSelNode == value && tnSelNode != null)
            {
              bLabelEditAllowed = true;
            }
            else
            {
              bLabelEditAllowed = false;
            }

            if (value == null || IsSelectNodeRegExSatisfied(value.Text))
            {
              tnSelNode = value;

              if ((MouseButtons & MouseButtons.Left) == 0 &&
                MultiSelect != TreeViewMultiSelect.NoMulti &&
                MultiSelect != TreeViewMultiSelect.Classic)
              {
                bPaintFocusRectAndHottracking = true;
                SelectedNode = value;
                bPaintFocusRectAndHottracking = false;
              }
            }

            OnAfterSelNodeChanged(new EventArgs());
          }
        }
        else if (value == null || IsSelectNodeRegExSatisfied(value.Text))
        {
          if ((MouseButtons & MouseButtons.Left) == 0 &&
            MultiSelect != TreeViewMultiSelect.NoMulti &&
            MultiSelect != TreeViewMultiSelect.Classic)
          {
            bPaintFocusRectAndHottracking = true;
            SelectedNode = value;
            bPaintFocusRectAndHottracking = false;
          }

          bLabelEditAllowed = true;
        }
      }
    }

    /// <summary>
    /// Occurs before the SelNode property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the SelNode property changes.")
    ]
    public event EventHandler BeforeSelNodeChanged;

    /// <summary>
    /// Raises the BeforeSelNodeChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeSelNodeChanged(MWCancelEventArgs e)
    {
      if (BeforeSelNodeChanged != null)
      {
        BeforeSelNodeChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the SelNode property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the SelNode property has changed.")
    ]
    public event EventHandler AfterSelNodeChanged;

    /// <summary>
    /// Raises the AfterSelNodeChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterSelNodeChanged(EventArgs e)
    {
      if (AfterSelNodeChanged != null)
      {
        AfterSelNodeChanged(this, e);
      }
    }

    #endregion SelNodeInt



    #region CheckedNodes

    /// <summary>
    /// HashTable containing the Checked TreeNodes as values and the TreeNode.GetHashCodes as keys.
    /// </summary>
    [
    Browsable(false),
    Category("Behavior"),
    Description("HashTable containing the Checked TreeNodes as values and the TreeNode.GetHashCodes as keys."),
    DefaultValue(null)
    ]
    public Hashtable CheckedNodes
    {
      get
      {
        return htCheckedNodes;
      }
      set
      {
        if (htCheckedNodes != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(htCheckedNodes, value);
          OnBeforeCheckedNodesChanged(e);

          if (!e.Cancel)
          {
            if (value == null)
            {
              if (CheckedNodes == null)
              {
                htCheckedNodes = new Hashtable();
              }

              if (CheckedNodes != null)
                foreach (TreeNode tn in CheckedNodes.Values)
                {
                  bForceCheckNode = true;
                  tn.Checked = false;
                  bForceCheckNode = false;
                }

              htCheckedNodes.Clear();
            }
            else
            {
              if (CheckedNodes == null)
              {
                htCheckedNodes = new Hashtable();
              }

              if (CheckedNodes != null)
                foreach (TreeNode tn in CheckedNodes.Values)
                {
                  bForceCheckNode = true;
                  tn.Checked = false;
                  bForceCheckNode = false;
                }

              foreach (TreeNode tn in value.Values)
              {
                bForceCheckNode = true;
                tn.Checked = true;
                bForceCheckNode = false;
              }
            }

            htCheckedNodes = value;

            EnsureAllCheckedNodesAreAllowed();

            OnAfterCheckedNodesChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the CheckedNodes property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the CheckedNodes property changes.")
    ]
    public event EventHandler BeforeCheckedNodesChanged;

    /// <summary>
    /// Raises the BeforeCheckedNodesChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeCheckedNodesChanged(MWCancelEventArgs e)
    {
      if (BeforeCheckedNodesChanged != null)
      {
        BeforeCheckedNodesChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the CheckedNodes property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the CheckedNodes property has changed.")
    ]
    public event EventHandler AfterCheckedNodesChanged;

    /// <summary>
    /// Raises the AfterCheckedNodesChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterCheckedNodesChanged(EventArgs e)
    {
      if (AfterCheckedNodesChanged != null)
      {
        AfterCheckedNodesChanged(this, e);
      }
    }

    #endregion CheckedNodes



    #region ScrollToSelNode

    /// <summary>
    /// True if scrolling is done so the SelNode (Last Selected Tree Node) is always displayed or false otherwise.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("True if scrolling is done so the SelNode (Last Selected Tree Node) is always displayed or false otherwise."),
    DefaultValue(true)
    ]
    public bool ScrollToSelNode
    {
      get
      {
        return bScrollToSelNode;
      }
      set
      {
        if (bScrollToSelNode != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(bScrollToSelNode, value);
          OnBeforeScrollToSelNodeChanged(e);

          if (!e.Cancel)
          {
            if (bScrollToSelNode != value && value)
            {
              if (MultiSelect == TreeViewMultiSelect.NoMulti)
              {
                if (SelectedNode != null)
                {
                  SelectedNode.EnsureVisible();
                }
              }
              else
              {
                if (SelNode != null)
                {
                  SelNode.EnsureVisible();
                }
              }
            }

            bScrollToSelNode = value;

            OnAfterScrollToSelNodeChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the ScrollToSelNode property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the ScrollToSelNode property changes.")
    ]
    public event EventHandler BeforeScrollToSelNodeChanged;

    /// <summary>
    /// Raises the BeforeScrollToSelNodeChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeScrollToSelNodeChanged(MWCancelEventArgs e)
    {
      if (BeforeScrollToSelNodeChanged != null)
      {
        BeforeScrollToSelNodeChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the ScrollToSelNode property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the ScrollToSelNode property has changed.")
    ]
    public event EventHandler AfterScrollToSelNodeChanged;

    /// <summary>
    /// Raises the AfterScrollToSelNodeChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterScrollToSelNodeChanged(EventArgs e)
    {
      if (AfterScrollToSelNodeChanged != null)
      {
        AfterScrollToSelNodeChanged(this, e);
      }
    }

    #endregion ScrollToSelNode



    #region AllowRubberbandSelect

    /// <summary>
    /// True if TreeNodes can be selected by a rubberband method or false otherwise.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("True if TreeNodes can be selected by a rubberband method or false otherwise."),
    DefaultValue(true)
    ]
    public bool AllowRubberbandSelect
    {
      get
      {
        return bAllowRubberbandSelect;
      }
      set
      {
        if (bAllowRubberbandSelect != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(bAllowRubberbandSelect, value);
          OnBeforeAllowRubberbandSelectChanged(e);

          if (!e.Cancel)
          {
            bAllowRubberbandSelect = value;

            OnAfterAllowRubberbandSelectChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the AllowRubberbandSelect property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the AllowRubberbandSelect property changes.")
    ]
    public event EventHandler BeforeAllowRubberbandSelectChanged;

    /// <summary>
    /// Raises the BeforeAllowRubberbandSelectChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeAllowRubberbandSelectChanged(MWCancelEventArgs e)
    {
      if (BeforeAllowRubberbandSelectChanged != null)
      {
        BeforeAllowRubberbandSelectChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the AllowRubberbandSelect property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the AllowRubberbandSelect property has changed.")
    ]
    public event EventHandler AfterAllowRubberbandSelectChanged;

    /// <summary>
    /// Raises the AfterAllowRubberbandSelectChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterAllowRubberbandSelectChanged(EventArgs e)
    {
      if (AfterAllowRubberbandSelectChanged != null)
      {
        AfterAllowRubberbandSelectChanged(this, e);
      }
    }

    #endregion AllowRubberbandSelect



    #region LabelEditRegEx

    /// <summary>
    /// Regular expression that has to be satisfied before the Text of a TreeNode can be changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Regular expression that has to be satisfied before the Text of a TreeNode can be changed."),
    DefaultValue("")
    ]
    public string LabelEditRegEx
    {
      get
      {
        return strLabelEditRegEx;
      }
      set
      {
        if (strLabelEditRegEx != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(strLabelEditRegEx, value);
          OnBeforeLabelEditRegExChanged(e);

          if (!e.Cancel)
          {
            strLabelEditRegEx = value;

            OnAfterLabelEditRegExChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the LabelEditRegEx property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the LabelEditRegEx property changes.")
    ]
    public event EventHandler BeforeLabelEditRegExChanged;

    /// <summary>
    /// Raises the BeforeLabelEditRegExChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeLabelEditRegExChanged(MWCancelEventArgs e)
    {
      if (BeforeLabelEditRegExChanged != null)
      {
        BeforeLabelEditRegExChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the LabelEditRegEx property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the LabelEditRegEx property has changed.")
    ]
    public event EventHandler AfterLabelEditRegExChanged;

    /// <summary>
    /// Raises the AfterLabelEditRegExChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterLabelEditRegExChanged(EventArgs e)
    {
      if (AfterLabelEditRegExChanged != null)
      {
        AfterLabelEditRegExChanged(this, e);
      }
    }

    #endregion LabelEditRegEx



    #region DisallowLabelEditRegEx

    /// <summary>
    /// Regular expression that cannot be satisfied if the Text of a TreeNode should be able to be changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Regular expression that cannot be satisfied if the Text of a TreeNode should be able to be changed."),
    DefaultValue("")
    ]
    public string DisallowLabelEditRegEx
    {
      get
      {
        return strDisallowLabelEditRegEx;
      }
      set
      {
        if (strDisallowLabelEditRegEx != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(strDisallowLabelEditRegEx, value);
          OnBeforeDisallowLabelEditRegExChanged(e);

          if (!e.Cancel)
          {
            strDisallowLabelEditRegEx = value;

            OnAfterDisallowLabelEditRegExChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the DisallowLabelEditRegEx property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the DisallowLabelEditRegEx property changes.")
    ]
    public event EventHandler BeforeDisallowLabelEditRegExChanged;

    /// <summary>
    /// Raises the BeforeDisallowLabelEditRegExChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeDisallowLabelEditRegExChanged(MWCancelEventArgs e)
    {
      if (BeforeDisallowLabelEditRegExChanged != null)
      {
        BeforeDisallowLabelEditRegExChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the DisallowLabelEditRegEx property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the DisallowLabelEditRegEx property has changed.")
    ]
    public event EventHandler AfterDisallowLabelEditRegExChanged;

    /// <summary>
    /// Raises the AfterDisallowLabelEditRegExChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterDisallowLabelEditRegExChanged(EventArgs e)
    {
      if (AfterDisallowLabelEditRegExChanged != null)
      {
        AfterDisallowLabelEditRegExChanged(this, e);
      }
    }

    #endregion DisallowLabelEditRegEx



    #region SelectNodeRegEx

    /// <summary>
    /// Regular expression that has to be satisfied before a TreeNode can be selected.
    /// Note that the AllowNoSelNode property is ignored if this property is used.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Regular expression that has to be satisfied before a TreeNode can be selected.\nNote that the AllowNoSelNode property is ignored if this property is used."),
    DefaultValue("")
    ]
    public string SelectNodeRegEx
    {
      get
      {
        return strSelectNodeRegEx;
      }
      set
      {
        if (strSelectNodeRegEx != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(strSelectNodeRegEx, value);
          OnBeforeSelectNodeRegExChanged(e);

          if (!e.Cancel)
          {
            strSelectNodeRegEx = value;

            EnsureAllSelectedNodesAreAllowed();

            OnAfterSelectNodeRegExChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the SelectNodeRegEx property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the SelectNodeRegEx property changes.")
    ]
    public event EventHandler BeforeSelectNodeRegExChanged;

    /// <summary>
    /// Raises the BeforeSelectNodeRegExChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeSelectNodeRegExChanged(MWCancelEventArgs e)
    {
      if (BeforeSelectNodeRegExChanged != null)
      {
        BeforeSelectNodeRegExChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the SelectNodeRegEx property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the SelectNodeRegEx property has changed.")
    ]
    public event EventHandler AfterSelectNodeRegExChanged;

    /// <summary>
    /// Raises the AfterSelectNodeRegExChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterSelectNodeRegExChanged(EventArgs e)
    {
      if (AfterSelectNodeRegExChanged != null)
      {
        AfterSelectNodeRegExChanged(this, e);
      }
    }

    #endregion SelectNodeRegEx



    #region CheckNodeRegEx

    /// <summary>
    /// Regular expression that has to be satisfied before a TreeNode can be checked.
    /// Note that the AllowNoSelNode property is ignored if this property is used.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Regular expression that has to be satisfied before a TreeNode can be checked."),
    DefaultValue("")
    ]
    public string CheckNodeRegEx
    {
      get
      {
        return strCheckNodeRegEx;
      }
      set
      {
        if (strCheckNodeRegEx != value)
        {
          MWCancelEventArgs e = new MWCancelEventArgs(strCheckNodeRegEx, value);
          OnBeforeCheckNodeRegExChanged(e);

          if (!e.Cancel)
          {
            strCheckNodeRegEx = value;

            EnsureAllCheckedNodesAreAllowed();

            OnAfterCheckNodeRegExChanged(new EventArgs());
          }
        }
      }
    }

    /// <summary>
    /// Occurs before the CheckNodeRegEx property changes.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs before the CheckNodeRegEx property changes.")
    ]
    public event EventHandler BeforeCheckNodeRegExChanged;

    /// <summary>
    /// Raises the BeforeCheckNodeRegExChanged Event.
    /// </summary>
    /// <param name="e">Standard MWCancelEventArgs object.</param>
    public virtual void OnBeforeCheckNodeRegExChanged(MWCancelEventArgs e)
    {
      if (BeforeCheckNodeRegExChanged != null)
      {
        BeforeCheckNodeRegExChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs after the CheckNodeRegEx property has changed.
    /// </summary>
    [
    Browsable(true),
    Category("Behavior"),
    Description("Occurs after the CheckNodeRegEx property has changed.")
    ]
    public event EventHandler AfterCheckNodeRegExChanged;

    /// <summary>
    /// Raises the AfterCheckNodeRegExChanged Event.
    /// </summary>
    /// <param name="e">Standard EventArgs object.</param>
    public virtual void OnAfterCheckNodeRegExChanged(EventArgs e)
    {
      if (AfterCheckNodeRegExChanged != null)
      {
        AfterCheckNodeRegExChanged(this, e);
      }
    }

    #endregion CheckNodeRegEx

    #endregion Properties and their EventHandlers



    #region Help Methods

    #region IsTreeNodeSelected

    /// <summary>
    /// Checks if the TreeNode supplied is selected or not.
    /// Note that passing in null as the TreeNode will result in false being returned.
    /// </summary>
    /// <param name="tn">TreeNode that should be checked if it is selected or not.</param>
    /// <returns>True if supplied TreeNode is selected or false otherwise.</returns>
    public bool IsTreeNodeSelected(TreeNode tn)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        if (MultiSelect == TreeViewMultiSelect.NoMulti)
        {
          if (SelectedNode == tn)
          {
            bRetVal = true;
          }
        }
        else
        {
          if (SelNodes != null)
          {
            if (SelNodes.Contains(tn.GetHashCode()))
            {
              bRetVal = true;
            }
          }
        }
      }

      return bRetVal;
    }

    #endregion IsTreeNodeSelected



    #region IsAnyChildTreeNodeSelected

    /// <summary>
    /// Checks if any Child TreeNode of the TreeNode supplied is selected.
    /// </summary>
    /// <param name="tn">TreeNode to check.</param>
    /// <returns>True if any Child TreeNode of the supplied TreeNode is selected or false otherwise.</returns>
    public bool IsAnyChildTreeNodeSelected(TreeNode tn)
    {
      bool bRetVal = false;
      int iLevel = GetTreeNodeLevel(tn);
      TreeNode tnGrandParent = GetTreeNodeGrandParent(tn);

      foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
      {
        int iLvl = GetTreeNodeLevel(mwtnw.Node);
        if (mwtnw.Node != tn && GetTreeNodeGrandParent(mwtnw.Node) == tnGrandParent && GetTreeNodeLevel(mwtnw.Node) > iLevel)
        {
          bRetVal = true;
          break;
        }
      }

      return bRetVal;
    }

    #endregion IsAnyChildTreeNodeSelected



    #region IsTreeNodeChecked

    /// <summary>
    /// Checks if the TreeNode supplied is checked or not.
    /// Note that passing in null as the TreeNode will result in false being returned.
    /// </summary>
    /// <param name="tn">TreeNode that should be checked if it is checked or not.</param>
    /// <returns>True if supplied TreeNode is checked or false otherwise.</returns>
    public bool IsTreeNodeChecked(TreeNode tn)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        if (CheckedNodes != null)
        {
          if (CheckedNodes.Contains(tn.GetHashCode()))
          {
            bRetVal = true;
          }
        }
      }

      return bRetVal;
    }

    #endregion IsTreeNodeChecked



    #region EnsureSelNodeNotNull

    /// <summary>
    /// If SelNode is null a TreeNode from the SelNodes HashTable gets selected (SelNode is set to the first TreeNode returned from the
    ///		SelNodes HashTable). If the SelNodes HashTable is empty and AllowNoSelNode is false a TreeNode is selected (SelNode is set
    ///		to the first TreeNode for the TreeView if there is one).
    /// </summary>
    /// <returns>True if SelNode is not null or if it was changed or false otherwise.</returns>
    public bool EnsureSelNodeNotNull()
    {
      bool bRetVal = false;

      if (SelNode == null)
      {
        if (SelNodes != null)
        {
          if (SelNodes.Count > 0)
          {
            if (SelNode == null)
            {
              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                bRetVal = true;
                SelectNode(mwtnw.Node, true);
                break;
              }
            }
          }
          else if (Nodes.Count > 0 && !AllowNoSelNode)
          {
            bRetVal = true;
            SelectNode(Nodes[0], true);
          }
        }
      }
      else
      {
        bRetVal = true;
      }

      return bRetVal;
    }

    #endregion EnsureSelNodeNotNull



    #region HiglightNode / LowlightNode

    /// <summary>
    /// Highlights a TreeNode as active.
    /// </summary>
    /// <param name="tn">TreeNode to highlight.</param>
    public void HighlightNode(TreeNode tn)
    {
      HighlightNode(tn, bActive);
    }

    /// <summary>
    /// Highlights TreeNode supplied as active or inactive.
    /// </summary>
    /// <param name="tn">TreeNode to highlight.</param>
    /// <param name="bTreeViewActive">True if TreeNode should be highlighted as active or false if it should be highlighted as inactive.</param>
    public void HighlightNode(TreeNode tn, bool bTreeViewActive)
    {
      if (bTreeViewActive)
      {
        if (tn.BackColor != SystemColors.Highlight)
        {
          tn.BackColor = SystemColors.Highlight;
        }

        if (tn.ForeColor != SystemColors.HighlightText)
        {
          tn.ForeColor = SystemColors.HighlightText;
        }
      }
      else
      {
        if (tn.BackColor != SystemColors.Control)
        {
          tn.BackColor = SystemColors.Control;
        }

        if (tn.ForeColor != ForeColor)
        {
          tn.ForeColor = ForeColor;
        }
      }
    }

    /// <summary>
    /// Removes highlight from supplied TreeNode: 'Lowlight'.
    /// </summary>
    /// <param name="tn">TreeNode to remove highlight from.</param>
    public void LowlightNode(TreeNode tn)
    {
      if (tn.BackColor != BackColor)
      {
        tn.BackColor = BackColor;
      }

      if (tn.ForeColor != ForeColor)
      {
        tn.ForeColor = ForeColor;
      }
    }

    #endregion HiglightNode / LowlightNode



    #region SelectNode / DeselectNode / ToggleNode

    /// <summary>
    /// Select TreeNode supplied.
    /// Note that all selected TreeNodes can be found in the SelNodes property and the most recently selected TreeNode can be found in the SelNode property.
    /// </summary>
    /// <param name="tn">TreeNode to select.</param>
    /// <param name="bChangeSelNode">True if the SelNode property should be changed when selecting the TreeNode.</param>
    /// <returns>True if the SelNode property was changed to the TreeNode supplied (even if the SelNode property was already set to the TreeNode supplied before this method was called).</returns>
    public bool SelectNode(TreeNode tn, bool bChangeSelNode)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        switch (MultiSelect)
        {
          case TreeViewMultiSelect.NoMulti:
            if (SelNodes.Count > 1)
            {
              ClearSelNodes();
            }

            SelectedNode = tn;

            SelNodeInt = SelectedNode;
            break;

          case TreeViewMultiSelect.Multi:
            if (!IsTreeNodeSelected(tn) && IsSelectNodeRegExSatisfied(tn.Text))
            {
              if (SelNodes == null)
              {
                SelNodes = new Hashtable();
              }

              SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

              HighlightNode(tn);
            }

            if (bChangeSelNode)
            {
              ChangeSelNode(tn);
            }
            break;

          case TreeViewMultiSelect.MultiSameBranchAndLevel:
            if (SelNodes != null && SelNodes.Count > 0)
            {
              TreeNode tnGrandParentSelNodes = null;
              int iLevel = 0;

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                tnGrandParentSelNodes = GetTreeNodeGrandParent(mwtnw.Node);
                iLevel = GetTreeNodeLevel(mwtnw.Node);
                break;
              }

              if (GetTreeNodeGrandParent(tn) == tnGrandParentSelNodes && GetTreeNodeLevel(tn) == iLevel)
              {
                if (!IsTreeNodeSelected(tn) && IsSelectNodeRegExSatisfied(tn.Text))
                {
                  SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

                  HighlightNode(tn);
                }
              }
              else if (IsSelectNodeRegExSatisfied(tn.Text))
              {
                ClearSelNodes();

                SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

                HighlightNode(tn);
              }
            }
            else if (IsSelectNodeRegExSatisfied(tn.Text))
            {
              if (SelNodes == null)
              {
                SelNodes = new Hashtable();
              }

              SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

              HighlightNode(tn);
            }

            if (bChangeSelNode)
            {
              ChangeSelNode(tn);
            }
            break;

          case TreeViewMultiSelect.MultiSameBranch:
            if (SelNodes != null && SelNodes.Count > 0)
            {
              TreeNode tnGrandParentSelNodes = null;

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                tnGrandParentSelNodes = GetTreeNodeGrandParent(mwtnw.Node);
                break;
              }

              if (GetTreeNodeGrandParent(tn) == tnGrandParentSelNodes)
              {
                if (!IsTreeNodeSelected(tn) && IsSelectNodeRegExSatisfied(tn.Text))
                {
                  SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

                  HighlightNode(tn);
                }
              }
              else if (IsSelectNodeRegExSatisfied(tn.Text))
              {
                ClearSelNodes();

                SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

                HighlightNode(tn);
              }
            }
            else if (IsSelectNodeRegExSatisfied(tn.Text))
            {
              if (SelNodes == null)
              {
                SelNodes = new Hashtable();
              }

              SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

              HighlightNode(tn);
            }

            if (bChangeSelNode)
            {
              ChangeSelNode(tn);
            }
            break;

          case TreeViewMultiSelect.MultiSameLevel:
            if (SelNodes != null && SelNodes.Count > 0)
            {
              int iLevel = 0;

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                iLevel = GetTreeNodeLevel(mwtnw.Node);
                break;
              }

              if (GetTreeNodeLevel(tn) == iLevel)
              {
                if (!IsTreeNodeSelected(tn) && IsSelectNodeRegExSatisfied(tn.Text))
                {
                  SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

                  HighlightNode(tn);
                }
              }
              else if (IsSelectNodeRegExSatisfied(tn.Text))
              {
                ClearSelNodes();

                SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

                HighlightNode(tn);
              }
            }
            else if (IsSelectNodeRegExSatisfied(tn.Text))
            {
              if (SelNodes == null)
              {
                SelNodes = new Hashtable();
              }

              SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

              HighlightNode(tn);
            }

            if (bChangeSelNode)
            {
              ChangeSelNode(tn);
            }
            break;

          case TreeViewMultiSelect.MultiPathToParents:
          case TreeViewMultiSelect.MultiPathToParent:
            if (SelNodes != null && SelNodes.Count > 0)
            {
              TreeNode tnGrandParentSelNodes = null;

              foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
              {
                tnGrandParentSelNodes = GetTreeNodeGrandParent(mwtnw.Node);
                break;
              }

              if (GetTreeNodeGrandParent(tn) == tnGrandParentSelNodes || MultiSelect == TreeViewMultiSelect.MultiPathToParents)
              {
                if (!IsTreeNodeSelected(tn))
                {
                  int iTNLevel = GetTreeNodeLevel(tn);
                  TreeNode tnTNGrandParent = GetTreeNodeGrandParent(tn);

                  int iMaxLevel = 0;
                  foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
                  {
                    iMaxLevel = Math.Max(iMaxLevel, GetTreeNodeLevel(mwtnw.Node));
                  }

                  if (iMaxLevel < iTNLevel)
                  {
                    if (MultiSelect == TreeViewMultiSelect.MultiPathToParent)
                    {
                      ClearSelNodes();
                    }
                    else if (MultiSelect == TreeViewMultiSelect.MultiPathToParents)
                    {
                      DeselectBranch(tnTNGrandParent, true, false);
                    }
                  }
                  else
                  {
                    foreach (TreeNode tnRoot in Nodes)
                    {
                      if (GetTreeNodeGrandParent(tn) == tnRoot)
                      {
                        ArrayList al = new ArrayList();

                        foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
                        {
                          if (tnRoot == GetTreeNodeGrandParent(mwtnw.Node))
                          {
                            int iLevel = GetTreeNodeLevel(mwtnw.Node);

                            if (iLevel > iTNLevel)
                            {
                              al.Add(mwtnw.Node);
                            }
                          }
                        }

                        for (int i = 0; i < al.Count; i++)
                        {
                          DeselectNode(al[i] as TreeNode, false);
                        }
                      }
                    }
                  }

                  if (IsSelectNodeRegExSatisfied(tn.Text))
                  {
                    SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

                    HighlightNode(tn);

                    TreeNode tnTemp = tn;
                    while (tnTemp.Parent != null)
                    {
                      if (!IsTreeNodeSelected(tnTemp.Parent))
                      {
                        SelNodes.Add(tnTemp.Parent.GetHashCode(), new MWTreeNodeWrapper(tnTemp.Parent));
                      }

                      HighlightNode(tnTemp.Parent);

                      tnTemp = tnTemp.Parent;
                    }
                  }
                }
                else
                {
                  TreeNode tnTemp = tn;
                  while (tnTemp.Parent != null)
                  {
                    if (IsSelectNodeRegExSatisfied(tn.Text))
                    {
                      if (!IsTreeNodeSelected(tnTemp.Parent))
                      {
                        SelNodes.Add(tnTemp.Parent.GetHashCode(), new MWTreeNodeWrapper(tnTemp.Parent));
                      }

                      HighlightNode(tnTemp.Parent);
                    }

                    tnTemp = tnTemp.Parent;
                  }
                }
              }
              else if (IsSelectNodeRegExSatisfied(tn.Text))
              {
                ClearSelNodes();

                SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

                HighlightNode(tn);

                TreeNode tnTemp = tn;
                while (tnTemp.Parent != null)
                {
                  if (!IsTreeNodeSelected(tnTemp.Parent))
                  {
                    SelNodes.Add(tnTemp.Parent.GetHashCode(), new MWTreeNodeWrapper(tnTemp.Parent));
                  }

                  HighlightNode(tnTemp.Parent);

                  tnTemp = tnTemp.Parent;
                }
              }
            }
            else if (IsSelectNodeRegExSatisfied(tn.Text))
            {
              if (SelNodes == null)
              {
                SelNodes = new Hashtable();
              }

              SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

              HighlightNode(tn);

              TreeNode tnTemp = tn;
              while (tnTemp.Parent != null)
              {
                if (!IsTreeNodeSelected(tnTemp.Parent))
                {
                  SelNodes.Add(tnTemp.Parent.GetHashCode(), new MWTreeNodeWrapper(tnTemp.Parent));
                }

                HighlightNode(tnTemp.Parent);

                tnTemp = tnTemp.Parent;
              }
            }

            if (bChangeSelNode)
            {
              ChangeSelNode(tn);
            }
            break;

          case TreeViewMultiSelect.SinglePathToParent:
            if (!IsTreeNodeSelected(tn) && IsSelectNodeRegExSatisfied(tn.Text))
            {
              ClearSelNodes();

              if (SelNodes == null)
              {
                SelNodes = new Hashtable();
              }

              SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));

              HighlightNode(tn);

              TreeNode tnTemp = tn;
              while (tnTemp.Parent != null)
              {
                if (!IsTreeNodeSelected(tnTemp.Parent))
                {
                  SelNodes.Add(tnTemp.Parent.GetHashCode(), new MWTreeNodeWrapper(tnTemp.Parent));
                }

                HighlightNode(tnTemp.Parent);

                tnTemp = tnTemp.Parent;
              }
            }

            if (bChangeSelNode)
            {
              ChangeSelNode(tn);
            }
            break;

          case TreeViewMultiSelect.SinglePathToParents:
            if (SelNodes != null && SelNodes.Count > 0)
            {
              GetTreeNodeLevel(tn);
              GetTreeNodeGrandParent(tn);

              foreach (TreeNode tnRoot in Nodes)
              {
                if (GetTreeNodeGrandParent(tn) == tnRoot)
                {
                  if (IsSelectNodeRegExSatisfied(tn.Text))
                  {
                    ClearSelBranch(tnRoot);

                    if (!IsTreeNodeSelected(tn))
                    {
                      SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));
                    }

                    HighlightNode(tn);

                    TreeNode tnTemp = tn;
                    while (tnTemp.Parent != null)
                    {
                      if (!IsTreeNodeSelected(tnTemp.Parent))
                      {
                        SelNodes.Add(tnTemp.Parent.GetHashCode(), new MWTreeNodeWrapper(tnTemp.Parent));
                      }

                      HighlightNode(tnTemp.Parent);

                      tnTemp = tnTemp.Parent;
                    }
                  }

                  break;
                }
              }
            }
            else
            {
              if (!IsTreeNodeSelected(tn) && IsSelectNodeRegExSatisfied(tn.Text))
              {
                ClearSelNodes();

                if (SelNodes == null)
                {
                  SelNodes = new Hashtable();
                }

                if (!IsTreeNodeSelected(tn))
                {
                  SelNodes.Add(tn.GetHashCode(), new MWTreeNodeWrapper(tn));
                }

                HighlightNode(tn);

                TreeNode tnTemp = tn;
                while (tnTemp.Parent != null)
                {
                  if (!IsTreeNodeSelected(tnTemp.Parent))
                  {
                    SelNodes.Add(tnTemp.Parent.GetHashCode(), new MWTreeNodeWrapper(tnTemp.Parent));
                  }

                  HighlightNode(tnTemp.Parent);

                  tnTemp = tnTemp.Parent;
                }
              }
            }

            if (bChangeSelNode)
            {
              ChangeSelNode(tn);
            }
            break;

          default:
            //Execution should never end up here!
            break;
        }

        if (SelNode == tn)
        {
          bRetVal = true;
        }
      }

      return bRetVal;
    }

    /// <summary>
    /// Change SelNode to the TreeNode supplied.
    /// </summary>
    /// <param name="tn">TreeNode to change SelNode into.</param>
    private void ChangeSelNode(TreeNode tn)
    {
      SelNodeInt = tn;

      if (ScrollToSelNode && !bRubberbandHasBeenPainted)
      {
        if (tn != null)
        {
          tn.EnsureVisible();
        }
      }
    }

    /// <summary>
    /// Deselect TreeNode supplied.
    /// Note that all selected TreeNodes can be found in the SelNodes property and the most recently selected TreeNode can be found in the SelNode property.
    /// </summary>
    /// <param name="tn">TreeNode to deselect.</param>
    /// <param name="bChangeSelNode">True if the SelNode property should be changed when deselecting the TreeNode.</param>
    /// <returns>True if the TreeNode supplied was successfully removed from the SelNodes property.</returns>
    public bool DeselectNode(TreeNode tn, bool bChangeSelNode)
    {
      bool bRetVal = false;

      bool bDeselectNode = false;

      if (tn != null && SelNodes.Count > 0 && IsTreeNodeSelected(tn))
      {
        if (MultiSelect == TreeViewMultiSelect.MultiPathToParent ||
          MultiSelect == TreeViewMultiSelect.MultiPathToParents ||
          MultiSelect == TreeViewMultiSelect.SinglePathToParent ||
          MultiSelect == TreeViewMultiSelect.SinglePathToParents)
        {
          if (tn.Nodes.Count == 0 || !IsAnyChildTreeNodeSelected(tn))
          {
            bDeselectNode = true;
          }
        }
        else
        {
          bDeselectNode = true;
        }
      }

      if (!AllowNoSelNode && SelNodes.Count == 1)
      {
        bDeselectNode = false;
      }

      if (bDeselectNode)
      {
        MWTreeNodeWrapper.Deselect(SelNodes[tn.GetHashCode()] as MWTreeNodeWrapper);
        SelNodes.Remove(tn.GetHashCode());

        LowlightNode(tn);

        if (!IsTreeNodeSelected(tn))
        {
          bRetVal = true;
        }
      }

      if (bChangeSelNode)
      {
        if (SelNode != null)
        {
          bRetVal = true;
        }

        SelNodeInt = null;
      }

      return bRetVal;
    }

    /// <summary>
    /// Toggle the selection of TreeNode supplied.
    /// Note that all selected TreeNodes can be found in the SelNodes property and the most recently selected TreeNode can be found in the SelNode property.
    /// </summary>
    /// <param name="tn">TreeNode to toggle selection of.</param>
    /// <param name="bChangeSelNode">True if the SelNode property should be changed when toggling the selection of the TreeNode.</param>
    /// <returns>True if supplied TreeNode got selected or false otherwise.</returns>
    public bool ToggleNode(TreeNode tn, bool bChangeSelNode)
    {
      bool bRetVal = true;

      if (SelNodes != null && IsTreeNodeSelected(tn))
      {
        DeselectNode(tn, bChangeSelNode);
      }
      else
      {
        SelectNode(tn, bChangeSelNode);

        bRetVal = false;
      }

      return bRetVal;
    }

    #endregion SelectNode / DeselectNode / ToggleNode



    #region SelectBranch / DeselectBranch

    /// <summary>
    /// Select a branch of TreeNodes.
    /// The TreeNode supplied and the TreeNodes under it, if any (the TreeNodes in its Nodes collection), are selected.
    /// If bChangeSubBranches is true all the branches of the branches etc of the TreeNode supplied are selected.
    /// </summary>
    /// <param name="tn">TreeNode whose branch (TreeNodes in its Nodes collection) should be selected.</param>
    /// <param name="bChangeSubBranches">True if all sub-branches should be deselected or false if only one level of child TreeNodes should be deselected.</param>
    /// <param name="bTopSelNode">True if the TreeNode passed in should be selected first and then all the child TreeNodes or false if all child TreeNodes should be selected and then the TreeNode supplied.\nThis should be true if TreeNodes are selected top-to-bottom and false if TreeNodes are selected bottom-to-top.</param>
    /// <param name="bchangeSelNode">True if the SelNode property should be changed when selecting the TreeNodes.</param>
    /// <returns>True if the TreeNode itself and/or its child TreeNodes got selected.</returns>
    public bool SelectBranch(TreeNode tn, bool bChangeSubBranches, bool bTopSelNode, bool bchangeSelNode)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        if (!bTopSelNode)
        {
          SelectNode(tn, bchangeSelNode);
        }

        foreach (TreeNode tn2 in tn.Nodes)
        {
          if (bChangeSubBranches)
          {
            SelectBranch(tn2, true, bTopSelNode, bchangeSelNode);
          }
          else
          {
            SelectNode(tn2, bchangeSelNode);
          }
        }

        if (bTopSelNode)
        {
          SelectNode(tn, bchangeSelNode);
        }

        bRetVal = true;
      }

      return bRetVal;
    }

    /// <summary>
    /// Deselect a branch of TreeNodes.
    /// The TreeNode supplied and the TreeNodes under it, if any (the TreeNodes in its Nodes collection), are deselected.
    /// If bChangeSubBranches is true all the branches of the branches etc of the TreeNode supplied are selected.
    /// </summary>
    /// <param name="tn">TreeNode whose branch (TreeNodes in its Nodes collection) should be deselected.</param>
    /// <param name="bChangeSubBranches">True if all sub-branches should be deselected or false if only one level of child TreeNodes should be deselected.</param>
    /// <param name="bChangeSelNode">True if the SelNode property should be changed when deselecting the TreeNodes.</param>
    /// <returns>True if the TreeNode itself and/or its child TreeNodes got selected.</returns>
    public bool DeselectBranch(TreeNode tn, bool bChangeSubBranches, bool bChangeSelNode)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        if (tn == SelNode && bChangeSelNode)
        {
          SelNode = null;
        }

        DeselectNode(tn, bChangeSelNode);

        if (tn.Nodes.Count > 0)
        {
          foreach (TreeNode tn2 in tn.Nodes)
          {
            if (bChangeSubBranches)
            {
              DeselectBranch(tn2, true, bChangeSelNode);
            }
            else
            {
              DeselectNode(tn2, bChangeSelNode);
            }
          }

        }

        bRetVal = true;
      }

      return bRetVal;
    }

    #endregion SelectBranch / DeselectBranch



    #region SelectAllParentNodes

    /// <summary>
    /// Select supplied TreeNode and all its Parents.
    /// </summary>
    /// <param name="tn">TreeNode whose Parents should be selected.</param>
    /// <param name="bChangeSelNode">True if the SelNode property should be changed when selecting TreeNodes.</param>
    public void SelectAllParentNodes(TreeNode tn, bool bChangeSelNode)
    {
      SelectNode(tn, bChangeSelNode);

      if (tn != null && tn.Parent != null)
      {
        SelectAllParentNodes(tn.Parent, bChangeSelNode);
      }
    }

    /// <summary>
    /// Deselect supplied TreeNode and all its Parents.
    /// </summary>
    /// <param name="tn">TreeNode whose Parents should be deselected.</param>
    /// <param name="bChangeSelNode">True if the SelNode property should be changed when deselecting TreeNodes.</param>
    public void DeselectAllParentNodes(TreeNode tn, bool bChangeSelNode)
    {
      DeselectNode(tn, bChangeSelNode);

      if (tn != null && tn.Parent != null)
      {
        DeselectAllParentNodes(tn.Parent, bChangeSelNode);
      }
    }

    #endregion SelectAllParentNodes



    #region SelectAllNodes

    /// <summary>
    /// Select all TreeNodes in this MWTreeView.
    /// </summary>
    public void SelectAllNodes()
    {
      foreach (TreeNode tn in Nodes)
      {
        SelectAllNodes(tn);
        SelectNode(tn, false);
      }

      SelectNode(Nodes[0], true);
    }

    /// <summary>
    /// Select all TreeNodes in the Nodes collection of the TreeNode supplied.
    /// </summary>
    /// <param name="tn"></param>
    public void SelectAllNodes(TreeNode tn)
    {
      foreach (TreeNode tn2 in tn.Nodes)
      {
        SelectAllNodes(tn2);
        SelectNode(tn2, false);
      }
    }

    /// <summary>
    /// Select all TreeNodes in the Nodes collection of this Control if they are of the supplied TreeNodeLevel.
    /// When a TreeNodeLevel greater than the one supplied is encountered stop iterating.
    /// </summary>
    /// <param name="iLevel">TreeNodeLevel that TreeNodes must be in order to be selected.</param>
    /// <param name="bChangeSelNode">True if the SelNode property should be changed when selecting TreeNodes.</param>
    public void SelectAllNodes(int iLevel, bool bChangeSelNode)
    {
      foreach (TreeNode tn in Nodes)
      {
        if (GetTreeNodeLevel(tn) == iLevel)
        {
          SelectNode(tn, bChangeSelNode);
        }
        else
        {
          if (GetTreeNodeLevel(tn) < iLevel)
          {
            SelectAllNodes(tn, iLevel, bChangeSelNode);
          }
          else
          {
            break;
          }
        }
      }
    }

    /// <summary>
    /// Select all TreeNodes in the Nodes collection of the TreeNode supplied if they are of the supplied TreeNodeLevel.
    /// When a TreeNodeLevel greater than the one supplied is encountered stop iterating.
    /// </summary>
    /// <param name="tn">TreeNode in whose Nodes collection TreeNodes should be selected.</param>
    /// <param name="iLevel">TreeNodeLevel that TreeNodes must be in order to be selected.</param>
    /// <param name="bChangeSelNode">True if the SelNode property should be changed when selecting TreeNodes.</param>
    public void SelectAllNodes(TreeNode tn, int iLevel, bool bChangeSelNode)
    {
      foreach (TreeNode tn2 in tn.Nodes)
      {
        if (GetTreeNodeLevel(tn2) == iLevel)
        {
          SelectNode(tn2, bChangeSelNode);
        }
        else
        {
          if (GetTreeNodeLevel(tn2) < iLevel)
          {
            SelectAllNodes(tn2, iLevel, bChangeSelNode);
          }
          else
          {
            break;
          }
        }
      }
    }

    #endregion SelectAllNodes



    #region ClearSelNodes

    /// <summary>
    /// Clear all TreeNodes in the SelNodes property and deselect them.
    /// </summary>
    public void ClearSelNodes()
    {
      if (SelNodes != null)
      {
        foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
        {
          MWTreeNodeWrapper.Deselect(mwtnw);
          LowlightNode(mwtnw.Node);
        }

        SelNodes.Clear();
      }
      else
      {
        SelNodes = new Hashtable();
      }
    }

    /// <summary>
    /// Clear all TreeNodes in the SelNodes property and deselect them, except for the tn TreeNode supplied.
    /// </summary>
    /// <param name="tn">TreeNode not to deselect.</param>
    public void ClearSelNodes(TreeNode tn)
    {
      if (tn == null)
      {
        ClearSelNodes();
      }
      else if (SelNodes != null)
      {
        int iMax = SelNodes.Count;

        for (int i = 0; i < iMax; i++)
        {
          foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
          {
            if (mwtnw.Node != tn)
            {
              MWTreeNodeWrapper.Deselect(mwtnw);
              LowlightNode(mwtnw.Node);
              SelNodes.Remove(mwtnw.Node.GetHashCode());

              break;
            }
          }
        }
      }
    }

    /// <summary>
    /// Clear all TreeNodes in the SelNodes property and deselect them, except for the visible TreeNodes in the range between, and
    ///		including, the two TreeNodes supplied.
    /// </summary>
    /// <param name="tnFrom">First TreeNode in range of TreeNodes that should not be deselected.</param>
    /// <param name="tnTo">Last TreeNode in range of TreeNodes that should not be deselected.</param>
    public void ClearSelNodes(TreeNode tnFrom, TreeNode tnTo)
    {
      ClearSelNodes(tnFrom, tnTo, true);
    }

    /// <summary>
    /// Clear all TreeNodes in the SelNodes property and deselect them, except for the TreeNodes in the range between, and including, the
    ///		two TreeNodes supplied.
    /// </summary>
    /// <param name="tnFrom">First TreeNode in range of TreeNodes that should not be deselected.</param>
    /// <param name="tnTo">Last TreeNode in range of TreeNodes that should not be deselected.</param>
    /// <param name="bOnlyVisible">True if only visible TreeNodes should be considered to be part of the range or false if both visible and non-visible TreeNodes should be considered to be part of the range.</param>
    public void ClearSelNodes(TreeNode tnFrom, TreeNode tnTo, bool bOnlyVisible)
    {
      if (tnFrom == tnTo)
      {
        if (tnFrom != null)
        {
          ClearSelNodes(tnFrom);
        }
        else
        {
          ClearSelNodes();
        }
      }
      else if (SelNodes != null)
      {
        if (tnTo == null)
        {
          tnTo = Nodes[Nodes.Count - 1];

          if (bOnlyVisible)
          {
            while (tnTo.NextVisibleNode != null)
            {
              tnTo = tnTo.NextVisibleNode;
            }
          }
          else
          {
            while (tnTo.NextNode != null)
            {
              tnTo = tnTo.NextNode;
            }
          }
        }

        Hashtable ht = new Hashtable();

        TreeNode tn = tnFrom;

        while (tn != null && tn != tnTo)
        {
          ht.Add(tn.GetHashCode(), tn);

          tn = bOnlyVisible ? tn.NextVisibleNode : tn.NextNode;
        }

        ht.Add(tnTo.GetHashCode(), tnTo);

        int iMax = SelNodes.Count;

        for (int i = 0; i < iMax; i++)
        {
          foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
          {
            if (!ht.Contains(mwtnw.Node.GetHashCode()))
            {
              MWTreeNodeWrapper.Deselect(mwtnw);
              LowlightNode(mwtnw.Node);
              SelNodes.Remove(mwtnw.Node.GetHashCode());

              break;
            }
          }
        }
      }
    }

    #endregion ClearSelNodes



    #region ClearSelBranch

    /// <summary>
    /// Clear SelNodes in a Branch.
    /// Note that the TreeNode supplied is also cleared.
    /// </summary>
    /// <param name="tn">TreeNode whose whole Branch should be cleared.</param>
    public void ClearSelBranch(TreeNode tn)
    {
      if (SelNodes != null)
      {
        ArrayList al = new ArrayList();

        foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
        {
          TreeNode tnGrandParent = GetTreeNodeGrandParent(mwtnw.Node);

          if (tnGrandParent == tn)
          {
            al.Add(mwtnw);
          }
        }

        for (int i = 0; i < al.Count; i++)
        {
          if (((MWTreeNodeWrapper)al[i]).Node == SelNode)
          {
            SelNode = null;
          }

          MWTreeNodeWrapper.Deselect(al[i] as MWTreeNodeWrapper);
          LowlightNode(((MWTreeNodeWrapper)al[i]).Node);

          SelNodes.Remove(((MWTreeNodeWrapper)al[i]).Node.GetHashCode());
        }
      }
    }

    #endregion ClearSelBranch



    #region CheckNode / UncheckNode / ToggleCheckNode

    /// <summary>
    /// Check the TreeNode supplied (set the Checked property to true).
    /// </summary>
    /// <param name="tn">TreeNode to check (set the Checked property to true).</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if the TreeNodes was checked (Checked property set to true) or false otherwise.</returns>
    public bool CheckNode(TreeNode tn, bool bUpdate)
    {
      bool bRetVal = false;

      bool bOldChecked = tn.Checked;

      if (AllowMultiCheck)
      {
        if (IsTreeNodeChecked(tn))
        {
          if (!tn.Checked && bUpdate)
          {
            tn.Checked = true;
          }
        }
        else
        {
          if (!tn.Checked)
          {
            if (bUpdate)
            {
              tn.Checked = true;
            }

            if (!IsTreeNodeChecked(tn) && IsCheckNodeRegExSatisfied(tn.Text))
            {
              CheckedNodes.Add(tn.GetHashCode(), tn);
            }
          }
        }
      }
      else
      {
        ClearCheckedNodes();

        if (bUpdate)
        {
          tn.Checked = true;
        }

        if (!IsTreeNodeChecked(tn) && IsCheckNodeRegExSatisfied(tn.Text))
        {
          CheckedNodes.Add(tn.GetHashCode(), tn);
        }
      }

      if (bOldChecked != tn.Checked)
      {
        bRetVal = true;
      }

      return bRetVal;
    }

    /// <summary>
    /// Uncheck the TreeNode supplied (set the Checked property to false).
    /// </summary>
    /// <param name="tn">TreeNode to uncheck (set the Checked property to false).</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if the TreeNodes was unchecked (Checked property set to false) or false otherwise.</returns>
    public bool UncheckNode(TreeNode tn, bool bUpdate)
    {
      bool bRetVal = false;

      bool bOldChecked = tn.Checked;

      if (IsTreeNodeChecked(tn))
      {
        if (tn.Checked)
        {
          if (bUpdate)
          {
            tn.Checked = false;
          }

          CheckedNodes.Remove(tn.GetHashCode());
        }
      }
      else
      {
        if (tn.Checked && bUpdate)
        {
          tn.Checked = false;
        }
      }

      if (bOldChecked != tn.Checked)
      {
        bRetVal = true;
      }

      return bRetVal;
    }

    /// <summary>
    /// Toggle the Checked property of the TreeNode supplied.
    /// </summary>
    /// <param name="tn">TreeNode to toggle the Checked property of.</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if the Checked property has been toggled or false otherwise.</returns>
    public bool ToggleCheckNode(TreeNode tn, bool bUpdate)
    {
      bool bRetVal = false;

      bool bOldChecked = tn.Checked;

      if (tn.Checked)
      {
        UncheckNode(tn, bUpdate);
      }
      else
      {
        CheckNode(tn, bUpdate);
      }

      if (bOldChecked != tn.Checked)
      {
        bRetVal = true;
      }

      return bRetVal;
    }

    #endregion CheckNode / UncheckNode / ToggleCheckNode



    #region CheckBranch / UncheckBranch / ToggleCheckBranch

    /// <summary>
    /// Check all child TreeNodes (Nodes collection) of the TreeNode supplied.
    /// Note that the TreeNode supplied doesn't get checked itself.
    /// Note that no TreeNode is excluded from being checked.
    /// </summary>
    /// <param name="tn">TreeNode to check all child TreeNodes of.</param>
    /// <param name="bChangeSubBranches">True if all sub-branches should be checked or false if only one level of child TreeNodes should be checked.</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if TreeNodes got checked or false otherwise.</returns>
    public bool CheckBranch(TreeNode tn, bool bChangeSubBranches, bool bUpdate)
    {
      return CheckBranch(tn, bChangeSubBranches, null, bUpdate);
    }

    /// <summary>
    /// Check all child TreeNodes (Nodes collection) of the TreeNode supplied.
    /// Note that the TreeNode supplied doesn't get checked itself.
    /// </summary>
    /// <param name="tn">TreeNode to check all child TreeNodes of.</param>
    /// <param name="bChangeSubBranches">True if all sub-branches should be checked or false if only one level of child TreeNodes should be checked.</param>
    /// <param name="tnExcluded">TreeNode that should be excluded from being unchecked.</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if TreeNodes got checked or false otherwise.</returns>
    public bool CheckBranch(TreeNode tn, bool bChangeSubBranches, TreeNode tnExcluded, bool bUpdate)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        foreach (TreeNode tn2 in tn.Nodes)
        {
          if (tnExcluded != tn2)
          {
            CheckNode(tn2, bUpdate);
          }

          if (bChangeSubBranches)
          {
            CheckBranch(tn2, true, bUpdate);
          }
        }

        bRetVal = true;
      }

      return bRetVal;
    }

    /// <summary>
    /// Uncheck all child TreeNodes (Nodes collection) of the TreeNode supplied.
    /// Note that the TreeNode supplied doesn't get unchecked itself.
    /// Note that no TreeNode is excluded from unchecked.
    /// </summary>
    /// <param name="tn">TreeNode to uncheck all child TreeNodes of.</param>
    /// <param name="bChangeSubBranches">True if all sub-branches should be unchecked or false if only one level of child TreeNodes should be unchecked.</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if TreeNodes got unchecked or false otherwise.</returns>
    public bool UncheckBranch(TreeNode tn, bool bChangeSubBranches, bool bUpdate)
    {
      return UncheckBranch(tn, bChangeSubBranches, null, bUpdate);
    }

    /// <summary>
    /// Uncheck all child TreeNodes (Nodes collection) of the TreeNode supplied.
    /// Note that the TreeNode supplied doesn't get unchecked itself.
    /// </summary>
    /// <param name="tn">TreeNode to uncheck all child TreeNodes of.</param>
    /// <param name="bChangeSubBranches">True if all sub-branches should be unchecked or false if only one level of child TreeNodes should be unchecked.</param>
    /// <param name="tnExcluded">TreeNode that should be excluded from being unchecked.</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if TreeNodes got unchecked or false otherwise.</returns>
    public bool UncheckBranch(TreeNode tn, bool bChangeSubBranches, TreeNode tnExcluded, bool bUpdate)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        foreach (TreeNode tn2 in tn.Nodes)
        {
          if (tnExcluded != tn2)
          {
            UncheckNode(tn2, bUpdate);
          }

          if (bChangeSubBranches)
          {
            UncheckBranch(tn2, true, bUpdate);
          }
        }

        bRetVal = true;
      }

      return bRetVal;
    }

    /// <summary>
    /// Toggle the Checked property of all child TreeNodes (Nodes collection) of the TreeNode supplied.
    /// Note that the Checked property of the TreeNode supplied doesn't get toggled.
    /// Note that no TreeNode is excluded from having its Checked property toggled.
    /// </summary>
    /// <param name="tn">TreeNode whose child TreeNode's Checked property should be toggled.</param>
    /// <param name="bChangeSubBranches">True if all sub-branches' Checked property should be toggled or false if only one level of child TreeNodes should have their Checked property toggled.</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if TreeNodes' Checked property got toggled or false otherwise.</returns>
    public bool ToggleCheckBranch(TreeNode tn, bool bChangeSubBranches, bool bUpdate)
    {
      return ToggleCheckBranch(tn, bChangeSubBranches, null, bUpdate);
    }

    /// <summary>
    /// Toggle the Checked property of all child TreeNodes (Nodes collection) of the TreeNode supplied.
    /// Note that the Checked property of the TreeNode supplied doesn't get toggled.
    /// </summary>
    /// <param name="tn">TreeNode whose child TreeNode's Checked property should be toggled.</param>
    /// <param name="bChangeSubBranches">True if all sub-branches' Checked property should be toggled or false if only one level of child TreeNodes should have their Checked property toggled.</param>
    /// <param name="tnExcluded">TreeNode that should be excluded from having its Checked property toggled.</param>
    /// <param name="bUpdate">True if the TreeNode's Checked property should be updated or false otherwise.</param>
    /// <returns>True if TreeNodes' Checked property got toggled or false otherwise.</returns>
    public bool ToggleCheckBranch(TreeNode tn, bool bChangeSubBranches, TreeNode tnExcluded, bool bUpdate)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        foreach (TreeNode tn2 in tn.Nodes)
        {
          if (tnExcluded != tn2)
          {
            ToggleCheckNode(tn2, bUpdate);
          }

          if (bChangeSubBranches)
          {
            ToggleCheckBranch(tn2, true, bUpdate);
          }
        }

        bRetVal = true;
      }

      return bRetVal;
    }

    #endregion CheckBranch / UncheckBranch / ToggleCheckBranch



    #region ClearCheckedNodes / CheckAllNodes

    /// <summary>
    /// Clear all TreeNodes in the CheckedNodes property.
    /// </summary>
    public void ClearCheckedNodes()
    {
      if (CheckedNodes != null)
      {
        foreach (TreeNode tn in CheckedNodes.Values)
        {
          tn.Checked = false;
        }

        CheckedNodes.Clear();
      }
    }

    /// <summary>
    /// Check all TreeNodes in this MWTreeView.
    /// </summary>
    public void CheckAllNodes()
    {
      foreach (TreeNode tn in Nodes)
      {
        CheckAllNodes(tn);
        CheckNode(tn, true);
      }
    }

    /// <summary>
    /// Check all TreeNodes in the Nodes collection of the TreeNode supplied.
    /// </summary>
    /// <param name="tn"></param>
    public void CheckAllNodes(TreeNode tn)
    {
      foreach (TreeNode tn2 in tn.Nodes)
      {
        CheckAllNodes(tn2);
        CheckNode(tn2, true);
      }
    }

    #endregion ClearCheckedNodes / CheckAllNodes



    #region ActivateSelNodes / DeactivateSelNodes & ForceDeactivateSelNodes

    /// <summary>
    /// Activate all selected TreeNodes of this MWTreeView.
    /// Activating means higlighting.
    /// </summary>
    private void ActivateSelNodes()
    {
      if (SelNodes != null && SelNodes.Count > 0)
      {
        bActive = true;

        foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
        {
          HighlightNode(mwtnw.Node);
        }
      }
    }

    /// <summary>
    /// Deactivate all selected TreeNodes of this MWTreeView.
    /// Deactivating means removing higlighting ('lowlighting').
    /// </summary>
    private void DeactivateSelNodes()
    {
      if (SelNodes != null && SelNodes.Count > 0)
      {
        bActive = false;

        if (HideSelection)
        {
          foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
          {
            LowlightNode(mwtnw.Node);
          }
        }
        else
        {
          foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
          {
            HighlightNode(mwtnw.Node, false);
          }
        }
      }
    }

    /// <summary>
    /// Forces deactivation of all selected TreeNodes of this MWTreeView.
    /// Deactivating means removing higlighting ('lowlighting').
    /// </summary>
    private void ForceDeactivateSelNodes()
    {
      if (SelNodes != null && SelNodes.Count > 0)
      {
        foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
        {
          LowlightNode(mwtnw.Node);
        }
      }
    }

    #endregion ActivateSelNodes / DeactivateSelNodes & ForceDeactivateSelNodes



    #region IsMouseMoveSelect

    /// <summary>
    /// Returns true if a mouse move is considered to be a select operation or false otherwise.
    /// This property is used when AllowRubberbandSelect is true and a TreeNode is expanded so that the TreeView is scrolled - no selection
    ///		of TreeNodes will occur for this until the mouse is moved slightly. Without this property TreeNodes were selected when they
    ///		shouldn't have been.
    ///	Note that the distance the mouse has to be moved to start selecting TreeNodes after a TreeNode has been expanded is the same as the
    ///		distance in which two consecutive Mouse Clicks are considered to be one DoubleClick (not talking about the time constraint
    ///		here).
    ///	Note that all this only applies when the left mouse button is still held down, if it is released all this does not apply.
    /// </summary>
    private bool IsMouseMoveSelect
    {
      get
      {
        Point ptMouseNow = PointToClient(MousePosition);

        return Math.Abs(ptMouseDownOrig.X - ptMouseNow.X) > SystemInformation.DoubleClickSize.Width || Math.Abs(ptMouseDownOrig.Y - ptMouseNow.Y) > SystemInformation.DoubleClickSize.Height;
      }
    }
    #endregion IsMouseMoveSelect



    #region IsLabelEditRegExSatisfied

    /// <summary>
    /// Checks if the proposed Label Text satisfies the regular expression or not.
    /// Note that a Label's Text could still be blank if the AllowBlankNodeText property is set to true.
    /// </summary>
    /// <param name="strLabelText">Label Text that should be checked against the regular expression in the LabelEditRegEx property.</param>
    /// <returns>True if the proposed Label Text satisfies the regular expression or false otherwise.</returns>
    private bool IsLabelEditRegExSatisfied(string strLabelText)
    {
      return Regex.IsMatch(strLabelText, LabelEditRegEx);
    }

    #endregion IsLabelEditRegExSatisfied



    #region IsDisallowLabelEditRegExSatisfied

    /// <summary>
    /// Checks if the proposed Label Text satisfies the regular expression or not. If it does, do not allow this TreeNode's Text to be
    ///		edited (done in the OnBeforeLabelEdit EventHandler).
    /// </summary>
    /// <param name="strLabelText">Label Text that should be checked against the regular expression in the DisallowLabelEditRegEx property.</param>
    /// <returns>True if the proposed Label Text satisfies the regular expression or false otherwise.</returns>
    private bool IsDisallowLabelEditRegExSatisfied(string strLabelText)
    {
      bool bRetVal;

      bRetVal = !(DisallowLabelEditRegEx == string.Empty) && Regex.IsMatch(strLabelText, DisallowLabelEditRegEx);

      return bRetVal;
    }

    #endregion IsDisallowLabelEditRegExSatisfied



    #region IsSelectNodeRegExSatisfied

    /// <summary>
    /// Checks if the TreeNode's Text satisfies the regular expression or not, if it does the TreeNode can be selected otherwise not.
    /// </summary>
    /// <param name="strText">TreeNode Text that should be checked against the regular expression in the SelectNodeRegEx property.</param>
    /// <returns>True if the TreeNode's Text satisfies the regular expression or false otherwise.</returns>
    private bool IsSelectNodeRegExSatisfied(string strText)
    {
      return Regex.IsMatch(strText, SelectNodeRegEx);
    }

    #endregion IsSelectNodeRegExSatisfied



    #region IsCheckNodeRegExSatisfied

    /// <summary>
    /// Checks if the TreeNode's Text satisfies the regular expression or not, if it does the TreeNode can be checked otherwise not.
    /// </summary>
    /// <param name="strText">TreeNode Text that should be checked against the regular expression in the CheckNodeRegEx property.</param>
    /// <returns>True if the TreeNode's Text satisfies the regular expression or false otherwise.</returns>
    private bool IsCheckNodeRegExSatisfied(string strText)
    {
      return Regex.IsMatch(strText, CheckNodeRegEx);
    }

    #endregion IsCheckNodeRegExSatisfied



    #region EnsureAllSelectedNodesAreAllowed

    /// <summary>
    /// Makes sure all the selected TreeNodes are allowed according to the SelectNodeRegEx property's regular expression.
    /// </summary>
    private void EnsureAllSelectedNodesAreAllowed()
    {
      ArrayList al = new ArrayList();

      if (SelNodes != null)
      {
        foreach (MWTreeNodeWrapper mwtnw in SelNodes.Values)
        {
          if (!IsSelectNodeRegExSatisfied(mwtnw.Node.Text))
          {
            al.Add(mwtnw.Node);
          }
        }
      }

      for (int i = 0; i < al.Count; i++)
      {
        DeselectNode(al[i] as TreeNode, false);
      }

      if (SelNode != null && !IsSelectNodeRegExSatisfied(SelNode.Text))
      {
        DeselectNode(SelNode, true);
      }
    }

    #endregion EnsureAllSelectedNodesAreAllowed



    #region EnsureAllCheckedNodesAreAllowed

    /// <summary>
    /// Makes sure all the selected TreeNodes are allowed according to the SelectNodeRegEx property's regular expression.
    /// </summary>
    private void EnsureAllCheckedNodesAreAllowed()
    {
      ArrayList al = new ArrayList();

      if (CheckedNodes != null)
      {
        foreach (TreeNode tn in CheckedNodes.Values)
        {
          if (!IsCheckNodeRegExSatisfied(tn.Text))
          {
            al.Add(tn);
          }
        }
      }

      bForceCheckNode = true;

      for (int i = 0; i < al.Count; i++)
      {
        UncheckNode(al[i] as TreeNode, true);
      }

      bForceCheckNode = false;
    }

    #endregion EnsureAllCheckedNodesAreAllowed



    #region ActivateOrDeactivateSelNodes

    /// <summary>
    /// Fixes activation and/or deactivation for TreeNodes.
    /// </summary>
    public void ActivateOrDeactivateSelNodes()
    {
      if (!HideSelection)
      {
        if (bActive)
        {
          ActivateSelNodes();
        }
        else
        {
          DeactivateSelNodes();
        }
      }
      else
      {
        if (!bActive)
        {
          ForceDeactivateSelNodes();
        }
      }
    }

    #endregion ActivateOrDeactivateSelNodes



    #region RemoveNode and RemoveNodes

    /// <summary>
    /// Remove one TreeNode contained in an MWTreeNodeWrapper. The TreeNode is also removed from the SelNodes property, the CheckedNodes
    ///		property and/or the SelNode property.
    /// </summary>
    /// <param name="mwtnw">MWTreeNodeWrapper to remove.</param>
    /// <returns>True if the TreeNode in the MWTreeNodeWrapper was removed or false otherwise.</returns>
    public bool RemoveNode(MWTreeNodeWrapper mwtnw)
    {
      bool bRetVal = false;

      if (mwtnw != null)
      {
        bRetVal = RemoveNode(mwtnw.Node);
      }

      return bRetVal;
    }

    /// <summary>
    /// Remove one TreeNode. The TreeNode is also removed from the SelNodes property, the CheckedNodes property and/or the SelNode property.
    /// </summary>
    /// <param name="tn">TreeNode to remove.</param>
    /// <returns>True if the TreeNode was removed or false otherwise.</returns>
    public bool RemoveNode(TreeNode tn)
    {
      bool bRetVal = false;

      if (tn != null)
      {
        DeleteNode(tn);

        try
        {
          tn.Remove();

          if (tn.TreeView == null && !IsTreeNodeSelected(tn) && !IsTreeNodeChecked(tn) && tn != SelNode)
          {
            bRetVal = true;
          }
        } catch
        {
        }
      }

      return bRetVal;
    }

    /// <summary>
    /// Delete the supplied TreeNode from the SelNodes property, the CheckedNodes property and/or the SelNode property.
    /// </summary>
    /// <param name="tn">TreeNode to delete.</param>
    private void DeleteNode(TreeNode tn)
    {
      foreach (TreeNode tnChild in tn.Nodes)
      {
        DeleteNode(tnChild);
      }

      if (tn != null)
      {
        if (IsTreeNodeSelected(tn))
        {
          if (SelNodes != null)
          {
            SelNodes.Remove(tn.GetHashCode());
          }
        }

        if (IsTreeNodeChecked(tn))
        {
          if (CheckedNodes != null)
          {
            CheckedNodes.Remove(tn.GetHashCode());
          }
        }

        if (SelNode != null && SelNode == tn)
        {
          SelNode = null;
        }
      }
    }

    /// <summary>
    /// Remove an array of TreeNodes. The TreeNodes are also removed from the SelNodes property, the CheckedNodes property and/or the
    ///		SelNode property.
    /// </summary>
    /// <param name="atn">TreeNode array to remove.</param>
    /// <returns>True if at least one TreeNode was removed or false if no TreeNodes were removed.</returns>
    public bool RemoveNodes(TreeNode[] atn)
    {
      bool bRetVal = false;

      if (atn != null)
      {
        foreach (TreeNode tn in atn)
        {
          if (RemoveNode(tn))
          {
            bRetVal = true;
          }
        }
      }

      return bRetVal;
    }

    /// <summary>
    /// Remove an array of TreeNodes contained in MWTreeNodeWrappers. The TreeNodes are also removed from the SelNodes property, the
    ///		CheckedNodes property and/or the SelNode property.
    /// </summary>
    /// <param name="amwtnw">MWTreeNodeWrapper array whose TreeNodes should be removed.</param>
    /// <returns>True if at least one TreeNode from the MWTreeNodeWrappers was removed or false if no TreeNodes were removed.</returns>
    public bool RemoveNodes(MWTreeNodeWrapper[] amwtnw)
    {
      bool bRetVal = false;

      if (amwtnw != null)
      {
        foreach (MWTreeNodeWrapper mwtnw in amwtnw)
        {
          if (RemoveNode(mwtnw))
          {
            bRetVal = true;
          }
        }
      }

      return bRetVal;
    }

    /// <summary>
    /// Remove an ArrayList of TreeNodes or an ArrayList of TreeNodes contained in MWTreeNodeWrappers. The TreeNodes are also removed from
    ///		the SelNodes property, the CheckedNodes property and/or the SelNode property.
    /// </summary>
    /// <param name="altn">ArrayList containing TreeNodes that should be removed or MWTreeNodeWrappers whose TreeNodes should be removed.</param>
    /// <returns>True if at least one TreeNode or one TreeNode from the MWTreeNodeWrappers was removed or false if no TreeNodes were removed.</returns>
    public bool RemoveNodes(ArrayList altn)
    {
      bool bRetVal = false;

      if (altn != null)
      {
        for (int i = 0; i < altn.Count; i++)
        {
          if (altn[i] is TreeNode)
          {
            if (RemoveNode(altn[i] as TreeNode))
            {
              bRetVal = true;
            }
          }
          else if (altn[i] is MWTreeNodeWrapper)
          {
            if (RemoveNode(altn[i] as MWTreeNodeWrapper))
            {
              bRetVal = true;
            }
          }
        }
      }

      return bRetVal;
    }

    #endregion RemoveNode and RemoveNodes

    #endregion Help Methods



    #region Static Help Methods

    #region GetTreeNodeLevel

    /// <summary>
    /// Get the TreeNodeLevel of the TreeNode supplied.
    /// Note that the TreeNodes in the TreeView's Nodes collection are considered to be of TreeNodeLevel zero (0) and each consecutive level thereafter is one more (1, 2, etc).
    /// </summary>
    /// <param name="tn">TreeNode whose TreeNodeLevel should be checked.</param>
    /// <returns>The TreeNodeLevel of the TreeNode supplied.</returns>
    public static int GetTreeNodeLevel(TreeNode tn)
    {
      const int i = 0;

      if (tn != null && tn.Parent != null)
      {
        return GetTreeNodeLevel(tn.Parent) + 1;
      }
      return i;
    }

    #endregion GetTreeNodeLevel



    #region GetTreeNodeGrandParent

    /// <summary>
    /// Get the GrandParent of the TreeNode supplied.
    /// The GrandParent is the outermost Parent of a TreeNode.
    /// </summary>
    /// <param name="tn">TreeNode to get GrandParent of.</param>
    /// <returns>The GrandParent of the TreeNode supplied.</returns>
    public static TreeNode GetTreeNodeGrandParent(TreeNode tn)
    {
      if (tn != null && tn.Parent != null)
      {
        return GetTreeNodeGrandParent(tn.Parent);
      }
      return tn;
    }

    #endregion GetTreeNodeGrandParent

    #endregion Static Help Methods

  }
}
