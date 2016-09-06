/*
GUIController.cpp
Written by Matthew Fisher

GUIController handles dispatching window events and rendering a set of GUIElement objects.
*/
#include "..\\..\\Main.h"
#include "GUIController.h"


void GUIController::Frame(InputManager &Input)
{
    Vec2f MousePos = Input.MousePosNormalizedToWindow();
    if(_ActiveElement != NULL)
    {
        if(Input.Mouse().Buttons[MouseButtonLeft])
        {
            _ActiveElement->MouseDrag(MousePos, *this);
        }
        else
        {
            _ActiveElement->MouseUp(MousePos, *this);
            _ActiveElement = NULL;
        }
    }
    else
    {
        if(Input.Mouse().Buttons[MouseButtonLeft])
        {
            for(UINT ElementIndex = 0; ElementIndex < _Elements.Length() && _ActiveElement == NULL; ElementIndex++)
            {
                GUIElement &curElement = *_Elements[ElementIndex];
                if(curElement.Dimensions().ContainsPt(MousePos))
                {
                    curElement.MouseDown(MousePos, *this);
                    _ActiveElement = &curElement;
                }
            }
        }
    }
}

void GUIController::AddGUIElement(GUIElement *E)
{
    _Elements.PushEnd(E);
}

void GUIController::Render(GraphicsDevice &GD, WindowManager &WM)
{
    //MatrixController MC( Matrix4::Translation(Vec3f(0.5f, 0.5f, 0.0f)) * Matrix4::Scaling(Vec3f(0.5f, 0.5f, 1.0f)) );
    MatrixController MC( Matrix4::Translation(Vec3f(-0.5f, -0.5f, 0.0f)) * Matrix4::Scaling(Vec3f(2.0f, -2.0f, 1.0f)) );
    GD.LoadMatrix(MC);

    for(UINT ElementIndex = 0; ElementIndex < _Elements.Length(); ElementIndex++)
    {
        GUIElement &curElement = *_Elements[ElementIndex];
        curElement.Render(GD, WM);
    }
}
