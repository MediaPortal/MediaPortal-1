using System;

namespace MediaPortal.GUI.Library
{
	public interface IDialogbox
	{
		void Add(string strLabel);
		void Add(GUIListItem pItem);
		void AddLocalizedString(int iLocalizedString);
		void SetHeading(int iString);
		void SetHeading( string strLine);
		void DoModal(int dwParentId);
		void Reset();

		string SelectedLabelText { get; }
		int SelectedId { get; }
		int SelectedLabel { get; }
	}
}
