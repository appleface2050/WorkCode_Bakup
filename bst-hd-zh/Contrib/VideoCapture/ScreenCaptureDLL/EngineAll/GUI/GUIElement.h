/*
GUIElement.h
Written by Matthew Fisher

GUIElement is an abstract class that all GUI elements use.
*/
#pragma once
class GUIController;

class GUIElement
{
public:
    GUIElement() {}
    virtual void MouseDown(const Vec2f &MousePos, GUIController &Parent) {}
    virtual void MouseUp(const Vec2f &MousePos, GUIController &Parent) {}
    virtual void MouseDrag(const Vec2f &MousePos, GUIController &Parent) {}
    virtual float GetFloatValue() { return 0.0f; }
    virtual void Render(GraphicsDevice &GD, WindowManager &WM) = 0;

    __forceinline const Rectangle2f& Dimensions() const
    {
        return _Dimensions;
    }

protected:
    Rectangle2f _Dimensions;
    String _Name;
};