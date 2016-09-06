/*
ZBuffer.cpp
Written by Matthew Fisher

A simple ZBuffer structure, for software rendering.  Stores the depth of each screen pixel.
See ZBuffer.h for a full description.
*/
#include "..\\..\\Main.h"
#include "ZBuffer.h"

ZBuffer::ZBuffer()
{
    Data = 0;
    Width = 0;
    Height = 0;
}

ZBuffer::ZBuffer(UINT _Width, UINT _Height)
{
    Data = 0;
    Allocate(_Width, _Height);
}

ZBuffer::~ZBuffer()
{
    FreeMemory();
}

void ZBuffer::FreeMemory()
{
    if(Data)
    {
        delete[] Data;
    }
    Data = 0;
    Width = 0;
    Height = 0;
}

void ZBuffer::Allocate(UINT _Width, UINT _Height)
{
    FreeMemory();
    Width = _Width;
    Height = _Height;
    Data = new float[Width * Height];
}

UINT ZBuffer::GetWidth()
{
    return Width;
}

UINT ZBuffer::GetHeight()
{
    return Height;
}

void ZBuffer::Clear(float Value)
{
    for(UINT i = 0; i < Width; i++)
    {
        Data[i] = Value;
    }

    for(UINT i2 = 1; i2 < Height; i2++)
    {
        memcpy(&Data[i2 * Width], &Data[0], sizeof(float) * Width);
    }
}