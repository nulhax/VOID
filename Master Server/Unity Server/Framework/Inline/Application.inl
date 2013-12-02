//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//
//  File Name :   Application.inl
//
//  Author    :   Bryce Booth
//  Mail      :   brycebooth@hotmail.com
//


// Local Includes


// Library Includes


// Implementation


CLogger& 
CApplication::GetLogger()
{
	return (*m_pLogger);
}


CClock& 
CApplication::GetClock()
{
	return (*m_pClock);
}


CRakMasterServer& 
CApplication::GetMasterServer()
{
	return (*m_pMasterServer);
}


bool
CApplication::IsQuitting()
{
	return (m_bQuit);
}


CApplication& 
CApplication::GetInstance()
{
	return (*s_pInstance);
}