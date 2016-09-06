/*
D3DTexture.h
Written by Matthew Fisher

DirectX implementation of BaseTexture.  See BaseTexture.h for a definiton of the base functions.
*/
#pragma  once

#ifdef USE_D3D9
class D3D9Texture : public BaseTexture
{
public:
    D3D9Texture();
    D3D9Texture(LPDIRECT3DDEVICE9 Device);
    ~D3D9Texture();

    //
    // Memory
    //
    void FreeMemory();
    void ReleaseMemory();
    void Reset(LPDIRECT3DDEVICE9 Device);
    void Associate(GraphicsDevice &GD);    //associates this texture with the provided graphics device.
                                           //must occur before loading.
    void Allocate(D3DFORMAT Format, UINT Width, UINT Height, bool RenderTarget);
    
    //
    // Load texture data from memory
    //
    void Load(const Bitmap &Bmp);
    void Load(const Grid<float> &Data);
    void Load(const Grid<Vec2f> &Data);
    void Load(const Grid<Vec3f> &Data);
    void Load(const Grid<Vec4f> &Data);
    void Load(D3D9Texture &Tex);
    void UpdateMipMapLevels();
    void Clear();

    //
    // Dump texture data to memory
    //
    void ReadData(Bitmap &Bmp);
    void ReadData(Grid<float> &Data);
    void ReadData(Grid<Vec2f> &Data);
    void ReadData(Grid<Vec3f> &Data);
    void ReadData(Grid<Vec4f> &Data);

    void Flush();

    void Set(int Index);
    void SetAsRenderTarget(int Index);
    void SetNull(int Index);

    //
    // Accessors
    //
    __forceinline LPDIRECT3DTEXTURE9 GetTexture()
    {
        return _Texture;
    }
    __forceinline LPDIRECT3DSURFACE9 SurfaceTopLevel()
    {
        return _SurfaceTopLevel;
    }
    __forceinline UINT Width()
    {
        return _Width;
    }
    __forceinline UINT Height()
    {
        return _Height;
    }

private:
    //
    // These are not permitted
    //
    D3D9Texture(const D3D9Texture &Tex);
    D3D9Texture& operator = (const D3D9Texture &Tex);

    bool _RenderTarget;
    UINT _Width, _Height;
    D3DFORMAT _Format;
    LPDIRECT3DTEXTURE9 _Texture;
    LPDIRECT3DSURFACE9 _SurfacePlain;
    LPDIRECT3DSURFACE9 _SurfaceTopLevel;
    LPDIRECT3DDEVICE9  _Device;
};
#endif
