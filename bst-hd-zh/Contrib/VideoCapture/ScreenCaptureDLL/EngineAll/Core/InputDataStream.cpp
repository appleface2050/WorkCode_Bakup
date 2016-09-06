/*
InputDataStream.cpp
Written by Matthew Fisher
*/
#include "..\\..\\Main.h"
#include "InputDataStream.h"

InputDataStream::InputDataStream()
{
    
}

InputDataStream::~InputDataStream()
{
    
}

void InputDataStream::LoadFromFile(const String &Filename)
{
    FILE *File = Utility::CheckedFOpen(Filename.CString(), "rb");
    UINT Length;
    Utility::CheckedFRead(&Length, sizeof(UINT), 1, File);
    _Data.Allocate(Length);
    Utility::CheckedFRead(_Data.CArray(), sizeof(BYTE), Length, File);
    fclose(File);
    _ReadPtr = 0;
}

void InputDataStream::LoadFromMemory(const Vector<BYTE> &Data)
{
    _Data = Data;
    _ReadPtr = 0;
}

void InputDataStream::ReadData(BYTE *Data, UINT BytesToRead)
{
    Assert(_Data.Length() >= _ReadPtr + BytesToRead, "Read past end of stream");
    if(BytesToRead > 0)
    {
        memcpy(Data, _Data.CArray() + _ReadPtr, BytesToRead);
        _ReadPtr += BytesToRead;
    }
}

InputDataStream& operator >> (InputDataStream &S, String &V)
{
    UINT Length;
    S >> Length;
    V.ReSize(Length);
    S.ReadData((BYTE *)V.CString(), Length);
    return S;
}
