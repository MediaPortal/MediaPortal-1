/**
 *	DVBTChannels.cpp
 *	Copyright (C) 2004 Nate
 *
 *	This file is part of DigitalWatch, a free DTV watching and recording
 *	program for the VisionPlus DVB-T.
 *
 *	DigitalWatch is free software; you can redistribute it and/or modify
 *	it under the terms of the GNU General Public License as published by
 *	the Free Software Foundation; either version 2 of the License, or
 *	(at your option) any later version.
 *
 *	DigitalWatch is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public License
 *	along with DigitalWatch; if not, write to the Free Software
 *	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#include "DVBTChannels.h"
//#include "ParseLine.h"
#include "GlobalFunctions.h"
#include "Globals.h"


//////////////////////////////////////////////////////////////////////
// DVBTChannels_Stream
//////////////////////////////////////////////////////////////////////

DVBTChannels_Stream::DVBTChannels_Stream()
{
	PID = 0;
	Type = unknown;
	Language = NULL;
	bActive = TRUE;
}

DVBTChannels_Stream::~DVBTChannels_Stream()
{
	if (Language)
		delete[] Language;
}

void DVBTChannels_Stream::SetLogCallback(LogMessageCallback *callback)
{
	LogMessageCaller::SetLogCallback(callback);
}

void DVBTChannels_Stream::UpdateStream(DVBTChannels_Stream *pNewStream)
{
	if ((PID != pNewStream->PID) ||
		(Type != pNewStream->Type) ||
		(Language == NULL) ||
		(pNewStream->Language && (_wcsicmp(Language, pNewStream->Language) != 0))
	   )
	{
		BOOL bExistingData = (PID != 0) || (Type != unknown) || (Language != NULL);

		if (bExistingData)
			(log << "Updating Stream Information\n").Write();
		else
			(log << "New Stream Information\n").Write();
		
		LogMessageIndent indent1(&log);

		if (bExistingData)
		{
			(log << "Original Values\n").Write();
			LogMessageIndent indent2(&log);
			PrintStreamDetails();
		}

		PID = pNewStream->PID;
		Type = pNewStream->Type;
		strCopy(Language, pNewStream->Language);

		if (bExistingData)
		{
			(log << "New Values\n").Write();
			LogMessageIndent indent2(&log);
			PrintStreamDetails();
		}
		else
		{
			PrintStreamDetails();
		}
	}
}

void DVBTChannels_Stream::PrintStreamDetails()
{
	(log << "PID      : " << PID << "\n").Write();
	(log << "Type     : " << DVBTChannels_Service_PID_Types_String[(int)Type] << "\n").Write();
	(log << "Language : " << (Language ? Language : L"<not set>") << "\n").Write();
}

//////////////////////////////////////////////////////////////////////
// DVBTChannels_Service
//////////////////////////////////////////////////////////////////////

DVBTChannels_Service::DVBTChannels_Service()
{
	serviceId = 0;
	logicalChannelNumber = 0;
	serviceName = NULL;
	favoriteID = 0;
	bManualUpdate = 0;
}

DVBTChannels_Service::~DVBTChannels_Service()
{
	CAutoLock lock(&m_streamsLock);

	std::vector<DVBTChannels_Stream *>::iterator it = m_streams.begin();
	for ( ; it != m_streams.end() ; it++ )
	{
		DVBTChannels_Stream *pStream = *it;
		if (pStream)
			delete pStream;
	}
	m_streams.clear();

	if (serviceName)
		delete[] serviceName;
}

void DVBTChannels_Service::SetLogCallback(LogMessageCallback *callback)
{
	LogMessageCaller::SetLogCallback(callback);

	CAutoLock lock(&m_streamsLock);

	std::vector<DVBTChannels_Stream *>::iterator it = m_streams.begin();
	for ( ; it != m_streams.end() ; it++ )
	{
		DVBTChannels_Stream *pStream = *it;
		pStream->SetLogCallback(callback);
	}
}

void DVBTChannels_Service::AddStream(DVBTChannels_Stream* pStream)
{
	if(!pStream)
		return;

	CAutoLock lock(&m_streamsLock);
	m_streams.push_back(pStream);
}
/*
HRESULT DVBTChannels_Service::LoadFromXML(XMLElement *pElement)
{
	CAutoLock lock(&m_streamsLock);

	XMLAttribute *attr;
	attr = pElement->Attributes.Item(L"serviceId");
	if (attr == NULL)
	{
		attr = pElement->Attributes.Item(L"ProgramNumber");
		if (attr == NULL)
			return (log << "serviceId must be supplied in a service definition").Write(E_FAIL);
	}
	serviceId = StringToLong(attr->value);

	attr = pElement->Attributes.Item(L"Name");
	strCopy(serviceName, (attr ? attr->value : L""));

	attr = pElement->Attributes.Item(L"LCN");
	if (attr)
		logicalChannelNumber = StringToLong(attr->value);

	attr = pElement->Attributes.Item(L"FavoriteID");
	if (attr)
		favoriteID = StringToLong(attr->value);

	attr = pElement->Attributes.Item(L"ManualUpdate");
	if (attr)
		bManualUpdate = (attr->value[0] != 0);
	else
		bManualUpdate = FALSE;

	int elementCount = pElement->Elements.Count();
	for ( int item=0 ; item<elementCount ; item++ )
	{
		XMLElement *element = pElement->Elements.Item(item);
		if (_wcsicmp(element->name, L"Stream") == 0)
		{
			DVBTChannels_Stream *pNewStream = new DVBTChannels_Stream();
			pNewStream->SetLogCallback(m_pLogCallback);

			attr = element->Attributes.Item(L"PID");
			if (attr == NULL)
			{
//				delete pNewStream;
				continue;
			}

			pNewStream->PID = StringToLong(attr->value);

			attr = element->Attributes.Item(L"Type");
			if (attr == NULL)
			{
//				delete pNewStream;
				continue;
			}

			pNewStream->Type = unknown;
			if ((attr->value[0]-'0' >= 0) && (attr->value[0]-'0' < DVBTChannels_Service_PID_Types_Count))
			{
				pNewStream->Type = (DVBTChannels_Service_PID_Types)StringToLong(attr->value);
			}
			else
			{
				for (int i=0 ; i<DVBTChannels_Service_PID_Types_Count ; i++ )
				{
					if (_wcsicmp(attr->value, DVBTChannels_Service_PID_Types_String[i]) == 0)
					{
						pNewStream->Type = (DVBTChannels_Service_PID_Types)i;
//						delete pNewStream;
						break;
					}
				}
			}

			attr = element->Attributes.Item(L"Active");
			if (attr == NULL)
				pNewStream->bActive = FALSE;
			else
				pNewStream->bActive = (attr->value[0] != 0);

			m_streams.push_back(pNewStream);
			continue;
		}
	}

	return S_OK;
}

HRESULT DVBTChannels_Service::SaveToXML(XMLElement *pElement)
{
	CAutoLock lock(&m_streamsLock);

	LPWSTR pValue = NULL;
	strCopyHex(pValue, serviceId);
	pElement->Attributes.Add(new XMLAttribute(L"serviceId", pValue));
	delete pValue;
	pValue = NULL;

	pElement->Attributes.Add(new XMLAttribute(L"name", (serviceName ? serviceName : L"")));

	if (logicalChannelNumber > 0)
	{
		strCopy(pValue, logicalChannelNumber);
		pElement->Attributes.Add(new XMLAttribute(L"LCN", pValue));
		delete pValue;
		pValue = NULL;
	}

	if (favoriteID > 0)
	{
		strCopy(pValue, favoriteID);
		pElement->Attributes.Add(new XMLAttribute(L"FavoriteID", pValue));
		delete pValue;
		pValue = NULL;
	}

	if (bManualUpdate)
		pElement->Attributes.Add(new XMLAttribute(L"ManualUpdate", L"1"));

	std::vector<DVBTChannels_Stream *>::iterator it = m_streams.begin();
	for ( ; it != m_streams.end() ; it++ )
	{
		DVBTChannels_Stream *pStream = *it;

		XMLElement *pStreamElement = new XMLElement(L"Stream");

		strCopyHex(pValue, pStream->PID);
		pStreamElement->Attributes.Add(new XMLAttribute(L"PID", pValue));
		delete pValue;
		pValue = NULL;

		pStreamElement->Attributes.Add(new XMLAttribute(L"Active", (pStream->bActive ? L"1" : L"0")));

		pStreamElement->Attributes.Add(new XMLAttribute(L"Type", DVBTChannels_Service_PID_Types_String[(int)pStream->Type]));
		delete pValue;
		pValue = NULL;

		pElement->Elements.Add(pStreamElement);
	}
	return S_OK;
}
*/
DVBTChannels_Service_PID_Types DVBTChannels_Service::GetStreamType(int index)
{
	CAutoLock lock(&m_streamsLock);

	if ((index >= 0) && (index < (int)m_streams.size()))
	{
		DVBTChannels_Stream *pStream = m_streams.at(index);
		return pStream->Type;
	}
	return unknown;
}

long DVBTChannels_Service::GetStreamPID(int index)
{
	CAutoLock lock(&m_streamsLock);

	if ((index >= 0) && (index < (int)m_streams.size()))
	{
		DVBTChannels_Stream *pStream = m_streams.at(index);
		return pStream->PID;
	}
	return 0;
}

long DVBTChannels_Service::GetStreamCount()
{
	return m_streams.size();
}

long DVBTChannels_Service::GetStreamPID(DVBTChannels_Service_PID_Types streamtype, int index)
{
	CAutoLock lock(&m_streamsLock);

	int found = 0;
	std::vector<DVBTChannels_Stream *>::iterator it = m_streams.begin();
	for ( ; it != m_streams.end() ; it++ )
	{
		DVBTChannels_Stream *pStream = *it;
		if ((pStream->Type == streamtype) && pStream->bActive)
		{
			if (found == index)
				return pStream->PID;
			found++;
		}
	}
	return 0;
}

long DVBTChannels_Service::GetStreamCount(DVBTChannels_Service_PID_Types streamtype)
{
	CAutoLock lock(&m_streamsLock);

	int found = 0;
	std::vector<DVBTChannels_Stream *>::iterator it = m_streams.begin();
	for ( ; it != m_streams.end() ; it++ )
	{
		DVBTChannels_Stream *pStream = *it;
		if ((pStream->Type == streamtype) && pStream->bActive)
		{
			found++;
		}
	}
	return found;
}

BOOL DVBTChannels_Service::UpdateService(DVBTChannels_Service *pNewService)
{
	if (bManualUpdate)
		return FALSE;

	BOOL bChange =  (serviceId != pNewService->serviceId) ||						// serviceId changed or
					(logicalChannelNumber != pNewService->logicalChannelNumber) ||	// lcn changed or
					(serviceName == NULL) ||										// current name isn't set yet or
					((pNewService->serviceName) &&									//  new name is set and
					 (_wcsicmp(pNewService->serviceName, L"") != 0) &&				//  new name isn't blank and
					 (_wcsicmp(serviceName, pNewService->serviceName) != 0));		//  new name has changed

	if (bChange)
	{
		BOOL bExistingData = (serviceId != 0) || (logicalChannelNumber != 0) || (serviceName != NULL);

		if (bExistingData)
			(log << "Updating Service Information\n").Write();
		else
			(log << "New Service Information\n").Write();
		LogMessageIndent indent1(&log);

		if (bExistingData)
		{
			(log << "Original Values\n").Write();
			LogMessageIndent indent2(&log);
			PrintServiceDetails();
		}

		serviceId = pNewService->serviceId;
		strCopy(serviceName, pNewService->serviceName);
		if (pNewService->logicalChannelNumber != 0)
			logicalChannelNumber = pNewService->logicalChannelNumber;

		if (bExistingData)
		{
			(log << "New Values\n").Write();
			LogMessageIndent indent2(&log);
			PrintServiceDetails();
		}
		else
		{
			PrintServiceDetails();
		}
	}

	bChange |= UpdateStreams(pNewService);

	return bChange;
}

BOOL DVBTChannels_Service::UpdateStreams(DVBTChannels_Service *pNewService)
{
	CAutoLock lock(&m_streamsLock);

	BOOL bChange = FALSE;

	// First we mark all streams as not detected
	std::vector<DVBTChannels_Stream *>::iterator it = m_streams.begin();
	for ( ; it != m_streams.end() ; it++ )
	{
		(*it)->bDetected = FALSE;
	}
	for ( it=pNewService->m_streams.begin() ; it != pNewService->m_streams.end() ; it++ )
	{
		(*it)->bDetected = FALSE;
	}

	// Second we find matching streams and mark them as detected
	for ( it=pNewService->m_streams.begin() ; it != pNewService->m_streams.end() ; it++ )
	{
		DVBTChannels_Stream *pNewStream = FindStreamByPID((*it)->PID);
		if (pNewStream)
		{
			(*it)->bDetected = TRUE;
			pNewStream->bDetected = TRUE;

			(*it)->UpdateStream(pNewStream);
		}
	}

	// All old streams not marked detected don't exist anymore so they can be removed.
	for ( it=m_streams.begin() ; it != m_streams.end() ; it++ )
	{
		if ((*it)->bDetected == FALSE)
		{
			(log << "Removing a stream\n").Write();
			LogMessageIndent indent(&log);

			(log << "Service : " << serviceId << ", " << (serviceName ? serviceName : L"<name not set>") << "\n").Write();
			(log << "Stream Information\n").Write();
			LogMessageIndent indent2(&log);
			(*it)->PrintStreamDetails();

			if (*it) delete *it;

			m_streams.erase(it);
			it=m_streams.begin();

			bChange = TRUE;
		}
	}

	// All new streams not marked as detected need to be added.
	for ( it=pNewService->m_streams.begin() ; it != pNewService->m_streams.end() ; it++ )
	{
		if ((*it)->bDetected == FALSE)
		{
			(log << "Adding a stream\n").Write();
			LogMessageIndent indent(&log);

			(log << "Service : " << serviceId << ", " << (serviceName ? serviceName : L"<name not set>") << "\n").Write();

			DVBTChannels_Stream *pNewStream = new DVBTChannels_Stream();
			pNewStream->SetLogCallback(m_pLogCallback);
			pNewStream->UpdateStream(*it);
			m_streams.push_back(pNewStream);

			bChange = TRUE;
		}
	}

	return bChange;
}

void DVBTChannels_Service::PrintServiceDetails()
{
	(log << "Service Id             : " << serviceId << "\n").Write();
	(log << "Service Name           : " << (serviceName ? serviceName : L"<not set>") << "\n").Write();
	(log << "Logical channel number : " << logicalChannelNumber << "\n").Write();
}

DVBTChannels_Stream *DVBTChannels_Service::FindStreamByPID(long PID)
{
	CAutoLock lock(&m_streamsLock);

	std::vector<DVBTChannels_Stream *>::iterator it = m_streams.begin();
	for (; it < m_streams.end(); it++)
	{
		DVBTChannels_Stream *pStream = *it;
		if (pStream->PID == PID)
			return pStream;
	}
	return NULL;
}

//////////////////////////////////////////////////////////////////////
// DVBTChannels_Network
//////////////////////////////////////////////////////////////////////

DVBTChannels_Network::DVBTChannels_Network(DVBTChannels *pChannels) :
	m_pChannels(pChannels)
{
	originalNetworkId = 0;
	transportStreamId = 0;
	networkId = 0;
	frequency = 0;
	bandwidth = 0;
	otherFrequencyFlag = 0;
	networkName = NULL;
	m_dataListName = NULL;
	m_dataListString = NULL;

}

DVBTChannels_Network::~DVBTChannels_Network()
{
	CAutoLock lock(&m_servicesLock);

	std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
	for ( ; it != m_services.end() ; it++ )
	{
		DVBTChannels_Service *pService = *it;
		if (pService)
			delete pService;
	}
	m_services.clear();

	if (networkName)
		delete[] networkName;

	if (m_dataListName)
		delete[] m_dataListName;

	if (m_dataListString)
		delete[] m_dataListString;
}

void DVBTChannels_Network::SetLogCallback(LogMessageCallback *callback)
{
	LogMessageCaller::SetLogCallback(callback);

	CAutoLock lock(&m_servicesLock);

	std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
	for ( ; it != m_services.end() ; it++ )
	{
		DVBTChannels_Service *pService = *it;
		pService->SetLogCallback(callback);
	}
}
/*
HRESULT DVBTChannels_Network::LoadFromXML(XMLElement *pElement)
{
	CAutoLock lock(&m_servicesLock);

	XMLAttribute *attr;

	attr = pElement->Attributes.Item(L"Frequency");
	if (attr == NULL)
		return (log << "Frequency must be supplied in a network definition\n").Write(E_FAIL);
	frequency = StringToLong(attr->value);

	attr = pElement->Attributes.Item(L"Bandwidth");
	if (attr)
		bandwidth = StringToLong(attr->value);

	attr = pElement->Attributes.Item(L"OriginalNetworkId");
	if (attr != NULL)
		originalNetworkId = StringToLong(attr->value);

	attr = pElement->Attributes.Item(L"TransportStreamId");
	if (attr != NULL)
		transportStreamId = StringToLong(attr->value);

	attr = pElement->Attributes.Item(L"NetworkId");
	if (attr != NULL)
		networkId = StringToLong(attr->value);

	attr = pElement->Attributes.Item(L"Name");
	if (attr != NULL)
		strCopy(networkName, (attr ? attr->value : L""));

	int elementCount = pElement->Elements.Count();
	for ( int item=0 ; item<elementCount ; item++ )
	{
		XMLElement *element = pElement->Elements.Item(item);
		if (_wcsicmp(element->name, L"Service") == 0)
		{
			DVBTChannels_Service *pService = new DVBTChannels_Service();
			pService->SetLogCallback(m_pLogCallback);
			if (pService->LoadFromXML(element) == S_OK)
				m_services.push_back(pService);
			else
				delete pService;
			continue;
		}
		else if (_wcsicmp(element->name, L"Program") == 0)
		{
			DVBTChannels_Service *pService = new DVBTChannels_Service();
			pService->SetLogCallback(m_pLogCallback);
			if (pService->LoadFromXML(element) == S_OK)
				m_services.push_back(pService);
			else
				delete pService;
			continue;
		}
	}

	//Re-Order services by channel numbers
	if (g_pData->settings.application.orderChannels)
	{
		std::vector<DVBTChannels_Service *> services;
		while (m_services.size())
		{
			long chNumb = MAXLONG;
			std::vector<DVBTChannels_Service *>::iterator itsave;// = NULL; //ORROR
			std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
			for (; it < m_services.end(); it++)
			{
				DVBTChannels_Service *pService = *it;
				if (pService->logicalChannelNumber < chNumb)
					chNumb = pService->logicalChannelNumber;
			}

			it = m_services.begin();
			for (; it < m_services.end(); it++)
			{
				DVBTChannels_Service *pService = *it;
				if (pService->logicalChannelNumber == chNumb)
				{
					itsave = it;
					services.push_back(pService);
					it = m_services.end();
				}
			}
			if(itsave != NULL)
				m_services.erase(itsave);
		}

		if(services.size())
		{
			std::vector<DVBTChannels_Service *>::iterator it = services.begin();
			for (; it < services.end(); it++)
			{
				DVBTChannels_Service *pService = *it;
				m_services.push_back(pService);
			}
			services.clear();
		}
	}

	return S_OK;
}

HRESULT DVBTChannels_Network::SaveToXML(XMLElement *pElement)
{
	CAutoLock lock(&m_servicesLock);

	LPWSTR pValue = NULL;

	strCopy(pValue, frequency);
	pElement->Attributes.Add(new XMLAttribute(L"Frequency", pValue));
	delete pValue;
	pValue = NULL;

	strCopy(pValue, bandwidth);
	pElement->Attributes.Add(new XMLAttribute(L"Bandwidth", pValue));
	delete pValue;
	pValue = NULL;

	strCopyHex(pValue, originalNetworkId);
	pElement->Attributes.Add(new XMLAttribute(L"OriginalNetworkId", pValue));
	delete pValue;
	pValue = NULL;

	strCopyHex(pValue, transportStreamId);
	pElement->Attributes.Add(new XMLAttribute(L"TransportStreamId", pValue));
	delete pValue;
	pValue = NULL;

	strCopyHex(pValue, networkId);
	pElement->Attributes.Add(new XMLAttribute(L"NetworkId", pValue));
	delete pValue;
	pValue = NULL;

	pElement->Attributes.Add(new XMLAttribute(L"Name", (networkName ? networkName : L"")));

	std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
	for ( ; it != m_services.end() ; it++ )
	{
		DVBTChannels_Service *pService = *it;
		XMLElement *pProgElement = new XMLElement(L"Service");
		if (pService->SaveToXML(pProgElement) == S_OK)
			pElement->Elements.Add(pProgElement);
		else
			delete pProgElement;
	}

	return S_OK;
}
*/
DVBTChannels_Service* DVBTChannels_Network::FindDefaultService()
{
	CAutoLock lock(&m_servicesLock);

	if (m_services.size() > 0)
	{
		// TODO: add an attribute to services to set them as default.
		return m_services.at(0);
		return m_services.at(0);
	}
	return NULL;
}

DVBTChannels_Service *DVBTChannels_Network::FindServiceByServiceId(long serviceId)
{
	CAutoLock lock(&m_servicesLock);

	std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
	for (; it < m_services.end(); it++)
	{
		DVBTChannels_Service *pService = *it;
		if (pService->serviceId == serviceId)
			return pService;
	}
	return NULL;
}

DVBTChannels_Service *DVBTChannels_Network::FindNextServiceByServiceId(long serviceId)
{
	CAutoLock lock(&m_servicesLock);

	std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
	for (; it < m_services.end(); it++)
	{
		DVBTChannels_Service *pService = *it;
		if (pService->serviceId == serviceId)
		{
			it++;
			if (it < m_services.end())
				return *it;
			return m_services.front();
		}
	}
	return NULL;
}

DVBTChannels_Service *DVBTChannels_Network::FindPrevServiceByServiceId(long serviceId)
{
	CAutoLock lock(&m_servicesLock);

	std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
	for (; it < m_services.end(); it++)
	{
		DVBTChannels_Service *pService = *it;
		if (pService->serviceId == serviceId)
		{
			it--;
			if (it >= m_services.begin())
				return *it;
			return m_services.back();
		}
	}
	return NULL;
}

BOOL DVBTChannels_Network::UpdateNetwork(DVBTChannels_Network *pNewNetwork)
{
	if ((pNewNetwork->originalNetworkId == 0) &&
		(pNewNetwork->transportStreamId == 0) &&
		(pNewNetwork->networkId == 0))
	{
		(log << "Not Updating Network Information. Invalid information found. \n").Write();
		PrintNetworkDetails(pNewNetwork);
		return FALSE;
	}

	BOOL bChange =
		(originalNetworkId != pNewNetwork->originalNetworkId) ||		// onid has changed or
		(transportStreamId != pNewNetwork->transportStreamId) ||		// tsid has changed or
		(networkId != pNewNetwork->networkId) ||						// nid has changed or
		((frequency == 0) && (frequency != pNewNetwork->frequency)) ||	// frequency isn't set yet and has changed or
		((bandwidth == 0) && (bandwidth != pNewNetwork->bandwidth)) ||	// bandwidth isn't set yet and has changed or
		(networkName == NULL) ||										// name isn't set yet or
		((pNewNetwork->networkName) &&									//  new name is set and
		 (_wcsicmp(pNewNetwork->networkName, L"") != 0) &&				//  new name isn't blank and
		 (_wcsicmp(pNewNetwork->networkName, networkName) != 0));		//  new name has changed

	if (bChange)
	{
		BOOL bExistingData = (originalNetworkId != 0) ||
							 (transportStreamId != 0) ||
							 (networkId != 0) ||
							 (frequency != 0) ||
							 (bandwidth != 0) ||
							 (networkName != NULL);

		if (bExistingData)
			(log << "Updating Network Information\n").Write();
		else
			(log << "New Network Information\n").Write();
		LogMessageIndent indent1(&log);

		if (bExistingData)
		{
			(log << "Original Values\n").Write();
			LogMessageIndent indent2(&log);
			PrintNetworkDetails(this);
		}

		originalNetworkId = pNewNetwork->originalNetworkId;
		transportStreamId = pNewNetwork->transportStreamId;
		networkId = pNewNetwork->networkId;
		frequency = pNewNetwork->frequency;
		bandwidth = pNewNetwork->bandwidth;
		strCopy(networkName, pNewNetwork->networkName);
		if (bExistingData)
		{
			(log << "New Values\n").Write();
			LogMessageIndent indent2(&log);
			PrintNetworkDetails(this);
		}
		else
		{
			PrintNetworkDetails(this);
		}
	}

	//TODO: Remove services from the channels file that no longer exist.
	//      or just set a disabled flag or something.

	{
		CAutoLock lock(&m_servicesLock);

		std::vector<DVBTChannels_Service *>::iterator it = pNewNetwork->m_services.begin();
		for ( ; it != pNewNetwork->m_services.end() ; it++ )
		{
			DVBTChannels_Service *pNewService = *it;
			DVBTChannels_Service *pService = FindServiceByServiceId(pNewService->serviceId);

			if (!pService)
			{
				(log << "Adding a service\n").Write();
				LogMessageIndent indent(&log);
				pService = new DVBTChannels_Service();
				pService->SetLogCallback(m_pLogCallback);
				m_services.push_back(pService);
			}
			bChange |= pService->UpdateService(pNewService);
		}
	}

	return bChange;
}

void DVBTChannels_Network::PrintNetworkDetails(DVBTChannels_Network *pNetwork)
{
	(log << "Original network id : " << pNetwork->originalNetworkId << "\n").Write();
	(log << "Transport stream id : " << pNetwork->transportStreamId << "\n").Write();
	(log << "Network id          : " << pNetwork->networkId << "\n").Write();
	(log << "Frequency           : " << pNetwork->frequency << "\n").Write();
	(log << "Bandwidth           : " << pNetwork->bandwidth << "\n").Write();
	(log << "Network Name        : " << (pNetwork->networkName ? pNetwork->networkName : L"<not set>") << "\n").Write();
}

// IDWOSDDataList Methods
LPWSTR DVBTChannels_Network::GetListName()
{
	if (!m_dataListName)
	{
		LPWSTR listName = new wchar_t[1024];
//		swprintf(listName, L"TVChannels.Services.%i", this->originalNetworkId);
		swprintf(listName, L"TVChannels.Services.%i", this->transportStreamId);
		strCopy(m_dataListName, listName);
		delete[] listName;
	}
	return m_dataListName;
}

LPWSTR DVBTChannels_Network::GetListItem(LPWSTR name, long nIndex)
{
	CAutoLock lock(&m_servicesLock);

	if (nIndex >= (long)m_services.size())
		return NULL;

	long startsWithLength = strStartsWith(name, m_dataListName);
	if (startsWithLength > 0)
	{
		name += startsWithLength;

		DVBTChannels_Service *pService = m_services.at(nIndex);
		if (_wcsicmp(name, L".Name") == 0)
		{
			return pService->serviceName;
		}
		else if (_wcsicmp(name, L".ServiceId") == 0)
		{
			strCopy(m_dataListString, pService->serviceId);
			return m_dataListString;
		}
		else if (_wcsicmp(name, L".LogicalChannelNumber") == 0)
		{
			strCopy(m_dataListString, pService->logicalChannelNumber);
			return m_dataListString;
		}
	}
	return NULL;
}

long DVBTChannels_Network::GetListSize()
{
	CAutoLock lock(&m_servicesLock);
	return m_services.size();
}

HRESULT DVBTChannels_Network::FindListItem(LPWSTR name, int *pIndex)
{
	if (!pIndex)
        return E_INVALIDARG;

	*pIndex = 0;

	CAutoLock lock(&m_servicesLock);
	std::vector<DVBTChannels_Service *>::iterator it = m_services.begin();
	for ( ; it < m_services.end() ; it++ )
	{
		if (_wcsicmp((*it)->serviceName, name) == 0)
			return S_OK;

		(*pIndex)++;
	}

	return E_FAIL;
}

//////////////////////////////////////////////////////////////////////
// DVBTChannels_NetworkList
//////////////////////////////////////////////////////////////////////

DVBTChannels_NetworkList::DVBTChannels_NetworkList()
{
}

DVBTChannels_NetworkList::~DVBTChannels_NetworkList()
{
	Clear();
}

void DVBTChannels_NetworkList::SetLogCallback(LogMessageCallback *callback)
{
	LogMessageCaller::SetLogCallback(callback);

	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for ( ; it != m_networks.end() ; it++ )
	{
		DVBTChannels_Network *pNetwork = *it;
		pNetwork->SetLogCallback(callback);
	}
}

HRESULT DVBTChannels_NetworkList::Clear()
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for ( ; it != m_networks.end() ; it++ )
	{
		DVBTChannels_Network *pNetwork = *it;
		if (pNetwork)
			delete pNetwork;
	}
	m_networks.clear();

	return S_OK;
}

DVBTChannels_Network *DVBTChannels_NetworkList::CreateNetwork(long originalNetworkId, long transportStreamId, long networkId)
{
	DVBTChannels_Network *pNetwork = new DVBTChannels_Network(NULL);
	pNetwork->SetLogCallback(m_pLogCallback);
	pNetwork->originalNetworkId = originalNetworkId;
	pNetwork->transportStreamId = transportStreamId;
	pNetwork->networkId = networkId;

	CAutoLock lock(&m_networksLock);
	m_networks.push_back(pNetwork);
	return pNetwork;
}

DVBTChannels_Network *DVBTChannels_NetworkList::FindNetwork(long originalNetworkId, long transportStreamId, long networkId)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
		if ((pNetwork->originalNetworkId == originalNetworkId) &&
			(pNetwork->transportStreamId == transportStreamId) &&
			(pNetwork->networkId == networkId))
			return pNetwork;
	}
	return NULL;
}

DVBTChannels_Network *DVBTChannels_NetworkList::FindNetworkByONID(long originalNetworkId)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
		if (pNetwork->originalNetworkId == originalNetworkId)
			return pNetwork;
	}
	return NULL;
}

DVBTChannels_Network *DVBTChannels_NetworkList::FindNetworkByTSID(long transportStreamId)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
		if (pNetwork->transportStreamId == transportStreamId)
			return pNetwork;
	}
	return NULL;
}

DVBTChannels_Network *DVBTChannels_NetworkList::FindNetworkByFrequency(long frequency)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
		if (pNetwork->frequency == frequency)
			return pNetwork;
	}
	return NULL;
}

DVBTChannels_Network *DVBTChannels_NetworkList::FindNextNetworkByOriginalNetworkId(long oldOriginalNetworkId)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
		if (pNetwork->originalNetworkId == oldOriginalNetworkId)
		{
			it++;
			if (it < m_networks.end())
				return *it;
			return m_networks.front();
		}
	}
	return NULL;
}

DVBTChannels_Network *DVBTChannels_NetworkList::FindPrevNetworkByOriginalNetworkId(long oldOriginalNetworkId)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
		if (pNetwork->originalNetworkId == oldOriginalNetworkId)
		{
			it--;
			if (it >= m_networks.begin())
				return *it;
			return m_networks.back();
		}
	}
	return NULL;
}

DVBTChannels_Network *DVBTChannels_NetworkList::FindNextNetworkByFrequency(long oldFrequency)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
		if (pNetwork->frequency == oldFrequency)
		{
			it++;
			if (it < m_networks.end())
				return *it;
			return m_networks.front();
		}
	}
	return NULL;
}

DVBTChannels_Network *DVBTChannels_NetworkList::FindPrevNetworkByFrequency(long oldFrequency)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
		if (pNetwork->frequency == oldFrequency)
		{
			it--;
			if (it >= m_networks.begin())
				return *it;
			return m_networks.back();
		}
	}
	return NULL;
}


//////////////////////////////////////////////////////////////////////
// DVBTChannels
//////////////////////////////////////////////////////////////////////

DVBTChannels::DVBTChannels()
{
	m_bandwidth = 7;
	m_filename = NULL;
	m_dataListName = NULL;
	m_dataListString = NULL;
}

DVBTChannels::~DVBTChannels()
{
	Destroy();

	if (m_dataListName)
		delete[] m_dataListName;
	if (m_dataListString)
		delete[] m_dataListString;
}

void DVBTChannels::SetLogCallback(LogMessageCallback *callback)
{
	DVBTChannels_NetworkList::SetLogCallback(callback);
}

HRESULT DVBTChannels::Destroy()
{
	if (m_filename)
		delete[] m_filename;

	m_filename = NULL;

	return DVBTChannels_NetworkList::Clear();
}
/*
HRESULT DVBTChannels::LoadChannels(LPWSTR filename)
{
	CAutoLock lock(&m_networksLock);

	(log << "Loading DVBT Channels file: " << filename << "\n").Write();
	LogMessageIndent indent(&log);

	HRESULT hr;

	strCopy(m_filename, filename);

	XMLDocument file;
	file.SetLogCallback(m_pLogCallback);
	if FAILED(hr = file.Load(m_filename))
	{
		return (log << "Could not load channels file: " << m_filename << "\n").Show(hr);
	}

	int elementCount = file.Elements.Count();
	for ( int item=0 ; item<elementCount ; item++ )
	{
		XMLElement *element = file.Elements.Item(item);
		if (_wcsicmp(element->name, L"Bandwidth") == 0)
		{
			m_bandwidth = StringToLong(element->value);
			continue;
		}

		if (_wcsicmp(element->name, L"Network") == 0)
		{
			DVBTChannels_Network *pNewNetwork = new DVBTChannels_Network(this);
			pNewNetwork->SetLogCallback(m_pLogCallback);
			pNewNetwork->bandwidth = m_bandwidth;
			if (pNewNetwork->LoadFromXML(element) == S_OK)
			{
				m_networks.push_back(pNewNetwork);
				g_pOSD->Data()->AddList(pNewNetwork);
			}
			else
				delete pNewNetwork;
			continue;
		}
	}

	if (m_networks.size() == 0)
		return (log << "You need to specify at least one network in your channels file\n").Show(E_FAIL);

	indent.Release();
	(log << "Finished Loading DVBT Channels file: " << filename << "\n").Write();

	return S_OK;
}

HRESULT DVBTChannels::SaveChannels(LPWSTR filename)
{
	CAutoLock lock(&m_networksLock);

	XMLDocument file;
	file.SetLogCallback(m_pLogCallback);

	XMLElement *pElement = new XMLElement(L"Bandwidth");
	strCopy(pElement->value, m_bandwidth);
	file.Elements.Add(pElement);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for ( ; it < m_networks.end() ; it++ )
	{
		pElement = new XMLElement(L"Network");
		DVBTChannels_Network *pNetwork = *it;
		pNetwork->SaveToXML(pElement);
		file.Elements.Add(pElement);
	}

	if (filename)
		file.Save(filename);
	else
		file.Save(m_filename);

	return S_OK;
}
*/
long DVBTChannels::GetDefaultBandwidth()
{
	return m_bandwidth;
}

DVBTChannels_Network* DVBTChannels::FindDefaultNetwork()
{
	CAutoLock lock(&m_networksLock);

	if (m_networks.size() > 0)
	{
		return m_networks.at(0);
	}
	return NULL;
}


// Update Method
BOOL DVBTChannels::UpdateNetwork(DVBTChannels_Network *pNewNetwork)
{
	DVBTChannels_Network *pNetwork = FindNetwork(pNewNetwork->originalNetworkId, pNewNetwork->transportStreamId, pNewNetwork->networkId);

	if (!pNetwork)
		pNetwork = FindNetworkByFrequency(pNewNetwork->frequency);

	if (!pNetwork)
	{
		(log << "Adding a network\n").Write();
		LogMessageIndent indent(&log);
		pNetwork = new DVBTChannels_Network(this);
		pNetwork->SetLogCallback(m_pLogCallback);

		CAutoLock lock(&m_networksLock);
		m_networks.push_back(pNetwork);

//		g_pOSD->Data()->AddList(pNetwork);
	}

	if (pNetwork->UpdateNetwork(pNewNetwork))
	{
//		SaveChannels();
//		g_pTv->ExecuteCommandsQueue(L"UpdateChannels()");

		return TRUE;
	}
	return FALSE;
}

//HRESULT DVBTChannels::MoveNetworkUp(long originalNetworkId)
HRESULT DVBTChannels::MoveNetworkUp(long transportStreamId)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
//		if (pNetwork->originalNetworkId == originalNetworkId)
		if (pNetwork->transportStreamId == transportStreamId)
		{
			if (it > m_networks.begin())
			{
				m_networks.erase(it);
				it--;
				m_networks.insert(it, pNetwork);
//				SaveChannels();
				return S_OK;
			}
		}
	}
	return S_FALSE;
}

//HRESULT DVBTChannels::MoveNetworkDown(long originalNetworkId)
HRESULT DVBTChannels::MoveNetworkDown(long transportStreamId)
{
	CAutoLock lock(&m_networksLock);

	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for (; it < m_networks.end(); it++)
	{
		DVBTChannels_Network *pNetwork = *it;
//		if (pNetwork->originalNetworkId == originalNetworkId)
		if (pNetwork->transportStreamId == transportStreamId)
		{
			if (it < m_networks.end()-1)
			{
				m_networks.erase(it);
				it++;
				m_networks.insert(it, pNetwork);
//				SaveChannels();
				return S_OK;
			}
		}
	}
	return S_FALSE;
}

// IDWOSDDataList Methods
LPWSTR DVBTChannels::GetListName()
{
	if (!m_dataListName)
		strCopy(m_dataListName, L"TVChannels.Networks");
	return m_dataListName;
}

LPWSTR DVBTChannels::GetListItem(LPWSTR name, long nIndex)
{
	CAutoLock lock(&m_networksLock);

	if (nIndex >= (long)m_networks.size())
		return NULL;

	long startsWithLength = strStartsWith(name, m_dataListName);
	if (startsWithLength > 0)
	{
		name += startsWithLength;

		DVBTChannels_Network *pNetwork = m_networks.at(nIndex);
		if (_wcsicmp(name, L".name") == 0)
		{
			return pNetwork->networkName;
		}
		else if (_wcsicmp(name, L".originalNetworkId") == 0)
		{
			strCopy(m_dataListString, pNetwork->originalNetworkId);
			return m_dataListString;
		}
		else if (_wcsicmp(name, L".transportStreamId") == 0)
		{
			strCopy(m_dataListString, pNetwork->transportStreamId);
			return m_dataListString;
		}
		else if (_wcsicmp(name, L".networkId") == 0)
		{
			strCopy(m_dataListString, pNetwork->networkId);
			return m_dataListString;
		}
		else if (_wcsicmp(name, L".frequency") == 0)
		{
			strCopy(m_dataListString, pNetwork->frequency);
			return m_dataListString;
		}
		else if (_wcsicmp(name, L".bandwidth") == 0)
		{
			strCopy(m_dataListString, pNetwork->bandwidth);
			return m_dataListString;
		}
	}
	return NULL;
}

long DVBTChannels::GetListSize()
{
	CAutoLock lock(&m_networksLock);
	return m_networks.size();
}

HRESULT DVBTChannels::FindListItem(LPWSTR name, int *pIndex)
{
	if (!pIndex)
        return E_INVALIDARG;

	*pIndex = 0;

	CAutoLock lock(&m_networksLock);
	std::vector<DVBTChannels_Network *>::iterator it = m_networks.begin();
	for ( ; it < m_networks.end() ; it++ )
	{
		if (_wcsicmp((*it)->networkName, name) == 0)
			return S_OK;

		(*pIndex)++;
	}

	return E_FAIL;
}


