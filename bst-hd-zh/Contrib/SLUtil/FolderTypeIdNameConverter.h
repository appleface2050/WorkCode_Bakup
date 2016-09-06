// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

#pragma once

#include <map>
#include <knownfolders.h>
#include <shTypes.h>

struct StringShortCompare
{
	bool operator()(PCWSTR first, PCWSTR second) const
	{
		int length = min(lstrlenW(first), lstrlenW(second));
		return CompareString(LOCALE_NEUTRAL, NORM_IGNORECASE, first, length, second, length) == CSTR_LESS_THAN;
	}
};

struct GuidCompare
{
	bool operator()(GUID first, GUID second) const
	{
		return memcmp(&first, &second, sizeof(GUID)) < 0;
	}
};

class CFolderTypeIdNameConverter
{
private:
	CFolderTypeIdNameConverter(const CFolderTypeIdNameConverter &) {};
	static wchar_t _customGuidBuffer[100];

	map<PCWSTR, FOLDERTYPEID, StringShortCompare> _fromNameToFolderTypeId;
	map<FOLDERTYPEID, PCWSTR, GuidCompare> _fromFolderTypeIdToName;

	void Insert(FOLDERTYPEID id, PCWSTR name);
	
public:
	CFolderTypeIdNameConverter();
	
	//Convert folder type id guid to folder template name
	PCWSTR GetFolderTypeIdName(FOLDERTYPEID folderTypeId);
	
	//Convert folder template name to a folder id guid
	HRESULT GetFolderTypeIdFromName(PCWSTR templateName, FOLDERTYPEID *folderTypeId);
};