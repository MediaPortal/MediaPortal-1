/*
 * (C) 2013-2018 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#pragma once

#include <list>
#include <string_view>
#include <unordered_map>

// The hash of a string, for the keys of the caches
inline ULONG HashString(const wchar_t* str, size_t length)
{
    return static_cast<ULONG>(std::hash<std::wstring_view>()(std::wstring_view(str, length)));
}

// A cache that keeps the most recently used values (the least recently used one goes when the cache is full).
// KTraits::Hash and KTraits::CompareElements say what the same key is.
template<typename K, typename V, class KTraits>
class CRenderingCache
{
private:
    struct KeyHasher {
        size_t operator()(const K& key) const {
            return KTraits::Hash(key);
        }
    };
    struct KeyEqual {
        bool operator()(const K& key1, const K& key2) const {
            return KTraits::CompareElements(key1, key2);
        }
    };
    using List = std::list<std::pair<K, V>>;

    size_t m_maxSize;
    List m_list;    // the most recently used first
    std::unordered_map<K, typename List::iterator, KeyHasher, KeyEqual> m_map;

public:
    CRenderingCache(size_t maxSize) : m_maxSize(maxSize) {};

    bool Lookup(const K& key, V& value) {
        auto it = m_map.find(key);
        if (it == m_map.end()) {
            return false;
        }

        m_list.splice(m_list.begin(), m_list, it->second);
        value = m_list.front().second;
        return true;
    };

    void SetAt(const K& key, const V& value) {
        auto it = m_map.find(key);

        if (it != m_map.end()) {
            m_list.splice(m_list.begin(), m_list, it->second);
            m_list.front().second = value;
        } else {
            if (m_list.size() >= m_maxSize && !m_list.empty()) {
                m_map.erase(m_list.back().first);
                m_list.pop_back();
            }
            m_list.emplace_front(key, value);
            m_map.emplace(m_list.front().first, m_list.begin());
        }
    };

    void Clear() {
        m_map.clear();
        m_list.clear();
    }
};

template <class Key>
class CKeyTraits
{
public:
    static size_t Hash(const Key& element) {
        return element.GetHash();
    };

    static bool CompareElements(const Key& element1, const Key& element2) {
        return (element1 == element2);
    };
};

// Keys that are strings
class CStringKeyTraits
{
public:
    static size_t Hash(const std::wstring& str) {
        return HashString(str.c_str(), int(str.size()));
    };

    static bool CompareElements(const std::wstring& str1, const std::wstring& str2) {
        return (str1 == str2);
    };
};

class STSStyle;

class CTextDimsKey
{
private:
    ULONG m_hash;

protected:
    tstring m_str;
    std::unique_ptr<STSStyle> m_style;

public:
    CTextDimsKey(const std::wstring& str, const STSStyle& style);
    CTextDimsKey(const CTextDimsKey& textDimsKey);

    ULONG GetHash() const { return m_hash; };

    void UpdateHash();

    bool operator==(const CTextDimsKey& textDimsKey) const;
};

class CPolygonPathKey
{
private:
    ULONG m_hash;

protected:
    tstring m_str;
    double m_scalex, m_scaley;

public:
    CPolygonPathKey(const std::wstring& str, double scalex, double scaley);
    CPolygonPathKey(const CPolygonPathKey& polygonPathKey);

    ULONG GetHash() const { return m_hash; };

    void UpdateHash();

    bool operator==(const CPolygonPathKey& polygonPathKey) const;
};

class CEllipseKey
{
private:
    ULONG m_hash;

protected:
    int m_rx, m_ry;

public:
    CEllipseKey(int rx, int ry)
        : m_hash(ULONG((rx << 16) | (ry & WORD_MAX)))
        , m_rx(rx)
        , m_ry(ry) {}

    ULONG GetHash() const { return m_hash; };

    bool operator==(const CEllipseKey& ellipseKey) const {
        return (m_rx == ellipseKey.m_rx && m_ry == ellipseKey.m_ry);
    }
};

class CWord;

class COutlineKey : public CTextDimsKey
{
private:
    ULONG m_hash;

protected:
    double m_scalex, m_scaley;
    POINT m_org = {};

public:
    COutlineKey(const CWord* word, POINT org);
    COutlineKey(const COutlineKey& outLineKey);

    ULONG GetHash() const { return m_hash; };

    void UpdateHash();

    bool operator==(const COutlineKey& outLineKey) const;
};

class COverlayKey : public COutlineKey
{
private:
    POINT m_subp = {};
    ULONG m_hash;

public:
    COverlayKey(const CWord* word, POINT p, POINT org);
    COverlayKey(const COverlayKey& overlayKey);

    ULONG GetHash() const { return m_hash; };

    void UpdateHash();

    bool operator==(const COverlayKey& overlayKey) const;
};

class CClipper;

class CClipperKey
{
    ULONG m_hash;
    std::shared_ptr<CClipper> m_clipper;

public:
    CClipperKey(const std::shared_ptr<CClipper>& clipper);

    ULONG GetHash() const { return m_hash; };

    void UpdateHash();

    bool operator==(const CClipperKey& clipperKey) const;
};
