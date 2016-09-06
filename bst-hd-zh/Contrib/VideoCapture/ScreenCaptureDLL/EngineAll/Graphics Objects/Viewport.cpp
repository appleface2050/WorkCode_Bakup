/*
Viewport.cpp
Written by Matthew Fisher

Viewport represents a rectangle on the screen used for rendering
*/
#include "..\\..\\Main.h"
#include "Viewport.h"

Viewport::Viewport()
{
    X = 0;
    Y = 0;
    Width = 512;
    Height = 512;
    MinZ = 0.0f;
    MaxZ = 1.0f;
}

Viewport::Viewport(UINT _Width, UINT _Height)
{
    X = 0;
    Y = 0;
    Width = _Width;
    Height = _Height;
    MinZ = 0.0f;
    MaxZ = 1.0f;
}

Viewport::Viewport(UINT _X, UINT _Y, UINT _Width, UINT _Height, float _MinZ, float _MaxZ)
{
    X = _X;
    Y = _Y;
    Width = _Width;
    Height = _Height;
    MinZ = _MinZ;
    MaxZ = _MaxZ;
}
