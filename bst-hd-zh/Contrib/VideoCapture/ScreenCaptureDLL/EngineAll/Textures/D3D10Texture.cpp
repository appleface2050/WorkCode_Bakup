/*
D3DTexture.cpp
Written by Matthew Fisher

DirectX implementation of BaseTexture.  See BaseTexture.h for a definiton of the base functions.
*/
#include "..\\..\\Main.h"

#ifdef USE_D3D10

D3D10Texture::D3D10Texture()
{
    _View = NULL;
    _Texture = NULL;
    _GD = NULL;
}

D3D10Texture::D3D10Texture(GraphicsDevice &GD)
{
    _View = NULL;
    _Texture = NULL;
    _GD = &GD;
}

D3D10Texture::~D3D10Texture()
{
    FreeMemory();
}

void D3D10Texture::FreeMemory()
{
    if(_Texture != NULL)
    {
        _Texture->Release();
        _Texture = NULL;
    }
    if(_View != NULL)
    {
        _View->Release();
        _View = NULL;
    }
}

void D3D10Texture::Associate(GraphicsDevice &GD)
{
    _GD = &GD;
}

void D3D10Texture::Allocate(UINT Width, UINT Height, bool ConstructMipmaps, DXGI_FORMAT Format, const Bitmap &Bmp)
{
    FreeMemory();
    PersistentAssert(_GD != NULL, "D3D10Texture::Allocate on unassociated texture");
    ID3D10Device* Device = _GD->CastD3D10().GetDevice();

    D3D10_TEXTURE2D_DESC TextureDesc;
    TextureDesc.Width = Width;
    TextureDesc.Height = Height;
    TextureDesc.MipLevels = 0;
    if(!ConstructMipmaps)
    {
        TextureDesc.MipLevels = 1;
    }
    TextureDesc.ArraySize = 1;
    TextureDesc.Format = Format;
    TextureDesc.SampleDesc.Count = 1;
    TextureDesc.SampleDesc.Quality = 0;
    TextureDesc.Usage = D3D10_USAGE_DEFAULT;
    TextureDesc.BindFlags = D3D10_BIND_SHADER_RESOURCE | D3D10_BIND_RENDER_TARGET;
    TextureDesc.CPUAccessFlags = 0;
    TextureDesc.MiscFlags = D3D10_RESOURCE_MISC_GENERATE_MIPS;

    D3D10_SUBRESOURCE_DATA Data[12];
    for(UINT Index = 0; Index < 12; Index++)
    {
        Data[Index].pSysMem = &(Bmp[0][0]);
        Data[Index].SysMemPitch = Bmp.Width() * sizeof(RGBColor);
    }

    HRESULT hr = Device->CreateTexture2D(&TextureDesc, Data, &_Texture);
    PersistentAssert(SUCCEEDED(hr), "CreateTexture2D failed");

    _Texture->GetDesc(&TextureDesc);

    D3D10_SHADER_RESOURCE_VIEW_DESC ViewDesc;
    ViewDesc.Format = Format;
    ViewDesc.ViewDimension = D3D10_SRV_DIMENSION_TEXTURE2D;
    ViewDesc.Texture2D.MostDetailedMip = 0;
    ViewDesc.Texture2D.MipLevels = TextureDesc.MipLevels;

    hr = Device->CreateShaderResourceView(_Texture, &ViewDesc, &_View);
    PersistentAssert(SUCCEEDED(hr), "CreateShaderResourceView failed");

    Device->GenerateMips(_View);
}

void D3D10Texture::Load(const Bitmap &Bmp)
{
    PersistentAssert(_GD != NULL, "D3D10Texture::Load on unassociated texture");

    Allocate(Bmp.Width(), Bmp.Height(), true, DXGI_FORMAT_R8G8B8A8_UNORM, Bmp);
}

void D3D10Texture::Set(int Index)
{
    SignalError("D3D10Texture must be set via effects");
}

void D3D10Texture::SetNull(int Index)
{
    SignalError("D3D10Texture must be set via effects");
}

#endif