;FontName include file for NSIS
;Written by Vytautas Krivickas (http://forums.winamp.com/member.php?s=&action=getinfo&userid=111891)
;
;If an error was generated the stack contains the translated error message
;and the error flag is set
;
;
;Translated To         - Translated By
;----------------------------------------------------------
;English (Default)     - Vytautas Krivickas
;Lithuanian            - Vytautas Krivickas
;German                - Jan T. Sott
;Hebrew                - kichik
;Portuguese (Brazil)   - deguix
;Arabic                - asdfuae
;Chinese (Traditional) - Kii Ali <kiiali@cpatch.org>
;Chinese (Simplified)  - Kii Ali <kiiali@cpatch.org>
;French                - evilO/Olive
;Spanish (Traditional) - Cecilio
;Macedonian            - Sasko Zdravkin <wingman2083@yahoo.com>

; Macros to use with FontName Plugin

!macro FontNameVer
  call TranslateFontName
  FontName::Version
!macroend

!macro FontName FONTFILE
  push ${FONTFILE}
  call TranslateFontName
  FontName::Name
  call CheckFontNameError
!macroend

; Private Functions - Called by the macros

Function TranslateFontName
  !define Index "LINE-${__LINE__}"

  StrCmp $LANGUAGE 1063 0 End-1063 ; Lithuanian (by Vytautas Krivickas)
    Push "Neteisinga šrifto versija"
    Push "Planines bylos adreso klaida: %u"
    Push "Planines bylos sukurimo klaida: %u"
    Push "Neteisingas bylos dydis: %u"
    Push "Neteisinga bylos rankena: %u"
    Push "FontName %s ijungiamoji byla i NSIS"
    goto ${Index}
  End-1063:

  StrCmp $LANGUAGE 1031 0 End-1031 ; German (by Jan T. Sott)
    Push "Falsche Fontversion"
    Push "MappedFile Addressfehler: %u"
    Push "MappedFile Fehler: %u"
    Push "Ungültige Dateigrösse: %u"
    Push "Ungültiges Dateihandle %u"
    Push "FontName %s Plugin für NSIS"
    goto ${Index}
  End-1031:

  StrCmp $LANGUAGE 1037 0 End-1037 ; Hebrew (by kichik)
    Push "âøñú ëåôï ùâåéä"
    Push "ùâéàú ëúåáú ÷åáõ îîåôä: %u"
    Push "ùâéàú ÷åáõ îîåôä: %u"
    Push "âåãì ÷åáõ ìà çå÷é: %u"
    Push "éãéú ÷åáõ ìà çå÷éú %u"
    Push "FontName %s plugin for NSIS"
    goto ${Index}
  End-1037:

  StrCmp $LANGUAGE 1046 0 End-1046 ; Portuguese (Brazil) (by deguix)
    Push "Versão de Fonte Errada"
    Push "Erro de Endereço do ArquivoMapeado: %u"
    Push "Erro do ArquivoMapeado: %u"
    Push "Tamanho de arquivo inválido: %u"
    Push "Manuseio de arquivo inválido %u"
    Push "FontName %s plugin para NSIS"
    goto ${Index}
  End-1046:

  StrCmp $LANGUAGE 1025 0 End-1025 ; Arabic (by asdfuae)
    Push "ÅÕÏÇÑ ÇáÎØ ÎÇØÆ"
    Push "ÎØÇÁ ÚäæÇä ÎÑíØÉÇáãáİ: %u"
    Push "ÎØÇÁ ÎÑíØÉ Çáãáİ: %u"
    Push "ÍÌã Çáãáİ ÛíÑÕÍíÍ: %u"
    Push "ãÚÇáÌ Çáãáİ ÛíÑ ÕÍíÍ %u"
    Push "ãŞÈÓ ÇÓã ÇáÎØ %s áäÓíÓ"
    goto ${Index}
  End-1025:

  StrCmp $LANGUAGE 1028 0 End-1028 ; Chinese (Traditional) by Kii Ali <kiiali@cpatch.org>
    Push "¿ù»~ªº¦r«¬ª©¥»"
    Push "¹ïÀ³ÀÉ®×¦ì§}¿ù»~: %u"
    Push "¹ïÀ³ÀÉ®×¿ù»~: %u"
    Push "µL®ÄªºÀÉ®×¤j¤p: %u"
    Push "µL®ÄªºÀÉ®×¬`µ{: %u"
    Push "¥Î©ó NSIS ªº¦r«¬¦WºÙ %s ´¡¥ó"
    goto ${Index}
  End-1028:

  StrCmp $LANGUAGE 2052 0 End-2052 ; Chinese (Simplified) by Kii Ali <kiiali@cpatch.org>
    Push "´íÎóµÄ×ÖÌå°æ±¾"
    Push "Ó³ÉäÎÄ¼şµØÖ·´íÎó: %u"
    Push "Ó³ÉäÎÄ¼ş´íÎó: %u"
    Push "ÎŞĞ§µÄÎÄ¼ş´óĞ¡: %u"
    Push "ÎŞĞ§µÄÎÄ¼ş±ú³Ì: %u"
    Push "ÓÃÓÚ NSIS µÄ×ÖÌåÃû³Æ %s ²å¼ş"
    goto ${Index}
  End-2052:

  StrCmp $LANGUAGE 1036 0 End-1036 ; French by evilO/Olive
    Push "Version de police incorrecte"
    Push "Erreur d'adresse du fichier mappé : %u"
    Push "Erreur de fichier mappé : %u"
    Push "Taille de fichier invalide : %u"
    Push "Descripteur de fichier invalide %u"
    Push "FontName %s plugin pour NSIS"
    goto ${Index}
  End-1036:

  StrCmp $LANGUAGE 1034 0 End-1034 ; Spanish (traditional) by Cecilio
    Push "Versión del font incorrecta"
    Push "Error de dirección de archivo mapeado: %u"
    Push "Error de archivo mapeado: %u"
    Push "Tamaño de archivo erroneo: %u"
    Push "Manipulador de archivo erroneo: %u"
    Push "Plugin de NSIS para FontName %s "
    goto ${Index}
  End-1034:

  StrCmp $LANGUAGE 1071 0 End-1071 ; Macedonian by Sasko Zdravkin <wingman2083@yahoo.com>
    Push "Ïîãğåøíà âåğçè¼à íà Ôîíòîò"
    Push "ÌàïèğàíàòàÄàòîòåêà Ãğåøêà íà àäğåñàòà: %u"
    Push "ÌàïèğàíàòàÄàòîòåêà Ãğåøêà: %u"
    Push "Ïîãğåøíà ãîëåìèíà íà äàòîòåêàòà: %u"
    Push "Ïîãğåøíî ğàêóâàœå ñî äàòîòåêàòà: %u"
    Push "FontName %s ïëóãèí çà NSIS"
    goto ${Index}
  End-1071:

; Add your languages here

  ; Default English (1033) by Vytautas Krivickas - MUST REMAIN LAST!
  Push "Wrong Font Version"
  Push "MappedFile Address Error: %u"
  Push "MappedFile Error: %u"
  Push "Invalid file size: %u"
  Push "Invalid file handle %u"
  Push "FontName %s plugin for NSIS"
  goto ${Index}

${Index}:
  !undef Index
FunctionEnd

Function CheckFontNameError
  !define Index "LINE-${__LINE__}"

  exch $1
  strcmp $1 "*:*" 0 Index
    pop $1
    exch $1
    SetErrors

Index:
  exch $1
  !undef Index
FunctionEnd
