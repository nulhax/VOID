//
//  Bryce Booth


//  Auckland
//  New Zealand
//
//  File Name :   StrUtilities.cpp
//
//  Author    :   Bryce Booth
//  Mail      :   bryce.booth@mediadesign.school.nz
//


#include "StrUtilities.h"


// Local Includes


// Library Includes
#include <sstream>


// Implementation


namespace StrUtilities
{


	std::string
	ExtractFilename(const std::string& _krFile)
	{
		uint uiPosition = _krFile.find_last_of('\\');


		return (_krFile.substr(uiPosition + 1));
	}


	std::string
	ExtractDirectoryPath(const std::string& _krFile)
	{
		size_t pos = _krFile.find_last_of("\\/");
		
		
		return (std::string::npos == pos) ? "" : _krFile.substr(0, pos + 1);
	}


	std::string ConvertToString(int _iValue)
	{
		std::stringstream StringStream;
		StringStream << _iValue;


		return (StringStream.str());
	}


	std::string&
	ConvertToString(const std::wstring& _kwrInput, std::string& _rOutput)
	{
		uint uiInputLength = _kwrInput.length();
		_rOutput.resize(uiInputLength);


		for (uint i = 0; i < uiInputLength; ++ i)
		{
			_rOutput[i] = static_cast<char>(_kwrInput[i]);
		}


		return (_rOutput);
	}


	std::wstring&
	ConvertToWString(const std::string& _krInput, std::wstring& _rwOutput)
	{
		uint uiInputLength = _krInput.length();
		_rwOutput.resize(uiInputLength);


		for (uint i = 0; i < uiInputLength; ++ i)
		{
			_rwOutput[i] = static_cast<char>(_krInput[i]);
		}


		return (_rwOutput);
	}


	std::string ConvertToString(uint _uiValue)
	{
		std::stringstream StringStream;
		StringStream << _uiValue;


		return (StringStream.str());
	}


	char*
	ConvertToString(const wchar_t* _kwcpInput, char*& _cprOutput)
	{
		// Get input string length plus null terminator
		unsigned int uiStrLength =  static_cast<uint>(wcslen(_kwcpInput)) + 1;


		// Create new wchar_t variable based on string length
		_cprOutput = new char[uiStrLength];


		// Literate through input string and copy characters
		for (uint i = 0; i < uiStrLength; ++i)
		{
			_cprOutput[i] = static_cast<char>(_kwcpInput[i]);
		}


		return (_cprOutput);
	}


	wchar_t*
	ConvertToWString(c_char* _kcpInput, wchar_t*& _wcprOutput)
	{
		// Get input string length plus null terminator
		unsigned int uiStrLength =  static_cast<uint>(strlen(_kcpInput)) + 1;


		// Create new wchar_t variable based on string length
		_wcprOutput = new wchar_t[uiStrLength];


		// Iterate through input string and copy characters
		for (unsigned int i = 0; i < uiStrLength; ++i)
		{
			_wcprOutput[i] = _kcpInput[i];
		}


		return (_wcprOutput);
	}


	void
	ConvertToInt(const std::string& _krString, int& _irReturnValue)
	{
		std::stringstream StringStream(_krString);
		StringStream >> _irReturnValue;
	}


	void
	ConvertToFloat(const std::string& _krString, float& _frReturnValue)
	{
		std::stringstream StringStream(_krString);
		StringStream >> _frReturnValue;
	}


	std::string&
	ToLower(std::string& _rString)
	{
		for (uint i = 0; i < _rString.length(); ++ i)
		{
			if (_rString[i] >= 'A' &&
				_rString[i] <= 'Z')
			{
				_rString[i] = _rString[i] + 32;
			}
		}


		return (_rString);
	}



	char*
	Copy(c_char* _kcpInput, char*& _prTarget)
	{
		// Input string is not instanced
		//assert(_kcpInput != 0);


		uint uiNumCharacters = strlen(_kcpInput) + 1;


		_prTarget= new char[uiNumCharacters];


		memcpy(_prTarget, _kcpInput, sizeof(char) * uiNumCharacters);


		return (_prTarget);
	}


	std::string&
	Trim(std::string& _rString)
	{
		TrimRight(_rString);
		TrimLeft(_rString);

		return (_rString);
	}


	std::string&
	TrimRight(std::string& _rString)
	{
		uint uiEndPosition = _rString.find_last_not_of(" \t");


		if(std::string::npos != uiEndPosition)
		{
			_rString = _rString.substr(0, uiEndPosition + 1);
		}


		return (_rString);
	}


	std::string&
	TrimLeft(std::string& _rString)
	{
		uint uiStartPosition = _rString.find_first_not_of(" \t");


		if(std::string::npos != uiStartPosition)
		{
			_rString = _rString.substr(uiStartPosition);
		}


		return (_rString);
	}


	bool
	Compare(c_char* _kcpLeft, c_char* _kcpRight)
	{
		return (strcmp(_kcpLeft, _kcpRight) == 0);
	}


};