/*
InputDataStream.h
Written by Matthew Fisher

InputDataStream class.  An istream but for binary data.
*/

#pragma once

class InputDataStream
{
public:
    InputDataStream();
    ~InputDataStream();
    void FreeMemory();

    //
    // Reads stream from file
    //
    void LoadFromFile(const String &Filename);
    void LoadFromMemory(const Vector<BYTE> &Data);

    //
    // Reads data represented by T as binary data from the stream
    //
    template<class type> void ReadData(type &T)
    {
        const UINT ByteCount = sizeof(T);
        BYTE *TOffset = (BYTE *)&T;
        const BYTE *StreamOffset = _Data.CArray() + _ReadPtr;
        for(UINT ByteIndex = 0; ByteIndex < ByteCount; ByteIndex++)
        {
            *TOffset = *StreamOffset;
            StreamOffset++;
            TOffset++;
            _ReadPtr++;
        }
    }

    //
    // Reads raw binary data from the stream
    //
    void ReadData(BYTE *Data, UINT BytesToRead);

protected:
    UINT _ReadPtr;
    Vector<BYTE> _Data;
};

//
// Input for several basic types
//

__forceinline InputDataStream& operator >> (InputDataStream &S, UINT &A)
{
    S.ReadData(A);
    return S;
}

__forceinline InputDataStream& operator >> (InputDataStream &S, int &A)
{
    S.ReadData(A);
    return S;
}

__forceinline InputDataStream& operator >> (InputDataStream &S, float &A)
{
    S.ReadData(A);
    return S;
}

__forceinline InputDataStream& operator >> (InputDataStream &S, double &A)
{
    S.ReadData(A);
    return S;
}

template<class type> InputDataStream& operator >> (InputDataStream &S, Vector<type> &V)
{
    UINT Length;
    S >> Length;
    V.ReSize(Length);
    for(UINT Index = 0; Index < Length; Index++)
    {
        S >> V[Index];
    }
    return S;
}

InputDataStream& operator >> (InputDataStream &S, String &V);