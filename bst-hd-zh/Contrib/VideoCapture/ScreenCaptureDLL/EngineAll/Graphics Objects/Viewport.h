/*
Viewport.h
Written by Matthew Fisher

Viewport represents a rectangle on the screen used for rendering
*/
#pragma once

struct Viewport
{
    Viewport();
    Viewport(UINT _X, UINT _Y, UINT _Width, UINT _Height, float _MinZ = 0.0f, float _MaxZ = 1.0f);
    Viewport(UINT _Width, UINT _Height);
    
    UINT X, Y, Width, Height;
    float MinZ, MaxZ;
};
