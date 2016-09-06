/*
D3DGraphicsDevice.h
Written by Matthew Fisher

a D3D instance of GraphicsDevice.  The only new functionality is GetDevice() which returns
the D3DDevice associated with this graphics device.
*/
#pragma once

#ifdef USE_D3D10

class D3D10GraphicsDevice : public GraphicsDevice
{
public:
    D3D10GraphicsDevice();
    ~D3D10GraphicsDevice();

    void FreeMemory();
    void Init3D(WindowObjects &O, const GraphicsDeviceParameters &Parameters);

    void PrepareFullScreenReSize(WindowObjects &O, AppInterface &App);
    void ReSize(WindowObjects &O, AppInterface &App);
    void ReSizeFullScreen(WindowObjects &O, AppInterface &App);
    void StartRender(WindowObjects &O);
    void FinishRender(WindowObjects &O, AppInterface &App);

    void DrawStringFloat(const String &Text, float x, float y, RGBColor c);

    void LoadViewport(Viewport &V);
    void LoadMatrix(MatrixController &MC);

    void Render(const Mesh &M);
    
    void CaptureScreen(WindowManager &WM, Bitmap &B);

    ID3D10Device* GetDevice();
    BaseTexture* CreateTexture();

    void SetWireframe();
    void DisableWireframe();

    void SetZState(ZState NewState);
    void SetWorldViewPerspectiveVariable(ID3D10EffectMatrixVariable *MatrixVariable);
    void SetFixedFunctionEffect(D3D10Effect *FixedFunctionEffect);
    
    const D3D10_BLEND_DESC& GetBlendState()
    {
        return _BlendDesc;
    }

    void SetBlendState(const D3D10_BLEND_DESC &NewBlendState);

private:
    void FreeCoreSurfaces();
    void AllocateSwapChain(WindowManager &WM);
    void AllocateCoreSurfaces(WindowManager &WM);
    void AllocateBackBufferStaging();
    void RestoreSystem();                    //restores the system after it has been reset
    void UpdateState();
    void LoadStateDefaults();

    ID3D10Device*           _Device;
    IDXGISwapChain*         _SwapChain;
    ID3D10RenderTargetView* _RenderTargetView;
    ID3D10Texture2D*        _DepthStencil;
    ID3D10DepthStencilView* _DepthStencilView;
    ID3DX10Font*            _Font;
    ID3D10Texture2D*        _BackBuffer;
    ID3D10Texture2D*        _BackBufferStaging;

    D3D10Mesh                    _DynamicMesh;
    D3D10Effect*                 _FixedFunctionEffect;
    ID3D10EffectMatrixVariable*  _MatrixVariable;

    //
    // State machine
    //
    D3D10_RASTERIZER_DESC       _RasterizerDesc;
    ID3D10RasterizerState*      _RasterizerState;
    D3D10_BLEND_DESC            _BlendDesc;
    ID3D10BlendState*           _BlendState;
    D3D10_DEPTH_STENCIL_DESC    _DepthStencilDesc;
    ID3D10DepthStencilState*    _DepthStencilState;
};

#endif