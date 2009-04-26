#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

!ifndef ___SELECT_OPTION_PAGE__NSH___
!define ___SELECT_OPTION_PAGE__NSH___

!include WordFunc.nsh
!include FileFunc.nsh

;!include include\nsDialogs_setImageOle.nsh

!macro SelectOptionPage ID OPTION0 OPTION1

#####    Server/Service Mode page
; $ServerServiceMode 0 = InputService
; $ServerServiceMode 1 = IRServer
Var SelectOption${ID}
Var SelectOptionPage${ID}.optBtn0
Var SelectOptionPage${ID}.optBtn0.state
Var SelectOptionPage${ID}.optImg0
Var SelectOptionPage${ID}.optImg0.handle
Var SelectOptionPage${ID}.optBtn1
Var SelectOptionPage${ID}.optBtn1.state
Var SelectOptionPage${ID}.optImg1
Var SelectOptionPage${ID}.optImg1.handle


Function SelectOptionPage${ID}
  Push $0

  !insertmacro MUI_HEADER_TEXT "$(SelectOptionPage${ID}_HEADER)" "$(SelectOptionPage${ID}_HEADER2)"

  nsDialogs::Create /NOUNLOAD 1018

  ${NSD_CreateLabel} 0 0 300u 24u "$(SelectOptionPage${ID}_INFO)"
  Pop $0



  ${NSD_CreateRadioButton} 0 30u 145u 8u "$(SelectOptionPage${ID}_OPT0)"
  Pop $SelectOptionPage${ID}.optBtn0
  ${NSD_OnClick} $SelectOptionPage${ID}.optBtn0 SelectOptionPage${ID}UpdateSelection

  ${NSD_CreateBitmap} 0 45u 145u 145u ""
  Pop $SelectOptionPage${ID}.optImg0
  ${NSD_SetImage} $SelectOptionPage${ID}.optImg0 "$PLUGINSDIR\preview${OPTION0}.bmp" $SelectOptionPage${ID}.optImg0.handle


  ${NSD_CreateRadioButton} 155u 30u 145u 8u "$(SelectOptionPage${ID}_OPT1)"
  Pop $SelectOptionPage${ID}.optBtn1
  ${NSD_OnClick} $SelectOptionPage${ID}.optBtn1 SelectOptionPage${ID}UpdateSelection

  ${NSD_CreateBitmap} 155u 45u 145u 145u ""
  Pop $SelectOptionPage${ID}.optImg1
  ${NSD_SetImage} $SelectOptionPage${ID}.optImg1 "$PLUGINSDIR\preview${OPTION1}.bmp" $SelectOptionPage${ID}.optImg1.handle



  ; set current ServerServiceMode to option buttons
  ${If} $SelectOption${ID} == 1
    ${NSD_Check} $SelectOptionPage${ID}.optBtn1
  ${Else}
    ${NSD_Check} $SelectOptionPage${ID}.optBtn0
  ${EndIf}

  nsDialogs::Show

  Pop $0
FunctionEnd

Function SelectOptionPage${ID}UpdateSelection

  ${NSD_GetState} $SelectOptionPage${ID}.optBtn0 $SelectOptionPage${ID}.optBtn0.state
  ${NSD_GetState} $SelectOptionPage${ID}.optBtn1 $SelectOptionPage${ID}.optBtn1.state

  ${If} $SelectOptionPage${ID}.optBtn1.state == ${BST_CHECKED}
    StrCpy $SelectOption${ID} 1
  ${Else}
    StrCpy $SelectOption${ID} 0
  ${EndIf}

FunctionEnd

Page custom SelectOptionPage${ID}

!macroend


!macro SelectOptionOnInit ID OPTION0 OPTION1
  StrCpy $SelectOption${ID} 0

  File "/oname=$PLUGINSDIR\preview${OPTION0}.bmp" "${svn_xface}\Customize\${ID}\${OPTION0}\preview.bmp"
  File "/oname=$PLUGINSDIR\preview${OPTION1}.bmp" "${svn_xface}\Customize\${ID}\${OPTION1}\preview.bmp"
!macroend


!macro SelectOptionInstall ID OPTION0 OPTION1
  ${If} $SelectOption${ID} == 1
    DetailPrint "Install SelectOption${ID} option ${OPTION0}"
    File /r /x svn /x preview.bmp "${svn_xface}\Customize\${ID}\${OPTION0}\*.*"
  ${Else}
    DetailPrint "Install SelectOption${ID} option ${OPTION1}"
    File /r /x svn /x preview.bmp "${svn_xface}\Customize\${ID}\${OPTION1}\*.*"
  ${EndIf}
!macroend


!endif # !___SELECT_OPTION_PAGE__NSH___
