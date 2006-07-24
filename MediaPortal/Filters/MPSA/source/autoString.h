class CAutoString
{
public:
	CAutoString(int len);
	virtual ~CAutoString();
	char* GetBuffer() ;
private:
	char* m_pBuffer;
};
