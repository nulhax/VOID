//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2011 Bryce Booth
//
//  File Name :   Logger.cpp
//
//  Author    :   Bryce Booth
//  Mail      :   bryce.booth@mediadesign.school.nz
//


#include "Logger.h"


// Local Includes
#include "Utilities/StrUtilities.h"
#include "Framework/Application.h"
#include "Framework/File.h"
#include "Defines/Macros.h"


// Library Includes
#include <sstream>
#include <stdlib.h>
#include <stdarg.h>
#include <stdio.h>
#include <windows.h>


// Implementation


/********************************
            Public
*********************************/


/*---------------------------------------------------------------------------------------------------------------------------
*
* Logger constructors
*
*---------------------------------------------------------------------------------------------------------------------------*/

CLogger::CLogger()
: m_pLogFile(0)
, m_uiMaxScreenEntries(15)
, m_bUpdateConsole(false)
, m_bErrorsEnabled(true)
, m_bWarningsEnabled(true)
{
	// Empty
}


/*---------------------------------------------------------------------------------------------------------------------------
*
* Logger deconstruction
*
*---------------------------------------------------------------------------------------------------------------------------*/

CLogger::~CLogger()
{
	FW_DELETE(m_pLogFile);
}


bool
CLogger::Initialise()
{
	APPLICATION.Subscribe(this, CApplication::SUBJECT_INIT_COMPLETE); // Cannot instance text until renderer is made


	//m_pLogFile = new IFile();
	//m_pLogFile->Open("Logger File.txt", false, true, true, true);


	return (true);
}


void
CLogger::Process()
{
	if (m_bUpdateConsole)
	{
		UpdateConsole();
	}


	UpdateScreenTrackText();
}


void
CLogger::WriteError(c_char* _kcpFunction, uint _uiLine, c_char* _kcpFormat, ...)
{
	// Checks error logging enabled
	if (m_bErrorsEnabled)
	{
		// Get parameters
		va_list cpVarList;
		va_start(cpVarList, _kcpFormat);


		// Write parameters
		char cWriteBuffer[WRITE_BUFFER_LENGTH];
		vsprintf_s(cWriteBuffer, _kcpFormat, cpVarList);


		// Log message
		WriteMessage("[ERROR] %s (%s:%d)", cWriteBuffer, _kcpFunction, _uiLine);


		va_end(cpVarList);
	}
}


void
CLogger::WriteWarning(c_char* _kcpFunction, uint _uiLine, c_char* _kcpFormat, ...)
{
	// Check warning logging enabled
	if (m_bWarningsEnabled)
	{
		// Get parameters
		va_list cpVarList;
		va_start(cpVarList, _kcpFormat);


		// Write parameters
		char cWriteBuffer[WRITE_BUFFER_LENGTH];
		vsprintf_s(cWriteBuffer, _kcpFormat, cpVarList);


		// Log message
		WriteMessage("[WARNING] %s (%s:%d)", cWriteBuffer, _kcpFunction, _uiLine);


		va_end(cpVarList);
	}
}


void
CLogger::WriteMessage(c_char* _kcpFormat, ...)
{
	// Get parameters
	va_list cpVarList;
	va_start(cpVarList, _kcpFormat);


	// Write parameters
	char cWriteBuffer[WRITE_BUFFER_LENGTH];
	vsprintf_s(cWriteBuffer, _kcpFormat, cpVarList);


	WriteScreen(cWriteBuffer);
	WriteOutput(cWriteBuffer);
	//WriteFile(cWriteBuffer);


	va_end(cpVarList);
}


void
CLogger::Clear()
{
	m_TrackingFloats.clear();
	m_TrackingInts.clear();
	m_TrackingUInts.clear();
	m_TrackingShorts.clear();
	m_TrackingUShorts.clear();
	m_TrackingChars.clear();
	m_TrackingUChars.clear();
	m_TrackingBools.clear();
	m_mTrackingNames.clear();
}


void 
CLogger::Notify(CApplication& _rSender, short _sSubject, void* _pData)
{
	// Have to wait for renderer to be made before the text can be initialized
	if (_sSubject == CApplication::SUBJECT_INIT_COMPLETE)
	{
		// Empty
	}
}


/********************************
            Protected
*********************************/


void
CLogger::UpdateConsole()
{
	if (m_MessageLog.size() > 0)
	{
		uint uiTotalLength = 0;
		std::vector<char*>::iterator StartMsg = m_MessageLog.begin();
		std::vector<char*>::iterator CurrentMsg = m_MessageLog.begin();
		std::vector<char*>::iterator End = m_MessageLog.end();


		if (m_MessageLog.size() > CONSOLE_MESSAGE_MAX)
		{
			StartMsg += (m_MessageLog.size() - CONSOLE_MESSAGE_MAX);
		}


		CurrentMsg = StartMsg;


		// Track message lengths
		uint uiMessageLengths[CONSOLE_MESSAGE_MAX];
		FW_MEMZERO(uiMessageLengths, sizeof(uint) * CONSOLE_MESSAGE_MAX);


		for (uint i = 0; i < CONSOLE_MESSAGE_MAX; ++ i)
		{
			// Save message length, plus new line
			uiMessageLengths[i] = strlen(*CurrentMsg) + 1;


			// Add length of message
			uiTotalLength += uiMessageLengths[i];


			// Increment message
			++ CurrentMsg;


			// Break if there is not enough messages
			if (CurrentMsg == End)
			{
				break;
			}
		}


		// Create buffer. plus null terminator
		char* cpBuffer = new char[uiTotalLength + 1];
		char* cpBufferPosition = cpBuffer;


		// Add null terminator
		cpBuffer[uiTotalLength] = '\0';
	
	
		// Reset message iterations
		CurrentMsg = StartMsg;


		for (uint i = 0; i < CONSOLE_MESSAGE_MAX; ++ i)
		{
			// Copy in text
			FW_MEMCOPY(cpBufferPosition, (*CurrentMsg), sizeof(char) * (uiMessageLengths[i] - 1));


			// Increment buffer position
			cpBufferPosition += (uiMessageLengths[i] - 1);


			// Add new line
			*(cpBufferPosition ++) = '\n';


			// Increment message
			++ CurrentMsg;


			// Break if there is not enough messages
			if (CurrentMsg == End)
			{
				break;
			}
		}


		// Cleanup
		FW_ADELETE(cpBuffer);
	}
}


void
CLogger::UpdateScreenTrackText()
{
	return ;
	static std::stringstream StringStream;
	StringStream.str("");

	std::list<c_float*>::iterator FloatCurrent = m_TrackingFloats.begin();
	std::list<c_int*>::iterator IntCurrent = m_TrackingInts.begin();
	std::list<c_uint*>::iterator UIntCurrent = m_TrackingUInts.begin();
	std::list<c_short*>::iterator ShortCurrent = m_TrackingShorts.begin();
	std::list<c_ushort*>::iterator UShortCurrent = m_TrackingUShorts.begin();
	std::list<c_char*>::iterator CharCurrent = m_TrackingChars.begin();
	std::list<c_uchar*>::iterator UCharCurrent = m_TrackingUChars.begin();
	std::list<c_bool*>::iterator BoolCurrent = m_TrackingBools.begin();
	std::map<c_void*, c_char*>::iterator TrackedVarName;


	// Floats
	for (FloatCurrent; FloatCurrent != m_TrackingFloats.end(); ++ FloatCurrent)
	{
		TrackedVarName = m_mTrackingNames.find((*FloatCurrent));
		StringStream << (*TrackedVarName).second << ": " << *(*FloatCurrent) << "\n";
	}


	// Ints
	for (IntCurrent; IntCurrent != m_TrackingInts.end(); ++ IntCurrent)
	{
		TrackedVarName = m_mTrackingNames.find((*IntCurrent));
		StringStream << (*TrackedVarName).second << ": " << *(*IntCurrent) << "\n";
	}


	// Unsigned Ints
	for (UIntCurrent; UIntCurrent != m_TrackingUInts.end(); ++ UIntCurrent)
	{
		TrackedVarName = m_mTrackingNames.find((*UIntCurrent));
		StringStream << (*TrackedVarName).second << ": " << *(*UIntCurrent) << "\n";
	}


	// Strings
	for (CharCurrent; CharCurrent != m_TrackingChars.end(); ++ CharCurrent)
	{
		TrackedVarName = m_mTrackingNames.find((*CharCurrent));
		StringStream << (*TrackedVarName).second << ": " << (*CharCurrent) << "\n";
	}
}


void
CLogger::WriteScreen(c_char* _kcpMessage)
{
	std::cout << _kcpMessage << std::endl;
}


void
CLogger::WriteOutput(c_char* _kcpMessage)
{
#ifdef _WIN32
	wchar_t* wcpMessage = 0;
	StrUtilities::ConvertToWString(_kcpMessage, wcpMessage);


	OutputDebugString(_kcpMessage);
	OutputDebugString("\n");


	FW_ADELETE(wcpMessage);
#endif //_WIN32
}


void
CLogger::WriteFile(c_char* _kcpMessage)
{
	//m_pLogFile->Write(_kcpMessage, strlen(_kcpMessage));
}


void
CLogger::Untrack(std::list<c_void*>& _rContainingList, c_void* _kpVariablePointer)
{
	std::list<c_void*>::iterator Current = _rContainingList.begin();
	std::list<c_void*>::iterator End = _rContainingList.end();


	while (Current != End)
	{
		if ((*Current) == _kpVariablePointer)
		{
			_rContainingList.erase(Current);

			break;
		}
	}


	std::map<c_void*, const char*>::iterator Name = m_mTrackingNames.find(_kpVariablePointer);


	if (Name != m_mTrackingNames.end())
	{
		FW_ADELETE((*Name).second);
		m_mTrackingNames.erase(Name);
	}
}


/********************************
            Private
*********************************/