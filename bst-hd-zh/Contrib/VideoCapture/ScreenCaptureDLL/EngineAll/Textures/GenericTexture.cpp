/*
GenericTexture.cpp
Written by Matthew Fisher

Generic texture class that maps to either a DirectX or OpenGL texture based upon what the current window is using.
*/
#include "..\\..\\Main.h"
GenericTexture::GenericTexture()
{
    _BaseTexture = NULL;
}

GenericTexture::~GenericTexture()
{
    FreeMemory();
}

void GenericTexture::FreeMemory()
{
    if(_BaseTexture)
    {
        _BaseTexture->FreeMemory();
        delete _BaseTexture;
        _BaseTexture = NULL;
    }
}

void GenericTexture::Init(GraphicsDevice &GD, const Bitmap &Bmp)
{
    FreeMemory();
    _BaseTexture = GD.CreateTexture();
    if(_BaseTexture)
    {
        _BaseTexture->Load(Bmp);
    }
}

void GenericTexture::Set(int Index)
{
    if(_BaseTexture)
    {
        _BaseTexture->Set(Index);
    }
}

void GenericTexture::SetNull(int Index)
{
    if(_BaseTexture)
    {
        _BaseTexture->SetNull(Index);
    }
}
