
#include "stdafx.h"
#include "../../include/mpc-hc_config.h"

#if USE_LIBASS
#pragma comment( lib, "libass" )
#include <ios>
#include <algorithm>
#include <fstream>
#include <codecvt>
#include <Shlwapi.h>
#include "SSASub.h"
#include "STS.h"
#include "Utf8.h"

std::string ConsumeAttribute(const char** ppsz_subtitle, std::string& attribute_value) {
    const char* psz_subtitle = *ppsz_subtitle;
    char psz_attribute_name[BUFSIZ];
    char psz_attribute_value[BUFSIZ];

    while (*psz_subtitle == ' ')
        psz_subtitle++;

    size_t attr_len = 0;
    char delimiter;

    while (*psz_subtitle && isalpha(*psz_subtitle)) {
        psz_subtitle++;
        attr_len++;
    }

    if (!*psz_subtitle || attr_len == 0)
        return std::string();

    strncpy_s(psz_attribute_name, psz_subtitle - attr_len, attr_len);
    psz_attribute_name[attr_len] = 0;

    // Skip over to the attribute value
    while (*psz_subtitle && *psz_subtitle != '=')
        psz_subtitle++;

    // Skip the '=' sign
    psz_subtitle++;

    // Aknowledge the delimiter if any
    while (*psz_subtitle && isspace(*psz_subtitle))
        psz_subtitle++;

    if (*psz_subtitle == '\'' || *psz_subtitle == '"') {
        // Save the delimiter and skip it
        delimiter = *psz_subtitle;
        psz_subtitle++;
    } else
        delimiter = 0;

    // Skip spaces, just in case
    while (*psz_subtitle && isspace(*psz_subtitle))
        psz_subtitle++;

    // Skip the first #
    if (*psz_subtitle == '#')
        psz_subtitle++;

    attr_len = 0;
    while (*psz_subtitle && ((delimiter != 0 && *psz_subtitle != delimiter) ||
        (delimiter == 0 && (isalnum(*psz_subtitle) || *psz_subtitle == '#')))) {
        psz_subtitle++;
        attr_len++;
    }

    strncpy_s(psz_attribute_value, psz_subtitle - attr_len, attr_len);
    psz_attribute_value[attr_len] = 0;
    attribute_value.assign(psz_attribute_value);

    // Finally, skip over the final delimiter
    if (delimiter != 0 && *psz_subtitle)
        psz_subtitle++;

    *ppsz_subtitle = psz_subtitle;

    return std::string(psz_attribute_name);
}

void ParseSrtLine(std::string& srtLine, const STSStyle& style) {
    const char* psz_subtitle = srtLine.data();
    std::string subtitle_output;

    while (*psz_subtitle) {
        /* HTML extensions */
        if (*psz_subtitle == '<') {
            std::string tagname = GetTag(&psz_subtitle, false);
            if (!tagname.empty()) {
                // Convert tagname to lowercase
                std::transform(tagname.begin(), tagname.end(), tagname.begin(), ::tolower);
                if (tagname == "br") {
                    subtitle_output.append("\\N");
                } else if (tagname == "b") {
                    subtitle_output.append("{\\b1}");
                } else if (tagname == "i") {
                    subtitle_output.append("{\\i1}");
                } else if (tagname == "u") {
                    subtitle_output.append("{\\u1}");
                } else if (tagname == "s") {
                    subtitle_output.append("{\\s1}");
                } else if (tagname == "font") {
                    std::string attribute_name;
                    std::string attribute_value;

                    attribute_name = ConsumeAttribute(&psz_subtitle, attribute_value);
                    while (!attribute_name.empty()) {
                        // Convert attribute_name to lowercase
                        std::transform(attribute_name.begin(), attribute_name.end(), attribute_name.begin(), ::tolower);
                        if (attribute_name == "face") {
                            subtitle_output.append("{\\fn" + attribute_value + "}");
                        } else if (attribute_name == "family") {
                        }
                        if (attribute_name == "size") {
                            double resy = style.SrtResY / 288.0;
                            int font_size = (int)std::round(std::stod(attribute_value) * resy);
                            subtitle_output.append("{\\fs" + std::to_string(font_size) + "}");
                        } else if (attribute_name == "color") {
                            MatchColorSrt(attribute_value);

                            // If color is invalid, use WHITE
                            if ((strtoul(attribute_value.c_str(), NULL, 16) == 0) && (attribute_value != "000000"))
                                attribute_value.assign("FFFFFF");

                            // HTML is RGB and we need BGR for libass
                            swapRGBtoBGR(attribute_value);
                            subtitle_output.append("{\\c&H" + attribute_value + "&}");
                        }
                        attribute_name = ConsumeAttribute(&psz_subtitle, attribute_value);
                    }
                } else {
                    // This is an unknown tag. We need to hide it if it's properly closed, and display it otherwise
                    if (!IsClosed(psz_subtitle, tagname.c_str())) {
                        //subtitle_output.append("<" + tagname + ">");
                    } else {
                    }
                    // In any case, fall through and skip to the closing tag.
                }
                // Skip potential spaces & end tag
                while (*psz_subtitle && *psz_subtitle != '>')
                    psz_subtitle++;
                if (*psz_subtitle == '>')
                    psz_subtitle++;

            } else if (!strncmp(psz_subtitle, "</", 2)) {
                tagname = GetTag(&psz_subtitle, true);
                if (!tagname.empty()) {
                    std::transform(tagname.begin(), tagname.end(), tagname.begin(), ::tolower);
                    if (tagname == "b") {
                        subtitle_output.append("{\\b0}");
                    } else if (tagname == "i") {
                        subtitle_output.append("{\\i0}");
                    } else if (tagname == "u") {
                        subtitle_output.append("{\\u0}");
                    } else if (tagname == "s") {
                        subtitle_output.append("{\\s0}");
                    } else if (tagname == "font") {
                        double resy = style.SrtResY / 288.0;
                        int font_size = (int)std::round(style.fontSize * resy);
                        subtitle_output.append("{\\c}");
                        CT2CA tmpFontName(style.fontName);
                        subtitle_output.append("{\\fn" + std::string(tmpFontName) + "}");
                        subtitle_output.append("{\\fs" + std::to_string(font_size) + "}");
                    } else {
                        // Unknown closing tag. If it is closing an unknown tag, ignore it. Otherwise, display it
                        //subtitle_output.append("</" + tagname + ">");
                    }
                    while (*psz_subtitle == ' ')
                        psz_subtitle++;
                    if (*psz_subtitle == '>')
                        psz_subtitle++;
                }
            } else {
                /* We have an unknown tag, just append it, and move on.
                * The rest of the string won't be recognized as a tag, and
                * we will ignore unknown closing tag
                */
                subtitle_output.push_back('<');
                psz_subtitle++;
            }
        }
        /* MicroDVD extensions */
        /* FIXME:
        *  - Currently, we don't do difference between X and x, and we should:
        *    Capital Letters applies to the whole text and not one line
        *  - We don't support Position and Coordinates
        *  - We don't support the DEFAULT flag (HEADER)
        */
        else if (psz_subtitle[0] == '{' && psz_subtitle[2] == ':' && strchr(&psz_subtitle[2], '}')) {
            const char* psz_tag_end = strchr(&psz_subtitle[2], '}');
            size_t i_len = psz_tag_end - &psz_subtitle[3];

            if (psz_subtitle[1] == 'Y' || psz_subtitle[1] == 'y') {
                if (psz_subtitle[3] == 'i') {
                    subtitle_output.append("{\\i1}");
                    psz_subtitle++;
                }
                if (psz_subtitle[3] == 'b') {
                    subtitle_output.append("{\\b1}");
                    psz_subtitle++;
                }
                if (psz_subtitle[3] == 'u') {
                    subtitle_output.append("{\\u1}");
                    psz_subtitle++;
                }
            } else if ((psz_subtitle[1] == 'C' || psz_subtitle[1] == 'c')
                && psz_subtitle[3] == '$' && i_len >= 7) {
                /* Yes, they use BBGGRR */
                char psz_color[7];
                psz_color[0] = psz_subtitle[4]; psz_color[1] = psz_subtitle[5];
                psz_color[2] = psz_subtitle[6]; psz_color[3] = psz_subtitle[7];
                psz_color[4] = psz_subtitle[8]; psz_color[5] = psz_subtitle[9];
                psz_color[6] = '\0';
                subtitle_output.append("{\\c&H").append(psz_color).append("&}");
            } else if (psz_subtitle[1] == 'F' || psz_subtitle[1] == 'f') {
                std::string font_name(&psz_subtitle[3], i_len);
                subtitle_output.append("{\\fn" + font_name + "}");
            } else if (psz_subtitle[1] == 'S' || psz_subtitle[1] == 's') {
                int size = atoi(&psz_subtitle[3]);
                if (size) {
                    double resy = style.SrtResY / 288.0;
                    int font_size = (int)std::round(size * resy);
                    subtitle_output.append("{\\fs" + std::to_string(font_size) + "}");
                }
            }
            // Hide other {x:y} atrocities, notably {o:x}
            psz_subtitle = psz_tag_end + 1;
        } else {
            if (*psz_subtitle == '\n' || !_strnicmp(psz_subtitle, "\\n", 2)) {
                subtitle_output.append("\\N");

                if (*psz_subtitle == '\n')
                    psz_subtitle++;
                else
                    psz_subtitle += 2;
            } else if (*psz_subtitle == '\r') {
                psz_subtitle++;
            } else {
                subtitle_output.push_back(*psz_subtitle);
                psz_subtitle++;
            }
        }
    }
    srtLine.assign(subtitle_output);
}

void srt_header(char (&outBuffer)[1024], const STSStyle& style, const SubRendererSettings& subRendererSettings) {
    double resx = style.SrtResX / 384.0;
    double resy = style.SrtResY / 288.0;

    CT2CA tmpFontName(style.fontName);

    // Generate a standard ass header
    CStringA langTagStr = "";
    if (subRendererSettings.openTypeLangHint[0]) {
        CStringA tagLang(subRendererSettings.openTypeLangHint);
        tagLang.Replace(" ", "");
        langTagStr.Format("Language: %s\n", tagLang.GetBuffer());
    }

    _snprintf_s(outBuffer, _TRUNCATE, "[Script Info]\n"
        "; Script generated by MPC-HC\n"
        "Title: MPC-HC generated file\n"
        "ScriptType: v4.00+\n"
        "WrapStyle: 0\n"
        "ScaledBorderAndShadow: %s\n"
        "Kerning: %s\n"
        "YCbCr Matrix: TV.709\n"
        "PlayResX: %u\n"
        "PlayResY: %u\n"
        "%s" /*language if set*/
        "[V4+ Styles]\n"
        "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, "
        "BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, "
        "BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n"
        "Style: Default,%s,%u,&H%X,&H%X,&H%X,&H%X,0,0,0,0,%lf,%lf,%lf,0,%u,%lf,%lf,%u,%u,%u,%u,0"
        "\n\n[Events]\n"
        "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n\n",
        style.ScaledBorderAndShadow ? "yes" : "no",
        style.Kerning ? "yes" : "no",
        style.SrtResX, style.SrtResY,
        (LPCSTR)langTagStr,
        std::string(tmpFontName).c_str(), (int)std::round(style.fontSize * resy), style.colors[0],
        style.colors[1], style.colors[2], style.colors[3],
        style.fontScaleX, style.fontScaleY, style.fontSpacing, (style.borderStyle == 1 ? 4 : 1), style.outlineWidthX,
        style.shadowDepthX, style.scrAlignment, (int)std::round(style.marginRect.left * resx),
        (int)std::round(style.marginRect.right * resx), (int)std::round(style.marginRect.top * resy));
}

ASS_Track* srt_read_file(ASS_Library* library, char* fname, const UINT codePage, const STSStyle& style, const SubRendererSettings& subRendererSettings) {
    std::ifstream srtFile(fname, std::ios::in);
    ASS_Track* track = ass_new_track(library);
    track->name = _strdup(fname);
    return srt_read_data(library, track, srtFile, codePage, style, subRendererSettings);
}

ASS_Track* srt_read_data(ASS_Library* library, ASS_Track* track, std::istream &stream, const UINT codePage, const STSStyle& style, const SubRendererSettings& subRendererSettings) {
    // Convert SRT to ASS
    std::string lineIn;
    std::string lineOut;
    char inBuffer[1024];
    char outBuffer[1024];
    int start[4], end[4];

    srt_header(outBuffer, style, subRendererSettings);
    ass_process_data(track, outBuffer, static_cast<int>(strnlen_s(outBuffer, sizeof(outBuffer))));

    char BOM[4];
    stream.read(BOM, 3);
    if (stream.fail()) {
        return track;
    }
    bool streamIsUTF8 = true;
    if (BOM[0] == 0xEF && BOM[1] == 0xBB && BOM[2] == 0xBF) { //utf-8 BOM is here for some reason, we don't need it
        //we seeked past it
    } else {
        stream.seekg(std::ios_base::beg);
    }

    while (!stream.eof()) {
        stream.getline(inBuffer, sizeof(inBuffer) - 1);
        lineIn.assign(inBuffer);
        if (lineIn.empty())
            continue;

        if (streamIsUTF8) {
            streamIsUTF8 &= Utf8::isStringValid((const unsigned char*)lineIn.c_str(), lineIn.length());
        }

        // Read the timecodes
        if (sscanf_s(inBuffer, "%d:%2d:%2d%*1[,.]%3d --> %d:%2d:%2d%*1[,.]%3d", &start[0], &start[1],
            &start[2], &start[3], &end[0], &end[1], &end[2], &end[3]) == 8) {
            lineOut.clear();
            stream.getline(inBuffer, sizeof(inBuffer) - 1);
            lineIn.assign(inBuffer);
            while (!lineIn.empty()) {
                if (streamIsUTF8) {
                    streamIsUTF8 &= Utf8::isStringValid((const unsigned char*)lineIn.c_str(), lineIn.length());
                }
                lineOut.append(lineIn);
                stream.getline(inBuffer, sizeof(inBuffer) - 1);
                lineIn.assign(inBuffer);
                if (!lineIn.empty())
                    lineOut.append("\\N");
            }
            //lineOut.append("\n");

            if (!streamIsUTF8) {
                // Convert to UTF-8 only if UTF-8 not detected
                if (codePage != 0)
                    ConvertCPToUTF8(codePage, lineOut);
            }

            ParseSrtLine(lineOut, style);

            CT2CA tmpCustomTags(style.customTags);
            _snprintf_s(outBuffer, _TRUNCATE, "Dialogue: 0,%d:%02d:%02d.%02d,%d:%02d:%02d.%02d,Default,,0,0,0,,{\\blur%u}%s%s",
                start[0], start[1], start[2],
                (int)floor((double)start[3] / 10.0), end[0], end[1],
                end[2], (int)floor((double)end[3] / 10.0), style.fBlur, std::string(tmpCustomTags).c_str(), lineOut.c_str());
            ass_process_data(track, outBuffer, static_cast<int>(strnlen_s(outBuffer, sizeof(outBuffer))));
        }
    }
    ass_process_force_style(track);
    return track;
}

void ConvertCPToUTF8(UINT CP, std::string& codepage_str) {
    int size = MultiByteToWideChar(CP, MB_PRECOMPOSED, codepage_str.c_str(),
        (int)codepage_str.length(), nullptr, 0);

    std::wstring utf16_str(size, '\0');
    MultiByteToWideChar(CP, MB_PRECOMPOSED, codepage_str.c_str(),
        (int)codepage_str.length(), &utf16_str[0], size);

    int utf8_size = WideCharToMultiByte(CP_UTF8, 0, utf16_str.c_str(),
        (int)utf16_str.length(), nullptr, 0,
        nullptr, nullptr);

    std::string utf8_str(utf8_size, '\0');
    WideCharToMultiByte(CP_UTF8, 0, utf16_str.c_str(),
        (int)utf16_str.length(), &utf8_str[0], utf8_size,
        nullptr, nullptr);
    codepage_str.assign(utf8_str);
}

std::string GetTag(const char** line, bool b_closing) {
    const char* psz_subtitle = *line;

    if (*psz_subtitle != '<')
        return std::string();

    // Skip the '<'
    psz_subtitle++;

    if (b_closing && *psz_subtitle == '/')
        psz_subtitle++;

    // Skip potential spaces
    while (*psz_subtitle == ' ')
        psz_subtitle++;

    // Now we need to verify if what comes next is a valid tag:
    if (!isalpha(*psz_subtitle))
        return std::string();

    size_t tag_size = 1;
    while (isalnum(psz_subtitle[tag_size]) || psz_subtitle[tag_size] == '_')
        tag_size++;

    char psz_tagname[BUFSIZ];
    strncpy_s(psz_tagname, psz_subtitle, tag_size);
    psz_tagname[tag_size] = 0;
    psz_subtitle += tag_size;
    *line = psz_subtitle;

    return std::string(psz_tagname);
}

bool IsClosed(const char* psz_subtitle, const char* psz_tagname) {
    const char* psz_tagpos = StrStrIA(psz_subtitle, psz_tagname);

    if (!psz_tagpos)
        return false;

    // Search for '</' and '>' immediatly before & after (minding the potential spaces)
    const char* psz_endtag = psz_tagpos + strlen(psz_tagname);

    while (*psz_endtag == ' ')
        psz_endtag++;

    if (*psz_endtag != '>')
        return false;

    // Skip back before the tag itself
    psz_tagpos--;
    while (*psz_tagpos == ' ' && psz_tagpos > psz_subtitle)
        psz_tagpos--;

    if (*psz_tagpos-- != '/')
        return false;

    if (*psz_tagpos != '<')
        return false;

    return true;
}

void MatchColorSrt(std::string& fntColor) {
    for (auto i = 0; i < _countof(color_tag); ++i) {
        if (strcmp(fntColor.c_str(), color_tag[i].color) == 0) {
            fntColor.assign(color_tag[i].hex);
            break;
        }
    }
}

std::wstring s2ws(const std::string& str) {
    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;

    return converterX.from_bytes(str);
}

std::string ws2s(const std::wstring& wstr) {
    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;

    return converterX.to_bytes(wstr);
}

#endif
