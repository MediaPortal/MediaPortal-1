// Copyright (C) 2014 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.


//========================================================================================================
// Including this definition for 'IMFTrackedSample' here is a *workaround* to fix compilation problems
// using Platform SDK v8.x i.e.'IMFTrackedSample' is not defined.
//
// The code is copied from C:\Program Files (x86)\Windows Kits\8.1\Include\um\mfidl.h
//--------------------------------------------------------------------------------------------------------


#ifndef __IMF_h__
#define __IMF_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

#ifndef __IMFTrackedSample_FWD_DEFINED__
#define __IMFTrackedSample_FWD_DEFINED__
typedef interface IMFTrackedSample IMFTrackedSample;
#endif 	/* __IMFTrackedSample_FWD_DEFINED__ */


#ifndef __IMFTrackedSample_INTERFACE_DEFINED__
#define __IMFTrackedSample_INTERFACE_DEFINED__

/* interface IMFTrackedSample */
/* [local][helpstring][uuid][object] */ 

EXTERN_C const IID IID_IMFTrackedSample;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("245BF8E9-0755-40f7-88A5-AE0F18D55E17")
    IMFTrackedSample : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE SetAllocator( 
            /* [annotation][in] */ 
            _In_  IMFAsyncCallback *pSampleAllocator,
            /* [unique][in] */ IUnknown *pUnkState) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IMFTrackedSampleVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IMFTrackedSample * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IMFTrackedSample * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IMFTrackedSample * This);
        
        HRESULT ( STDMETHODCALLTYPE *SetAllocator )( 
            IMFTrackedSample * This,
            /* [annotation][in] */ 
            _In_  IMFAsyncCallback *pSampleAllocator,
            /* [unique][in] */ IUnknown *pUnkState);
        
        END_INTERFACE
    } IMFTrackedSampleVtbl;

    interface IMFTrackedSample
    {
        CONST_VTBL struct IMFTrackedSampleVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IMFTrackedSample_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IMFTrackedSample_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IMFTrackedSample_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IMFTrackedSample_SetAllocator(This,pSampleAllocator,pUnkState)	\
    ( (This)->lpVtbl -> SetAllocator(This,pSampleAllocator,pUnkState) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */


#endif 	/* __IMFTrackedSample_INTERFACE_DEFINED__ */


#endif


