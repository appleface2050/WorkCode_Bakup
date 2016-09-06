/*
BaseTexture.h
Written by Matthew Fisher

BaseTexture is an abstract class that defines base-level texture functionality.
*/

//
// Supported texture types
//
#pragma  once
enum TextureType
{
    TextureD3D9,
    TextureD3D10,
    TextureOpenGL,
    TextureNull,
    TextureCount
};

class BaseTexture
{
public:
    BaseTexture();

    virtual void FreeMemory() = 0;
    
    virtual void Load(const Bitmap &Bmp) = 0;   //loads the image pointed to by filename into this texture
    virtual void Set(int Index = 0) = 0;        //loads this texture into texture slot Index
    virtual void SetNull(int Index = 0) = 0;    //loads an empty texture into texture slow Index

    __forceinline TextureType Type()
    {
        return _Type;
    }

protected:
    TextureType _Type;
};