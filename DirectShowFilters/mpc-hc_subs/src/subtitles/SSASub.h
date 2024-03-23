#pragma once
#include "ass/ass.h"
#include <string>
#include <streambuf>
#include "SubRendererSettings.h"

class STSStyle;

struct ASS_LibraryDeleter {
    void operator()(ASS_Library* p) { if (p) ass_library_done(p); }
};

struct ASS_RendererDeleter {
    void operator()(ASS_Renderer* p) { if (p) ass_renderer_done(p); }
};

struct ASS_TrackDeleter {
    void operator()(ASS_Track* p) { if (p) ass_free_track(p); }
};

std::string ConsumeAttribute(const char** ppsz_subtitle, std::string& attribute_value);
ASS_Track* srt_read_file(ASS_Library* library, char* fname, const UINT codePage, const STSStyle& style, const SubRendererSettings& subRendererSettings);
ASS_Track* srt_read_data(ASS_Library* library, ASS_Track* track, std::istream &stream, const UINT codePage, const STSStyle& style, const SubRendererSettings& subRendererSettings);
void srt_header(char (&outBuffer)[1024], const STSStyle& style, const SubRendererSettings& subRendererSettings);
void ConvertCPToUTF8(UINT CP, std::string& codepage_str);
std::string GetTag(const char** line, bool b_closing);
bool IsClosed(const char* psz_subtitle, const char* psz_tagname);
void ParseSrtLine(std::string& srtLine, const STSStyle& style);
std::string GetTag(const char** line, bool b_closing);
void MatchColorSrt(std::string& fntColor);
std::string ws2s(const std::wstring& wstr);
std::wstring s2ws(const std::string& str);

inline void swapRGBtoBGR(std::string& color) {
    std::string tmp = color;

    color[0] = tmp[4];
    color[1] = tmp[5];
    color[4] = tmp[0];
    color[5] = tmp[1];
}

static const struct s_color_tag {
    const char* color;
    const char* hex;
} color_tag[] = {
    //    name              hex value
        { "aqua",           "00FFFF" },
        { "azure",          "F0FFFF" },
        { "beige",          "F5F5DC" },
        { "black",          "000000" },
        { "blue",           "0000FF" },
        { "brown",          "A52A2A" },
        { "chartreuse",     "7FFF00" },
        { "chocolate",      "D2691E" },
        { "coral",          "FF7F50" },
        { "crimson",        "DC143C" },
        { "cyan",           "00FFFF" },
        { "fuchsia",        "FF00FF" },
        { "gold",           "FFD700" },
        { "gray",           "808080" },
        { "green",          "008000" },
        { "grey",           "808080" },
        { "indigo",         "4B0082" },
        { "ivory",          "FFFFF0" },
        { "khaki",          "F0E68C" },
        { "lavender",       "E6E6FA" },
        { "lime",           "00FF00" },
        { "linen",          "FAF0E6" },
        { "magenta",        "FF00FF" },
        { "maroon",         "800000" },
        { "navy",           "000080" },
        { "olive",          "808000" },
        { "orange",         "FFA500" },
        { "orchid",         "DA70D6" },
        { "pink",           "FFC0CB" },
        { "plum",           "DDA0DD" },
        { "purple",         "800080" },
        { "red",            "FF0000" },
        { "salmon",         "FA8072" },
        { "sienna",         "A0522D" },
        { "silver",         "C0C0C0" },
        { "snow",           "FFFAFA" },
        { "tan",            "D2B48C" },
        { "teal",           "008080" },
        { "thistle",        "D8BFD8" },
        { "tomato",         "FF6347" },
        { "turquoise",      "40E0D0" },
        { "violet",         "EE82EE" },
        { "white",          "FFFFFF" },
        { "yellow",         "FFFF00" },
};
