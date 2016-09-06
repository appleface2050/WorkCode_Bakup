/*
WindowManager.h
Written by Matthew Fisher

WindowManager handles creating and updating the application's main window.
*/

class WindowsWindowManager;
class GLUTWindowManager;

enum WindowManagerType
{
    WindowManagerWindows,
    WindowManagerGLUT,
};

class WindowManager
{
public:
    //
    // Cast
    //
    WindowsWindowManager& CastWindows()
    {
        Assert(_Type == WindowManagerWindows, "Invalid windows manager cast");
        return *((WindowsWindowManager*)this);
    }

    GLUTWindowManager& CastGLUT()
    {
        Assert(_Type == WindowManagerGLUT, "Invalid windows manager cast");
        return *((GLUTWindowManager*)this);
    }

    virtual void FreeMemory() = 0;
    virtual int GetWidth() = 0;
    virtual int GetHeight() = 0;
    virtual bool GetFullScreen() = 0;
    virtual void RenameWindow(const String &NewName) = 0;
    virtual Vec2f MapAbsoluteWindowCoordToRelative(const Vec2f &Absolute) = 0;
    virtual Vec2f MapRelativeWindowCoordToAbsolute(const Vec2f &Relative) = 0;
    virtual void ReSize(int NewWidth, int NewHeight) = 0;

protected:
    WindowManagerType _Type;
};

class WindowsWindowManager : public WindowManager
{
public:
    WindowsWindowManager();

    void FreeMemory();    //release all memory
    void FreeWindow();    //release the window (but not the window class.)  Used for toggling between full screen/windowed mode

    void InitAll(HINSTANCE hInstance, int nCmdShow, bool FullScreen, int ScreenWidth, int ScreenHeight, const String &WindowName);        //initalize everything (window class and the main window)
    void InitWindow(HINSTANCE hInstance, int nCmdShow, bool FullScreen, int ScreenWidth, int ScreenHeight, const String &WindowName);    //initalize just the window (assume the window class already exists)
    void ReSize(int NewClientAreaWidth, int NewClientAreaHeight);
    Vec2f GetBorderDimensions();

    void ResetWindow(bool FullScreen, int nCmdShow, int Width, int Height);

    void RenameWindow(const String &NewName);

    Vec2f MapAbsoluteWindowCoordToRelative(const Vec2f &Absolute);
    Vec2f MapRelativeWindowCoordToAbsolute(const Vec2f &Relative);

    int GetWidth();
    int GetHeight();
    Vec2i GetDimensions();
    HWND GetHWND();
    bool GetFullScreen();
    __forceinline double& FixedAspectRatio()
    {
        return _FixedAspectRatio;
    }

private:
    String _ClassName;
    WNDCLASS _WindowClass;
    HWND _WindowHandle;
    bool _FullScreen;
    double _FixedAspectRatio;
};

class GLUTWindowManager : public WindowManager
{
public:
    GLUTWindowManager();

    void FreeMemory();
    void FreeWindow();

    void Init(int ScreenWidth, int ScreenHeight, const String &WindowName);
    void ReSize(int NewClientAreaWidth, int NewClientAreaHeight);
    Vec2f GetBorderDimensions();

    void ResetWindow(bool FullScreen, int nCmdShow, int Width, int Height);

    void RenameWindow(const String &NewName);

    Vec2f MapAbsoluteWindowCoordToRelative(const Vec2f &Absolute);
    Vec2f MapRelativeWindowCoordToAbsolute(const Vec2f &Relative);

    int GetWidth();
    int GetHeight();
    Vec2i GetDimensions();
    bool GetFullScreen();

private:

};
