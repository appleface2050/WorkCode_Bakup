/*
GraphicsDevice.h
Written by Matthew Fisher

GraphicsDevice is the abstract class that contains functionality that all graphics devices must expose.
any GraphicsDevice can do simple things like render meshes into the main rendering window and handle
the world, view and projection transforms.
*/
#pragma once
//
// Allowed Z-buffer states
//
enum ZState
{
        ZStateAlways = 0,
        ZStateLessEqual = 1
};

//
// Allowed graphics device types
//
enum GraphicsDeviceType
{
    GD_D3D9 = 1,
    GD_D3D10 = 2,
    GD_OpenGL = 3,
    GD_Software = 4
};

struct GraphicsDeviceParameters
{
    GraphicsDeviceParameters()
    {
        MultisampleCount = 0;
        ScreenColor = RGBColor::Black;
        WaitForVSync = false;
    }
    String MainFontName;
    UINT MultisampleCount;
    RGBColor ScreenColor;
    bool WaitForVSync;
};

class BaseTexture;

class GraphicsDevice
{
public:
    virtual void FreeMemory() = 0;
    virtual void Init3D(WindowObjects &O, const GraphicsDeviceParameters &Parameters) = 0;

    virtual void PrepareFullScreenReSize(WindowObjects &O, AppInterface &App);
    virtual void ReSizeFullScreen(WindowObjects &O, AppInterface &App) = 0;
    virtual void ReSize(WindowObjects &O, AppInterface &App) = 0;

    virtual void StartRender(WindowObjects &O) = 0;                        //prepare rendering of a new frame
    virtual void FinishRender(WindowObjects &O, AppInterface &App) = 0;    //finalize rendering of a new frame
    
    virtual bool NeedToRestoreDevices();    //determine whether devices need to be restored
    virtual void DevicesRestored();         //called after devices have been restored

    //
    // Rendering
    //
    virtual void DrawString(const String &Text, const Rectangle2f &Rect, RGBColor c) {}
    virtual void DrawStringFloat(const String &Text, float x, float y, RGBColor c) = 0;

    virtual void DrawString(const String &Text, int x, int y);
    virtual void DrawInteger(const String &Text, int N, int x, int y);
    virtual void DrawDouble(const String &Text, double N, int x, int y);
    virtual void DrawBoolean(const String &Text, bool Boolean, int x, int y);
    virtual void DrawVector(const String &Text, const Vec3f &V, int x, int y);

    virtual void Render(const Mesh &M) = 0;               //renders the mesh M as a series of triangles

    //
    // State modifiers
    //
    virtual void LoadViewport(Viewport &V) = 0;           //use the provided viewport for rendering in screen coordinates
    virtual void LoadMatrix(MatrixController &MC) = 0;    //use the provided matrices for world, view, and projection transforms
    virtual void SetWireframe() = 0;
    virtual void DisableWireframe() = 0;
    virtual void ToggleWireframe() = 0;
    virtual void SetZState(ZState NewState) = 0;
    virtual void ClearTexture(UINT Index) = 0;

    //
    // State query
    //
    virtual void CaptureScreen(WindowManager &WM, Bitmap &B) = 0;    //captue the current screen into the bitmap B.
    
    virtual BaseTexture* CreateTexture() = 0;        //create a new BaseTexture of the appropriate type

    //
    // Accessors
    //
    bool GetSystemActive();
    bool GetFullScreen();
    GraphicsDeviceType GetType();
    __forceinline const GraphicsDeviceParameters& Parameters() const
    {
        return _Parameters;
    }
    __forceinline GraphicsDeviceParameters& Parameters()
    {
        return _Parameters;
    }

    //
    // Cast
    //
    D3D9GraphicsDevice& CastD3D9()
    {
        Assert(_Type == GD_D3D9, "Invalid graphics device cast");
        return *((D3D9GraphicsDevice*)this);
    }

    D3D10GraphicsDevice& CastD3D10()
    {
        Assert(_Type == GD_D3D10, "Invalid graphics device cast");
        return *((D3D10GraphicsDevice*)this);
    }
    SoftwareGraphicsDevice& CastSoftware()
    {
        Assert(_Type == GD_Software, "Invalid graphics device cast");
        return *((SoftwareGraphicsDevice*)this);
    }

protected:
    GraphicsDeviceType _Type;
    GraphicsDeviceParameters _Parameters;
    bool _FullScreen;
    bool _SystemActive;
    bool _RestoreDevices;
};
