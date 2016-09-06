/*
D3D9RenderTargetSurface.h
Written by Matthew Fisher

D3D9RenderTargetSurface is a surface that can be used as a render target.
*/
#pragma once

#ifdef USE_D3D9
class D3D9RenderTargetSurface : public D3D9Object
{
public:
    D3D9RenderTargetSurface();
    ~D3D9RenderTargetSurface();
    void FreeMemory();
    void ReleaseMemory();
    void Reset(LPDIRECT3DDEVICE9 Device);

    void Init(D3D9GraphicsDevice &GD, UINT Width, UINT Height);
    bool ReSizeToBackBuffer(D3D9GraphicsDevice &GD);
    void SetAsRenderTarget(D3D9GraphicsDevice &GD);
    void CopySurfaceToOffScreen(D3D9GraphicsDevice &GD);
    void CopyOffScreenToBackBuffer(D3D9GraphicsDevice &GD);

    __forceinline UINT Width()
    {
        return _Width;
    }
    __forceinline UINT Height()
    {
        return _Height;
    }

private:
    LPDIRECT3DSURFACE9 _Surface;
    LPDIRECT3DSURFACE9 _OffScreenPlainSurface;
    UINT _Width, _Height;
};

#endif