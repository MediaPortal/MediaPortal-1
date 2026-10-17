// Loads a DirectShow filter DLL (.ax) from a file without registering it in the registry.
#pragma once

#include <windows.h>
#include <dshow.h>
#include <string>

// COM for the whole test run (one apartment per process).
class ComScope
{
public:
  ComScope() { m_hr = CoInitializeEx(NULL, COINIT_MULTITHREADED); }
  ~ComScope() { if (SUCCEEDED(m_hr)) CoUninitialize(); }
  HRESULT Result() const { return m_hr; }
private:
  HRESULT m_hr;
};

// Smallest possible COM pointer for the tests (the filters themselves use WRL / their own classes).
template <class T>
class Ptr
{
public:
  Ptr() : p(NULL) {}
  ~Ptr() { Reset(); }
  Ptr(const Ptr&) = delete;
  Ptr& operator=(const Ptr&) = delete;
  void Reset() { if (p) { p->Release(); p = NULL; } }
  T** operator&() { Reset(); return &p; }
  T* operator->() const { return p; }
  T* Get() const { return p; }
  explicit operator bool() const { return p != NULL; }
  template <class Q> HRESULT As(Ptr<Q>& out) const { return p ? p->QueryInterface(__uuidof(Q), reinterpret_cast<void**>(&out)) : E_POINTER; }
  T* p;
};

class FilterModule
{
public:
  FilterModule() : m_module(NULL) {}
  ~FilterModule();

  // When set, the DLLs stay loaded until the process ends (a filter DLL that is unloaded and loaded again for every test
  // is not what a player does). The local tests set it.
  static bool& KeepLoaded() { static bool keep = false; return keep; }

  // Path of a built filter: next to the test executable (the CMake layout puts all binaries in one folder).
  static std::wstring PathOf(const wchar_t* fileName);

  // LoadLibrary; on failure Error() has the Windows error text.
  bool Load(const wchar_t* fileName);
  const std::wstring& Error() const { return m_error; }

  bool HasExport(const char* name) const;
  // Address of an exported function, or NULL
  void* Export(const char* name) const;

  // IClassFactory::CreateInstance through the DLL's DllGetClassObject (no registry involved).
  HRESULT CreateFilter(REFCLSID clsid, IBaseFilter** filter);
  HRESULT GetClassObject(REFCLSID clsid, REFIID iid, void** object);

  HRESULT CanUnloadNow();

private:
  HMODULE m_module;
  std::wstring m_error;
};

// The DirectShow filter graph manager (quartz.dll, part of Windows).
HRESULT CreateGraph(IGraphBuilder** graph);

// Number of pins of one direction on a filter.
int CountPins(IBaseFilter* filter, PIN_DIRECTION direction);
