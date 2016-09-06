/*
D3D9Font.h
Written by Matthew Fisher

D3D9Font wraps the ID3DXFont object.
*/
#pragma once

#ifdef USE_D3D9

class D3D9Font : public D3D9Object
{
public:
    D3D9Font();
    ~D3D9Font();

    void ReleaseMemory();
    void FreeMemory();
    void Reset(LPDIRECT3DDEVICE9 Device);

    void Init(LPDIRECT3DDEVICE9 Device, const String &FontName, int FontHeight, int FontWeight);
    void DrawString(const String &Text, const Vec2f &TopLeft, RGBColor Color) const;
    void DrawString(const String &Text, const Vec2f &TopLeft, const float Width, RGBColor Color) const;
    void DrawString(const String &Text, const Rectangle2f &Rect, RGBColor Color) const;
    
private:
    String _FontName;
    int _FontHeight;
    int _FontWeight;
    LPDIRECT3DDEVICE9 _Device;
    LPD3DXFONT _Font;
};

#endif