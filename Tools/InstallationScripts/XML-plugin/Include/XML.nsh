; TODO: ANSI codepage support
!include "${git_InstallScripts}\XML-plugin\Include\CallANSIPlugin.nsh"

!define xml::SetCondenseWhiteSpace `!insertmacro xml::SetCondenseWhiteSpace`

!macro xml::SetCondenseWhiteSpace _BOOL
	${PushAsANSI} `${_BOOL}`
	xml::_SetCondenseWhiteSpace /NOUNLOAD
!macroend


!define xml::SetEncoding `!insertmacro xml::SetEncoding`

!macro xml::SetEncoding _ENCODING
	${PushAsANSI} `${_ENCODING}`
	xml::_SetEncoding /NOUNLOAD
!macroend


!define xml::LoadFile `!insertmacro xml::LoadFile`

!macro xml::LoadFile _FILE _ERR
	${PushAsANSI} `${_FILE}`
	xml::_LoadFile /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::SaveFile `!insertmacro xml::SaveFile`

!macro xml::SaveFile _FILE _ERR
	${PushAsANSI} `${_FILE}`
	xml::_SaveFile /NOUNLOAD ;`${_FILE}`
	${PopAsANSI} ${_ERR}
!macroend


!define xml::DeclarationVersion `!insertmacro xml::DeclarationVersion`

!macro xml::DeclarationVersion _ERR1 _ERR2
	xml::_DeclarationVersion /NOUNLOAD
	${PopAsANSI} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::DeclarationEncoding `!insertmacro xml::DeclarationEncoding`

!macro xml::DeclarationEncoding _ERR1 _ERR2
	xml::_DeclarationEncoding /NOUNLOAD
	${PopAsANSI} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::DeclarationStandalone `!insertmacro xml::DeclarationStandalone`

!macro xml::DeclarationStandalone _ERR1 _ERR2
	xml::_DeclarationStandalone /NOUNLOAD
	${PopAsANSI} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::GetText `!insertmacro xml::GetText`

!macro xml::GetText _ERR1 _ERR2
	xml::_GetText /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::SetText `!insertmacro xml::SetText`

!macro xml::SetText _VALUE _ERR
	${PushAsUTF8} ${_VALUE}
	xml::_SetText /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::SetCDATA `!insertmacro xml::SetCDATA`

!macro xml::SetCDATA _BOOL _ERR
	${PushAsANSI} ${_BOOL}
	xml::_SetCDATA /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::IsCDATA `!insertmacro xml::IsCDATA`

!macro xml::IsCDATA _ERR
	xml::_IsCDATA /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::GetNodeValue `!insertmacro xml::GetNodeValue`

!macro xml::GetNodeValue _ERR
	xml::_GetNodeValue /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::SetNodeValue `!insertmacro xml::SetNodeValue`

!macro xml::SetNodeValue _VALUE
	${PushAsANSI} ${_VALUE}
	xml::_SetNodeValue /NOUNLOAD
!macroend


!define xml::FindNextElement `!insertmacro xml::FindNextElement`

!macro xml::FindNextElement _NAME _ERR1 _ERR2
	${PushAsUTF8} `${_NAME}`
	xml::_FindNextElement /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::FindCloseElement `!insertmacro xml::FindCloseElement`

!macro xml::FindCloseElement
	xml::_FindCloseElement /NOUNLOAD
!macroend


!define xml::RootElement `!insertmacro xml::RootElement`

!macro xml::RootElement _ERR1 _ERR2
	xml::_RootElement /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::FirstChildElement `!insertmacro xml::FirstChildElement`

!macro xml::FirstChildElement _NAME _ERR1 _ERR2
	${PushAsUTF8} `${_NAME}`
	xml::_FirstChildElement /NOUNLOAD ;`${_NAME}`
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::FirstChild `!insertmacro xml::FirstChild`

!macro xml::FirstChild _NAME _ERR1 _ERR2
	${PushAsUTF8} `${_NAME}`
	xml::_FirstChild /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::LastChild `!insertmacro xml::LastChild`

!macro xml::LastChild _NAME _ERR1 _ERR2
	${PushAsUTF8} `${_NAME}`
	xml::_LastChild /NOUNLOAD 
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::Parent `!insertmacro xml::Parent`

!macro xml::Parent _ERR1 _ERR2
	xml::_Parent /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::NoChildren `!insertmacro xml::NoChildren`

!macro xml::NoChildren _ERR
	xml::_NoChildren /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::NextSiblingElement `!insertmacro xml::NextSiblingElement`

!macro xml::NextSiblingElement _NAME _ERR1 _ERR2
	${PushAsUTF8} `${_NAME}`
	xml::_NextSiblingElement /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::NextSibling `!insertmacro xml::NextSibling`

!macro xml::NextSibling _NAME _ERR1 _ERR2
	${PushAsUTF8} `${_NAME}`
	xml::_NextSibling /NOUNLOAD `${_NAME}`
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::PreviousSibling `!insertmacro xml::PreviousSibling`

!macro xml::PreviousSibling _NAME _ERR1 _ERR2
	${PushAsUTF8} `${_NAME}`
	xml::_PreviousSibling /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::InsertAfterNode `!insertmacro xml::InsertAfterNode`

!macro xml::InsertAfterNode _HANDLE _ERR
	${PushAsANSI} `${_HANDLE}`
	xml::_InsertAfterNode /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::InsertBeforeNode `!insertmacro xml::InsertBeforeNode`

!macro xml::InsertBeforeNode _HANDLE _ERR
	${PushAsANSI} `${_HANDLE}`
	xml::_InsertBeforeNode /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::InsertEndChild `!insertmacro xml::InsertEndChild`

!macro xml::InsertEndChild _HANDLE _ERR
	${PushAsANSI} `${_HANDLE}`
	xml::_InsertEndChild /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::ReplaceNode `!insertmacro xml::ReplaceNode`

!macro xml::ReplaceNode _HANDLE _ERR
	${PushAsANSI} `${_HANDLE}`
	xml::_ReplaceNode /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::RemoveNode `!insertmacro xml::RemoveNode`

!macro xml::RemoveNode _ERR
	xml::_RemoveNode /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::RemoveAllChild `!insertmacro xml::RemoveAllChild`

!macro xml::RemoveAllChild
	xml::_RemoveAllChild /NOUNLOAD
!macroend


!define xml::CreateText `!insertmacro xml::CreateText`

!macro xml::CreateText _TEXT _ERR
	${PushAsUTF8} `${_TEXT}`
	xml::_CreateText /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::CreateNode `!insertmacro xml::CreateNode`

!macro xml::CreateNode _NODE _ERR
	${PushAsUTF8} `${_NODE}`
	xml::_CreateNode /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::CloneNode `!insertmacro xml::CloneNode`

!macro xml::CloneNode _ERR
	xml::_CloneNode /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::FreeNode `!insertmacro xml::FreeNode`

!macro xml::FreeNode _HANDLE _ERR
	${PushAsANSI} `${_HANDLE}`
	xml::_FreeNode /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::NodeHandle `!insertmacro xml::NodeHandle`

!macro xml::NodeHandle _ERR
	xml::_NodeHandle /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::GotoHandle `!insertmacro xml::GotoHandle`

!macro xml::GotoHandle _HANDLE _ERR
	${PushAsANSI} `${_HANDLE}`
	xml::_GotoHandle /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::XPathString `!insertmacro xml::XPathString`

!macro xml::XPathString _EXPRESSION _ERR1 _ERR2
	${PushAsUTF8} `${_EXPRESSION}`
	xml::_XPathString /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::XPathNode `!insertmacro xml::XPathNode`

!macro xml::XPathNode _EXPRESSION _ERR
	${PushAsUTF8} `${_EXPRESSION}`
	xml::_XPathNode /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::XPathAttribute `!insertmacro xml::XPathAttribute`

!macro xml::XPathAttribute _EXPRESSION _ERR
	${PushAsUTF8} `${_EXPRESSION}`
	xml::_XPathAttribute /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::ElementPath `!insertmacro xml::ElementPath`

!macro xml::ElementPath _ERR
	xml::_ElementPath /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::GotoPath `!insertmacro xml::GotoPath`

!macro xml::GotoPath _PATH _ERR
	${PushAsUTF8} `${_PATH}`
	xml::_GotoPath /NOUNLOAD ;`${_PATH}`
	${PopAsANSI} ${_ERR}
!macroend


!define xml::NodeType `!insertmacro xml::NodeType`

!macro xml::NodeType _ERR
	xml::_NodeType /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::Coordinate `!insertmacro xml::Coordinate`

!macro xml::Coordinate _ERR1 _ERR2 ;_ERR3
	xml::_Coordinate /NOUNLOAD
	${PopAsANSI} ${_ERR1}
	${PopAsANSI} ${_ERR2}
;	${PopAsANSI} ${_ERR3} ;[bug]xml::_Coordinate isn't pushing the error/success value
!macroend


!define xml::GetAttribute `!insertmacro xml::GetAttribute`

!macro xml::GetAttribute _NAME _ERR1 _ERR2
	${PushAsUTF8} `${_NAME}`
	xml::_GetAttribute /NOUNLOAD ;`${_NAME}`
	${PopAsUTF8} ${_ERR1}
	${PopAsANSI} ${_ERR2}
!macroend


!define xml::SetAttribute `!insertmacro xml::SetAttribute`

!macro xml::SetAttribute _NAME _VALUE _ERR
	${PushAsUTF8} `${_VALUE}`
	${PushAsUTF8} `${_NAME}`
	xml::_SetAttribute /NOUNLOAD ;`${_NAME}` `${_VALUE}`
	${PopAsANSI} ${_ERR}
!macroend


!define xml::RemoveAttribute `!insertmacro xml::RemoveAttribute`

!macro xml::RemoveAttribute _NAME _ERR
	${PushAsUTF8} `${_NAME}`
	xml::_RemoveAttribute /NOUNLOAD
	${PopAsANSI} ${_ERR}
!macroend


!define xml::FirstAttribute `!insertmacro xml::FirstAttribute`

!macro xml::FirstAttribute _ERR1 _ERR2 _ERR3
	xml::_FirstAttribute /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsUTF8} ${_ERR2}
	${PopAsANSI} ${_ERR3}
!macroend


!define xml::LastAttribute `!insertmacro xml::LastAttribute`

!macro xml::LastAttribute _ERR1 _ERR2 _ERR3
	xml::_LastAttribute /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsUTF8} ${_ERR2}
	${PopAsANSI} ${_ERR3}
!macroend


!define xml::NextAttribute `!insertmacro xml::NextAttribute`

!macro xml::NextAttribute _ERR1 _ERR2 _ERR3
	xml::_NextAttribute /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsUTF8} ${_ERR2}
	${PopAsANSI} ${_ERR3}
!macroend


!define xml::PreviousAttribute `!insertmacro xml::PreviousAttribute`

!macro xml::PreviousAttribute _ERR1 _ERR2 _ERR3
	xml::_PreviousAttribute /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsUTF8} ${_ERR2}
	${PopAsANSI} ${_ERR3}
!macroend


!define xml::CurrentAttribute `!insertmacro xml::CurrentAttribute`

!macro xml::CurrentAttribute _ERR1 _ERR2 _ERR3
	xml::_CurrentAttribute /NOUNLOAD
	${PopAsUTF8} ${_ERR1}
	${PopAsUTF8} ${_ERR2}
	${PopAsANSI} ${_ERR3}
!macroend


!define xml::SetAttributeName `!insertmacro xml::SetAttributeName`

!macro xml::SetAttributeName _NAME
	${PushAsUTF8} `${_NAME}`
	xml::_SetAttributeName /NOUNLOAD
!macroend


!define xml::SetAttributeValue `!insertmacro xml::SetAttributeValue`

!macro xml::SetAttributeValue _VALUE
	${PushAsUTF8} `${_NAME}`
	xml::_SetAttributeValue /NOUNLOAD
!macroend


!define xml::Unload `!insertmacro xml::Unload`

!macro xml::Unload
	xml::_Unload
!macroend

;CallANSIPlugin::Call "*$EXEDIR\Plugins\xml.dll" _LoadFile 1 `${_FILE}`
