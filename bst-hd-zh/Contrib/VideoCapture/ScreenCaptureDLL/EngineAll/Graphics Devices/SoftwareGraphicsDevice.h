/*
SoftwareGraphicsDevice.h
Written by Matthew Fisher

a software instance of GraphicsDevice.  Contains no new functionality over GraphicsDevice.
Doesn't support several things, like textures or lighting.
*/
#pragma once
class SoftwareGraphicsDevice : public GraphicsDevice
{
public:
    SoftwareGraphicsDevice();
    ~SoftwareGraphicsDevice();

    void FreeMemory();
    void Init3D(WindowObjects &O, const GraphicsDeviceParameters &Parameters);

    void ReSize(WindowObjects &O, AppInterface &App);
    void ReSizeFullScreen(WindowObjects &O, AppInterface &App);
    void StartRender(WindowObjects &O);
    void FinishRender(WindowObjects &O, AppInterface &App);

    void DrawStringFloat(const String &Text, float x, float y, RGBColor c);

    void LoadViewport(Viewport &V)
    {
    }
    void LoadMatrix(MatrixController &MC);

    void RenderLineStrip(const Mesh &M);
    void Render(const Mesh &M);

    BaseTexture* CreateTexture();    //textures are not supported in software

    void CaptureScreen(WindowManager &WM, Bitmap &B);

    void SetWireframe();
    void DisableWireframe();
    void ToggleWireframe();
    void ClearTexture(UINT Index);

    void SetZState(ZState NewState);

    void RenderSolidWireframe(BaseMesh *BM)
    {
    }

    void LoadNewBmp(const Bitmap &_Bmp)
    {
        Bmp = _Bmp;
    }

private:
    
    bool           _Wireframe;

    Bitmap         Bmp;     //the rendering target
    ZBuffer        Z;       //the depth buffer
    BITMAPINFO     Info;    //information about the screen

    HDC            hDC;     //the device context
    HWND           hWnd;    //the window handle
    Matrix4        Total,Viewport;    //Total is the World*View*Perspective transform; Viewport is the NDC -> screen Matrix4

    AliasRender PR;         //the interface used for rendering primitives
};
