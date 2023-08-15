#pragma once

//see here: https://docs.microsoft.com/en-us/typography/opentype/spec/languagetags


class OpenTypeLang {
public:
    constexpr static int OTLangHintLen = 3; //see harfbuzz hb-ot-tag-table.hh
    typedef char HintStr[OTLangHintLen + 1];
    typedef struct OpenTypeLangTag {
        HintStr lang;
        const wchar_t* langDescription;
    } T;
    static OpenTypeLangTag OpenTypeLangTags[763];
};
