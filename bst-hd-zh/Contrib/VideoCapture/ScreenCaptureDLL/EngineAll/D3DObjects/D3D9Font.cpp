/*
D3D9Font.cpp
Written by Matthew Fisher

D3D9Font wraps the ID3DXFont object.
*/
#include "..\\..\\Main.h"
#include "D3D9Font.h"

#ifdef USE_D3D9

D3D9Font::D3D9Font()
{
    _Font = NULL;
    _Device = NULL;
}

D3D9Font::~D3D9Font()
{
    FreeMemory();
}

void D3D9Font::FreeMemory()
{
    if(_Font)
    {
        _Font->Release();
        _Font = NULL;
    }
    if(_Device)
    {
        _Device->Release();
        _Device = NULL;
    }
}

void D3D9Font::ReleaseMemory()
{
    FreeMemory();
}

void D3D9Font::Reset(LPDIRECT3DDEVICE9 Device)
{
    FreeMemory();

    _Device = Device;
    Assert(Device != NULL, "Device == NULL");

    HRESULT hr;
    hr = D3DXCreateFont(
            _Device,
            _FontHeight,                 // Height
            0,                           // Width
            _FontWeight,                 // Weight
            1,                           // MipLevels, 0 = autogen mipmaps
            FALSE,                       // Italic
            DEFAULT_CHARSET,             // CharSet
            OUT_DEFAULT_PRECIS,          // OutputPrecision
            DEFAULT_QUALITY,             // Quality
            DEFAULT_PITCH | FF_DONTCARE, // PitchAndFamily
            _FontName.CString(),         // pFaceName
            &_Font);                     // ppFont
    Assert(SUCCEEDED(hr) && _Font != NULL, "D3DXCreateFont failed");
}

void D3D9Font::Init(LPDIRECT3DDEVICE9 Device, const String &FontName, int FontHeight, int FontWeight)
{
    _FontName = FontName;
    _FontHeight = FontHeight;
    _FontWeight = FontWeight;
    Reset(Device);
}

void D3D9Font::DrawString(const String &Text, const Vec2f &TopLeft, RGBColor Color) const
{
    Rectangle2f Rect(Rectangle2f(TopLeft, Vec2f(TopLeft.x + 512.0f, TopLeft.y)));
    DrawString(Text, Rect, Color);
}

void D3D9Font::DrawString(const String &Text, const Vec2f &TopLeft, const float Width, RGBColor Color) const
{
    Rectangle2f Rect(Rectangle2f(TopLeft, Vec2f(TopLeft.x + Width, TopLeft.y)));
    DrawString(Text, Rect, Color);
}

void D3D9Font::DrawString(const String &Text, const Rectangle2f &Rect, RGBColor Color) const
{
    RECT CurRect = Rect.ToRect();
    HRESULT CoopLevel = _Device->TestCooperativeLevel();
    if(CoopLevel == D3D_OK)
    {
        _Font->DrawText( NULL, Text.CString(), Text.Length(), &CurRect, DT_WORDBREAK | DT_CALCRECT, 0 );
        _Font->DrawText( NULL, Text.CString(), Text.Length(), &CurRect, DT_WORDBREAK | DT_NOCLIP, D3DXCOLOR(Color.r / 255.0f, Color.g / 255.0f, Color.b / 255.0f, 1.0f ));
    }
}

#endif