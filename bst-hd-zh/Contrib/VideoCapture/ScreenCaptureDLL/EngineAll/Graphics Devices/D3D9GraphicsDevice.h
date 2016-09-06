/*
D3DGraphicsDevice.h
Written by Matthew Fisher

a D3D instance of GraphicsDevice.  The only new functionality is GetDevice() which returns
the D3DDevice associated with this graphics device.
*/
#pragma once

#ifdef USE_D3D9

class D3D9GraphicsDevice : public GraphicsDevice
{
public:
    D3D9GraphicsDevice();
    ~D3D9GraphicsDevice();

    void FreeMemory();
    void Init3D(WindowObjects &O, const GraphicsDeviceParameters &Parameters);

    void ReSize(WindowObjects &O, AppInterface &App);
    void ReSizeFullScreen(WindowObjects &O, AppInterface &App);
    void StartRender(WindowObjects &O);
    void FinishRender(WindowObjects &O, AppInterface &App);

    void DrawStringFloat(const String &Text, float x, float y, RGBColor c);
    void DrawString(const String &Text, const Rectangle2f &Rect, RGBColor c);

    void LoadViewport(Viewport &V);
    void LoadMatrix(MatrixController &MC);

    void Render(const Mesh &M);
    void RenderLineStrip(const Mesh &M);

    void CaptureDesktop(WindowManager &WM, Bitmap &B);
    void CaptureScreen(WindowManager &WM, Bitmap &B);

    LPDIRECT3DDEVICE9 GetDevice();
    BaseTexture* CreateTexture();

    void SetWireframe();
    void DisableWireframe();
    void ToggleWireframe();
    void ClearTexture(UINT Index);

    void SetZState(ZState NewState);

    void RenderSolidWireframe(BaseMesh *BM);

private:
    //internal functions
    void LoadPresentParameters(D3DPRESENT_PARAMETERS &PresentParameters, WindowsWindowManager &WM);
    void InitializeStateMachine();          //load default values into the state machine
    void ResetSystem(WindowsWindowManager &WM);    //resets the system
    void RestoreSystem();                   //restores the system after it has been reset

    LPDIRECT3D9       _D3D;        //the main D3D object, used to create the D3DDevice object
    LPDIRECT3DDEVICE9 _Device;     //the D3DDevice object which this GraphicsDevice encapsulates
    LPD3DXFONT        _MainFont;   //the font used for text rendering

    LPDIRECT3DSURFACE9 _DesktopSurface; //used for capturing the desktop
    LPDIRECT3DSURFACE9 _WindowSurface;  //used for capturing the window
};

#endif