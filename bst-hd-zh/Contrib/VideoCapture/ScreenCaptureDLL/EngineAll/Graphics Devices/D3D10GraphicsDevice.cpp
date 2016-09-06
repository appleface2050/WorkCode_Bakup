/*
D3DGraphicsDevice.cpp
Written by Matthew Fisher

a D3D instance of GraphicsDevice.  The only new functionality is GetDevice() which returns
the D3DDevice associated with this graphics device.
*/
#include "..\\..\\Main.h"
#include "D3D10GraphicsDevice.h"

#ifdef USE_D3D10

D3D10GraphicsDevice::D3D10GraphicsDevice()
{
    _Type = GD_D3D10;
    _Device = NULL;
    _SwapChain = NULL;
    _RenderTargetView = NULL;
    _Font = NULL;
    _DepthStencil = NULL;
    _DepthStencilView = NULL;
    _RasterizerState = NULL;
    _DepthStencilState = NULL;
    _BlendState = NULL;
    _MatrixVariable = NULL;
    _FixedFunctionEffect = NULL;
    _BackBuffer = NULL;
    _BackBufferStaging = NULL;
}

D3D10GraphicsDevice::~D3D10GraphicsDevice()
{
    FreeMemory();
}

void D3D10GraphicsDevice::FreeMemory()
{
    if(_Device)
    {
        _Device->ClearState();
        _Device->Release();
        _Device = NULL;
    }
    _DynamicMesh.FreeMemory();
    if(_SwapChain)
    {
        BOOL FullScreen;
        IDXGIOutput *Output;
        _SwapChain->GetFullscreenState(&FullScreen, &Output);
        if(FullScreen)
        {
            _SwapChain->SetFullscreenState(FALSE, NULL);
        }
        _SwapChain->Release();
        _SwapChain = NULL;
    }
    FreeCoreSurfaces();
    if(_RasterizerState)
    {
        _RasterizerState->Release();
        _RasterizerState = NULL;
    }
    if(_BlendState)
    {
        _BlendState->Release();
        _BlendState = NULL;
    }
    if(_DepthStencilState)
    {
        _DepthStencilState->Release();
        _DepthStencilState = NULL;
    }
    if(_Font)
    {
        _Font->Release();
        _Font = NULL;
    }
    _MatrixVariable = NULL;
    _FixedFunctionEffect = NULL;
}

void D3D10GraphicsDevice::FreeCoreSurfaces()
{
    if(_Device)
    {
        ID3D10RenderTargetView *NullRenderTargetView = NULL;
        _Device->OMSetRenderTargets(1, &NullRenderTargetView, NULL);
    }
    if(_RenderTargetView)
    {
        _RenderTargetView->Release();
        _RenderTargetView = NULL;
    }
    if(_DepthStencil)
    {
        _DepthStencil->Release();
        _DepthStencil = NULL;
    }
    if(_DepthStencilView)
    {
        _DepthStencilView->Release();
        _DepthStencilView = NULL;
    }
    if(_BackBuffer)
    {
        _BackBuffer->Release();
        _BackBuffer = NULL;
    }
    if(_BackBufferStaging)
    {
        _BackBufferStaging->Release();
        _BackBufferStaging = NULL;
    }
    
    _RestoreDevices = true;
}

void D3D10GraphicsDevice::PrepareFullScreenReSize(WindowObjects &O, AppInterface &App)
{
    FreeCoreSurfaces();
}

void D3D10GraphicsDevice::AllocateSwapChain(WindowManager &WM)
{
    HRESULT hr = S_OK;

    DXGI_SWAP_CHAIN_DESC SwapChainDesc;
    ZeroMemory(&SwapChainDesc, sizeof(SwapChainDesc));
    SwapChainDesc.BufferCount = 1;
    SwapChainDesc.BufferDesc.Width = WM.GetWidth();
    SwapChainDesc.BufferDesc.Height = WM.GetHeight();
    SwapChainDesc.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    SwapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
    SwapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
    SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    SwapChainDesc.OutputWindow = WM.GetHWND();
    SwapChainDesc.SampleDesc.Count = 1;
    SwapChainDesc.SampleDesc.Quality = 0;

    _FullScreen = WM.GetFullScreen();
    if(_FullScreen)
    {
        SwapChainDesc.Windowed = TRUE;
    }
    else
    {
        SwapChainDesc.Windowed = TRUE;
    }

    LONG Long = GetWindowLong(WM.GetHWND(), GWL_STYLE);
    IDXGIFactory *Factory = NULL;
    hr = CreateDXGIFactory(__uuidof(IDXGIFactory), (void**)(&Factory) );
    PersistentAssert(SUCCEEDED(hr), "CreateDXGIFactory failed");

    
    hr =  Factory->CreateSwapChain(_Device, &SwapChainDesc, &_SwapChain);
    PersistentAssert(SUCCEEDED(hr), "CreateSwapChain failed");

    Factory->MakeWindowAssociation(WM.GetHWND(), DXGI_MWA_VALID);
    Factory->Release();
}

void D3D10GraphicsDevice::AllocateCoreSurfaces(WindowManager &WM)
{
    HRESULT hr = S_OK;

    hr = _SwapChain->GetBuffer( 0, __uuidof(ID3D10Texture2D), (void**)&_BackBuffer );
    PersistentAssert(SUCCEEDED(hr), "_SwapChain->GetBuffer failed");

    hr = _Device->CreateRenderTargetView( _BackBuffer, NULL, &_RenderTargetView );
    PersistentAssert(SUCCEEDED(hr), "_Device->CreateRenderTargetView failed");

    // Create depth stencil texture
    D3D10_TEXTURE2D_DESC DepthStencilDesc;
    DepthStencilDesc.Width = Math::Max(WM.GetWidth(), 8);
    DepthStencilDesc.Height = Math::Max(WM.GetHeight(), 8);
    DepthStencilDesc.MipLevels = 1;
    DepthStencilDesc.ArraySize = 1;
    DepthStencilDesc.Format = DXGI_FORMAT_D32_FLOAT;
    DepthStencilDesc.SampleDesc.Count = 1;
    DepthStencilDesc.SampleDesc.Quality = 0;
    DepthStencilDesc.Usage = D3D10_USAGE_DEFAULT;
    DepthStencilDesc.BindFlags = D3D10_BIND_DEPTH_STENCIL;
    DepthStencilDesc.CPUAccessFlags = 0;
    DepthStencilDesc.MiscFlags = 0;
    hr = _Device->CreateTexture2D( &DepthStencilDesc, NULL, &_DepthStencil );
    PersistentAssert(SUCCEEDED(hr), "_Device->CreateTexture2D failed");

    // Create the depth stencil view
    D3D10_DEPTH_STENCIL_VIEW_DESC DepthStencilViewDesc;
    DepthStencilViewDesc.Format = DepthStencilDesc.Format;
    DepthStencilViewDesc.ViewDimension = D3D10_DSV_DIMENSION_TEXTURE2D;
    DepthStencilViewDesc.Texture2D.MipSlice = 0;
    hr = _Device->CreateDepthStencilView( _DepthStencil, &DepthStencilViewDesc, &_DepthStencilView );
    PersistentAssert(SUCCEEDED(hr), "_Device->CreateDepthStencilView failed");

    _Device->OMSetRenderTargets( 1, &_RenderTargetView, _DepthStencilView );

    // Setup the viewport
    D3D10_VIEWPORT vp;
    vp.Width = WM.GetWidth();
    vp.Height = WM.GetHeight();
    vp.MinDepth = 0.0f;
    vp.MaxDepth = 1.0f;
    vp.TopLeftX = 0;
    vp.TopLeftY = 0;
    _Device->RSSetViewports( 1, &vp );
}

void D3D10GraphicsDevice::LoadStateDefaults()
{
    _RasterizerDesc.FillMode = D3D10_FILL_SOLID;
    _RasterizerDesc.CullMode = D3D10_CULL_NONE;
    _RasterizerDesc.FrontCounterClockwise = FALSE;
    _RasterizerDesc.DepthBias = 0;
    _RasterizerDesc.DepthBiasClamp = 0.0f;
    _RasterizerDesc.SlopeScaledDepthBias = 0.0f;
    _RasterizerDesc.DepthClipEnable = TRUE;
    _RasterizerDesc.ScissorEnable = FALSE;
    _RasterizerDesc.MultisampleEnable = FALSE;
    _RasterizerDesc.AntialiasedLineEnable = FALSE;

    _BlendDesc.AlphaToCoverageEnable = FALSE;
    for(UINT BlendIndex = 0; BlendIndex < 8; BlendIndex++)
    {
        _BlendDesc.BlendEnable[BlendIndex] = FALSE;
        _BlendDesc.RenderTargetWriteMask[BlendIndex] = D3D10_COLOR_WRITE_ENABLE_ALL;
    }
    _BlendDesc.SrcBlend = D3D10_BLEND_ONE;
    _BlendDesc.DestBlend = D3D10_BLEND_ZERO;
    _BlendDesc.BlendOp = D3D10_BLEND_OP_ADD;
    _BlendDesc.SrcBlendAlpha = D3D10_BLEND_ONE;
    _BlendDesc.DestBlendAlpha = D3D10_BLEND_ZERO;
    _BlendDesc.BlendOpAlpha = D3D10_BLEND_OP_ADD;

    _DepthStencilDesc.DepthEnable = TRUE;
    _DepthStencilDesc.DepthWriteMask = D3D10_DEPTH_WRITE_MASK_ALL;
    _DepthStencilDesc.DepthFunc = D3D10_COMPARISON_LESS;
    _DepthStencilDesc.StencilEnable = FALSE;
    _DepthStencilDesc.StencilReadMask = D3D10_DEFAULT_STENCIL_READ_MASK;
    _DepthStencilDesc.StencilWriteMask = D3D10_DEFAULT_STENCIL_WRITE_MASK;
    
    _DepthStencilDesc.FrontFace.StencilDepthFailOp = D3D10_STENCIL_OP_KEEP;
    _DepthStencilDesc.FrontFace.StencilPassOp = D3D10_STENCIL_OP_KEEP;
    _DepthStencilDesc.FrontFace.StencilFailOp = D3D10_STENCIL_OP_KEEP;
    _DepthStencilDesc.FrontFace.StencilFunc = D3D10_COMPARISON_ALWAYS;
}

void D3D10GraphicsDevice::Init3D(WindowObjects &O)
{
    HRESULT hr = S_OK;
    WindowManager &WM = O.GetWindowManager();

    FreeMemory();
    LoadStateDefaults();

    UINT CreateDeviceFlags = 0;
#ifdef _DEBUG
    CreateDeviceFlags |= D3D10_CREATE_DEVICE_DEBUG;
#endif

    hr = D3D10CreateDevice( NULL, D3D10_DRIVER_TYPE_HARDWARE, NULL, CreateDeviceFlags, D3D10_SDK_VERSION, &_Device );
    PersistentAssert(SUCCEEDED(hr), "D3D10CreateDevice failed");

    AllocateSwapChain(WM);
    AllocateCoreSurfaces(WM);

    hr = D3DX10CreateFont( _Device, 20, 0, FW_BOLD, 1, FALSE, DEFAULT_CHARSET,
                           OUT_DEFAULT_PRECIS, DEFAULT_QUALITY, DEFAULT_PITCH | FF_DONTCARE,
                           "Arial", &_Font );
    PersistentAssert(SUCCEEDED(hr), "D3DX10CreateFont failed");

    RestoreSystem();

    _DynamicMesh.SetGD(*this);
}

void D3D10GraphicsDevice::SetBlendState(const D3D10_BLEND_DESC &NewBlendState)
{
    _BlendDesc = NewBlendState;
    ID3D10BlendState *StoredBlendState = _BlendState;
    _Device->CreateBlendState(&_BlendDesc, &_BlendState);
    _Device->OMSetBlendState(_BlendState, NULL, 0xffffffff);
    if(StoredBlendState)
    {
        StoredBlendState->Release();
    }
}

void D3D10GraphicsDevice::UpdateState()
{
    ID3D10RasterizerState *StoredRasterizerState = _RasterizerState;
    _Device->CreateRasterizerState(&_RasterizerDesc, &_RasterizerState);
    _Device->RSSetState(_RasterizerState);
    if(StoredRasterizerState)
    {
        StoredRasterizerState->Release();
    }

    ID3D10BlendState *StoredBlendState = _BlendState;
    _Device->CreateBlendState(&_BlendDesc, &_BlendState);
    _Device->OMSetBlendState(_BlendState, NULL, 0xffffffff);
    if(StoredBlendState)
    {
        StoredBlendState->Release();
    }

    ID3D10DepthStencilState *StoredDepthStencilState = _DepthStencilState;
    _Device->CreateDepthStencilState(&_DepthStencilDesc, &_DepthStencilState);
    _Device->OMSetDepthStencilState(_DepthStencilState, 0);
    if(StoredDepthStencilState)
    {
        StoredDepthStencilState->Release();
    }
}

void D3D10GraphicsDevice::StartRender(WindowObjects &O)
{
    Assert(_Device != NULL, "Device not initialized");
    
    _Device->ClearRenderTargetView(_RenderTargetView, &(Vec4f(_ScreenColor)[0]));
    _Device->ClearDepthStencilView(_DepthStencilView, D3D10_CLEAR_DEPTH, 1.0f, 0);
}

void D3D10GraphicsDevice::FinishRender(WindowObjects &O, AppInterface &App)
{
    _SwapChain->Present(1, 0);
    //_SwapChain->Present(0, 0);
}

void D3D10GraphicsDevice::RestoreSystem()
{
    _SystemActive = true;
    if(_Wireframe)
    {
        _RasterizerDesc.FillMode = D3D10_FILL_WIREFRAME;
    }
    else
    {
        _RasterizerDesc.FillMode = D3D10_FILL_SOLID;
    }
    UpdateState();
}

void D3D10GraphicsDevice::ReSize(WindowObjects &O, AppInterface &App)
{
    Assert(_Device != NULL, "Device == NULL");
    App.ResetAllDevices(O);
    FreeCoreSurfaces();

    UINT Width = Math::Max(O.GetWindowManager().GetWidth(), 8);
    UINT Height = Math::Max(O.GetWindowManager().GetHeight(), 8);

    _SwapChain->ResizeBuffers(2, Width, Height, DXGI_FORMAT_R8G8B8A8_UNORM, DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH);
    AllocateCoreSurfaces(O.GetWindowManager());
    _SystemActive = true;
}

void D3D10GraphicsDevice::ReSizeFullScreen(WindowObjects &O, AppInterface &App)
{
    Assert(_Device != NULL, "Device == NULL");
    App.ResetAllDevices(O);
    FreeCoreSurfaces();
    _SwapChain->ResizeBuffers(2, O.GetWindowManager().GetWidth(), O.GetWindowManager().GetHeight(), DXGI_FORMAT_R8G8B8A8_UNORM, DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH);
    AllocateCoreSurfaces(O.GetWindowManager());
    _SystemActive = true;
}

void D3D10GraphicsDevice::DrawStringFloat(const String &Text, float x, float y, RGBColor c)
{
    if(_Wireframe)
    {
        _RasterizerDesc.FillMode = D3D10_FILL_SOLID;
        UpdateState();
    }

    RECT Rect;
    SetRect( &Rect, int(x), int(y), 0, 0 );
    _Font->DrawText( NULL, Text.CString(), Text.Length(), &Rect, DT_NOCLIP, D3DXCOLOR(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f, 1.0f ));
    
    if(_Wireframe)
    {
        _RasterizerDesc.FillMode = D3D10_FILL_WIREFRAME;
    }
    UpdateState();
    
}

void D3D10GraphicsDevice::LoadMatrix(MatrixController &MC)
{
    if(_MatrixVariable)
    {
        Matrix4 M = MC.TotalMatrix();
        _MatrixVariable->SetMatrix(&(M[0][0]));
    }
}

void D3D10GraphicsDevice::SetWireframe()
{
    _Wireframe = true;
    _RasterizerDesc.FillMode = D3D10_FILL_WIREFRAME;
    UpdateState();
}

void D3D10GraphicsDevice::DisableWireframe()
{
    _Wireframe = false;
    _RasterizerDesc.FillMode = D3D10_FILL_SOLID;
    UpdateState();
}

void D3D10GraphicsDevice::Render(const Mesh &M)
{
    if(_FixedFunctionEffect)
    {
        _FixedFunctionEffect->Set(*this);
    }
    if(_DynamicMesh.VertexCount() < M.VertexCount() || _DynamicMesh.FaceCount() < M.FaceCount())
    {
        _DynamicMesh = M;
    }
    else
    {
        memcpy(_DynamicMesh.Vertices(), M.Vertices(), sizeof(MeshVertex) * M.VertexCount());
        memcpy(_DynamicMesh.Indices(), M.Indices(), sizeof(DWORD) * M.IndexCount());
    }
    _DynamicMesh.Commit();
    
    _Device->IASetPrimitiveTopology( D3D10_PRIMITIVE_TOPOLOGY_TRIANGLELIST );

    ID3D10Buffer *VertexBuffer = NULL;
    ID3D10Buffer *IndexBuffer = NULL;

    _DynamicMesh.GetMesh()->GetDeviceVertexBuffer(0, &VertexBuffer);
    Assert(VertexBuffer != NULL, "GetDeviceVertexBuffer failed");
    _DynamicMesh.GetMesh()->GetDeviceIndexBuffer(&IndexBuffer);
    Assert(IndexBuffer != NULL, "GetDeviceIndexBuffer failed");

    UINT Stride = sizeof(MeshVertex);
    UINT Offset = 0;
    _Device->IASetVertexBuffers(0, 1, &VertexBuffer, &Stride, &Offset);
    _Device->IASetIndexBuffer(IndexBuffer, DXGI_FORMAT_R32_UINT, 0);

    _Device->DrawIndexed(M.IndexCount(), 0, 0);

    VertexBuffer->Release();
    IndexBuffer->Release();
    
}

ID3D10Device* D3D10GraphicsDevice::GetDevice()
{
    return _Device;
}

BaseTexture* D3D10GraphicsDevice::CreateTexture()
{
    BaseTexture *Result = new D3D10Texture(*this);
    return Result;
}

void D3D10GraphicsDevice::SetWorldViewPerspectiveVariable(ID3D10EffectMatrixVariable *MatrixVariable)
{
    _MatrixVariable = MatrixVariable;
}

void D3D10GraphicsDevice::SetFixedFunctionEffect(D3D10Effect *FixedFunctionEffect)
{
    _FixedFunctionEffect = FixedFunctionEffect;
}

void D3D10GraphicsDevice::SetZState(ZState NewState)
{
    //if(NewState == Z_ALWAYS)
    //    D3DDev->SetRenderState(D3DRS_ZFUNC,D3DCMP_ALWAYS);
    //else if(NewState == Z_LESSEQUAL)
    //    D3DDev->SetRenderState(D3DRS_ZFUNC,D3DCMP_LESSEQUAL);
}

void D3D10GraphicsDevice::AllocateBackBufferStaging()
{
    D3D10_TEXTURE2D_DESC StagingDesc;
    _BackBuffer->GetDesc(&StagingDesc);
    StagingDesc.Usage = D3D10_USAGE_STAGING;
    StagingDesc.BindFlags = 0;
    StagingDesc.CPUAccessFlags = D3D10_CPU_ACCESS_READ;
    StagingDesc.MiscFlags = 0;
    
    HRESULT hr = _Device->CreateTexture2D(&StagingDesc, NULL, &_BackBufferStaging);
    PersistentAssert(SUCCEEDED(hr), "_Device->CreateTexture2D failed");
}

void D3D10GraphicsDevice::CaptureScreen(WindowManager &WM, Bitmap &B)
{
    if(_BackBufferStaging == NULL)
    {
        AllocateBackBufferStaging();
    }

    //_Device->OMSetRenderTargets( 0, NULL, NULL );
    _Device->CopyResource(_BackBufferStaging, _BackBuffer);
    //_Device->OMSetRenderTargets( 1, &_RenderTargetView, _DepthStencilView );

    D3D10_MAPPED_TEXTURE2D Data;
    HRESULT hr = _BackBufferStaging->Map(0, D3D10_MAP_READ, 0, &Data);
    PersistentAssert(SUCCEEDED(hr), "_BackBufferStaging->Map failed");

    D3D10_TEXTURE2D_DESC StagingDesc;
    _BackBufferStaging->GetDesc(&StagingDesc);

    B.Allocate(StagingDesc.Width, StagingDesc.Height);
    
    for(UINT y = 0; y < StagingDesc.Height; y++)
    {
        RGBColor *RowStart = (RGBColor *)((BYTE *)Data.pData + Data.RowPitch * (StagingDesc.Height - 1 - y));
        for(UINT x = 0; x < StagingDesc.Width; x++)
        {
            RGBColor CurColor = RGBColor(RowStart[x].b, RowStart[x].r, RowStart[x].g, RowStart[x].a);
            B[y][x] = CurColor;
        }
    }

    _BackBufferStaging->Unmap(0);
}

void D3D10GraphicsDevice::LoadViewport(Viewport &V)
{
    //D3DVIEWPORT9 ThisViewport = {V.X, V.Y, V.Width, V.Height, V.MinZ, V.MaxZ};
    //D3DDev->SetViewport(&ThisViewport);
}

#endif