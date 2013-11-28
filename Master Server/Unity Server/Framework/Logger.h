//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2011 Bryce Booth
//
//  File Name   :   Logger.h
//  Description :   --------------------------
//
//  Author      :   Bryce Booth
//  Mail        :   bryce.booth@mediadesign.school.nz
//


#pragma once


#ifndef __Logger_H__
#define __Logger_H__


// Library Includes
#include <stdio.h>
#include <stdarg.h>
#include <list>
#include <string>
#include <map>


// Local Includes
#include "Framework/Application.h"
#include "Library/ProcessObject.h"
#include "Library/Observer.h"
#include "Defines/DataTypes.h"
#include "Defines/Macros.h"


// Defines
#define LOGGER CApplication::GetInstance().GetLogger()


#define LOG_MESSAGE(MESSAGE, ...) LOGGER.WriteMessage(MESSAGE, ##__VA_ARGS__);

#define LOG_CRITICAL_ERROR(MESSAGE_FORMAT, ...) LOGGER.WriteError(__FUNCTION__, __LINE__, MESSAGE_FORMAT, ##__VA_ARGS__); FW_BREAKPOINT;
#define LOG_CRITICAL_ERROR_ON(CONDITION, MESSAGE, ...) if (CONDITION) { LOG_CRITICAL_ERROR(MESSAGE, ##__VA_ARGS__); };

#define LOG_ERROR(MESSAGE_FORMAT, ...) LOGGER.WriteError(__FUNCTION__, __LINE__, MESSAGE_FORMAT, ##__VA_ARGS__); FW_BREAKPOINT;
#define LOG_ERROR_ON(CONDITION, MESSAGE, ...) if (CONDITION) { LOG_ERROR(MESSAGE, ##__VA_ARGS__); };


#define LOG_WARNING(MESSAGE_FORMAT, ...) LOGGER.WriteWarning(__FUNCTION__, __LINE__, MESSAGE_FORMAT,  ##__VA_ARGS__);
#define LOG_WARNING_ON(CONDITION, MESSAGE, ...) if (CONDITION) { LOG_WARNING(MESSAGE, ##__VA_ARGS__); };


#define VALIDATE(FUNCTION_CALL) if (!FUNCTION_CALL) { LOGGER.WriteError(__FUNCTION__, __LINE__, # FUNCTION_CALL); return (false); }
#define VALIDATE_VOID(FUNCTION_CALL) if (!FUNCTION_CALL) { LOGGER.WriteError(__FUNCTION__, __LINE__, # FUNCTION_CALL); return ; }


// Prototypes
class CFile;
class CApplication;


class CLogger : public CProcessObject, public IObserver<CApplication>
{

	// Member Types
public:


private:


	enum ESettings
	{
		WRITE_BUFFER_LENGTH = 4096,
		CONSOLE_MESSAGE_MAX = 10,
	};


	// Member Functions
public:


	 CLogger();
	~CLogger();


	bool Initialise();
	void Process();


	void WriteError(c_char* _kcpFunction, uint _uiLine, c_char* _kcpFormat, ...);
	void WriteWarning(c_char* _kcpFunction, uint _uiLine, c_char* _kcpFormat, ...);
	void WriteMessage(c_char* _kcpFormat, ...);


	void Clear();


	virtual void Notify(CApplication& _rSender, short _sSubject, void* _pData);


	// Inline Functions
	inline void SetErrorReportingEnabled(bool _bEnabled);
	inline void SetWarningReportingEnabled(bool _bEnabled);


	inline CFile& GetLoggerFile();


	inline void Track(c_char* _kcpName, c_float*  _kfpVariable);
	inline void Track(c_char* _kcpName, c_int*    _kipVariable);
	inline void Track(c_char* _kcpName, c_uint*   _kuipVariable);
	inline void Track(c_char* _kcpName, c_short*  _kspVariable);
	inline void Track(c_char* _kcpName, c_ushort* _kuspVariable);
	inline void Track(c_char* _kcpName, c_char*   _kcpVariable);
	inline void Track(c_char* _kcpName, c_uchar*  _kucpVariable);
	inline void Track(c_char* _kcpName, c_bool*   _kbpVariable);


	inline void Untrack(c_float*  _pfVariable);
	inline void Untrack(c_int*    _piVariable);
	inline void Untrack(c_uint*   _puiVariable);
	inline void Untrack(c_short*  _kspVariable);
	inline void Untrack(c_ushort* _kuspVariable);
	inline void Untrack(c_char*   _pcVariable);
	inline void Untrack(c_uchar*  _pucVariable);
	inline void Untrack(c_bool*   _pbVariable);


protected:


	void UpdateConsole();
	void UpdateScreenTrackText();


	void WriteVarList(uint _uiTargets, c_char* _kcpFormat, va_list& _rVarList);
	void WriteScreen(c_char* _kcpMessage);
	void WriteOutput(c_char* _kcpMessage);
	void WriteFile(c_char* _kcpMessage);


	void Untrack(std::list<c_void*>& _rContainingList, c_void* _kpVariablePointer);


private:


	CLogger(const CLogger& _krLogger);
	CLogger& operator = (const CLogger& _krLogger);


	// Member Variables
protected:


private:


	CFile* m_pLogFile;


	uint m_uiMaxScreenEntries;


	bool m_bUpdateConsole;
	bool m_bErrorsEnabled;
	bool m_bWarningsEnabled;


	std::vector<char*> m_MessageLog;


	std::list<c_float*> m_TrackingFloats;
	std::list<c_int*> m_TrackingInts;
	std::list<c_uint*> m_TrackingUInts;
	std::list<c_short*> m_TrackingShorts;
	std::list<c_ushort*> m_TrackingUShorts;
	std::list<c_char*> m_TrackingChars;
	std::list<c_uchar*> m_TrackingUChars;
	std::list<c_bool*> m_TrackingBools;


	std::map<c_void*, c_char*> m_mTrackingNames;


};


#include "Inline/Logger.inl"


#endif //__Logger_H__