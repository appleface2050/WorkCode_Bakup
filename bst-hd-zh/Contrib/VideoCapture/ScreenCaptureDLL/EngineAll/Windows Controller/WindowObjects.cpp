/*
WindowObjects.cpp
Written by Matthew Fisher

A set of useful objects associated with each window (the graphics device, window manager, input manager, and
frame timer.)
*/
#include "..\\..\\Main.h"

WindowObjects::WindowObjects()
{
    _GraphicsDevice = NULL;
    _WindowManager = NULL;
    _PossibleReSize = false;
    _FirstReSizeSkipped = false;
}
