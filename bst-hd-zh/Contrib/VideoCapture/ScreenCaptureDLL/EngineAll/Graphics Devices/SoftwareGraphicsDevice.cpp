/*
SoftwareGraphicsDevice.cpp
Written by Matthew Fisher

a software instance of GraphicsDevice.  Contains no new functionality over GraphicsDevice.
Doesn't support several things, like textures or lighting.
*/
#include "..\\..\\Main.h"
#include "SoftwareGraphicsDevice.h"

SoftwareGraphicsDevice::SoftwareGraphicsDevice()
{
    _Type = GD_Software;
    hDC = 0;
    hWnd = 0;
    _RestoreDevices = false;
    _SystemActive = true;
}

SoftwareGraphicsDevice::~SoftwareGraphicsDevice()
{
    FreeMemory();
}

void SoftwareGraphicsDevice::FreeMemory()
{
    if(hDC && hWnd)
    {
        ReleaseDC(hWnd, hDC);
        hDC=0;
    }
    Bmp.FreeMemory();
    Z.FreeMemory();
}

void SoftwareGraphicsDevice::Init3D(WindowObjects &O, const GraphicsDeviceParameters &Parameters)
{
    //the beginning of this function is identical to the OpenGL version
    FreeMemory();

    unsigned int PixelFormat;
    hWnd = O.GetWindowManager().CastWindows().GetHWND();
    _FullScreen = O.GetWindowManager().GetFullScreen();
    UINT Width = O.GetWindowManager().GetWidth();
    UINT Height = O.GetWindowManager().GetHeight();

    if(_FullScreen)
    {
        DEVMODE dmScreenSettings;
        memset(&dmScreenSettings, 0, sizeof(dmScreenSettings));
        dmScreenSettings.dmSize=sizeof(dmScreenSettings);
        dmScreenSettings.dmPelsWidth    = Width;
        dmScreenSettings.dmPelsHeight    = Height;
        dmScreenSettings.dmBitsPerPel    = 32;
        dmScreenSettings.dmFields=DM_BITSPERPEL|DM_PELSWIDTH|DM_PELSHEIGHT;

        LONG Result = ChangeDisplaySettings(&dmScreenSettings,CDS_FULLSCREEN);
        Assert(Result == DISP_CHANGE_SUCCESSFUL, "Failed to change display resolution");
    }

    static PIXELFORMATDESCRIPTOR pfd=  // pfd Tells Windows How We Want Things To Be
    {
        sizeof(PIXELFORMATDESCRIPTOR), // Size Of This Pixel Format Descriptor
        1,                             // Version Number
        PFD_DRAW_TO_WINDOW |           // Format Must Support Window
        PFD_SUPPORT_OPENGL |           // Format Must Support OpenGL
        PFD_DOUBLEBUFFER,              // Must Support Double Buffering
        PFD_TYPE_RGBA,                 // Request An RGBA Format
        32,                            // Select Our Color Depth
        0, 0, 0, 0, 0, 0,              // Color Bits Ignored
        0,                             // No Alpha Buffer
        0,                             // Shift Bit Ignored
        0,                             // No Accumulation Buffer
        0, 0, 0, 0,                    // Accumulation Bits Ignored
        32,                            // 16Bit Z-Buffer (Depth Buffer)  
        0,                             // No Stencil Buffer
        0,                             // No Auxiliary Buffer
        PFD_MAIN_PLANE,                // Main Drawing Layer
        0,                             // Reserved
        0, 0, 0                        // Layer Masks Ignored
    };
    
    hDC = GetDC(O.GetWindowManager().CastWindows().GetHWND());
    PixelFormat = ChoosePixelFormat(hDC, &pfd);
    SetPixelFormat(hDC, PixelFormat, &pfd);
    
    Bmp.Allocate(Width, Height);    
    Bmp.Clear(_Parameters.ScreenColor);
    
    Z.Allocate(Width, Height);
    Z.Clear(FLT_MAX);
    Viewport = Matrix4::Scaling(Vec3f(float(Width), float(Height), 1.0f));

    Info.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    Info.bmiHeader.biPlanes = 1;
    Info.bmiHeader.biBitCount = 32;
    Info.bmiHeader.biCompression = BI_RGB;
    Info.bmiHeader.biSizeImage = 0;
    Info.bmiHeader.biXPelsPerMeter = 3800;
    Info.bmiHeader.biYPelsPerMeter = 3800;
    Info.bmiHeader.biClrUsed = 0;
    Info.bmiHeader.biClrImportant = 0;
}

void SoftwareGraphicsDevice::StartRender(WindowObjects &O)
{
    Bmp.Clear(_Parameters.ScreenColor);
    Z.Clear(FLT_MAX);
}

void SoftwareGraphicsDevice::FinishRender(WindowObjects &O, AppInterface &App)
{
    UINT Width = O.GetWindowManager().GetWidth();
    UINT Height = O.GetWindowManager().GetHeight();
    Info.bmiHeader.biWidth = Width;
    Info.bmiHeader.biHeight = Height;

    //load Bmp into our window (represented by hDC)
    SetDIBitsToDevice(hDC, 0, 0, Width, Height, 0, 0, 0, Height, &Bmp[0][0], &Info, DIB_RGB_COLORS);
}

void SoftwareGraphicsDevice::ReSize(WindowObjects &O, AppInterface &App)
{
    UINT Width = O.GetWindowManager().GetWidth();
    UINT Height = O.GetWindowManager().GetHeight();
    Bmp.Allocate(Width, Height);
    Bmp.Clear(_Parameters.ScreenColor);
    Z.Allocate(Width, Height);    //reallocate Z buffer and screen space
    Z.Clear(10000.0f);
    Viewport = Matrix4::Scaling(Vec3f(float(Width), float(Height), 1.0f));    //reset the viewport Matrix4
    _RestoreDevices = true;
}

void SoftwareGraphicsDevice::ReSizeFullScreen(WindowObjects &O, AppInterface &App)
{
    if(!O.GetWindowManager().GetFullScreen())
    {
        ChangeDisplaySettings(NULL,0);    //restore the display from full screen mode
    }

    GraphicsDeviceParameters Parameters;
    Init3D(O, Parameters);

    _RestoreDevices = true;
}

void SoftwareGraphicsDevice::DrawStringFloat(const String &Text, float x, float y, RGBColor c)
{
    //text is not supported in software
}

void SoftwareGraphicsDevice::LoadMatrix(MatrixController &MC)
{
    Total = MC.TotalMatrix();
}

void SoftwareGraphicsDevice::SetWireframe()
{
    _Wireframe = true;
}

void SoftwareGraphicsDevice::DisableWireframe()
{
    _Wireframe = false;
}

void SoftwareGraphicsDevice::ToggleWireframe()
{
    _Wireframe = !_Wireframe;
}

void SoftwareGraphicsDevice::ClearTexture(UINT Index)
{

}

void SoftwareGraphicsDevice::RenderLineStrip(const Mesh &M)
{

}

void SoftwareGraphicsDevice::CaptureScreen(WindowManager &WM, Bitmap &B)
{
    int i, i2, Width, Height;
    RECT WindowCoord;
    RGBColor Color;

    GetClientRect(WM.CastWindows().GetHWND(), &WindowCoord);
    MapWindowPoints(WM.CastWindows().GetHWND(), GetDesktopWindow(), (LPPOINT)&WindowCoord, 2);

    Width = WindowCoord.right - WindowCoord.left;
    Height = WindowCoord.bottom - WindowCoord.top;    //get the Width/Height

    B.Allocate(Width, Height);                //allocate space
    
    for(i=0;i<Height;i++)
    {
        for(i2=0;i2<Width;i2++)
        {
            B[Height-i-1][i2] = Bmp[i][i2];    //load the current screen into B
        }
    }
}

void SoftwareGraphicsDevice::Render(const Mesh &M)
{
    //uses AliasRender to render the provided polygon.
    MeshVertex Polygon[10];
    int i,i2,i2p1,ic=M.IndexCount(),LocalVC;
    const MeshVertex *V = M.Vertices();
    const DWORD *I = M.Indices();

    Clipper Clip;
    Clip.Init(float(Bmp.Width()-1),float(Bmp.Height()-1));    //prepare for clipping

    Matrix4 VTranslate = Matrix4::Translation(Vec3f(1.0f,1.0f,0.0f));
    Matrix4 VScale = Matrix4::Scaling(Vec3f(Bmp.Width()/2.0f,Bmp.Height()/2.0f,1.0f));
    Matrix4 Viewport = VTranslate * VScale;

    Matrix4 TotalViewport = Total * Viewport;    //get the total transform

    for(i=0;i<ic;i+=3)
    {
        LocalVC = 3;
        Polygon[0] = V[I[i+0]];
        Polygon[1] = V[I[i+1]];
        Polygon[2] = V[I[i+2]];    //load the current triangle

        //
        // Transform into screen space
        //
        Polygon[0].Pos = TotalViewport.TransformPoint(Polygon[0].Pos);
        Polygon[1].Pos = TotalViewport.TransformPoint(Polygon[1].Pos);
        Polygon[2].Pos = TotalViewport.TransformPoint(Polygon[2].Pos);
        
        Clip.Clip(Polygon, LocalVC);    //clip

        if(_Wireframe)
        {
            for(i2=0;i2<LocalVC;i2++)
            {
                i2p1 = i2 + 1;
                if(i2p1 == LocalVC) i2p1 = 0;

                //if we're in _Wireframe, draw lines representing the polygon
                if(Polygon[i2].Pos.z >= 0.0f && Polygon[i2].Pos.z <= 1.0f && Polygon[i2p1].Pos.z >= 0.0f && Polygon[i2p1].Pos.z <= 1.0f)
                    PR.DrawLine(Bmp, int(Polygon[i2].Pos.x), int(Polygon[i2].Pos.y), int(Polygon[i2p1].Pos.x), int(Polygon[i2p1].Pos.y), Polygon[i2].Color);
            }
        }
        else
        {
            //if we're not in _Wireframe, draw the polygon directly
            PR.DrawPolygon(Bmp, Z, Polygon, LocalVC);
        }
    }
}

BaseTexture* SoftwareGraphicsDevice::CreateTexture()
{
    return NULL;    //returns an empty texture; software texture rendering isn't supported.
}

void SoftwareGraphicsDevice::SetZState(ZState NewState)
{
    
}
