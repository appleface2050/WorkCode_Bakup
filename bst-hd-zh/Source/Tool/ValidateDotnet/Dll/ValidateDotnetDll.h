#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_VALIDATEDOTNETDLL_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_VALIDATEDOTNETDLL_H

#ifdef VALIDATEDOTNETDLL_EXPORTS
#define VALIDATEDOTNETDLL_API __declspec(dllexport)
#else
#define VALIDATEDOTNETDLL_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

	//************************************
	// Method:    ValidateDotnet
	// FullName:  ValidateDotnet
	// Access:    public 
	// Returns:   int
	//            0£º.net is ok
	//            1: reg fial
	//            2: run fial
	// Qualifier:
	// Parameter: DWORD checkItems
	//            0: check all
	//            1: check reg
	//            2: check the run
	//            3: check the reg and the run
	// Parameter: wchar_t * runExe  the full path fo program of .net 
	// Parameter: bool isRegistry, true to write the registry
	//************************************
	VALIDATEDOTNETDLL_API DWORD ValidateDotnet(wchar_t * params[], int paramsSize);

	
	
#ifdef __cplusplus
}
#endif

#endif// TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_VALIDATEDOTNETDLL_H