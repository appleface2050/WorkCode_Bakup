/*
ZBuffer.h
Written by Matthew Fisher

A simple ZBuffer structure, for software rendering.  Stores the depth of each screen pixel.
*/
#pragma once
class ZBuffer {
public:
    ZBuffer();
    ZBuffer(UINT _Width, UINT _Height);

    ~ZBuffer();

    //
    // Memory
    //
    void FreeMemory();
    void Allocate(UINT _Width, UINT _Height);

    //
    // Accessors
    //
    UINT GetWidth();
    UINT GetHeight();
    float* operator [] (int Row) {return &Data[Row * Width];}

    //
    // Modifiers
    //
    void Clear(float Value);

private:
    UINT Width, Height;
    float* Data;
};