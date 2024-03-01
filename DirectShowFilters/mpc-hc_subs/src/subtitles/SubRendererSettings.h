#pragma once
#include "OpenTypeLangTags.h"

class SubRendererSettings {
public:
    SubRendererSettings() {}
    virtual ~SubRendererSettings() {}
    int renderUsingLibass = false;
    OpenTypeLang::HintStr openTypeLangHint = { 0 };
};
