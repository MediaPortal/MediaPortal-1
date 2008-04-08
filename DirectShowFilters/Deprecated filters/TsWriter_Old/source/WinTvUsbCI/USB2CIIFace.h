/* this ALWAYS GENERATED file contains the definitions for the interfaces */


/* File created by MIDL compiler version 5.01.0164 */
/* at Tue Dec 12 10:20:26 2006
 */
/* Compiler settings for D:\Projects\USB2CI\Dev\Plugin\USB2CIiface.idl:
    Os (OptLev=s), W1, Zp8, env=Win32, ms_ext, c_ext
    error checks: allocation ref bounds_check enum stub_data 
*/
//@@MIDL_FILE_HEADING(  )


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 440
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __USB2CIiface_h__
#define __USB2CIiface_h__

#ifdef __cplusplus
extern "C"{
#endif 

/* Forward Declarations */ 

#ifndef __IUSB2CIBDAConfig_FWD_DEFINED__
#define __IUSB2CIBDAConfig_FWD_DEFINED__
typedef interface IUSB2CIBDAConfig IUSB2CIBDAConfig;
#endif 	/* __IUSB2CIBDAConfig_FWD_DEFINED__ */


#ifndef __USB2CIBDAPage_FWD_DEFINED__
#define __USB2CIBDAPage_FWD_DEFINED__

#ifdef __cplusplus
typedef class USB2CIBDAPage USB2CIBDAPage;
#else
typedef struct USB2CIBDAPage USB2CIBDAPage;
#endif /* __cplusplus */

#endif 	/* __USB2CIBDAPage_FWD_DEFINED__ */


/* header files for imported files */
#include "unknwn.h"
#include "strmif.h"

void __RPC_FAR * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void __RPC_FAR * ); 


#ifndef __USB2CIBDA_LIBRARY_DEFINED__
#define __USB2CIBDA_LIBRARY_DEFINED__

/* library USB2CIBDA */
/* [control][helpstring][version][uuid] */ 


DEFINE_GUID(LIBID_USB2CIBDA,0xC29D9628,0xAE8B,0x430d,0x90,0xB4,0x10,0xD6,0xEA,0xF2,0xF2,0x10);

#ifndef __IUSB2CIBDAConfig_INTERFACE_DEFINED__
#define __IUSB2CIBDAConfig_INTERFACE_DEFINED__

/* interface IUSB2CIBDAConfig */
/* [unique][uuid][object] */ 


DEFINE_GUID(IID_IUSB2CIBDAConfig,0xDD5A9B44,0x348A,0x4607,0xBF,0x72,0xCF,0xD8,0x23,0x9E,0x44,0x32);

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("DD5A9B44-348A-4607-BF72-CFD8239E4432")
    IUSB2CIBDAConfig : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE USB2CI_Init( 
            /* [in] */ PVOID pCallBack) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE USB2CI_OpenMMI( void) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE USB2CI_APDUToCAM( 
            long SizeofAPDU,
            unsigned char __RPC_FAR *APDU) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE USB2CI_GuiSendPMT( 
            /* [in] */ unsigned char __RPC_FAR *pPMT,
            /* [in] */ short Size) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE USB2CI_GetVersion( 
            /* [in] */ PVOID VersionTab) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IUSB2CIBDAConfigVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IUSB2CIBDAConfig __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IUSB2CIBDAConfig __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IUSB2CIBDAConfig __RPC_FAR * This);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *USB2CI_Init )( 
            IUSB2CIBDAConfig __RPC_FAR * This,
            /* [in] */ PVOID pCallBack);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *USB2CI_OpenMMI )( 
            IUSB2CIBDAConfig __RPC_FAR * This);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *USB2CI_APDUToCAM )( 
            IUSB2CIBDAConfig __RPC_FAR * This,
            long SizeofAPDU,
            unsigned char __RPC_FAR *APDU);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *USB2CI_GuiSendPMT )( 
            IUSB2CIBDAConfig __RPC_FAR * This,
            /* [in] */ unsigned char __RPC_FAR *pPMT,
            /* [in] */ short Size);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *USB2CI_GetVersion )( 
            IUSB2CIBDAConfig __RPC_FAR * This,
            /* [in] */ PVOID VersionTab);
        
        END_INTERFACE
    } IUSB2CIBDAConfigVtbl;

    interface IUSB2CIBDAConfig
    {
        CONST_VTBL struct IUSB2CIBDAConfigVtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IUSB2CIBDAConfig_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IUSB2CIBDAConfig_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IUSB2CIBDAConfig_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IUSB2CIBDAConfig_USB2CI_Init(This,pCallBack)	\
    (This)->lpVtbl -> USB2CI_Init(This,pCallBack)

#define IUSB2CIBDAConfig_USB2CI_OpenMMI(This)	\
    (This)->lpVtbl -> USB2CI_OpenMMI(This)

#define IUSB2CIBDAConfig_USB2CI_APDUToCAM(This,SizeofAPDU,APDU)	\
    (This)->lpVtbl -> USB2CI_APDUToCAM(This,SizeofAPDU,APDU)

#define IUSB2CIBDAConfig_USB2CI_GuiSendPMT(This,pPMT,Size)	\
    (This)->lpVtbl -> USB2CI_GuiSendPMT(This,pPMT,Size)

#define IUSB2CIBDAConfig_USB2CI_GetVersion(This,VersionTab)	\
    (This)->lpVtbl -> USB2CI_GetVersion(This,VersionTab)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE IUSB2CIBDAConfig_USB2CI_Init_Proxy( 
    IUSB2CIBDAConfig __RPC_FAR * This,
    /* [in] */ PVOID pCallBack);


void __RPC_STUB IUSB2CIBDAConfig_USB2CI_Init_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IUSB2CIBDAConfig_USB2CI_OpenMMI_Proxy( 
    IUSB2CIBDAConfig __RPC_FAR * This);


void __RPC_STUB IUSB2CIBDAConfig_USB2CI_OpenMMI_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IUSB2CIBDAConfig_USB2CI_APDUToCAM_Proxy( 
    IUSB2CIBDAConfig __RPC_FAR * This,
    long SizeofAPDU,
    unsigned char __RPC_FAR *APDU);


void __RPC_STUB IUSB2CIBDAConfig_USB2CI_APDUToCAM_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IUSB2CIBDAConfig_USB2CI_GuiSendPMT_Proxy( 
    IUSB2CIBDAConfig __RPC_FAR * This,
    /* [in] */ unsigned char __RPC_FAR *pPMT,
    /* [in] */ short Size);


void __RPC_STUB IUSB2CIBDAConfig_USB2CI_GuiSendPMT_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IUSB2CIBDAConfig_USB2CI_GetVersion_Proxy( 
    IUSB2CIBDAConfig __RPC_FAR * This,
    /* [in] */ PVOID VersionTab);


void __RPC_STUB IUSB2CIBDAConfig_USB2CI_GetVersion_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IUSB2CIBDAConfig_INTERFACE_DEFINED__ */


DEFINE_GUID(CLSID_USB2CIBDAPage,0x8BF24573,0xE336,0x4986,0xB3,0x06,0x0C,0x7E,0xFB,0x7E,0x58,0x23);

#ifdef __cplusplus

class DECLSPEC_UUID("8BF24573-E336-4986-B306-0C7EFB7E5823")
USB2CIBDAPage;
#endif
#endif /* __USB2CIBDA_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif
