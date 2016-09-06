/*
GUISlider.cpp
Written by Matthew Fisher

GUISlider is a draggable slider GUIElement.
*/
#include "..\\..\\Main.h"
#include "GUISlider.h"

float GUISlider::GetFloatValue()
{
    return _Value;
}

void GUISlider::UpdateValue(const Vec3f &MousePos, GUIController &Parent)
{
    _Value = Utility::Bound(Math::LinearMap(_Dimensions.Min.x, _Dimensions.Max.x, _LowRange, _HighRange, MousePos.x), _LowRange, _HighRange);
    Parent.AddEvent(GUIEvent(this, GUIEventSliderUpdate, _SliderUpdateEventId));
}

void GUISlider::MouseDown(const Vec3f &MousePos, GUIController &Parent)
{
    UpdateValue(MousePos, Parent);
}

void GUISlider::MouseDrag(const Vec3f &MousePos, GUIController &Parent)
{
    UpdateValue(MousePos, Parent);
}

void GUISlider::MouseUp(const Vec3f &MousePos, GUIController &Parent)
{
    
}

void GUISlider::Render(GraphicsDevice &GD, WindowManager &WM)
{
    if(_SliderMesh.VertexCount() == 0)
    {
        _SliderMesh.SetGD(GD);
        _SliderMesh.CreatePlane(1.0f, 2, 2);
        _SliderMesh.Vertices()[0].Pos = Vec3f(_Dimensions.Min.x, _Dimensions.Min.y, 0.01f);
        _SliderMesh.Vertices()[1].Pos = Vec3f(_Dimensions.Max.x, _Dimensions.Min.y, 0.01f);
        _SliderMesh.Vertices()[2].Pos = Vec3f(_Dimensions.Min.x, _Dimensions.Max.y, 0.01f);
        _SliderMesh.Vertices()[3].Pos = Vec3f(_Dimensions.Max.x, _Dimensions.Max.y, 0.01f);
        _SliderMesh.SetColor(RGBColor::White);
    }

    if(_MarkerMesh.VertexCount() == 0)
    {
        _MarkerMesh.SetGD(GD);
        _MarkerMesh.CreatePlane(1.0f, 2, 2);
        _MarkerMesh.SetColor(RGBColor::Red);
    }

    Vec2f MarkerVariance = Vec2f(_Dimensions.Dimensions().y * 0.25f, _Dimensions.Dimensions().y * 0.25f);
    Vec2f MarkerCenter = Vec2f(Math::LinearMap(_LowRange, _HighRange, _Dimensions.Min.x, _Dimensions.Max.x, _Value), _Dimensions.Center().y);
    _MarkerMesh.Vertices()[0].Pos = Vec3f(MarkerCenter.x - MarkerVariance.x, MarkerCenter.y - MarkerVariance.y, 0.001f);
    _MarkerMesh.Vertices()[1].Pos = Vec3f(MarkerCenter.x + MarkerVariance.x, MarkerCenter.y - MarkerVariance.y, 0.001f);
    _MarkerMesh.Vertices()[2].Pos = Vec3f(MarkerCenter.x - MarkerVariance.x, MarkerCenter.y + MarkerVariance.y, 0.001f);
    _MarkerMesh.Vertices()[3].Pos = Vec3f(MarkerCenter.x + MarkerVariance.x, MarkerCenter.y + MarkerVariance.y, 0.001f);
    
    _SliderMesh.Render();
    _MarkerMesh.Render();

    Vec2f TopLeft = WM.MapRelativeWindowCoordToAbsolute(_Dimensions.Min);
    GD.DrawStringFloat(_Name, float(TopLeft.x), float(TopLeft.y), RGBColor::Blue);
}
