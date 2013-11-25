//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//
//  File Name :   ProcessObject.inl
//
//  Author    :   Bryce Booth
//  Mail      :   brycebooth@hotmail.com
//


// Local Includes


// Library Includes


// Implementation

uint 
CProcessObject::GetProcessOrderIndex() const
{
	return (m_uiProcessOrderIndex);
}

bool 
CProcessObject::IsProcessingEnabled() const
{
	return (m_bProcessingEnabled);
}

std::list<CProcessObject*>& 
CProcessObject::GetProcessObjectsList()
{
	return (s_ProcessObjects);
}