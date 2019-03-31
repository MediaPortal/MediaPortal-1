/*
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#pragma once
#include <algorithm>  // copy(), min()
#include <cstring>    // strcmp(), strlen(), strncpy()
#include <map>
#include <sstream>
#include <string>
#include <vector>

using namespace std;


extern void LogDebug(const wchar_t* fmt, ...);

class CUtils
{
  public:
    template<class T> static void CleanUpStringSet(map<T, char*>& stringSet)
    {
      map<T, char*>::iterator it = stringSet.begin();
      for ( ; it != stringSet.end(); it++)
      {
        char* s = it->second;
        if (s != NULL)
        {
          delete[] s;
          it->second = NULL;
        }
      }
      stringSet.clear();
    }

    template<class T1, class T2> static void CleanUpMapOfStringSets(map<T1, map<T2, char*>*>& mapOfStringSets)
    {
      map<T1, map<T2, char*>*>::iterator it = mapOfStringSets.begin();
      for ( ; it != mapOfStringSets.end(); it++)
      {
        map<T2, char*>* stringSet = it->second;
        if (stringSet != NULL)
        {
          CleanUpStringSet(*stringSet);
          delete stringSet;
          it->second = NULL;
        }
      }
      mapOfStringSets.clear();
    }

    template<class T1, class T2> static bool CompareMaps(const map<T1, T2>& m1,
                                                          const map<T1, T2>& m2)
    {
      if (m1.size() != m2.size())
      {
        return false;
      }
      map<T1, T2>::const_iterator it1 = m1.begin();
      for ( ; it1 != m1.end(); it1++)
      {
        map<T1, T2>::const_iterator it2 = m2.find(it1->first);
        if (it2 == m2.end() || it1->second != it2->second)
        {
          return false;
        }
      }
      return true;
    }

    static bool CompareStrings(const char* s1, const char* s2)
    {
      if (
        (s1 == NULL && s2 != NULL) ||
        (s1 != NULL && s2 == NULL) ||
        (s1 != NULL && strcmp(s1, s2) != 0)
      )
      {
        return false;
      }
      return true;
    }

    template<class T> static bool CompareStringSets(const map<T, char*>& ss1,
                                                    const map<T, char*>& ss2)
    {
      if (ss1.size() != ss2.size())
      {
        return false;
      }
      map<T, char*>::const_iterator it1 = ss1.begin();
      for ( ; it1 != ss1.end(); it1++)
      {
        map<T, char*>::const_iterator it2 = ss2.find(it1->first);
        if (it2 == ss2.end() || !CompareStrings(it1->second, it2->second))
        {
          return false;
        }
      }
      return true;
    }

    template<class T1, class T2> static bool CompareMapsOfStringSets(const map<T1, map<T2, char*>*>& mss1,
                                                                      const map<T1, map<T2, char*>*>& mss2)
    {
      if (mss1.size() != mss2.size())
      {
        return false;
      }
      map<T1, map<T2, char*>*>::const_iterator it1 = mss1.begin();
      for ( ; it1 != mss1.end(); it1++)
      {
        map<T1, map<T2, char*>*>::const_iterator it2 = mss2.find(it1->first);
        if (
          it2 == mss2.end() ||
          (it1->second == NULL && it2->second != NULL) ||
          (it1->second != NULL && it2->second == NULL) ||
          (it1->second != NULL && !CompareStringSets(*(it1->second), *(it2->second)))
        )
        {
          return false;
        }
      }
      return true;
    }

    template<class T> static bool CompareVectors(const vector<T>& v1, const vector<T>& v2)
    {
      if (v1.size() != v2.size())
      {
        return false;
      }
      vector<T>::const_iterator it = v1.begin();
      for ( ; it != v1.end(); it++)
      {
        if (find(v2.begin(), v2.end(), *it) == v2.end())
        {
          return false;
        }
      }
      return true;
    }

    template<class T> static bool CopyArrayToArray(const T* source,
                                                    unsigned char sourceSize,
                                                    T* destination,
                                                    unsigned char& destinationSize)
    {
      if (source == NULL || sourceSize == 0)
      {
        destinationSize = 0;
        return true;
      }
      if (destination == NULL)
      {
        destinationSize = 0;
        return false;
      }

      destinationSize = min(destinationSize, sourceSize);
      if (destinationSize > 0)
      {
        copy(source, source + destinationSize, destination);
      }
      return destinationSize == sourceSize;
    }

    static bool CopyStringToBuffer(const char* source,
                                    char* destination,
                                    unsigned short& destinationBufferSize,
                                    unsigned short& requiredBufferSize)
    {
      if (source == NULL)
      {
        if (destination != NULL && destinationBufferSize != 0)
        {
          destination[0] = NULL;
          destinationBufferSize = 1;
        }
        else
        {
          destinationBufferSize = 0;
        }
        requiredBufferSize = 1;
        return true;
      }

      requiredBufferSize = strlen(source) + 1;
      if (destination == NULL)
      {
        destinationBufferSize = 0;
        return false;
      }
      if (requiredBufferSize <= destinationBufferSize)
      {
        strncpy(destination, source, requiredBufferSize);
        destinationBufferSize = requiredBufferSize;
        return true;
      }

      strncpy(destination, source, destinationBufferSize - 1);
      destination[destinationBufferSize - 1] = NULL;
      return false;
    }

    template<class T1, class T2> static bool CopyVectorToArray(const vector<T1>& source,
                                                                T1* destination,
                                                                T2& destinationSize,
                                                                T2& requiredSize)
    {
      requiredSize = source.size();
      if (source.size() == 0)
      {
        destinationSize = 0;
        return true;
      }
      if (destination == NULL)
      {
        destinationSize = 0;
        return false;
      }

      destinationSize = min(destinationSize, requiredSize);
      if (destinationSize > 0)
      {
        copy(source.begin(), source.begin() + destinationSize, destination);
      }
      return destinationSize == requiredSize;
    }

    template<class T1, class T2> static void DebugMap(const map<T1, T2>& m,
                                                      const wchar_t* mapName,
                                                      const wchar_t* keyName,
                                                      const wchar_t* elementName,
                                                      const wchar_t* indent = L"  ")
    {
      if (m.size() == 0)
      {
        return;
      }

      if (mapName != NULL)
      {
        LogDebug(L"%s%s...", indent == NULL ? L"" : indent, mapName);
      }
      map<T1, T2>::const_iterator it = m.begin();
      for ( ; it != m.end(); it++)
      {
        wstringstream tempStream;
        if (indent != NULL)
        {
          tempStream << indent;
        }
        tempStream << L"  ";
        if (keyName != NULL)
        {
          tempStream << keyName << L" = ";
        }
        tempStream << it->first << L", ";
        if (elementName != NULL)
        {
          tempStream << elementName << L" = ";
        }
        tempStream << it->second;
        wstring tempString(tempStream.str());
        LogDebug(tempString.c_str());
      }
    }

    template<class T1, class T2> static void DebugMapOfStringSets(const map<T1, map<T2, char*>*>& mss,
                                                                  const wchar_t* mapName,
                                                                  const wchar_t* key1Name,
                                                                  const wchar_t* key2Name,
                                                                  const wchar_t* elementName)
    {
      if (mss.size() == 0)
      {
        return;
      }

      if (mapName != NULL)
      {
        LogDebug(L"  %s...", mapName);
      }
      map<T1, map<T2, char*>*>::const_iterator it = mss.begin();
      for ( ; it != mss.end(); it++)
      {
        wstringstream tempStream;
        tempStream << L"    ";
        if (key1Name != NULL)
        {
          tempStream << key1Name << L" = ";
        }
        tempStream << it->first << L"...";
        wstring tempString(tempStream.str());
        LogDebug(tempString.c_str());

        if (it->second != NULL)
        {
          DebugStringMap(*(it->second), NULL, key2Name, elementName, L"    ");
        }
      }
    }

    template<class T> static void DebugStringMap(const map<T, char*>& m,
                                                  const wchar_t* setName,
                                                  const wchar_t* keyName,
                                                  const wchar_t* elementName,
                                                  const wchar_t* indent = L"  ")
    {
      if (m.size() == 0)
      {
        return;
      }

      if (setName != NULL)
      {
        LogDebug(L"%s%s...", indent == NULL ? L"" : indent, setName);
      }
      map<T, char*>::const_iterator it = m.begin();
      for ( ; it != m.end(); it++)
      {
        LogDebug(L"%s  %s = %S, %s = %S",
                  indent == NULL ? L"" : indent, keyName, (char*)&(it->first),
                  elementName, it->second == NULL ? "" : it->second);
      }
    }

    template<class T> static void DebugVector(const vector<T>& v,
                                              const wchar_t* name,
                                              bool elementsAreStrings,
                                              const wchar_t* indent = L"  ")
    {
      if (v.size() == 0)
      {
        return;
      }

      wstringstream tempStream;
      if (indent != NULL)
      {
        tempStream << indent;
      }
      if (name != NULL)
      {
        tempStream << name << L" = ";
      }
      vector<T>::const_iterator it = v.begin();
      for ( ; it != v.end(); it++)
      {
        if (it != v.begin())
        {
          tempStream << L", ";
        }
        if (elementsAreStrings)
        {
          tempStream << (char*)(&(*it));
        }
        else
        {
          tempStream << *it;
        }
      }
      wstring tempString(tempStream.str());
      LogDebug(tempString.c_str());
    }
};