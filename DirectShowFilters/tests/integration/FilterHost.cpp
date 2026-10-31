#include "FilterHost.h"

#include <vector>

#ifndef MP_FILTER_DIR
#define MP_FILTER_DIR L"."
#endif

static std::wstring DirectoryOfThisExecutable()
{
  wchar_t path[MAX_PATH * 2] = {};
  DWORD n = GetModuleFileNameW(NULL, path, static_cast<DWORD>(sizeof(path) / sizeof(path[0])));
  std::wstring s(path, n);
  size_t slash = s.find_last_of(L"\\/");
  return slash == std::wstring::npos ? L"." : s.substr(0, slash);
}

std::wstring FilterModule::PathOf(const wchar_t* fileName)
{
  std::wstring beside = DirectoryOfThisExecutable() + L"\\" + fileName;
  if (GetFileAttributesW(beside.c_str()) != INVALID_FILE_ATTRIBUTES)
    return beside;
  return std::wstring(MP_FILTER_DIR) + L"\\" + fileName;
}

FilterModule::~FilterModule()
{
  if (m_module && !KeepLoaded())
    FreeLibrary(m_module);
}

bool FilterModule::Load(const wchar_t* fileName)
{
  std::wstring path = PathOf(fileName);
  // Load dependencies (unrar.dll, ...) from the folder of the filter
  m_module = LoadLibraryExW(path.c_str(), NULL, LOAD_WITH_ALTERED_SEARCH_PATH);
  if (m_module)
    return true;

  DWORD error = GetLastError();
  wchar_t text[512] = {};
  FormatMessageW(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, NULL, error, 0, text, 512, NULL);
  m_error = path + L": error " + std::to_wstring(error) + L" " + text;
  return false;
}

bool FilterModule::HasExport(const char* name) const
{
  return m_module && GetProcAddress(m_module, name) != NULL;
}

void* FilterModule::Export(const char* name) const
{
  return m_module ? reinterpret_cast<void*>(GetProcAddress(m_module, name)) : NULL;
}

HRESULT FilterModule::GetClassObject(REFCLSID clsid, REFIID iid, void** object)
{
  typedef HRESULT(STDAPICALLTYPE * Fn)(REFCLSID, REFIID, LPVOID*);
  Fn fn = m_module ? reinterpret_cast<Fn>(GetProcAddress(m_module, "DllGetClassObject")) : NULL;
  if (!fn)
    return E_NOTIMPL;
  return fn(clsid, iid, object);
}

HRESULT FilterModule::CreateFilter(REFCLSID clsid, IBaseFilter** filter)
{
  IClassFactory* factory = NULL;
  HRESULT hr = GetClassObject(clsid, IID_IClassFactory, reinterpret_cast<void**>(&factory));
  if (FAILED(hr))
    return hr;
  hr = factory->CreateInstance(NULL, IID_IBaseFilter, reinterpret_cast<void**>(filter));
  factory->Release();
  return hr;
}

HRESULT FilterModule::CanUnloadNow()
{
  typedef HRESULT(STDAPICALLTYPE * Fn)();
  Fn fn = m_module ? reinterpret_cast<Fn>(GetProcAddress(m_module, "DllCanUnloadNow")) : NULL;
  return fn ? fn() : E_NOTIMPL;
}

HRESULT CreateGraph(IGraphBuilder** graph)
{
  return CoCreateInstance(CLSID_FilterGraph, NULL, CLSCTX_INPROC_SERVER, IID_IGraphBuilder, reinterpret_cast<void**>(graph));
}

int CountPins(IBaseFilter* filter, PIN_DIRECTION direction)
{
  Ptr<IEnumPins> pins;
  if (FAILED(filter->EnumPins(&pins)))
    return -1;
  int count = 0;
  for (;;)
  {
    Ptr<IPin> pin;
    if (pins->Next(1, &pin, NULL) != S_OK)
      break;
    PIN_DIRECTION d;
    if (SUCCEEDED(pin->QueryDirection(&d)) && d == direction)
      count++;
  }
  return count;
}
