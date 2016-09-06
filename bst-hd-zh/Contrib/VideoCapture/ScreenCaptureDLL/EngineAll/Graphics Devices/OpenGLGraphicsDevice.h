/*
OpenGLGraphicsDevice.h
Written by Matthew Fisher

an OpenGL instance of GraphicsDevice.  Contains no new functionality over GraphicsDevice.
*/
#pragma once

#ifdef USE_OPENGL

class OpenGLGraphicsDevice : public GraphicsDevice
{
public:
    OpenGLGraphicsDevice();
    ~OpenGLGraphicsDevice();

    void FreeMemory();
    void Init3D(WindowObjects &O, const GraphicsDeviceParameters &Parameters);

    void ReSize(WindowObjects &O, AppInterface &App);
    void ReSizeFullScreen(WindowObjects &O, AppInterface &App);
    void StartRender(WindowObjects &O);
    void FinishRender(WindowObjects &O, AppInterface &App);

    void DrawStringFloat(const String &Text, float x, float y, RGBColor c);
    void LoadMatrix(MatrixController &MC);
    void Render(const Mesh &M);
    void RenderLineStrip(const Mesh &M);

    void CaptureScreen(WindowManager &WM, Bitmap &B);

    BaseTexture* CreateTexture();

    void SetWireframe();
    void DisableWireframe();
    void ToggleWireframe();
    
    void SetScreenColor(RGBColor _ScreenColor);

    void SetZState(ZState NewState);
    void ClearTexture(UINT Index);

    void RenderSolidWireframe(BaseMesh *BM)
    {
    }

    void LoadViewport(Viewport &V)
    {
    }

private:
    void InitializeStateMachine(WindowManager &WM);    //resets the state machine
    void RestoreSystem();

    Matrix4     _SavedMatrix;

    int         _FontBase, _FontScreenWidth, _FontScreenHeight;    //local storage for the font
    bool        _BaseInitalized;

    HDC         _DeviceContext;    //device context
    HGLRC       _RenderContext;    //rendering context
    HWND        _Window;           //the window handle
    GraphicsDeviceParameters _Parameters;
};

#endif
