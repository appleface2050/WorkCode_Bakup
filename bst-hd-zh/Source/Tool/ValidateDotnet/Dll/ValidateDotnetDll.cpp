#include <windows.h>
#include <iostream>
#include "ValidateDotnetDll.h"
#include "CValidateDotnet.h"
#include "CMultiData.h"

// This is an example of an exported function.
VALIDATEDOTNETDLL_API DWORD ValidateDotnet(wchar_t * params[], int paramsSize)
{
	Parameters parameters;
	if(NULL != params)
	{
		for (int index = 0; index < paramsSize; ++index)
		{
			const wchar_t * wc = params[index];
			if(NULL != wc)
			{
				parameters.AddParamter(wc);
			}
		}
	}

	DWORD result = 0;
	if (parameters.GetHelp())
	{
		parameters.OutputHelp();
	}else
	{
		result = CValidateDotnet::ValidateDotNet(parameters);
	}
	return result;
}