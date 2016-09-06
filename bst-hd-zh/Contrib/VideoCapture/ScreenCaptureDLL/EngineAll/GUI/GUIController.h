/*
GUIController.h
Written by Matthew Fisher

GUIController handles dispatching window events and rendering a set of GUIElement objects.
*/
#pragma once
enum GUIEventType
{
    GUIEventButtonPressed,
    GUIEventSliderUpdate,
};

struct GUIEvent
{
    GUIEvent() {}
    GUIEvent(GUIElement *_Element, GUIEventType _Type, UINT _EventId)
    {
        Element = _Element;
        Type = _Type;
        EventId = _EventId;
    }

    GUIElement *Element;
    GUIEventType Type;
    UINT EventId;
};

class GUIController
{
public:
    GUIController()
    {
        _ActiveElement = NULL;
    }

    void Frame(InputManager &Input);
    void Render(GraphicsDevice &GD, WindowManager &WM);
    
    void AddGUIElement(GUIElement *E);

    void AddEvent(const GUIEvent &NewEvent)
    {
        _Events.PushEnd(NewEvent);
    }
    
    __forceinline GUIEvent PopEvent()
    {
        Assert(_Events.Length() > 0, "No events");
        GUIEvent Result = _Events.Last();
        _Events.PopEnd();
        return Result;
    }
    __forceinline bool EventsLeft()
    {
        return (_Events.Length() > 0);
    }

private:
    Vector<GUIElement*> _Elements;
    Vector<GUIEvent> _Events;
    GUIElement *_ActiveElement;
};