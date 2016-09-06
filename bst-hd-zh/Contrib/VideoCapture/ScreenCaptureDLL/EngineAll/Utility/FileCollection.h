/*
FileCollection.h
Written by Matthew Fisher

A FileCollection stores a large number of files as a single file.  It is similar to a tar file in functionality.
*/
#pragma  once

const UINT MaxFilenameLength = 512;

struct FileCollectionHeader
{
    UINT FileCount;
};

struct FileCollectionFileHeader
{
    char Filename[MaxFilenameLength];
    UINT FileSize;
};

struct FileCollectionFile
{
    void GetFileLines(Vector<String> &Lines);
    String Filename;
    Vector<BYTE> Data;
};

class FileCollection
{
public:
    FileCollection();
    ~FileCollection();
    void FreeMemory();

    //
    // Save/Load collection to disk
    //
    void LoadAll(const String &Filename, bool Muddle = true);
    void SaveAll(const String &Filename, bool Muddle = true);
    void DumpCollectionToDisk();

    //
    // Modify collection
    //
    void AddFileFromMemory(const String &FileCollectionName, const Vector<BYTE> &Data);
    void AddFileFromDisk(const String &FileCollectionName, const String &ExistingFilename);
    void RemoveFile(const String &FileCollectionName);
    FileCollectionFile* AddAndUpdateFile(const String &FileCollectionName, const String &ExistingFilename);
    FileCollectionFile* FindFile(const String &FileCollectionName);
    void GetFileLines(const String &Filename, Vector<String> &Lines);

    __forceinline UINT FileCount()
    {
        return _FileList.Length();
    }
    __forceinline const FileCollectionFile& GetFile(UINT Index)
    {
        return *(_FileList[Index]);
    }

private:
    void MuddleData(Vector<BYTE> &Data);

    Mutex _FileListMutex;
    Vector<FileCollectionFile *> _FileList;
    map<String, FileCollectionFile *, String::LexicographicComparison> _FileMap;
};
