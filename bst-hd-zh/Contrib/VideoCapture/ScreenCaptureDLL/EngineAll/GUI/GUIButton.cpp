/*
GUIButton.cpp
Written by Matthew Fisher

GUIBUtton is a clickable button GUIElement.
*/
#include "..\\..\\Main.h"
#include "GUIButton.h"

void GUIButton::MouseDown(const Vec2f &MousePos, GUIController &Parent)
{
    _Pressed = true;
}

void GUIButton::MouseUp(const Vec2f &MousePos, GUIController &Parent)
{
    if(_Pressed)
    {
        _Pressed = false;
        if(_Dimensions.ContainsPt(MousePos))
        {
            Parent.AddEvent(GUIEvent(this, GUIEventButtonPressed, _ButtonPressEventId));
        }
    }
}

void GUIButton::Render(GraphicsDevice &GD, WindowManager &WM)
{
    if(_ButtonMesh.VertexCount() == 0)
    {
        _ButtonMesh.SetGD(GD);
        _ButtonMesh.CreatePlane(1.0f, 2, 2);
        _ButtonMesh.Vertices()[0].Pos = Vec3f(_Dimensions.Min.x, _Dimensions.Min.y, 0.01f);
        _ButtonMesh.Vertices()[1].Pos = Vec3f(_Dimensions.Max.x, _Dimensions.Min.y, 0.01f);
        _ButtonMesh.Vertices()[2].Pos = Vec3f(_Dimensions.Min.x, _Dimensions.Max.y, 0.01f);
        _ButtonMesh.Vertices()[3].Pos = Vec3f(_Dimensions.Max.x, _Dimensions.Max.y, 0.01f);
        _ButtonMesh.SetColor(RGBColor::White);
    }
    
    _ButtonMesh.Render();

    Vec2f TopLeft = WM.MapRelativeWindowCoordToAbsolute(_Dimensions.Min);
    GD.DrawStringFloat(_Name, float(TopLeft.x), float(TopLeft.y), RGBColor::Blue);
}
