/*
WindowManager.cpp
Written by Matthew Fisher

WindowManager handles creating and updating the application's main window.
*/
#include "..\\..\\Main.h"

WindowsWindowManager::WindowsWindowManager()
{
    _Type = WindowManagerWindows;
    _WindowHandle = NULL;
    _FullScreen = false;
}

bool WindowsWindowManager::GetFullScreen()
{
    return _FullScreen;
}

void WindowsWindowManager::FreeMemory()
{
    FreeWindow();
    UnregisterClass( _ClassName.CString(), _WindowClass.hInstance );
}

void WindowsWindowManager::FreeWindow()
{
    DestroyWindow(_WindowHandle);
}

void WindowsWindowManager::InitAll(HINSTANCE hInstance, int nCmdShow, bool _FullScreen, int _ScreenWidth, int _ScreenHeight, const String &WindowName)
{
    _FixedAspectRatio = 0.0;

    _ClassName = WindowName;
    //_WindowClass.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC; 
    _WindowClass.style = CS_HREDRAW | CS_VREDRAW;
    _WindowClass.lpfnWndProc = (WNDPROC) WndProc; 
    _WindowClass.cbClsExtra = 0; 
    _WindowClass.cbWndExtra = 0; 
    _WindowClass.hInstance = hInstance; 
    _WindowClass.hIcon = LoadIcon(NULL, IDI_WINLOGO);
    _WindowClass.hCursor = LoadCursor(NULL, IDC_ARROW);
    _WindowClass.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH); 
    _WindowClass.lpszMenuName =  NULL;
    _WindowClass.lpszClassName = _ClassName.CString();           //window class paramaters
    RegisterClass(&_WindowClass);                                //register the class with windows

    InitWindow(hInstance, nCmdShow, _FullScreen, _ScreenWidth, _ScreenHeight, WindowName);    //initalize the window
}

void WindowsWindowManager::InitWindow(HINSTANCE hInstance, int nCmdShow, bool FullScreen, int ScreenWidth, int ScreenHeight, const String &WindowName)
{
    _FullScreen = FullScreen;

    DWORD dwStyle;
    if(FullScreen)
    {
        dwStyle = WS_POPUP;
    }
    else
    {
        //dwStyle = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX;
        dwStyle = WS_OVERLAPPEDWINDOW;

        //dwStyle = WS_BORDER | WS_CAPTION | WS_MINIMIZEBOX | WS_SYSMENU | WS_VISIBLE;
        //dwStyle = WS_DLGFRAME | WS_VISIBLE;
    }

    _WindowHandle = CreateWindow(
        WindowName.CString(), 
        WindowName.CString(), 
        dwStyle, 
        0, //CW_USEDEFAULT
        0, //CW_USEDEFAULT
        ScreenWidth, 
        ScreenHeight, 
        (HWND) NULL, 
        (HMENU) NULL, 
        hInstance,
        (LPVOID) NULL); //create the window with the appropriate paramaters
                        //see Win32 documentation for a description of the CreateWindow function
    PersistentAssert(_WindowHandle != NULL, "CreateWindow failed");

    ShowWindow(_WindowHandle, nCmdShow);
    UpdateWindow(_WindowHandle);            //show and update the window
}

void WindowsWindowManager::RenameWindow(const String &NewName)
{
    SetWindowText(_WindowHandle, NewName.CString());
}

Vec2f WindowsWindowManager::GetBorderDimensions()
{
    RECT ClientRect, ScreenRect;
    GetClientRect(_WindowHandle, &ClientRect);
    GetWindowRect(_WindowHandle, &ScreenRect);
    int BorderWidth = ScreenRect.right - ScreenRect.left - ClientRect.right;
    int BorderHeight = ScreenRect.bottom - ScreenRect.top - ClientRect.bottom;
    return Vec2f(float(BorderWidth), float(BorderHeight));
}

void WindowsWindowManager::ReSize(int NewWindowWidth, int NewWindowHeight)
{
    RECT ClientRect, ScreenRect;
    GetClientRect(_WindowHandle, &ClientRect);
    GetWindowRect(_WindowHandle, &ScreenRect);
    int BorderWidth = ScreenRect.right - ScreenRect.left - ClientRect.right;
    int BorderHeight = ScreenRect.bottom - ScreenRect.top - ClientRect.bottom;
    MoveWindow(_WindowHandle, ScreenRect.left, ScreenRect.top, NewWindowWidth + BorderWidth, NewWindowHeight + BorderHeight, TRUE);
}

void WindowsWindowManager::ResetWindow(bool FullScreen, int nCmdShow, int Width, int Height)
{
    _FullScreen = FullScreen;

    if( FullScreen )
    {
        SetWindowLong( _WindowHandle, GWL_STYLE, WS_POPUP );
    }
    else
    {
        Width--;
        Height--;
        SetWindowLong( _WindowHandle, GWL_STYLE, WS_OVERLAPPEDWINDOW );    //handle the difference b/w Full screen and not full screen mode
    }

    //SetWindowPos resets the window
    SetWindowPos(_WindowHandle, HWND_TOP, 0, 0, Width, Height, SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOCOPYBITS | SWP_SHOWWINDOW);

    
    //ShowWindow(_WindowHandle, nCmdShow);
    //UpdateWindow(_WindowHandle);            //show and update the window    
}

int WindowsWindowManager::GetWidth()
{
    RECT Rect;
    GetClientRect(_WindowHandle, &Rect);
    return Rect.right - Rect.left;
}

int WindowsWindowManager::GetHeight()
{
    RECT Rect;
    GetClientRect(_WindowHandle, &Rect);
    return Rect.bottom - Rect.top;
}

Vec2i WindowsWindowManager::GetDimensions()
{
    RECT Rect;
    GetClientRect(_WindowHandle, &Rect);
    return Vec2i(Rect.right - Rect.left, Rect.bottom - Rect.top);
}

Vec2f WindowsWindowManager::MapAbsoluteWindowCoordToRelative(const Vec2f &Absolute)
{
    RECT Rect;
    GetClientRect(_WindowHandle, &Rect);
    UINT Width = Rect.right - Rect.left;
    UINT Height = Rect.bottom - Rect.top;
    Vec2f Result(Math::LinearMap(0.0f, float(Width), 0.0f, 1.0f, Absolute.x),
                 Math::LinearMap(0.0f, float(Height), 0.0f, 1.0f, Absolute.y));
    return Result;
}

Vec2f WindowsWindowManager::MapRelativeWindowCoordToAbsolute(const Vec2f &Relative)
{
    RECT Rect;
    GetClientRect(_WindowHandle, &Rect);
    UINT Width = Rect.right - Rect.left;
    UINT Height = Rect.bottom - Rect.top;
    Vec2f Result(Math::LinearMap(0.0f, 1.0f, 0.0f, float(Width), Relative.x),
                Math::LinearMap(0.0f, 1.0f, 0.0f, float(Height), Relative.y));
    return Result;
}

HWND WindowsWindowManager::GetHWND()
{
    return _WindowHandle;
}