#include <iostream>
#include "Framework/Application.h"




int main()
{
	CApplication cApplication;
	cApplication.Initialise();

	while (!cApplication.IsQuitting())
	{
		bool bExecuted = cApplication.ExecuteOneFrame();

		if (!bExecuted)
		{
			break;
		}
	}

	return (0);
}