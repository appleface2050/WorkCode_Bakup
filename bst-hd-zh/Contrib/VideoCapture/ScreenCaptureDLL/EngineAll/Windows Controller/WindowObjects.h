/*
WindowObjects.h
Written by Matthew Fisher

A set of useful objects associated with each window (the graphics device, window manager, input manager, and
frame timer.)
*/

class WindowObjects
{
public:
    WindowObjects();
    __forceinline WindowManager& GetWindowManager()
    {
        return *_WindowManager;
    }
    __forceinline GraphicsDevice& GetGraphicsDevice()
    {
        Assert(_GraphicsDevice != NULL, "GraphicsDevice not initalized");
        return *_GraphicsDevice;
    }
    __forceinline void SetWindowManager(WindowManager *WM)
    {
        Assert(_WindowManager == NULL, "SetWindowManager called twice");
        Assert(WM != NULL, "Invalid window manager for SetWindowManager");
        _WindowManager = WM;
    }
    __forceinline void SetGraphicsDevice(GraphicsDevice *GD)
    {
        Assert(_GraphicsDevice == NULL, "SetGraphicsDevice called twice");
        Assert(GD != NULL, "Invalid device for SetGraphicsDevice");
        _GraphicsDevice = GD;
    }
    __forceinline FrameTimer& GetFrameTimer()
    {
        return _FrameTimer;
    }
    __forceinline InputManager& GetInputManager()
    {
        return _InputManager;
    }
    __forceinline bool AcknowledgeReSize()
    {
        bool Result = _PossibleReSize;
        _PossibleReSize = false;
        return Result;
    }
    __forceinline void QueueReSize()
    {
        if(!_FirstReSizeSkipped)
        {
            _FirstReSizeSkipped = true;
        }
        else
        {
            _PossibleReSize = true;
        }
    }

private:
    WindowManager  *_WindowManager;
    GraphicsDevice *_GraphicsDevice;
    FrameTimer     _FrameTimer;
    InputManager   _InputManager;
    bool           _PossibleReSize;
    bool           _FirstReSizeSkipped;
};
