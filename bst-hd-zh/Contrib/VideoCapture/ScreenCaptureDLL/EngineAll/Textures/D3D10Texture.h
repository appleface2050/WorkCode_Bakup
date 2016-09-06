/*
D3DTexture.h
Written by Matthew Fisher

D3D implementation of BaseTexture.  See BaseTexture.h for a definiton of the base functions.
*/
#pragma  once

#ifdef USE_D3D10
class D3D10Texture : public BaseTexture
{
public:
    D3D10Texture();
    D3D10Texture(GraphicsDevice &GD);
    ~D3D10Texture();
    void FreeMemory();

    void Associate(GraphicsDevice &GD);
    void Allocate(UINT Width, UINT Height, bool ConstructMipmaps, DXGI_FORMAT Format, const Bitmap &Bmp);
    void Load(const Bitmap &Bmp);
    
    void Set(int Index);
    void SetNull(int Index);

    __forceinline ID3D10ShaderResourceView* View()
    {
        return _View;
    }
    
private:
    //
    // These are not permitted
    //
    D3D10Texture(const D3D10Texture &Tex);
    D3D10Texture& operator = (const D3D10Texture &Tex);

    ID3D10Texture2D            *_Texture;
    ID3D10ShaderResourceView   *_View;
    GraphicsDevice             *_GD;
};
#endif