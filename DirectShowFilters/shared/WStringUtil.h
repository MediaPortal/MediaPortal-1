// Small std::wstring helpers shared by the filters.
#pragma once

#include <string>

// Replaces every occurrence of 'from' in 's' and returns how many were replaced.
// Same behaviour as CString::Replace (an empty 'from' replaces nothing).
inline int ReplaceAll(std::wstring& s, const std::wstring& from, const std::wstring& to)
{
  if (from.empty())
    return 0;

  int count = 0;
  for (size_t pos = s.find(from); pos != std::wstring::npos; pos = s.find(from, pos + to.size()))
  {
    s.replace(pos, from.size(), to);
    count++;
  }
  return count;
}
