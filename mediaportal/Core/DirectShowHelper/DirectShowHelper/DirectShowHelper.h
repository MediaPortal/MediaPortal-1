

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 6.00.0361 */
/* at Sat Mar 12 23:03:24 2005
 */
/* Compiler settings for .\DirectShowHelper.idl:
    Oicf, W1, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __DirectShowHelper_h__
#define __DirectShowHelper_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IVMR9Callback_FWD_DEFINED__
#define __IVMR9Callback_FWD_DEFINED__
typedef interface IVMR9Callback IVMR9Callback;
#endif 	/* __IVMR9Callback_FWD_DEFINED__ */


#ifndef __IVMR9Helper_FWD_DEFINED__
#define __IVMR9Helper_FWD_DEFINED__
typedef interface IVMR9Helper IVMR9Helper;
#endif 	/* __IVMR9Helper_FWD_DEFINED__ */


#ifndef __VMR9Callback_FWD_DEFINED__
#define __VMR9Callback_FWD_DEFINED__

#ifdef __cplusplus
typedef class VMR9Callback VMR9Callback;
#else
typedef struct VMR9Callback VMR9Callback;
#endif /* __cplusplus */

#endif 	/* __VMR9Callback_FWD_DEFINED__ */


#ifndef __VMR9Helper_FWD_DEFINED__
#define __VMR9Helper_FWD_DEFINED__

#ifdef __cplusplus
typedef class VMR9Helper VMR9Helper;
#else
typedef struct VMR9Helper VMR9Helper;
#endif /* __cplusplus */

#endif 	/* __VMR9Helper_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"
#include "amstream.h"

#ifdef __cplusplus
extern "C"{
#endif 

void * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void * ); 

/* interface __MIDL_itf_DirectShowHelper_0000 */
/* [local] */ 

#if 0
typedef DWORD IDirect3DTexture9;

#endif


extern RPC_IF_HANDLE __MIDL_itf_DirectShowHelper_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_DirectShowHelper_0000_v0_0_s_ifspec;

#ifndef __IVMR9Callback_INTERFACE_DEFINED__
#define __IVMR9Callback_INTERFACE_DEFINED__

/* interface IVMR9Callback */
/* [unique][helpstring][uuid][object] */ 


EXTERN_C const IID IID_IVMR9Callback;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("6BD43BC8-22A3-478C-A571-EA723BD9F019")
    IVMR9Callback : public IUnknown
    {
    public:
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE PresentImage( 
            /* [in] */ int width,
            /* [in] */ int height,
            /* [in] */ DWORD texture) = 0;
        
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE PresentSurface( 
            /* [in] */ int width,
            /* [in] */ int height,
            /* [in] */ DWORD surface) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IVMR9CallbackVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IVMR9Callback * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IVMR9Callback * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IVMR9Callback * This);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *PresentImage )( 
            IVMR9Callback * This,
            /* [in] */ int width,
            /* [in] */ int height,
            /* [in] */ DWORD texture);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *PresentSurface )( 
            IVMR9Callback * This,
            /* [in] */ int width,
            /* [in] */ int height,
            /* [in] */ DWORD surface);
        
        END_INTERFACE
    } IVMR9CallbackVtbl;

    interface IVMR9Callback
    {
        CONST_VTBL struct IVMR9CallbackVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IVMR9Callback_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IVMR9Callback_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IVMR9Callback_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IVMR9Callback_PresentImage(This,width,height,texture)	\
    (This)->lpVtbl -> PresentImage(This,width,height,texture)

#define IVMR9Callback_PresentSurface(This,width,height,surface)	\
    (This)->lpVtbl -> PresentSurface(This,width,height,surface)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Callback_PresentImage_Proxy( 
    IVMR9Callback * This,
    /* [in] */ int width,
    /* [in] */ int height,
    /* [in] */ DWORD texture);


void __RPC_STUB IVMR9Callback_PresentImage_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Callback_PresentSurface_Proxy( 
    IVMR9Callback * This,
    /* [in] */ int width,
    /* [in] */ int height,
    /* [in] */ DWORD surface);


void __RPC_STUB IVMR9Callback_PresentSurface_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IVMR9Callback_INTERFACE_DEFINED__ */


#ifndef __IVMR9Helper_INTERFACE_DEFINED__
#define __IVMR9Helper_INTERFACE_DEFINED__

/* interface IVMR9Helper */
/* [unique][helpstring][uuid][object] */ 


EXTERN_C const IID IID_IVMR9Helper;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("1463CD20-B360-49B8-A81C-47981347FDB5")
    IVMR9Helper : public IUnknown
    {
    public:
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE Init( 
            /* [in] */ IVMR9Callback *callback,
            /* [in] */ DWORD dwD3DDevice,
            /* [in] */ IBaseFilter *vmr9Filter,
            /* [in] */ DWORD monitor) = 0;
        
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetDeinterlace( 
            /* [in] */ DWORD dwInterlace) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IVMR9HelperVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IVMR9Helper * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IVMR9Helper * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IVMR9Helper * This);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *Init )( 
            IVMR9Helper * This,
            /* [in] */ IVMR9Callback *callback,
            /* [in] */ DWORD dwD3DDevice,
            /* [in] */ IBaseFilter *vmr9Filter,
            /* [in] */ DWORD monitor);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *SetDeinterlace )( 
            IVMR9Helper * This,
            /* [in] */ DWORD dwInterlace);
        
        END_INTERFACE
    } IVMR9HelperVtbl;

    interface IVMR9Helper
    {
        CONST_VTBL struct IVMR9HelperVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IVMR9Helper_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IVMR9Helper_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IVMR9Helper_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IVMR9Helper_Init(This,callback,dwD3DDevice,vmr9Filter,monitor)	\
    (This)->lpVtbl -> Init(This,callback,dwD3DDevice,vmr9Filter,monitor)

#define IVMR9Helper_SetDeinterlace(This,dwInterlace)	\
    (This)->lpVtbl -> SetDeinterlace(This,dwInterlace)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Helper_Init_Proxy( 
    IVMR9Helper * This,
    /* [in] */ IVMR9Callback *callback,
    /* [in] */ DWORD dwD3DDevice,
    /* [in] */ IBaseFilter *vmr9Filter,
    /* [in] */ DWORD monitor);


void __RPC_STUB IVMR9Helper_Init_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Helper_SetDeinterlace_Proxy( 
    IVMR9Helper * This,
    /* [in] */ DWORD dwInterlace);


void __RPC_STUB IVMR9Helper_SetDeinterlace_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IVMR9Helper_INTERFACE_DEFINED__ */



#ifndef __DirectShowHelperLib_LIBRARY_DEFINED__
#define __DirectShowHelperLib_LIBRARY_DEFINED__

/* library DirectShowHelperLib */
/* [helpstring][version][uuid] */ 


EXTERN_C const IID LIBID_DirectShowHelperLib;

EXTERN_C const CLSID CLSID_VMR9Callback;

#ifdef __cplusplus

class DECLSPEC_UUID("A7D8DDD4-2104-42C0-966C-08B400F5498F")
VMR9Callback;
#endif

EXTERN_C const CLSID CLSID_VMR9Helper;

#ifdef __cplusplus

class DECLSPEC_UUID("D23CF2BC-5AD3-407F-B562-A7CB0FD342DB")
VMR9Helper;
#endif
#endif /* __DirectShowHelperLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


