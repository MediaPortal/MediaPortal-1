using System;

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
namespace MWCommon
{
	#region TextDir Enum

	/// <summary>
	/// Decides which way Text is painted.
	/// </summary>
	//[System.Runtime.InteropServices.Guid("85B20D35-BF88-4287-B578-DB0E6DAD736E")]
	public enum TextDir
	{
		Normal = 0,
		UpsideDown = 1,
		Left = 2,
		Right = 3
	}

	#endregion TextDir Enum



	#region StringFormatEnum

	/// <summary>
	/// Decides the base StringFormat.
	/// </summary>
	public enum StringFormatEnum
	{
		GenericDefault = 0,
		GenericTypographic = 1
	}

	#endregion StringFormatEnum



	#region TreeViewMultiSelect Enum

	/// <summary>
	/// Decides the multi select characteristics of an MWTreeView Control.
	/// </summary>
	public enum TreeViewMultiSelect
	{
		Classic = 0,
		NoMulti = 1,
		Multi = 2,
		MultiSameBranchAndLevel = 3,
		MultiSameBranch = 4,
		MultiSameLevel = 5,
		MultiPathToParent = 6,
		MultiPathToParents = 7,
		SinglePathToParent = 8,
		SinglePathToParents = 9
	}

	#endregion TreeViewMultiSelect Enum



	#region MWCheckBoxPaintOrder

	/// <summary>
	/// Decides which order to Paint the Check, the Image and the Text for MWCheckBoxes (last is topmost).
	/// </summary>
	public enum CheckBoxPaintOrder
	{
		CheckImageText = 0,
		CheckTextImage = 1,
		ImageCheckText = 2,
		ImageTextCheck = 3,
		TextCheckImage = 4,
		TextImageCheck = 5
	}

	#endregion MWCheckBoxPaintOrder

}
