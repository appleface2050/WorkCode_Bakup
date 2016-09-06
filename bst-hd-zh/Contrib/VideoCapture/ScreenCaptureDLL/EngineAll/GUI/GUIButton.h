/*
GUIButton.h
Written by Matthew Fisher

GUIBUtton is a clickable button GUIElement.
*/
#pragma once

class GUIButton : public GUIElement
{
public:
    GUIButton() {}
    GUIButton(const Rectangle2f &Dimensions, const String &Name, UINT ButtonPressEventId)
    {
        _Dimensions = Dimensions;
        _ButtonPressEventId = ButtonPressEventId;
        _Pressed = false;
        _Name = Name;
    }

    void MouseDown(const Vec2f &MousePos, GUIController &Parent);
    void MouseUp(const Vec2f &MousePos, GUIController &Parent);
    
    void Render(GraphicsDevice &GD, WindowManager &WM);

private:
    bool _Pressed;
    UINT _ButtonPressEventId;
    Mesh _ButtonMesh;
};