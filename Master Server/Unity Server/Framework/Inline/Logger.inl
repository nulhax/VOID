//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//
//  File Name : Logger.inl
//
//  Author :	  Bryce Booth
//  Mail :	  bryce.booth@mediadesign.school.nz
//


// Library Includes


// Local Includes


// Implementation


void
CLogger::SetErrorReportingEnabled(bool _bEnabled)
{
	m_bErrorsEnabled = _bEnabled;
}


void
CLogger::SetWarningReportingEnabled(bool _bEnabled)
{
	m_bWarningsEnabled = _bEnabled;
}


CFile&
CLogger::GetLoggerFile()
{
	return (*m_pLogFile);
}


void
CLogger::Track(c_char* _kcpName, c_float*  _kfpVariable)
{
	m_TrackingFloats.push_back(_kfpVariable);
	m_mTrackingNames.insert( std::pair<c_void*, c_char*>(_kfpVariable, _kcpName) );
}


void
CLogger::Track(c_char* _kcpName, c_int* _kipVariable)
{
	m_TrackingInts.push_back(_kipVariable);
	m_mTrackingNames.insert( std::pair<c_void*, c_char*>(_kipVariable, _kcpName) );
}


void
CLogger::Track(c_char* _kcpName, c_uint* _kuipVariable)
{
	m_TrackingUInts.push_back(_kuipVariable);
	m_mTrackingNames.insert( std::pair<c_void*, c_char*>(_kuipVariable, _kcpName) );
}


void
CLogger::Track(c_char* _kcpName, c_short*  _kspVariable)
{
	m_TrackingShorts.push_back(_kspVariable);
	m_mTrackingNames.insert( std::pair<c_void*, c_char*>(_kspVariable, _kcpName) );
}


void
CLogger::Track(c_char* _kcpName, c_ushort* _kuspVariable)
{
	m_TrackingUShorts.push_back(_kuspVariable);
	m_mTrackingNames.insert( std::pair<c_void*, c_char*>(_kuspVariable, _kcpName) );
}


void
CLogger::Track(c_char* _kcpName, c_char* _kcpVariable)
{
	m_TrackingChars.push_back(_kcpVariable);
	m_mTrackingNames.insert( std::pair<c_void*, c_char*>(_kcpVariable, _kcpName) );
}


void
CLogger::Track(c_char* _kcpName, c_uchar*  _kucpVariable)
{
	m_TrackingUChars.push_back(_kucpVariable);
	m_mTrackingNames.insert( std::pair<c_void*, c_char*>(_kucpVariable, _kcpName) );
}


void
CLogger::Track(c_char* _kcpName, c_bool* _kbpVariable)
{
	m_TrackingBools.push_back(_kbpVariable);
	m_mTrackingNames.insert( std::pair<c_void*, c_char*>(_kbpVariable, _kcpName) );
}


void
CLogger::Untrack(c_float*  _pfVariable)
{
	Untrack(reinterpret_cast<std::list<c_void*>&>(m_TrackingFloats), reinterpret_cast<c_void*>(_pfVariable));
}


void
CLogger::Untrack(c_int* _piVariable)
{
	Untrack(reinterpret_cast<std::list<c_void*>&>(m_TrackingInts), reinterpret_cast<c_void*>(_piVariable));
}


void
CLogger::Untrack(c_uint* _puiVariable)
{
	Untrack(reinterpret_cast<std::list<c_void*>&>(m_TrackingUInts), reinterpret_cast<c_void*>(_puiVariable));
}


void
CLogger::Untrack(c_short*  _kspVariable)
{
	Untrack(reinterpret_cast<std::list<c_void*>&>(m_TrackingShorts), reinterpret_cast<c_void*>(_kspVariable));
}


void
CLogger::Untrack(c_ushort* _kuspVariable)
{
	Untrack(reinterpret_cast<std::list<c_void*>&>(m_TrackingUShorts), reinterpret_cast<c_void*>(_kuspVariable));
}


void
CLogger::Untrack(c_char* _pcVariable)
{
	Untrack(reinterpret_cast<std::list<c_void*>&>(m_TrackingChars), reinterpret_cast<c_void*>(_pcVariable));
}


void
CLogger::Untrack(c_uchar*  _pucVariable)
{
	Untrack(reinterpret_cast<std::list<c_void*>&>(m_TrackingUChars), reinterpret_cast<c_void*>(_pucVariable));
}


void
CLogger::Untrack(c_bool* _pbVariable)
{
	Untrack(reinterpret_cast<std::list<c_void*>&>(m_TrackingBools), reinterpret_cast<c_void*>(_pbVariable));
}