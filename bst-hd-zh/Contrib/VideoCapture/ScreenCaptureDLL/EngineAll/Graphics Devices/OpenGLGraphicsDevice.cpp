/*
OpenGLGraphicsDevice.cpp
Written by Matthew Fisher
Certain parts are borrowed from the NeHe tutorials, http://nehe.gamedev.net/

an OpenGL instance of GraphicsDevice.  Contains no new functionality over GraphicsDevice.
*/
#include "..\\..\\Main.h"
#include "OpenGLGraphicsDevice.h"

#ifdef USE_OPENGL

OpenGLGraphicsDevice::OpenGLGraphicsDevice()
{
    _Type = GD_OpenGL;
    _DeviceContext = 0;
    _RenderContext = 0;
    _Window = 0;
    _RestoreDevices = false;
    _BaseInitalized = false;
}

OpenGLGraphicsDevice::~OpenGLGraphicsDevice()
{
    FreeMemory();
}

void OpenGLGraphicsDevice::FreeMemory()
{
    if(_RenderContext)
    {
        wglMakeCurrent(NULL,NULL);
        wglDeleteContext(_RenderContext);
        _RenderContext = 0;
    }
    if(_DeviceContext && _Window)
    {
        ReleaseDC(_Window, _DeviceContext);
        _DeviceContext = 0;
    }
    if(_BaseInitalized)
    {
        glDeleteLists(_FontBase, 96);
        _BaseInitalized = false;
    }
}

void OpenGLGraphicsDevice::CaptureScreen(WindowManager &WM, Bitmap &B)
{
    int i, i2, Width, Height;
    RECT WindowCoord;
    RGBColor Color;
    Bitmap Temp;

    HWND Hwnd = WM.CastWindows().GetHWND();

    GetClientRect(Hwnd, &WindowCoord);
    MapWindowPoints(Hwnd, GetDesktopWindow(), (LPPOINT)&WindowCoord, 2);

    Width = WindowCoord.right - WindowCoord.left;
    Height = WindowCoord.bottom - WindowCoord.top;    //get the screen width and height

    Temp.Allocate(Width, Height);
    B.Allocate(Width, Height);        //allocate space in the bitmaps
    glReadBuffer(GL_FRONT);           //read from the front buffer
    glReadPixels(0, 0, Width, Height, GL_RGBA, GL_UNSIGNED_BYTE, &Temp[0][0]);    //load the data into the Temp bitmap
    
    for(i=0;i<Height;i++)
    {
        for(i2=0;i2<Width;i2++)
        {
            Color = Temp[Height-1-i][i2];    //get the color,
            B[i][i2] = RGBColor(Color.b, Color.g, Color.r, Color.a);    //flip it from BGRA to RGBA
        }
    }
}

void OpenGLGraphicsDevice::InitializeStateMachine(WindowManager &WM)
{
    //default state machine options
    glShadeModel(GL_SMOOTH);
    glClearColor(1.0f, 1.0f, 1.0f, 0.5f);
    glClearDepth(1.0f);
    glEnable(GL_DEPTH_TEST);
    glDepthFunc(GL_LEQUAL);
    glHint(GL_PERSPECTIVE_CORRECTION_HINT, GL_NICEST);

    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();

    glViewport(0,0,WM.GetWidth(),WM.GetHeight());
}

void OpenGLGraphicsDevice::SetScreenColor(RGBColor _ScreenColor)
{
    glClearColor(_ScreenColor.r / 255.0f, _ScreenColor.g / 255.0f, _ScreenColor.b / 255.0f, 0.5f);
}

void OpenGLGraphicsDevice::Init3D(WindowObjects &O, const GraphicsDeviceParameters &Parameters)
{
    FreeMemory();

    _Parameters = Parameters;
    if(_Parameters.MainFontName.Length() == 0)
    {
        _Parameters.MainFontName = "Georgia";
    }

    unsigned int PixelFormat;
    _Window = O.GetWindowManager().CastWindows().GetHWND();
    _FullScreen = O.GetWindowManager().GetFullScreen();

    if(_FullScreen)    //if we're full screen we need to change the screen resolution
    {
        DEVMODE dmScreenSettings;
        memset(&dmScreenSettings, 0, sizeof(dmScreenSettings));
        dmScreenSettings.dmSize = sizeof(dmScreenSettings);
        dmScreenSettings.dmPelsWidth  = O.GetWindowManager().GetWidth();
        dmScreenSettings.dmPelsHeight = O.GetWindowManager().GetHeight();
        dmScreenSettings.dmBitsPerPel = 32;
        dmScreenSettings.dmFields = DM_BITSPERPEL | DM_PELSWIDTH | DM_PELSHEIGHT;

        LONG Result = ChangeDisplaySettings(&dmScreenSettings,CDS_FULLSCREEN);
        Assert(Result == DISP_CHANGE_SUCCESSFUL, "Failed to change display resolution");
    }

    static    PIXELFORMATDESCRIPTOR pfd=            // pfd Tells Windows How We Want Things To Be
    {
        sizeof(PIXELFORMATDESCRIPTOR),              // Size Of This Pixel Format Descriptor
        1,                                          // Version Number
        PFD_DRAW_TO_WINDOW |                        // Format Must Support Window
        PFD_SUPPORT_OPENGL |                        // Format Must Support OpenGL
        PFD_DOUBLEBUFFER,                           // Must Support Double Buffering
        PFD_TYPE_RGBA,                              // Request An RGBA Format
        32,                                         // Select Our Color Depth
        0, 0, 0, 0, 0, 0,                           // Color Bits Ignored
        0,                                          // No Alpha Buffer
        0,                                          // Shift Bit Ignored
        0,                                          // No Accumulation Buffer
        0, 0, 0, 0,                                 // Accumulation Bits Ignored
        32,                                         // 16Bit Z-Buffer (Depth Buffer)  
        0,                                          // No Stencil Buffer
        0,                                          // No Auxiliary Buffer
        PFD_MAIN_PLANE,                             // Main Drawing Layer
        0,                                          // Reserved
        0, 0, 0                                     // Layer Masks Ignored
    };
    
    _DeviceContext = GetDC(O.GetWindowManager().CastWindows().GetHWND());
    PixelFormat = ChoosePixelFormat(_DeviceContext, &pfd);
    SetPixelFormat(_DeviceContext, PixelFormat, &pfd);
    _RenderContext = wglCreateContext(_DeviceContext);
    wglMakeCurrent(_DeviceContext, _RenderContext);

    //
    // Create the font for text rendering
    //

    HFONT    font;                                   // Windows Font ID
    HFONT    oldfont;                                // Used For Good House Keeping

    _FontBase = glGenLists(96);                           // Storage For 96 Characters

    font = CreateFont(  -16,                         // Height Of Font
                        0,                           // Width Of Font
                        0,                           // Angle Of Escapement
                        0,                           // Orientation Angle
                        FW_BOLD,                     // Font Weight
                        FALSE,                       // Italic
                        FALSE,                       // Underline
                        FALSE,                       // Strikeout
                        ANSI_CHARSET,                // Character Set Identifier
                        OUT_TT_PRECIS,               // Output Precision
                        CLIP_DEFAULT_PRECIS,         // Clipping Precision
                        ANTIALIASED_QUALITY,         // Output Quality
                        FF_DONTCARE|DEFAULT_PITCH,   // Family And Pitch
                        _Parameters.MainFontName.CString());    // Font Name

    _FontScreenWidth = O.GetWindowManager().GetWidth();
    _FontScreenHeight = O.GetWindowManager().GetHeight();

    oldfont = (HFONT)SelectObject(_DeviceContext, font);  // Selects The Font We Want
    wglUseFontBitmaps(_DeviceContext, 32, 96, _FontBase); // Builds 96 Characters Starting At Character 32
    _BaseInitalized = true;
    SelectObject(_DeviceContext, oldfont);                // Selects The Font We Want
    DeleteObject(font);                                   // Delete The Font

    InitializeStateMachine(O.GetWindowManager());
}

void OpenGLGraphicsDevice::StartRender(WindowObjects &O)
{
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
}

void OpenGLGraphicsDevice::FinishRender(WindowObjects &O, AppInterface &App)
{
    SwapBuffers(_DeviceContext);
}

void OpenGLGraphicsDevice::RestoreSystem()
{
    _SystemActive = true;
}

void OpenGLGraphicsDevice::ReSize(WindowObjects &O, AppInterface &App)
{
    glViewport(0,0,O.GetWindowManager().GetWidth(),O.GetWindowManager().GetHeight());
    _FontScreenWidth = O.GetWindowManager().GetWidth();
    _FontScreenHeight = O.GetWindowManager().GetHeight();
    _RestoreDevices = true;
}

void OpenGLGraphicsDevice::ReSizeFullScreen(WindowObjects &O, AppInterface &App)
{
    if(!O.GetWindowManager().GetFullScreen()) ChangeDisplaySettings(NULL,0);
    Init3D(O, _Parameters);
    _RestoreDevices = true;
}

void OpenGLGraphicsDevice::DrawStringFloat(const String &Text, float x, float y, RGBColor c)
{
    glMatrixMode(GL_MODELVIEW);    //see NeHe for a full description
    glLoadIdentity();
    glTranslatef(0.0f, 0.0f, -1.0f);
    glColor3f(c.r / 255.0f,c.g / 255.0f,c.b / 255.0f);
    glRasterPos2f( float(Math::LinearMap(0.0f, float(_FontScreenWidth), -1.0f, 1.0f, x)),
                   float(Math::LinearMap(0.0f, float(_FontScreenHeight), 1.0f, -1.0f, y + 20.0f)));

    glPushAttrib(GL_LIST_BIT);
    glListBase(_FontBase - 32);
    glCallLists(int(Text.Length()), GL_UNSIGNED_BYTE, Text.CString());
    glPopAttrib();

    glLoadIdentity();
    glLoadMatrixf(&(_SavedMatrix[0][0]));
}

void OpenGLGraphicsDevice::LoadMatrix(MatrixController &MC)
{
    Matrix4 Total = MC.TotalMatrix();
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    glLoadMatrixf(&(Total[0][0]));
    _SavedMatrix = Total;
}

void OpenGLGraphicsDevice::SetWireframe()
{
    glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
}

void OpenGLGraphicsDevice::DisableWireframe()
{
    glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
}

void OpenGLGraphicsDevice::ToggleWireframe()
{
    SignalError("Not implemented");
}

void OpenGLGraphicsDevice::ClearTexture(UINT Index)
{

}

void OpenGLGraphicsDevice::Render(const Mesh &M)
{
    glBegin(GL_TRIANGLES);

    UINT VertexCount = M.VertexCount(), IndexCount = M.IndexCount();
    const MeshVertex *Vertices = M.Vertices();
    const DWORD *Indices = M.Indices();

    for(UINT IndexIndex = 0; IndexIndex < IndexCount; IndexIndex++)
    {
        const MeshVertex &Vtx = Vertices[Indices[IndexIndex]];
        glColor3f(Vtx.Color.r / 255.0f, Vtx.Color.g / 255.0f, Vtx.Color.b / 255.0f);
        glTexCoord2f(Vtx.TexCoord.x, Vtx.TexCoord.y);
        glVertex3f(Vtx.Pos.x, Vtx.Pos.y, Vtx.Pos.z);
    }

    glEnd();
}

BaseTexture* OpenGLGraphicsDevice::CreateTexture()
{
#ifdef USE_OPENGL_TEXTURES
    BaseTexture *Tex = new OpenGLTexture;    //return an OpenGLTexture
    return Tex;
#else
    return NULL;
#endif
}

void OpenGLGraphicsDevice::SetZState(ZState NewState)
{
    if(NewState == ZStateAlways)
    {
        glDepthFunc(GL_ALWAYS);
    }
    else if(NewState == ZStateLessEqual)
    {
        glDepthFunc(GL_LEQUAL);
    }
}

void OpenGLGraphicsDevice::RenderLineStrip(const Mesh &M)
{
    //not implemented yet
}

#endif