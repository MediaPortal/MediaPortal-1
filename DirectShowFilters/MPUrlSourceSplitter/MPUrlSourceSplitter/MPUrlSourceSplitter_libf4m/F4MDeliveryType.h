/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#ifndef __F4M_DELIVERY_TYPE_DEFINED
#define __F4M_DELIVERY_TYPE_DEFINED

class CF4MDeliveryType
{
public:
  // initializes a new instance of CF4MDeliveryType class
  CF4MDeliveryType(void);

  // destructor
  ~CF4MDeliveryType(void);

  /* get methods */

  // gets delivery type
  // @return : delivery type or NULL if not specified
  const wchar_t *GetDeliveryType(void);

  /* set methods */

  // sets delivery type
  // @param deliveryType : the delivery type to set
  // @return : true if successful, false otherwise
  bool SetDeliveryType(const wchar_t *deliveryType);

  /* other methods */

  // tests if delivery type is streaming
  // @return : true if delivery type is streaming, false otherwise
  bool IsStreaming(void);

  // tests if delivery type is progressive
  // @return : true if delivery type is progressive, false otherwise
  bool IsProgressive(void);

  // clears current instance to default state
  void Clear(void);

private:
  // stores delivery type
  wchar_t *deliveryType;
};

#endif