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

#include "StdAfx.h"
#include "FolderTypeIdNameConverter.h"
#include <shlGuid.h>

wchar_t CFolderTypeIdNameConverter::_customGuidBuffer[100];

void CFolderTypeIdNameConverter::Insert(FOLDERTYPEID id, PCWSTR name)
{
	_fromNameToFolderTypeId[name] = id;
	_fromFolderTypeIdToName[id] = name;
}
CFolderTypeIdNameConverter::CFolderTypeIdNameConverter() 
{
	Insert(FOLDERTYPEID_Pictures,	L"Pictures");
	Insert(FOLDERTYPEID_Music,		L"Music");
	Insert(FOLDERTYPEID_Documents,	L"Documents");
	Insert(FOLDERTYPEID_Videos,		L"Videos");
	Insert(FOLDERTYPEID_Generic,	L"Generic");
}

//Convert folder type id guid to folder template name
PCWSTR CFolderTypeIdNameConverter::GetFolderTypeIdName(FOLDERTYPEID folderTypeId)
{
	map<FOLDERTYPEID, PCWSTR, GuidCompare>::iterator i = _fromFolderTypeIdToName.find(folderTypeId);
	if (i == _fromFolderTypeIdToName.end())
	{
		StringFromGUID2(folderTypeId, _customGuidBuffer, 100);
		return _customGuidBuffer;
	}
	return i->second;
}

//Convert folder template name to a folder id guid
HRESULT CFolderTypeIdNameConverter::GetFolderTypeIdFromName(PCWSTR templateName, FOLDERTYPEID *folderTypeId)
{
	map<PCWSTR, FOLDERTYPEID, StringShortCompare>::iterator i = _fromNameToFolderTypeId.find(templateName);
	if (i == _fromNameToFolderTypeId.end())
		return E_FAIL;
	*folderTypeId = i->second;
	return S_OK;
}