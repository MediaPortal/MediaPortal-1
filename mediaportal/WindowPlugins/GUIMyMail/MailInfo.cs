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
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Topbar;
using MediaPortal.GUI.Pictures;
using MediaPortal.Player;
using System.Runtime.InteropServices;
using Core.Util;
namespace MyMail
{
	/// <summary>
	/// Zusammenfassung für MailInfo.
	/// </summary>
	public class MailInfo : GUIWindow
	{

		enum Controls
		{
			CONTROL_MAIL_FROM=20
			,CONTROL_MAIL_TO
			,CONTROL_MAIL_SUBJECT
			,CONTROL_MAIL_BODY=24
			,CONTROL_SHOW_HTML=25
			,CONTROL_SET_MAIL_IS_READ
			,CONTROL_SHOW_ATTACHMENTS
			,CONTROL_THUMB_VIEW=51

			
		}
		//
		//
		MediaPortal.Player.AudioPlayerWMP9 player=new AudioPlayerWMP9();
		eMail m_mailToDisplay;
		MailBox m_mailBox;
		bool m_viewText;
		
		public MailInfo()
		{
			GetID = 8001;
						
		}
		public struct ShellFileInfo 
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};


		[DllImport("shell32.dll")]
		public static extern IntPtr SHGetFileInfo(string filePath,uint dwFileAttributes,ref ShellFileInfo fileInfo,uint cbSizeFileInfo,uint uFlags);

		void SetLabel(int iControl,  string strLabel)
		{
			string strLabel1=strLabel;
    		
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET,GetID,0,iControl,0,0,null);
			msg.Label=(strLabel1);
			OnMessage(msg);
			

		}
		public eMail SetEMail
		{
			set
			{
				m_mailToDisplay=value;

			}
		}

		public MailBox SetMailBox
		{
			set{m_mailBox=value;}
		}
		public void ShowMail()
		{

		}
		public override bool Init()
		{

			bool bResult = Load(GUIGraphicsContext.Skin + @"\mailInfo.xml");
			return bResult;

		}
		public override bool OnMessage(GUIMessage message)
		{
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT :
					base.OnMessage(message);
     		  GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(8000));
					m_viewText=true; // show always the body text first
					GUIButtonControl htmlButton=(GUIButtonControl)GetControl((int)Controls.CONTROL_SHOW_HTML);
					if(htmlButton!=null)
					{
						if(m_mailToDisplay.HTML=="")
							htmlButton.Disabled=true;
						else
							htmlButton.Disabled=false;
					}
					BuildAttachmentList();
					Update();
					SwitchAttachmentButtonLabel(m_viewText);
					return true;
					

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
					break;

				case GUIMessage.MessageType.GUI_MSG_CLICKED : 
					int iControl = message.SenderControlId;
					if(iControl==(int)Controls.CONTROL_SHOW_HTML)
					{
						try
						{
							string fileName="file:///"+m_mailBox.AttachmentFolder+@"/mail-body.html";
							if(System.IO.File.Exists(m_mailBox.AttachmentFolder+@"/mail-body.html"))
							{
								Process openHtmlMail=new Process();
								openHtmlMail.StartInfo.FileName= fileName;
								//openHtmlMail.StartInfo.Arguments=fileName;
								openHtmlMail.Start();
								openHtmlMail.Close();
								openHtmlMail.Dispose();
							}
						}
						catch(Exception )
						{ 
						}

							//m_mailBox.DeleteMail(m_mailToDisplay);
						//GUIWindowManager.ActivateWindow(8000);
					}
					if(iControl==(int)Controls.CONTROL_SHOW_ATTACHMENTS)
					{
						m_viewText=!m_viewText;
						BuildAttachmentList();
						Update();
						SwitchAttachmentButtonLabel(m_viewText);
					}
					if(iControl==(int)Controls.CONTROL_SET_MAIL_IS_READ)
					{
						m_mailToDisplay.SetRead=true;
						GUIWindowManager.ActivateWindow(8000);
					}
					if(iControl==(int)Controls.CONTROL_THUMB_VIEW)
					{
						OnClick(iControl);
					}
					break;
				

			}

			return base.OnMessage(message);
		}
		//
		void SwitchAttachmentButtonLabel(bool state)
		{
			GUIButtonControl attachmentButton=(GUIButtonControl)GetControl((int)Controls.CONTROL_SHOW_ATTACHMENTS);
			if(attachmentButton!=null)
			{
				if(state==true)
					attachmentButton.Label=GUILocalizeStrings.Get(8022);
				else
					attachmentButton.Label=GUILocalizeStrings.Get(8024);
			}
		}
		//
		void OnClick(int iItem)
		{
			GUIListItem item = GetSelectedItem();
			int attachmentKind=-1;
			if (item==null) return;
			try
			{
				attachmentKind=int.Parse(item.Label2);
			}
			catch
			{}
			if(attachmentKind==1) // image
				OnShowPicture(item.Path);  
			if(attachmentKind==2)
				player.Play(item.Path);
				
			
		}
		//
		GUIListItem GetSelectedItem()
		{
			int iControl;
			iControl=(int)Controls.CONTROL_THUMB_VIEW;
			
			GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
			return item;
		}
		int GetSelectedItemNo()
		{
			int iControl;
			iControl=(int)Controls.CONTROL_THUMB_VIEW;

			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
			OnMessage(msg);         
			int iItem=(int)msg.Param1;
			return iItem;
		}

		void OnShowPicture(string strFile)
		{
			GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
			if (SlideShow==null) return;

			SlideShow.Reset();
			SlideShow.Add(strFile);
			SlideShow.Select(strFile);
			GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
		}
		//
		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				GUIWindowManager.ShowPreviousWindow();
				return;
			}
			base.OnAction(action);
		}
		//
		void BuildAttachmentList()
		{
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_THUMB_VIEW);
			// show attachments
			GUIThumbnailPanel thePanel=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMB_VIEW);
			ArrayList listItems=new ArrayList();
			listItems.Clear();
			if(thePanel!=null)
			{
				ArrayList theList=new ArrayList();
				theList.Clear();
				try
				{
					m_mailToDisplay.GetAttachmentList(ref theList);
					string path=m_mailToDisplay.AttachmentsPath+@"\";
					foreach(MailClass.MailAttachment mailAttachment in theList)
					{
						GUIListItem theItem=new GUIListItem(mailAttachment.attFileName);
						theItem.Path=path+mailAttachment.attFileName;
						try // try to get a icon from the shell
						{
							System.Drawing.Bitmap bitmp=new Bitmap(32,32);
							ShellFileInfo fileInfo=new ShellFileInfo();
							IntPtr hImgSmall = SHGetFileInfo(theItem.Path, 0, ref fileInfo,(uint)Marshal.SizeOf(fileInfo),0x100);
							System.Drawing.Icon fileIcon =System.Drawing.Icon.FromHandle(fileInfo.hIcon);
							bitmp=fileIcon.ToBitmap();
							bitmp.Save(path+mailAttachment.attFileName+".png",System.Drawing.Imaging.ImageFormat.Png);
							theItem.ThumbnailImage=path+mailAttachment.attFileName+".png";
						}
						catch
						{
							theItem.ThumbnailImage="defaultHardDiskBig.png";
						}
					
						theItem.FileInfo=new FileInformation(theItem.Path);
						theItem.Label3=Convert.ToString(theItem.FileInfo.Length);
						theItem.IsFolder=false;
						theItem.Label2=Convert.ToString(mailAttachment.attKind);
						listItems.Add(theItem);
					
					}
					thePanel.IsVisible=!m_viewText;
					foreach(GUIListItem item in listItems)
					{
						GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMB_VIEW,item);
					}
				}
				catch{}

			}
		}
		void Update()
		{
			if (m_mailToDisplay==null) return;
			GUIButtonControl attButton=(GUIButtonControl)GetControl((int)Controls.CONTROL_SHOW_ATTACHMENTS);
			ArrayList List=new ArrayList();
			m_mailToDisplay.GetAttachmentList(ref List);
			if(List.Count>0)
			{
				if(attButton!=null)
				   attButton.Disabled=false;
			}
			else
			{
				if(attButton!=null)
					attButton.Disabled=true;
			}
			GUIFadeLabel fader=(GUIFadeLabel)GetControl((int)Controls.CONTROL_MAIL_SUBJECT);
			SetLabel((int)Controls.CONTROL_MAIL_FROM, m_mailToDisplay.From );
			SetLabel((int)Controls.CONTROL_MAIL_TO, m_mailToDisplay.To );
			if(fader!=null)
			{
				fader.IsVisible=true;
				fader.Label=m_mailToDisplay.Subject;
				fader.AllowScrolling=true;

			}
			GUITextControl textBox=(GUITextControl)GetControl((int)Controls.CONTROL_MAIL_BODY);
			if(textBox!=null)
			{
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_MAIL_BODY,m_mailToDisplay.Body);
				textBox.IsVisible=m_viewText;
			}

		
		}

	}
}
