/*
GraphicsDevice.cpp
Written by Matthew Fisher

GraphicsDevice is the abstract class that contains functionality that all graphics devices must expose.
any GraphicsDevice can do simple things like render meshes into the main rendering window and handle
the world, view and projection transforms.  All these basic functions are rather trivial.
*/
#include "..\\..\\Main.h"
#include "GraphicsDevice.h"

void GraphicsDevice::PrepareFullScreenReSize(WindowObjects &O, AppInterface &App)
{

}

void GraphicsDevice::DrawString(const String &Text, int x, int y)
{
    DrawStringFloat(Text, float(x), float(y), RGBColor::Red);
}

void GraphicsDevice::DrawInteger(const String &Text, int N, int x, int y)
{
    DrawStringFloat(Text + String(N), float(x), float(y), RGBColor::Red);
}

void GraphicsDevice::DrawDouble(const String &Text, double N, int x, int y)
{
    DrawStringFloat(Text + String(N), float(x), float(y), RGBColor::Red);
}

void GraphicsDevice::DrawBoolean(const String &Text, bool Boolean, int x, int y)
{
    String BooleanString = "False";
    if(Boolean)
    {
        BooleanString = "True";
    }
    DrawStringFloat(Text + BooleanString, float(x), float(y), RGBColor::Red);
}

void GraphicsDevice::DrawVector(const String &Text, const Vec3f &V, int x, int y)
{
    DrawStringFloat(Text + String(V.x) + String(", ") + String(V.y) + String(", ") + String(V.z), float(x), float(y), RGBColor::Red);
}

bool GraphicsDevice::GetSystemActive()
{
    return _SystemActive;
}

bool GraphicsDevice::GetFullScreen()
{
    return _FullScreen;
}

bool GraphicsDevice::NeedToRestoreDevices()
{
    return _RestoreDevices;
}

void GraphicsDevice::DevicesRestored()
{
    _RestoreDevices = false;
}

GraphicsDeviceType GraphicsDevice::GetType()
{
    return _Type;
}