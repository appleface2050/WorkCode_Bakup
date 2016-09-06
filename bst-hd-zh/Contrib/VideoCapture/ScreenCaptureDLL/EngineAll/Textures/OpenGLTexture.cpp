/*
OpenGLTexture.cpp
Written by Matthew Fisher

OpenGL implementation of BaseTexture.  See BaseTexture.h for a definiton of the base functions.
*/
#include "..\\..\\Main.h"
#ifdef USE_OPENGL

OpenGLTexture::OpenGLTexture()
{
    surface = 0;
}

OpenGLTexture::~OpenGLTexture()
{
    FreeMemory();
}

void OpenGLTexture::FreeMemory()
{
    
}

void OpenGLTexture::ReleaseMemory()
{

}

void OpenGLTexture::Load(const Bitmap &Bmp)
{
    FreeMemory();
    glGenTextures(1, &surface);
    glBindTexture(GL_TEXTURE_2D, surface);
    glTexImage2D(GL_TEXTURE_2D, 0, 3, Bmp.Width(), Bmp.Height(), 0, GL_RGBA, GL_UNSIGNED_BYTE, &Bmp[0][0]);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);
    glEnable(GL_TEXTURE_2D);
}

void OpenGLTexture::Set(int Index)
{
    glBindTexture(GL_TEXTURE_2D, surface);
}

void OpenGLTexture::SetNull(int Index)
{
    glBindTexture(GL_TEXTURE_2D, 0);
}

#endif