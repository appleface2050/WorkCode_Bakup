/*
FileCollection.cpp
Written by Matthew Fisher

A FileCollection stores a large number of files as a single file.  It is similar to a tar file in functionality.
*/
#include "..\\..\\Main.h"

void FileCollectionFile::GetFileLines(Vector<String> &Lines)
{
    FILE *TempFile = Utility::CheckedFOpen("Temp.txt", "wb");
    Utility::CheckedFWrite(Data.CArray(), sizeof(BYTE), Data.Length(), TempFile);
    fclose(TempFile);

    Utility::GetFileLines("Temp.txt", Lines);

    ofstream File("Temp.txt");
    File.close();
}

FileCollection::FileCollection()
{
    
}

FileCollection::~FileCollection()
{
    FreeMemory();
}

void FileCollection::FreeMemory()
{
    _FileListMutex.Acquire();
    for(UINT FileIndex = 0; FileIndex < _FileList.Length(); FileIndex++)
    {
        delete _FileList[FileIndex];
    }
    _FileList.FreeMemory();
    _FileMap.clear();
    _FileListMutex.Release();
}

void FileCollection::MuddleData(Vector<BYTE> &Data)
{
    BYTE *DataArray = Data.CArray();
    UINT DataLength = Data.Length();
    for(UINT Index = 0; Index < DataLength; Index++)
    {
        DataArray[Index] = 255 - DataArray[Index];
    }
}

void FileCollection::LoadAll(const String &Filename, bool Muddle)
{
    _FileListMutex.Acquire();
    FreeMemory();
    
    if(Utility::FileExists(Filename))
    {
        FILE *File = Utility::CheckedFOpen(Filename.CString(), "rb");
        
        FileCollectionHeader CollectionHeader;
        Utility::CheckedFRead(&CollectionHeader, sizeof(FileCollectionHeader), 1, File);

        _FileList.Allocate(CollectionHeader.FileCount);
        for(UINT FileIndex = 0; FileIndex < _FileList.Length(); FileIndex++)
        {
            FileCollectionFileHeader FileHeader;

            Utility::CheckedFRead(&FileHeader, sizeof(FileCollectionFileHeader), 1, File);
            
            FileCollectionFile *NewFile = new FileCollectionFile;
            NewFile->Filename = String(FileHeader.Filename);
            NewFile->Data.Allocate(FileHeader.FileSize);
            
            Utility::CheckedFRead(NewFile->Data.CArray(), sizeof(BYTE), FileHeader.FileSize, File);
            if(Muddle)
            {
                MuddleData(NewFile->Data);
            }

            _FileList[FileIndex] = NewFile;
            _FileMap[FileHeader.Filename] = NewFile;
        }

        fclose(File);
    }

    _FileListMutex.Release();
}

void FileCollection::SaveAll(const String &Filename, bool Muddle)
{
    _FileListMutex.Acquire();
    
    FILE *File = Utility::CheckedFOpen(Filename.CString(), "wb");
    
    FileCollectionHeader CollectionHeader;
    CollectionHeader.FileCount = _FileList.Length();
    Utility::CheckedFWrite(&CollectionHeader, sizeof(FileCollectionHeader), 1, File);

    for(UINT FileIndex = 0; FileIndex < _FileList.Length(); FileIndex++)
    {
        FileCollectionFile &CurFile = *(_FileList[FileIndex]);
        FileCollectionFileHeader FileHeader;
        strcpy(FileHeader.Filename, CurFile.Filename.CString());
        FileHeader.FileSize = CurFile.Data.Length();

        if(Muddle)
        {
            MuddleData(CurFile.Data);
        }
        Utility::CheckedFWrite(&FileHeader, sizeof(FileCollectionFileHeader), 1, File);
        Utility::CheckedFWrite(CurFile.Data.CArray(), sizeof(BYTE), FileHeader.FileSize, File);
        if(Muddle)
        {
            MuddleData(CurFile.Data);
        }
    }

    fclose(File);
    _FileListMutex.Release();
}

void FileCollection::DumpCollectionToDisk()
{
    _FileListMutex.Acquire();
    for(UINT FileIndex = 0; FileIndex < _FileList.Length(); FileIndex++)
    {
        const FileCollectionFile &CurFile = *(_FileList[FileIndex]);
        FILE *File = Utility::CheckedFOpen(CurFile.Filename.CString(), "wb");
        Utility::CheckedFWrite(CurFile.Data.CArray(), sizeof(BYTE), CurFile.Data.Length(), File);
        fclose(File);
    }
    _FileListMutex.Release();
}

void FileCollection::RemoveFile(const String &FileCollectionName)
{
    _FileListMutex.Acquire();
    for(UINT FileIndex = 0; FileIndex < _FileList.Length(); FileIndex++)
    {
        FileCollectionFile *CurFile = _FileList[FileIndex];
        if(CurFile->Filename == FileCollectionName)
        {
            delete CurFile;
            _FileList.RemoveSwap(FileIndex);
            PersistentAssert(_FileMap.find(FileCollectionName.CString()) != _FileMap.end(), "File not found in map");
            _FileMap.erase(_FileMap.find(FileCollectionName.CString()));
            _FileListMutex.Release();
            return;
        }
    }
    _FileListMutex.Release();
}

void FileCollection::AddFileFromDisk(const String &FileCollectionName, const String &ExistingFilename)
{
    _FileListMutex.Acquire();
    Vector<BYTE> Data;
    RemoveFile(FileCollectionName);
    if(!Utility::FileExists(ExistingFilename))
    {
        SignalError(String("Required file not found: ") + FileCollectionName);
    }
    Utility::GetFileData(ExistingFilename, Data);
    AddFileFromMemory(FileCollectionName, Data);
    _FileListMutex.Release();
}

void FileCollection::AddFileFromMemory(const String &FileCollectionName, const Vector<BYTE> &Data)
{
    _FileListMutex.Acquire();
    RemoveFile(FileCollectionName);
    FileCollectionFile *NewFile = new FileCollectionFile;
    NewFile->Filename = FileCollectionName;
    NewFile->Data = Data;
    _FileList.PushEnd(NewFile);
    _FileMap[NewFile->Filename.CString()] = NewFile;
    _FileListMutex.Release();
}

FileCollectionFile* FileCollection::FindFile(const String &FileCollectionName)
{
    _FileListMutex.Acquire();
    /*for(UINT i = 0; i < _FileList.Length(); i++)
    {
        FileCollectionFile *File = _FileList[i];
        if(File->Filename == FileCollectionName)
        {
            _FileListMutex.Release();
            return File;
        }
    }*/
    FileCollectionFile *Result = NULL;

    map<String, FileCollectionFile *, String::LexicographicComparison>::const_iterator FileSearchResult = _FileMap.find(FileCollectionName.CString());
    if(FileSearchResult != _FileMap.end())
    {
        Result = FileSearchResult->second;
    }
    _FileListMutex.Release();
    return Result;
}

FileCollectionFile* FileCollection::AddAndUpdateFile(const String &FileCollectionName, const String &ExistingFilename)
{
    if(Utility::FileExists(ExistingFilename))
    {
        AddFileFromDisk(FileCollectionName, ExistingFilename);
    }
    return FindFile(FileCollectionName);
}

void FileCollection::GetFileLines(const String &FileCollectionName, Vector<String> &Lines)
{
    _FileListMutex.Acquire();
    
    FileCollectionFile* CurFile = FindFile(FileCollectionName);
    PersistentAssert(CurFile != NULL, String("Failed to find ") + String(FileCollectionName) + " in database.");
    CurFile->GetFileLines(Lines);

    _FileListMutex.Release();
}
