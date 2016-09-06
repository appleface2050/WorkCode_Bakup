/*
OpenGLTexture.h
Written by Matthew Fisher

OpenGL implementation of BaseTexture.  See BaseTexture.h for a definiton of the base functions.
*/
#pragma  once

#ifdef USE_OPENGL

class OpenGLTexture : public BaseTexture
{
public:
    OpenGLTexture();
    ~OpenGLTexture();
    void FreeMemory();
    void ReleaseMemory();

    void Load(const Bitmap &Bmp);
    void Set(int Index);
    void SetNull(int Index);

private:
    unsigned int surface;
};

#endif
