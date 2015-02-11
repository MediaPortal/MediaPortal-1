#include "../Parsing/CCParser.h"

interface ICcDataSink;

class CCcDataProcessor	: public CCcParser
{
public:
	CCcDataProcessor();
	~CCcDataProcessor();
	
	STDMETHODIMP AddDataSink( ICcDataSink* pSink, DWORD* pidSink );
    STDMETHODIMP RemoveDataSink( DWORD idSink );
    
	STDMETHODIMP get_Channel( int* iChannel );
    STDMETHODIMP put_Channel( int iChannel );
        
    STDMETHODIMP get_Service( AM_LINE21_CCSERVICE* piService );
    STDMETHODIMP put_Service( AM_LINE21_CCSERVICE iService );

    STDMETHODIMP get_XformType( ICcParser_CCTYPE* piType );
    STDMETHODIMP put_XformType( ICcParser_CCTYPE piType );

	void ProcessData( int cbData, const BYTE* pSrc, BYTE* pToTransform, CAtlArray<WORD>* pCCData, bool bIsSubtypeAVC1, DWORD dwFlags, REFERENCE_TIME sourceTime );

	//	CCcParser
	virtual bool OnCc( int nType, int iField, CCWORD ccField );
	virtual const BYTE* OnUserData( bool bPorI, const BYTE* p, const BYTE* pStop, REFERENCE_TIME sourceTime, bool bIsSubtypeAVC1 );
	virtual bool OnCCSet( bool bPorI, int nType, const CCWORDSET& ccSet, bool bIsSubtypeAVC1 );
	virtual void Reset();

private:
	CCcTextParser::CCPROFILE m_profile;
	ICcParser_CCTYPE m_iTransformType;

    auto_pif<ICcDataSink> m_pifDataSink; //TODO: multiple

	const BYTE* m_pSrcData;
	CAtlArray<WORD>* m_pCCData;
	BYTE* m_pDataToTransform;

	ICcParser_CCTYPE m_nRecentlyRecognizedType;
	CCWORDSET m_ccRecentlyRecognizedSet;
};

