#ifndef __AMF_H__
#define __AMF_H__
/*
 *      Copyright (C) 2005-2008 Team XBMC
 *      http://www.xbmc.org
 *      Copyright (C) 2008-2009 Andrej Stepanchuk
 *      Copyright (C) 2009-2010 Howard Chu
 *
 *  This file is part of librtmp.
 *
 *  librtmp is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as
 *  published by the Free Software Foundation; either version 2.1,
 *  or (at your option) any later version.
 *
 *  librtmp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with librtmp see the file COPYING.  If not, write to
 *  the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 *  Boston, MA  02110-1301, USA.
 *  http://www.gnu.org/copyleft/lgpl.html
 */

#include <stdint.h>

#ifndef TRUE
#define TRUE	1
#define FALSE	0
#endif

#ifdef __cplusplus
extern "C"
{
#endif

  typedef enum
  { AMF_NUMBER = 0, AMF_BOOLEAN, AMF_STRING, AMF_OBJECT,
    AMF_MOVIECLIP,		/* reserved, not used */
    AMF_NULL, AMF_UNDEFINED, AMF_REFERENCE, AMF_ECMA_ARRAY, AMF_OBJECT_END,
    AMF_STRICT_ARRAY, AMF_DATE, AMF_LONG_STRING, AMF_UNSUPPORTED,
    AMF_RECORDSET,		/* reserved, not used */
    AMF_XML_DOC, AMF_TYPED_OBJECT,
    AMF_AVMPLUS,		/* switch to AMF3 */
    AMF_INVALID = 0xff
  } AMFDataType;

  typedef enum
  { AMF3_UNDEFINED = 0, AMF3_NULL, AMF3_FALSE, AMF3_TRUE,
    AMF3_INTEGER, AMF3_DOUBLE, AMF3_STRING, AMF3_XML_DOC, AMF3_DATE,
    AMF3_ARRAY, AMF3_OBJECT, AMF3_XML, AMF3_BYTE_ARRAY
  } AMF3DataType;

  typedef struct AVal
  {
    char *av_val;
    int av_len;
  } AVal;
#define AVC(str)	{str,sizeof(str)-1}
#define AVMATCH(a1,a2)	((a1)->av_len == (a2)->av_len && !memcmp((a1)->av_val,(a2)->av_val,(a1)->av_len))

  struct AMFObjectProperty;

  typedef struct AMFObject
  {
    int o_num;
    struct AMFObjectProperty *o_props;
  } AMFObject;

  typedef struct AMFObjectProperty
  {
    AVal p_name;
    AMFDataType p_type;
    union
    {
      double p_number;
      AVal p_aval;
      AMFObject p_object;
    } p_vu;
    int16_t p_UTCoffset;
  } AMFObjectProperty;

  char *AMF_EncodeString(struct RTMP *r, char *output, char *outend, const AVal * str);
  char *AMF_EncodeNumber(struct RTMP *r, char *output, char *outend, double dVal);
  char *AMF_EncodeInt16(struct RTMP *r, char *output, char *outend, short nVal);
  char *AMF_EncodeInt24(struct RTMP *r, char *output, char *outend, int nVal);
  char *AMF_EncodeInt32(struct RTMP *r, char *output, char *outend, int nVal);
  char *AMF_EncodeBoolean(struct RTMP *r, char *output, char *outend, int bVal);

  /* Shortcuts for AMFProp_Encode */
  char *AMF_EncodeNamedString(struct RTMP *r, char *output, char *outend, const AVal * name, const AVal * value);
  char *AMF_EncodeNamedNumber(struct RTMP *r, char *output, char *outend, const AVal * name, double dVal);
  char *AMF_EncodeNamedBoolean(struct RTMP *r, char *output, char *outend, const AVal * name, int bVal);

  unsigned short AMF_DecodeInt16(struct RTMP *r, const char *data);
  unsigned int AMF_DecodeInt24(struct RTMP *r, const char *data);
  unsigned int AMF_DecodeInt32(struct RTMP *r, const char *data);
  void AMF_DecodeString(struct RTMP *r, const char *data, AVal * str);
  void AMF_DecodeLongString(struct RTMP *r, const char *data, AVal * str);
  int AMF_DecodeBoolean(struct RTMP *r, const char *data);
  double AMF_DecodeNumber(struct RTMP *r, const char *data);

  char *AMF_Encode(struct RTMP *r, AMFObject * obj, char *pBuffer, char *pBufEnd);
  int AMF_Decode(struct RTMP *r, AMFObject * obj, const char *pBuffer, int nSize,
		 int bDecodeName);
  int AMF_DecodeArray(struct RTMP *r, AMFObject * obj, const char *pBuffer, int nSize,
		      int nArrayLen, int bDecodeName);
  int AMF3_Decode(struct RTMP *r, AMFObject * obj, const char *pBuffer, int nSize,
		  int bDecodeName);
  void AMF_Dump(struct RTMP *r, AMFObject * obj);
  void AMF_Reset(struct RTMP *r, AMFObject * obj);

  void AMF_AddProp(struct RTMP *r, AMFObject * obj, const AMFObjectProperty * prop);
  int AMF_CountProp(struct RTMP *r, AMFObject * obj);
  AMFObjectProperty *AMF_GetProp(struct RTMP *r, AMFObject * obj, const AVal * name,
				 int nIndex);

  AMFDataType AMFProp_GetType(struct RTMP *r, AMFObjectProperty * prop);
  void AMFProp_SetNumber(struct RTMP *r, AMFObjectProperty * prop, double dval);
  void AMFProp_SetBoolean(struct RTMP *r, AMFObjectProperty * prop, int bflag);
  void AMFProp_SetString(struct RTMP *r, AMFObjectProperty * prop, AVal * str);
  void AMFProp_SetObject(struct RTMP *r, AMFObjectProperty * prop, AMFObject * obj);

  void AMFProp_GetName(struct RTMP *r, AMFObjectProperty * prop, AVal * name);
  void AMFProp_SetName(struct RTMP *r, AMFObjectProperty * prop, AVal * name);
  double AMFProp_GetNumber(struct RTMP *r, AMFObjectProperty * prop);
  int AMFProp_GetBoolean(struct RTMP *r, AMFObjectProperty * prop);
  void AMFProp_GetString(struct RTMP *r, AMFObjectProperty * prop, AVal * str);
  void AMFProp_GetObject(struct RTMP *r, AMFObjectProperty * prop, AMFObject * obj);

  int AMFProp_IsValid(struct RTMP *r, AMFObjectProperty * prop);

  char *AMFProp_Encode(struct RTMP *r, AMFObjectProperty * prop, char *pBuffer, char *pBufEnd);
  int AMF3Prop_Decode(struct RTMP *r, AMFObjectProperty * prop, const char *pBuffer,
		      int nSize, int bDecodeName);
  int AMFProp_Decode(struct RTMP *r, AMFObjectProperty * prop, const char *pBuffer,
		     int nSize, int bDecodeName);

  void AMFProp_Dump(struct RTMP *r, AMFObjectProperty * prop);
  void AMFProp_Reset(struct RTMP *r, AMFObjectProperty * prop);

  typedef struct AMF3ClassDef
  {
    AVal cd_name;
    char cd_externalizable;
    char cd_dynamic;
    int cd_num;
    AVal *cd_props;
  } AMF3ClassDef;

  void AMF3CD_AddProp(struct RTMP *r, AMF3ClassDef * cd, AVal * prop);
  AVal *AMF3CD_GetProp(struct RTMP *r, AMF3ClassDef * cd, int idx);

#ifdef __cplusplus
}
#endif

#endif				/* __AMF_H__ */
