/*
D3D9GraphicsDevice.cpp
Written by Matthew Fisher

a _D3D instance of GraphicsDevice.  The only new functionality is GetDevice() which returns
the D3DDevice associated with this graphics device.
*/
#include "..\\..\\Main.h"
#include "D3D9GraphicsDevice.h"

#ifdef USE_D3D9

D3D9GraphicsDevice::D3D9GraphicsDevice()
{
    _Type = GD_D3D9;
    _D3D = NULL;
    _Device = NULL;
    _MainFont = NULL;
    _RestoreDevices = false;
    _WindowSurface = NULL;
    _DesktopSurface = NULL;
}

D3D9GraphicsDevice::~D3D9GraphicsDevice()
{
    FreeMemory();
}

void D3D9GraphicsDevice::FreeMemory()
{
    if(_Device)
    {
        _Device->Release();
        _Device = 0;
    }
    if(_D3D)
    {
        _D3D->Release();
        _D3D = 0;
    }

    if(_MainFont)
    {
        _MainFont->Release();
        _MainFont = 0;
    }
}

void D3D9GraphicsDevice::LoadPresentParameters(D3DPRESENT_PARAMETERS &PresentParameters, WindowsWindowManager &WM)
{
    ZeroMemory( &PresentParameters, sizeof(PresentParameters) );
    PresentParameters.BackBufferWidth = WM.GetWidth();
    PresentParameters.BackBufferHeight = WM.GetHeight();    //get the width and height from WM
    PresentParameters.BackBufferFormat = D3DFMT_A8R8G8B8;    //32-bit pixel format
    PresentParameters.BackBufferCount = 1;                        //double buffering
    PresentParameters.MultiSampleQuality = 0;
    PresentParameters.SwapEffect = D3DSWAPEFFECT_DISCARD; //D3DSWAPEFFECT_DISCARD
    PresentParameters.hDeviceWindow = WM.GetHWND();
    
    if(_Parameters.MultisampleCount <= 1)
    {
        PresentParameters.MultiSampleType = D3DMULTISAMPLE_NONE;
        PresentParameters.Flags |= D3DPRESENTFLAG_LOCKABLE_BACKBUFFER;
    }
    else
    {
        PresentParameters.MultiSampleType = D3DMULTISAMPLE_TYPE(_Parameters.MultisampleCount);
    }

    _FullScreen = WM.GetFullScreen();
    if(_FullScreen)
    {
        PresentParameters.Windowed = FALSE;
    }
    else
    {
        PresentParameters.Windowed = TRUE;
    }

    PresentParameters.EnableAutoDepthStencil = TRUE;
    PresentParameters.AutoDepthStencilFormat = D3DFMT_D24S8;    //24-bit depth buffer, 8 bit stencil buffer
    PresentParameters.FullScreen_RefreshRateInHz = D3DPRESENT_RATE_DEFAULT;

    if(_Parameters.WaitForVSync)
    {
        PresentParameters.PresentationInterval = D3DPRESENT_INTERVAL_ONE;
    }
    else
    {
        PresentParameters.PresentationInterval = D3DPRESENT_INTERVAL_IMMEDIATE;
    }
}

void D3D9GraphicsDevice::InitializeStateMachine()
{
    D3DXMATRIXA16 matIdentity;
    D3DXMatrixIdentity(&matIdentity);

    _Device->SetTransform(D3DTS_WORLD, &matIdentity);
    _Device->SetTransform(D3DTS_VIEW, &matIdentity);
    _Device->SetTransform(D3DTS_PROJECTION, &matIdentity);    //the default transformations are Identity

    _Device->SetRenderState(D3DRS_LIGHTING, FALSE);
    _Device->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);
    _Device->SetRenderState(D3DRS_AMBIENTMATERIALSOURCE, D3DMCS_MATERIAL);
    _Device->SetRenderState(D3DRS_SPECULARMATERIALSOURCE, D3DMCS_MATERIAL);
    _Device->SetRenderState(D3DRS_SRCBLEND,  D3DBLEND_ONE);
    _Device->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_ONE);

    _Device->SetSamplerState(0,D3DSAMP_MAGFILTER, D3DTEXF_LINEAR);
    _Device->SetSamplerState(0,D3DSAMP_MINFILTER, D3DTEXF_LINEAR);
    _Device->SetSamplerState(0,D3DSAMP_MIPFILTER, D3DTEXF_LINEAR);

    //_Device->SetSamplerState(0,D3DSAMP_ADDRESSU, D3DTADDRESS_MIRROR);
    //_Device->SetSamplerState(0,D3DSAMP_ADDRESSV, D3DTADDRESS_MIRROR);
    //_Device->SetSamplerState(0,D3DSAMP_ADDRESSW, D3DTADDRESS_MIRROR);

    _Device->SetTextureStageState( 0, D3DTSS_COLORARG1, D3DTA_TEXTURE );
    _Device->SetTextureStageState( 0, D3DTSS_COLORARG2, D3DTA_DIFFUSE );
    _Device->SetTextureStageState( 0, D3DTSS_COLOROP, D3DTOP_MODULATE );    //default values
}

void D3D9GraphicsDevice::Init3D(WindowObjects &O, const GraphicsDeviceParameters &Parameters)
{
    FreeMemory();
    
    _Parameters = Parameters;
    if(_Parameters.MainFontName.Length() == 0)
    {
        _Parameters.MainFontName = "Verdana";
    }

#ifndef SKIP_APP_INTERFACE
    _D3D = Direct3DCreate9( D3D_SDK_VERSION );
#endif

    Assert(_D3D != NULL, "Direct3DCreate9 failed");
    
    D3DPRESENT_PARAMETERS d3dpp;
    LoadPresentParameters(d3dpp, O.GetWindowManager().CastWindows());    //get the paramaters

    HWND HWnd = O.GetWindowManager().CastWindows().GetHWND();
    //D3DCREATE_FPU_PRESERVE
    HRESULT hr = _D3D->CreateDevice( D3DADAPTER_DEFAULT , D3DDEVTYPE_HAL,
                                     HWnd, D3DCREATE_MIXED_VERTEXPROCESSING, &d3dpp,
                                     &_Device );
    if(_Device == NULL || FAILED(hr))
    {
        hr = _D3D->CreateDevice( D3DADAPTER_DEFAULT , D3DDEVTYPE_HAL,
                                 HWnd, D3DCREATE_SOFTWARE_VERTEXPROCESSING, &d3dpp,
                                 &_Device );
    }
    Assert(_Device != NULL, "CreateDevice failed.  This application requires a DirectX 9.0c or better graphics card.");

    HDC hDC = GetDC( NULL );
    int nLogPixelsY = GetDeviceCaps(hDC, LOGPIXELSY);
    ReleaseDC( NULL, hDC );
    //int FontHeight = -12 * nLogPixelsY / 72;
    int FontHeight = -10 * nLogPixelsY / 72;

    hr = D3DXCreateFont( _Device,
            FontHeight,             // Height
            0,                      // Width
            FW_BOLD,                // Weight
            1,                      // MipLevels, 0 = autogen mipmaps
            FALSE,                  // Italic
            DEFAULT_CHARSET,        // CharSet
            OUT_DEFAULT_PRECIS,     // OutputPrecision
            DEFAULT_QUALITY,        // Quality
            DEFAULT_PITCH | FF_DONTCARE, // PitchAndFamily
            _Parameters.MainFontName.CString(), // pFaceName
            &_MainFont);             // ppFont
    Assert(SUCCEEDED(hr), "D3DXCreateFont failed");

    RestoreSystem();
}

void D3D9GraphicsDevice::StartRender(WindowObjects &O)
{
    if(!_Device)
    {
        return;
    }

    //clear the screen and the z-buffer
    RGBColor ScreenColor = _Parameters.ScreenColor;
    _Device->Clear( 0, NULL, D3DCLEAR_ZBUFFER | D3DCLEAR_TARGET, D3DCOLOR_ARGB(ScreenColor.a, ScreenColor.r, ScreenColor.g, ScreenColor.b), 1.0f, 0 );
    
    //prepare for rendering
    _Device->BeginScene();
}

void D3D9GraphicsDevice::FinishRender(WindowObjects &O, AppInterface &App)
{
    if(!_Device) return;
    _Device->EndScene();    //ends the scene (forcing rendering of queued data)

    //attempt to flip the back buffer onto the front buffer
    if(_Device->Present( NULL, NULL, NULL, NULL ) == D3DERR_DEVICELOST)
    {
        if(_SystemActive)                    //if we just now lost our devices
        {
            _MainFont->OnLostDevice();
#ifndef SKIP_APP_INTERFACE
            App.ResetAllDevices(O);
#endif
            _SystemActive = false;
        }
        else
        {
            ResetSystem(O.GetWindowManager().CastWindows());
        }
    }
}

void D3D9GraphicsDevice::RestoreSystem()
{
    _MainFont->OnResetDevice();   // prepare our font for rendering
    _SystemActive = true;         // activate the system
    InitializeStateMachine();     // reset the state machine
}

void D3D9GraphicsDevice::ReSize(WindowObjects &O, AppInterface &App)
{
    _MainFont->OnLostDevice();
#ifndef SKIP_APP_INTERFACE
    App.ResetAllDevices(O);
#endif
    ResetSystem(O.GetWindowManager().CastWindows());
}

void D3D9GraphicsDevice::ReSizeFullScreen(WindowObjects &O, AppInterface &App)
{
#ifndef SKIP_APP_INTERFACE
    App.ResetAllDevices(O);
#endif
    Init3D(O, _Parameters);
    _RestoreDevices = true;
}

void D3D9GraphicsDevice::ResetSystem(WindowsWindowManager &WM)
{
    D3DPRESENT_PARAMETERS d3dpp;
    LoadPresentParameters(d3dpp, WM);

    HRESULT CoopLevel = _Device->TestCooperativeLevel();    //test to see if we can reset at this time
    if(CoopLevel == D3DERR_DEVICENOTRESET || CoopLevel == D3D_OK)    //if we can,
    {
        if(_Device->Reset(&d3dpp) == D3D_OK)    //reset,
        {
            RestoreSystem();        //if it worked restore the system
            _RestoreDevices = true;
        }
    }
}

void D3D9GraphicsDevice::DrawString(const String &Text, const Rectangle2f &Rect, RGBColor c)
{
    RECT CurRect = Rect.ToRect();
    HRESULT CoopLevel = _Device->TestCooperativeLevel();
    if(CoopLevel == D3D_OK)
    {
        _MainFont->DrawText( NULL, Text.CString(), Text.Length(), &CurRect, DT_WORDBREAK | DT_NOCLIP, D3DXCOLOR(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f, 1.0f ));
    }
}

void D3D9GraphicsDevice::DrawStringFloat(const String &Text, float x, float y, RGBColor c)
{
    RECT Rect;
    SetRect( &Rect, int(x), int(y), 0, 0 );
    HRESULT CoopLevel = _Device->TestCooperativeLevel();
    if(CoopLevel == D3D_OK)
    {
        _MainFont->DrawText( NULL, Text.CString(), Text.Length(), &Rect, DT_NOCLIP, D3DXCOLOR(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f, 1.0f ));
    }
}

void D3D9GraphicsDevice::LoadMatrix(MatrixController &MC)
{
    D3DXMATRIXA16 matWorld, matPerspective, MatView;
    matWorld = D3DXMATRIX(MC.World);
    MatView = D3DXMATRIX(MC.View);
    matPerspective = D3DXMATRIX(MC.Perspective);        //convert our matrices into D3DXMATRIX matrices
    _Device->SetTransform(D3DTS_WORLD, &matWorld);
    _Device->SetTransform(D3DTS_VIEW, &MatView);
    _Device->SetTransform(D3DTS_PROJECTION, &matPerspective);    //load the world, view, and perspective matrices
}

void D3D9GraphicsDevice::SetWireframe()
{
    _Device->SetRenderState(D3DRS_FILLMODE,D3DFILL_WIREFRAME);
}

void D3D9GraphicsDevice::DisableWireframe()
{
    _Device->SetRenderState(D3DRS_FILLMODE,D3DFILL_SOLID);
}

void D3D9GraphicsDevice::ToggleWireframe()
{
    DWORD CurState;
    _Device->GetRenderState(D3DRS_FILLMODE, &CurState);
    if(CurState == D3DFILL_SOLID)
    {
        _Device->SetRenderState(D3DRS_FILLMODE, D3DFILL_WIREFRAME);
    }
    else
    {
        _Device->SetRenderState(D3DRS_FILLMODE, D3DFILL_SOLID);
    }
}

void D3D9GraphicsDevice::ClearTexture(UINT Index)
{
    _Device->SetTexture(Index, NULL);
}

void D3D9GraphicsDevice::Render(const Mesh &M)
{
    _Device->SetFVF(D3DMeshFVF);
    _Device->DrawIndexedPrimitiveUP(D3DPT_TRIANGLELIST, 0, M.VertexCount(), M.FaceCount(), M.Indices(), D3DFMT_INDEX32, M.Vertices(), sizeof(MeshVertex));
}

void D3D9GraphicsDevice::RenderLineStrip(const Mesh &M)
{
    _Device->SetFVF(D3DMeshFVF);
    _Device->DrawPrimitiveUP(D3DPT_LINESTRIP, M.VertexCount()-1, M.Vertices(), sizeof(MeshVertex));
}

LPDIRECT3DDEVICE9 D3D9GraphicsDevice::GetDevice()
{
    return _Device;
}

BaseTexture* D3D9GraphicsDevice::CreateTexture()
{
    D3D9Texture *Tex = new D3D9Texture;
    Tex->Associate(*this);
    return Tex;
}

void D3D9GraphicsDevice::SetZState(ZState NewState)
{
    if(NewState == ZStateAlways)
        _Device->SetRenderState(D3DRS_ZFUNC,D3DCMP_ALWAYS);
    else if(NewState == ZStateLessEqual)
        _Device->SetRenderState(D3DRS_ZFUNC,D3DCMP_LESSEQUAL);
}

void D3D9GraphicsDevice::CaptureDesktop(WindowManager &WM, Bitmap &B)
{
    //
    // see http://www.codeproject.com/KB/system/snapshot.aspx for an alternate way
    //

    RECT DesktopRect;
    GetClientRect(GetDesktopWindow(), &DesktopRect);

    if(_DesktopSurface == NULL)
    {
        //D3DFMT_R8G8B8 is not supported for offscreen plain surfaces
        HRESULT hr = _Device->CreateOffscreenPlainSurface(DesktopRect.right, DesktopRect.bottom, D3DFMT_X8R8G8B8, D3DPOOL_SYSTEMMEM, &_DesktopSurface, NULL);
        PersistentAssert(!FAILED(hr) && _DesktopSurface != NULL, "CreateOffscreenPlainSurface failed");
    }

    //HRESULT hr = _Device->GetFrontBufferData(0, _DesktopSurface);
    //PersistentAssert(!FAILED(hr), "GetFrontBufferData failed");

    HDC DesktopSurfaceDC;
    _DesktopSurface->GetDC(&DesktopSurfaceDC);
    BitBlt(DesktopSurfaceDC, 0, 0, DesktopRect.right, DesktopRect.bottom, GetDC(GetDesktopWindow()), 0, 0, SRCCOPY);
    _DesktopSurface->ReleaseDC(DesktopSurfaceDC);

    B.LoadFromSurface(_DesktopSurface);
}

void D3D9GraphicsDevice::CaptureScreen(WindowManager &WM, Bitmap &Bmp)
{
    const bool CaptureFrontBuffer = true;
    if(CaptureFrontBuffer)
    {
        RECT DesktopRect;
        GetWindowRect(GetDesktopWindow(), &DesktopRect);

        LPDIRECT3DSURFACE9 Surface;
        HRESULT hr = _Device->CreateOffscreenPlainSurface(DesktopRect.right - DesktopRect.left, DesktopRect.bottom - DesktopRect.top, D3DFMT_A8R8G8B8, D3DPOOL_SYSTEMMEM, &Surface, NULL);
        PersistentAssert(!FAILED(hr), "CreateOffscreenPlainSurface failed");
        PersistentAssert(Surface != NULL, "Invalid surface handle from CreateOffscreenPlainSurface");

        hr = _Device->GetFrontBufferData(0, Surface);
        PersistentAssert(!FAILED(hr), "GetFrontBufferData failed");

        Bitmap DesktopBmp;
        DesktopBmp.LoadFromSurface(Surface);
        DesktopBmp.FlipVertical();
        Surface->Release();
        
        RECT WindowCoord;
        GetClientRect(WM.CastWindows().GetHWND(), &WindowCoord);
        MapWindowPoints(WM.CastWindows().GetHWND(), GetDesktopWindow(), (LPPOINT)&WindowCoord, 2);

        UINT Width = WindowCoord.right - WindowCoord.left;
        UINT Height = WindowCoord.bottom - WindowCoord.top;

        Bmp.Allocate(Width, Height);
        DesktopBmp.BltTo(Bmp, 0, 0, WindowCoord.left, WindowCoord.top, Width, Height);
        Bmp.FlipBlueAndRed();
        Bmp.FlipVertical();
    }
    else
    {
        LPDIRECT3DSURFACE9 RenderTarget = NULL;
        D3DAlwaysValidate(_Device->GetRenderTarget(0, &RenderTarget), "GetRenderTarget");

        D3DSURFACE_DESC RenderTargetDesc;
        D3DAlwaysValidate(RenderTarget->GetDesc(&RenderTargetDesc), "GetDesc");

        bool WindowSurfaceValid = (_WindowSurface != NULL);
        if(WindowSurfaceValid)
        {
            D3DSURFACE_DESC WindowSurfaceDesc;
            D3DAlwaysValidate(_WindowSurface->GetDesc(&WindowSurfaceDesc), "GetDesc");
            if(WindowSurfaceDesc.Width != RenderTargetDesc.Width || WindowSurfaceDesc.Height != RenderTargetDesc.Height)
            {
                WindowSurfaceValid = false;
            }
        }
        if(!WindowSurfaceValid)
        {
            if(_WindowSurface != NULL)
            {
                _WindowSurface->Release();
            }
            D3DAlwaysValidate(_Device->CreateOffscreenPlainSurface(RenderTargetDesc.Width, RenderTargetDesc.Height, D3DFMT_A8R8G8B8, D3DPOOL_SYSTEMMEM, &_WindowSurface, NULL), "CreateOffscreenPlainSurface");
        }

        D3DAlwaysValidate(_Device->GetRenderTargetData(RenderTarget, _WindowSurface), "GetRenderTargetData");
        RenderTarget->Release();

        Bmp.LoadFromSurface(_WindowSurface);

        /*LPDIRECT3DSURFACE9 BackBufferSurface = NULL;
        D3DSURFACE_DESC Desc;
        D3DAlwaysValidate(_Device->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &BackBufferSurface), "GetBackBuffer");
        D3DAlwaysValidate(BackBufferSurface->GetDesc(&Desc), "GetDesc");
        
        D3DLOCKED_RECT Rect;
        D3DAlwaysValidate(BackBufferSurface->LockRect(&Rect, NULL, NULL), "LockRect");

        RECT WindowCoord;
        WindowCoord.top = 0;
        WindowCoord.left = 0;
        WindowCoord.bottom = Desc.Height;
        WindowCoord.right = Desc.Width;

        B.Allocate(Desc.Width, Desc.Height);
        RGBColor *Bits = (RGBColor*)Rect.pBits;
        
        UINT Pitch = Rect.Pitch / 4;
        for(int i2 = WindowCoord.top; i2 < WindowCoord.bottom; i2++)
        {
            for(int i = WindowCoord.left; i < WindowCoord.right; i++)
            {
                {
                    B[B.Height() - (i2 - WindowCoord.top) - 1][i - WindowCoord.left] = Bits[i2 * Pitch + i];
                }
            }
        }
        BackBufferSurface->UnlockRect();
        BackBufferSurface->Release();*/
    }
}

void D3D9GraphicsDevice::RenderSolidWireframe(BaseMesh *BM)
{
    SetWireframe();
    BM->SetColor(RGBColor::Black);
    BM->Render();

    DisableWireframe();
    BM->ColorNormals();

    D3DXMATRIXA16 matPerspective, matPerspectiveSave, matTranslate;
    _Device->GetTransform(D3DTS_PROJECTION, &matPerspective);
    matPerspectiveSave = matPerspective;
    D3DXMatrixTranslation(&matTranslate, 0.0f, 0.0f, -0.002f);
    D3DXMatrixMultiply(&matPerspective, &matTranslate, &matPerspective);
    _Device->SetTransform(D3DTS_PROJECTION, &matPerspective);
    BM->Render();
    _Device->SetTransform(D3DTS_PROJECTION, &matPerspectiveSave);
}

void D3D9GraphicsDevice::LoadViewport(Viewport &V)
{
    D3DVIEWPORT9 ThisViewport = {V.X, V.Y, V.Width, V.Height, V.MinZ, V.MaxZ};
    _Device->SetViewport(&ThisViewport);
}

#endif