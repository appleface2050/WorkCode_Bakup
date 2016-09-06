/*
D3D9Object.h
Written by Matthew Fisher

D3D9Object is an abstract class used for all resetable D3D9 graphics objects
*/
#pragma once

#ifdef USE_D3D9

class D3D9Object
{
public:
    virtual void ReleaseMemory() = 0;
    virtual void FreeMemory() = 0;
    virtual void Reset(LPDIRECT3DDEVICE9 Device) = 0;
};

#endif