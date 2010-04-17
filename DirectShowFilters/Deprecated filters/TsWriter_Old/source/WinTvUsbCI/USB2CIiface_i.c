/* this file contains the actual definitions of */
/* the IIDs and CLSIDs */

/* link this file in with the server and any clients */


/* File created by MIDL compiler version 5.01.0164 */
/* at Tue Dec 12 10:20:26 2006
 */
/* Compiler settings for D:\Projects\USB2CI\Dev\Plugin\USB2CIiface.idl:
    Os (OptLev=s), W1, Zp8, env=Win32, ms_ext, c_ext
    error checks: allocation ref bounds_check enum stub_data 
*/
//@@MIDL_FILE_HEADING(  )
#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IID_DEFINED__
#define __IID_DEFINED__

typedef struct _IID
{
    unsigned long x;
    unsigned short s1;
    unsigned short s2;
    unsigned char  c[8];
} IID;

#endif // __IID_DEFINED__

#ifndef CLSID_DEFINED
#define CLSID_DEFINED
typedef IID CLSID;
#endif // CLSID_DEFINED

const IID LIBID_USB2CIBDA = {0xC29D9628,0xAE8B,0x430d,{0x90,0xB4,0x10,0xD6,0xEA,0xF2,0xF2,0x10}};


const IID IID_IUSB2CIBDAConfig = {0xDD5A9B44,0x348A,0x4607,{0xBF,0x72,0xCF,0xD8,0x23,0x9E,0x44,0x32}};


const CLSID CLSID_USB2CIBDAPage = {0x8BF24573,0xE336,0x4986,{0xB3,0x06,0x0C,0x7E,0xFB,0x7E,0x58,0x23}};


#ifdef __cplusplus
}
#endif

