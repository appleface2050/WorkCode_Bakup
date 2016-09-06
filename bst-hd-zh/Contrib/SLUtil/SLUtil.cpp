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

// SLUtil.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "CommandLineInterpreter.h"
#include "FolderTypeIdNameConverter.h"

int _tmain(int argc, _TCHAR* argv[])
{
	//Initiate COM
	CoInitialize(NULL);
	CCommandLineInterpreter::Execute(argc, argv);
	CoUninitialize();
	return 0;
}

// Get the library com object and the full path of a library.
// If shellLibrary is NULL, don't return it.
// If libraryFullPath is NULL, don't return it.
// Call relese on the shellLibrary object. Call CoTaskMemFree on the libraryFullPath
HRESULT GetLibraryFromLibrariesFolder(PCWSTR libraryName, IShellLibrary **shellLibrary, bool openRead = true, PWSTR *libraryFullPath = NULL)
{
	//Create the real library file name
	wstring realLibraryName(libraryName);
	realLibraryName += L".library-ms";

	IShellItem2 *shellItem = NULL;

	//Get the shell item that represent the library
	HRESULT hr = SHCreateItemInKnownFolder(FOLDERID_UsersLibraries, KF_FLAG_DEFAULT_PATH|KF_FLAG_NO_ALIAS , realLibraryName.c_str(), IID_PPV_ARGS(&shellItem));
	if (FAILED(hr))
	{
		return hr;
	}

	//In case a file-system full path is needed
	//extract the information from the Shell Item ParsingPath property
	if (libraryFullPath != NULL)
	{
		hr = shellItem->GetString(PKEY_ParsingPath, libraryFullPath); 
	}

	//Get the shellLibrary object from the shell item with a read/write permitions
	if (shellLibrary != NULL)
	{
		hr = SHLoadLibraryFromItem(shellItem, openRead ? STGM_READ : STGM_READWRITE, IID_PPV_ARGS(shellLibrary));
	}
	
	if (shellItem != NULL)
		shellItem->Release();
	
	return hr;
}

//compare strings for equality for the length of the shorter 
//string
bool CompareStringShort(PCWSTR first, PCWSTR second)
{
	int length = min(lstrlenW(first), lstrlenW(second));
	return CompareString(LOCALE_NEUTRAL, NORM_IGNORECASE, first, length, second, length) == CSTR_EQUAL;
}


//Create a new shell library
void CreateLibrary(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = NULL;
	IShellItem *savedTo = NULL;

	//Create the shell library COM object
	HRESULT hr = SHCreateLibrary(IID_PPV_ARGS(&shellLibrary));
	if (FAILED(hr))
	{
		wcerr << L"Create: Can't create Shell Library COM object." << endl;
		exit(4);
	}
		
	//Save the new library under the user's Libraries folder.
	//If a library with the same name is already exists, add a number to the name to create
	//unique name
	hr = shellLibrary->SaveInKnownFolder(FOLDERID_UsersLibraries, arguments[0], LSF_MAKEUNIQUENAME, &savedTo);
	if (FAILED(hr))
	{
		wcerr << L"Create: Can't create Shell Library." << endl;
		exit(5);
	}
	
	if (shellLibrary != NULL)
		shellLibrary->Release();

	if (savedTo != NULL)
		savedTo->Release();
}
COMMAND(L"Create", L"SLUtil Create LibraryName", L"Create a new library", L"SLUtil Create MyLib", 1, CreateLibrary);


//Open an existing library
IShellLibrary *OpenLibrary(PCWSTR commandName, PCWSTR libraryName, bool openRead = true)
{
	IShellLibrary *shellLibrary = NULL;
	
	HRESULT hr = GetLibraryFromLibrariesFolder(libraryName, &shellLibrary, openRead);
	if (FAILED(hr))
	{
		wcerr << commandName << L": Can't load library." << endl;
		exit(4);
	}
	return shellLibrary;
}


//Add a folder to a library
void AddFolder(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = OpenLibrary(L"AddFolder", arguments[0], false);
		
	HRESULT hr = SHAddFolderPathToLibrary(shellLibrary, arguments[1]);
	if (FAILED(hr))
	{
		wcerr << L"AddFolder: Can't add folder to the library." << endl;
		exit(6);
	}

	//Commit the library changes
	shellLibrary->Commit();

	if (shellLibrary != NULL)
			shellLibrary->Release();
} 
COMMAND(L"AddFolder", L"SLUtil AddFolder LibraryName FolderPath", L"Add a folder to a library", L"SLUtil AddFolder Documents C:\\Docs", 2, AddFolder);

//Remove a folder from a library
void RemoveFolder(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = OpenLibrary(L"RemoveFolder", arguments[0], false);
		
	HRESULT hr = SHRemoveFolderPathFromLibrary(shellLibrary, arguments[1]);
	if (FAILED(hr))
	{
		wcerr << L"RemoveFolder: Can't remove folder from the library." << endl;
		exit(5);
	}

	//Commit the library changes
	shellLibrary->Commit();

	if (shellLibrary != NULL)
			shellLibrary->Release();
} 
COMMAND(L"RemoveFolder", L"SLUtil RemoveFolder LibraryName FolderPath", L"Remove a folder from a library", L"SLUtil RemoveFolder Documents C:\\Docs", 2, RemoveFolder);

//Delete a library
void Delete(const CCommand &command, const vector<PCWSTR> &arguments)
{
	PWSTR libraryFullPath;
	HRESULT hr = GetLibraryFromLibrariesFolder(arguments[0], NULL, false, &libraryFullPath);
	
	if (FAILED(hr))
	{
		wcerr << L"Delete: Can't get library." << endl;
		exit(4);
	}
	//We use delete file with the library file-system based full path
	DeleteFile(libraryFullPath);
	CoTaskMemFree(libraryFullPath);
}
COMMAND(L"Delete", L"SLUtil Delete LibraryName", L"Delete a library", L"SLUtil Delete MyLib", 1, Delete);

//Rename an existing library
void Rename(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = OpenLibrary(L"Rename", arguments[0]);
	IShellItem *savedTo = NULL;

	//Save a new copy of the library under the user's Libraries folder with the new name.
	HRESULT hr = shellLibrary->SaveInKnownFolder(FOLDERID_UsersLibraries, arguments[1], LSF_MAKEUNIQUENAME, &savedTo);
	if (FAILED(hr))
	{
		wcerr << L"Rename: Can't save library." << endl;
		exit(5);
	}

	if (shellLibrary != NULL)
			shellLibrary->Release();
	if (savedTo != NULL)
		savedTo->Release();

	//Create parameters to delete the old copy of the library
	vector<PCWSTR> deleteArguments;
	deleteArguments.push_back(arguments[0]);

	//Call the delete command
	Delete(command, deleteArguments);
} 
COMMAND(L"Rename", L"SLUtil Rename OldName NewName", L"Rename a library", L"SLUtil Rename MyLib MyLibNewName", 2, Rename);


//Set or get the library's save folder path
void SaveFolder(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = NULL;
	IShellItem2 *shellItemSaveFolder = NULL;
	HRESULT hr = S_OK;
	//Show current default save folder
	if (arguments.size() == 1)
	{
		shellLibrary = OpenLibrary(L"SaveFolder", arguments[0]);
		hr = shellLibrary->GetDefaultSaveFolder(DSFT_DETECT, IID_PPV_ARGS(&shellItemSaveFolder));
		if (FAILED(hr))
		{
			wcerr << L"SaveFolder: Can't extract default save folder." << endl;
			exit(5);
		}
		
		IShellItem2 *shellItemResolvedSaveFolder = NULL;
		//Fix folder path changes
		hr = shellLibrary->ResolveFolder(shellItemSaveFolder, 5000, IID_PPV_ARGS(&shellItemResolvedSaveFolder));
		if (FAILED(hr))
		{
			wcerr << L"SaveFolder: Default save location Unavailable." << endl;
			exit(6);
		}

		PWSTR defaultSaveFolder;
		hr = shellItemResolvedSaveFolder->GetString(PKEY_ParsingPath, &defaultSaveFolder);
		wcout << L"Library " << arguments[0] << L" has folder " << defaultSaveFolder 
			<< L" as a default save location." << endl;
		
		shellItemResolvedSaveFolder->Release();
		CoTaskMemFree(defaultSaveFolder);
	}
	else //Set current default save folder
	{
		shellLibrary = OpenLibrary(L"SaveFolder", arguments[0], false);
		//Create shell item from folder path
		hr = SHCreateItemFromParsingName(arguments[1], 0, IID_PPV_ARGS(&shellItemSaveFolder));
		if (FAILED(hr))
		{
			wcerr << L"SaveFolder: Can't find folder: " << arguments[1] << endl;
			exit(6);
		}

		shellLibrary->SetDefaultSaveFolder(DSFT_DETECT, shellItemSaveFolder);
		if (FAILED(hr))
		{
			wcerr << L"SaveFolder: Can't set default save folder to" << arguments[1] << endl;
			exit(7);
		}

		//Commit the library changes
		shellLibrary->Commit();
	}

	if (shellItemSaveFolder != NULL)
			shellItemSaveFolder->Release();
	if (shellLibrary != NULL)
			shellLibrary->Release();
} 
COMMAND(L"SaveFolder", L"SLUtil SaveFolder LibraryName [FolderPath]", L"Set or get the library's save folder path", L"SLUtil SaveFolder Documents C:\\Docs", 1, SaveFolder);


//Set or get the library's pinned to navigation pane state
void NavPanePinnedState(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = NULL;
	HRESULT hr = S_OK;
	//Show current pinned to navigation pane state
	if (arguments.size() == 1)
	{
		//Open library with read permissions
		IShellLibrary *shellLibrary = OpenLibrary(L"NavPanePinnedState", arguments[0]);

		LIBRARYOPTIONFLAGS optionFlags;
		hr = shellLibrary->GetOptions(&optionFlags);
		if (FAILED(hr))
		{
			wcerr << L"NavPanePinnedState: Can't get current pinned to navigation pane state." << endl;
			exit(5);
		}
		
		wcout << L"Library " << arguments[0] << L" is" <<
			(((optionFlags & LOF_PINNEDTONAVPANE) != 0) ? L" " : L" not ")
			<< L"pinned to naveigation pane." << endl;
	}
	else //Set the current pinned to navigation pane state
	{
		//Open library with write permissions
		IShellLibrary *shellLibrary = OpenLibrary(L"NavPanePinnedState", arguments[0], false);

		LIBRARYOPTIONFLAGS optionFlags = CompareStringShort(arguments[1], L"TRUE") ? LOF_PINNEDTONAVPANE : LOF_DEFAULT;

		hr = shellLibrary->SetOptions(LOF_MASK_ALL, optionFlags);
		if (FAILED(hr))
		{
			wcerr << L"NavPanePinnedState: Can't set pinned to navigation pane state." << endl;
			exit(6);
		}
		//Commit the library changes
		shellLibrary->Commit();
	}
	if (shellLibrary != NULL)
			shellLibrary->Release();
} 
COMMAND(L"NavPanePinnedState", L"SLUtil NavPanePinnedState LibraryName [TRUE|FALSE]", L"Set or get the library's Pinned to navigation pane state", L"SLUtil NavPanePinnedState MyLib TRUE", 1, NavPanePinnedState);


//Set or get the library's icon
void Icon(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = NULL;
	HRESULT hr = S_OK;
	//Show current icon resource name
	if (arguments.size() == 1)
	{
		//Open library with read permissions
		IShellLibrary *shellLibrary = OpenLibrary(L"Icon", arguments[0]);

		PWSTR iconName;
		hr = shellLibrary->GetIcon(&iconName);
		if (FAILED(hr))
		{
			wcerr << L"Icon: Can't get icon resource name." << endl;
			exit(5);
		}
		wcout << L"Library " << arguments[0] << L": Icon resource name: "
			  << iconName << endl;

		CoTaskMemFree(iconName);
	}
	else //Set the current icon resource name
	{
		//Open library with write permissions
		IShellLibrary *shellLibrary = OpenLibrary(L"Icon", arguments[0], false);

		hr = shellLibrary->SetIcon(arguments[1]);
		if (FAILED(hr))
		{
			wcerr << L"Icon: Can't set icon resource name." << endl;
			exit(6);
		}
		//Commit the library changes
		shellLibrary->Commit();
	}
	if (shellLibrary != NULL)
			shellLibrary->Release();
} 
COMMAND(L"Icon", L"SLUtil Icon LibraryName [Icon]", L"Set or get the library's icon", L"SLUtil Icon MyLib imageres.dll,-1005", 1, Icon);


//Set or get the library's folder template
void FolderType(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = NULL;
	HRESULT hr = S_OK;
	FOLDERTYPEID folderTypeId;

	CFolderTypeIdNameConverter converter;

	//Show current folder type
	if (arguments.size() == 1)
	{
		//Open library with read permissions
		IShellLibrary *shellLibrary = OpenLibrary(L"FolderType", arguments[0]);

		hr = shellLibrary->GetFolderType(&folderTypeId);
		if (FAILED(hr))
		{
			wcerr << L"FolderType: Can't get the library's folder template." << endl;
			exit(5);
		}
		wcout << L"Library " << arguments[0] << L": Folder template is: "
			  << converter.GetFolderTypeIdName(folderTypeId) << endl;
	}
	else //Set the current folder type
	{
		//Open library with write permissions
		IShellLibrary *shellLibrary = OpenLibrary(L"FolderType", arguments[0], false);

		hr = converter.GetFolderTypeIdFromName(arguments[1], &folderTypeId);
		if (FAILED(hr))
		{
			wcerr << L"FolderType: Invalida folder template name." << endl;
			exit(6);
		}

		hr = shellLibrary->SetFolderType(folderTypeId);
		if (FAILED(hr))
		{
			wcerr << L"FolderType: Can't set the library's folder template," << endl;
			exit(7);
		}
		//Commit the library changes
		shellLibrary->Commit();
	}
	if (shellLibrary != NULL)
			shellLibrary->Release();
} 
COMMAND(L"FolderType", L"SLUtil FolderType LibraryName [Documents|Pictures|Music|Videos|Generic]", L"Set or get the library's folder template", L"SLUtil MyLib Documents", 1, FolderType);


//List all library folders
void ListFolders(const CCommand &command, const vector<PCWSTR> &arguments)
{
	IShellLibrary *shellLibrary = OpenLibrary(L"ListFolders", arguments[0]);
	HRESULT hr = S_OK;
	IShellItemArray *shellItemArray = NULL;

	shellLibrary->GetFolders(LFF_ALLITEMS, IID_PPV_ARGS(&shellItemArray));

	if (FAILED(hr))
	{
		wcerr << L"ListFolders: Can't get the library's folder list." << endl;
		exit(5);
	}

	wcout << L"Folder list of " << arguments[0] << L" library:" << endl;
	DWORD count;
	shellItemArray->GetCount(&count);

	//Iterate through all library folders
	for (DWORD i = 0; i < count; ++i)
	{
		IShellItem *shellItem;
		IShellItem2 *shellItem2;

		hr = shellItemArray->GetItemAt(i, &shellItem);
		if (FAILED(hr))
			continue;
		//Convert IShellItem to IShellItem2
		shellItem->QueryInterface(IID_PPV_ARGS(&shellItem2));
		shellItem->Release();

		IShellItem2 *shellItemResolvedFolder = NULL;
		//Fix folder path changes
		hr = shellLibrary->ResolveFolder(shellItem2, 5000, IID_PPV_ARGS(&shellItemResolvedFolder));
		if (SUCCEEDED(hr))
		{
			//Point to the fixed folder
			shellItem2->Release();
			shellItem2 = shellItemResolvedFolder;
		}
		//else we will show the unfixed folder
			
		PWSTR folderPath;
		hr = shellItem2->GetString(PKEY_ParsingPath, &folderPath);
		if (SUCCEEDED(hr))
		{
			wcout << folderPath << endl;
		}
		CoTaskMemFree(folderPath);
		shellItem2->Release();
	}
	shellItemArray->Release();
	shellLibrary->Release();
}
COMMAND(L"ListFolders", L"SLUtil ListFolders LibraryName", L"List all library folders.", L"SLUtil ListFolders Documents", 1, ListFolders);

//Show the Shell Library management UI
void ManageUI(const CCommand &command, const vector<PCWSTR> &arguments)
{
	PWSTR libraryFullPath = NULL;
	HRESULT hr = GetLibraryFromLibrariesFolder(arguments[0], NULL, false, &libraryFullPath);
	
	if (FAILED(hr))
	{
		wcerr << L"ManageUI: Can't get library." << endl;
		exit(4);
	}

	IShellItem *shellItem = NULL;
	hr = SHCreateItemFromParsingName(libraryFullPath, 0, IID_PPV_ARGS(&shellItem));
	if (FAILED(hr))
	{
		wcerr << L"ManageUI: Can't create COM object." << endl;
		exit(5);
	}

	PCWSTR title = arguments[0];
	PCWSTR instruction = L"Manage Library folders and settings";

	if (arguments.size() > 1)
		title = arguments[1];

	if (arguments.size() > 2)
		instruction = arguments[2];

	SHShowManageLibraryUI(shellItem, NULL, title, instruction, LMD_ALLOWUNINDEXABLENETWORKLOCATIONS);

	shellItem->Release();
	CoTaskMemFree(libraryFullPath);
}
COMMAND(L"ManageUI", L"SLUtil ManageUI LibraryName [Title] [Instruction]", L"Show the Shell Library management UI", L"SLUtil ManageUI Documents", 1, ManageUI);