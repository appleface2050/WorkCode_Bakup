/*
D3D9RenderTargetSurface.cpp
Written by Matthew Fisher

D3D9RenderTargetSurface is a surface that can be used as a render target.
*/
#include "..\\..\\Main.h"
#include "D3D9RenderTargetSurface.h"

#ifdef USE_D3D9

D3D9RenderTargetSurface::D3D9RenderTargetSurface()
{
    _Surface = NULL;
  _OffScreenPlainSurface = NULL;
  _Width = 0;
  _Height = 0;
}

D3D9RenderTargetSurface::~D3D9RenderTargetSurface()
{
    FreeMemory();
}

void D3D9RenderTargetSurface::ReleaseMemory()
{
    FreeMemory();
}

void D3D9RenderTargetSurface::FreeMemory()
{
    _Width = 0;
    _Height = 0;
    if(_Surface)
    {
        _Surface->Release();
        _Surface = NULL;
    }
    if(_OffScreenPlainSurface)
    {
        _OffScreenPlainSurface->Release();
        _OffScreenPlainSurface = NULL;
    }
}

void D3D9RenderTargetSurface::Reset(LPDIRECT3DDEVICE9 Device)
{
    FreeMemory();
}

void D3D9RenderTargetSurface::Init(D3D9GraphicsDevice &GD, UINT Width, UINT Height)
{
    FreeMemory();

    D3DAlwaysValidate(GD.GetDevice()->CreateRenderTarget(Width, Height, D3DFMT_A8R8G8B8, D3DMULTISAMPLE_NONE, 0, FALSE, &_Surface, NULL), "CreateRenderTarget");
    D3DAlwaysValidate(GD.GetDevice()->CreateOffscreenPlainSurface(Width, Height, D3DFMT_A8R8G8B8, D3DPOOL_SYSTEMMEM, &_OffScreenPlainSurface, NULL), "CreateOffscreenPlainSurface");

    _Width = Width;
    _Height = Height;
}

bool D3D9RenderTargetSurface::ReSizeToBackBuffer(D3D9GraphicsDevice &GD)
{
    IDirect3DSurface9 *BackBuffer;
    HRESULT hr = GD.GetDevice()->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &BackBuffer);
    if(FAILED(hr))
    {
        FreeMemory();
        return false;
    }
    D3DSURFACE_DESC Desc;
    D3DValidate(BackBuffer->GetDesc(&Desc), "GetDesc");
    BackBuffer->Release();

    if(_Width != Desc.Width || _Height != Desc.Height)
    {
        Init(GD, Desc.Width, Desc.Height);
        return true;
    }
    else
    {
        return false;
    }
}

void D3D9RenderTargetSurface::SetAsRenderTarget(D3D9GraphicsDevice &GD)
{
    HRESULT hr = GD.GetDevice()->SetRenderTarget(0, _Surface);
}

void D3D9RenderTargetSurface::CopySurfaceToOffScreen(D3D9GraphicsDevice &GD)
{
    HRESULT hr = GD.GetDevice()->GetRenderTargetData(_Surface, _OffScreenPlainSurface);
}

void D3D9RenderTargetSurface::CopyOffScreenToBackBuffer(D3D9GraphicsDevice &GD)
{
  IDirect3DSurface9 *BackBuffer;
  HRESULT hr = GD.GetDevice()->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &BackBuffer);
  if(FAILED(hr))
  {
      return;
  }
  hr = GD.GetDevice()->UpdateSurface(_OffScreenPlainSurface, NULL, BackBuffer, NULL);
  BackBuffer->Release();
}

#endif