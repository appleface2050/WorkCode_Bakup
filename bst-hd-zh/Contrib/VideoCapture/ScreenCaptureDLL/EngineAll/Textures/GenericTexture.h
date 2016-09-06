/*
GenericTexture.h
Written by Matthew Fisher

Generic texture class that maps to either a DirectX or OpenGL texture based upon what the current window is using.
*/
#pragma  once

class GenericTexture
{
public:
    GenericTexture();
    ~GenericTexture();
    void FreeMemory();
    
    __forceinline BaseTexture* Texture()
    {
        return _BaseTexture;
    }

    void Init(GraphicsDevice &GD, const Bitmap &Bmp);
    void Set(int Index = 0);
    void SetNull(int Index = 0);

private:
    BaseTexture *_BaseTexture;
};
