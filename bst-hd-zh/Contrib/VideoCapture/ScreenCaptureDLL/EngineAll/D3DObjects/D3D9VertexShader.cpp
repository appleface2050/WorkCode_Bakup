/*
D3D9VertexShader.cpp
Written by Matthew Fisher

D3D9VertexShader wraps the IDirect3DVertexShader9 object and an associated ID3DXConstantTable.
*/
#include "..\\..\\Main.h"
#include "D3D9VertexShader.h"

#ifdef USE_D3D9

D3D9VertexShader::D3D9VertexShader()
{
    _Shader = NULL;
    _Device = NULL;
    _ConstantTable = NULL;
}

D3D9VertexShader::~D3D9VertexShader()
{
    FreeMemory();
}

void D3D9VertexShader::FreeMemory()
{
    if(_Shader)
    {
        _Shader->Release();
        _Shader = NULL;
    }
    if(_ConstantTable)
    {
        _ConstantTable->Release();
        _ConstantTable = NULL;
    }
}

void D3D9VertexShader::ReleaseMemory()
{
    FreeMemory();
}

void D3D9VertexShader::Reset(LPDIRECT3DDEVICE9 Device)
{
    FreeMemory();

    HRESULT hr;

    _Device = Device;
    Assert(Device != NULL, "Device == NULL");

    _Decl.Init(Device);

    // Define DEBUG_VS and/or DEBUG_PS to debug vertex and/or pixel shaders with the 
    // shader debugger. Debugging vertex shaders requires either REF or software vertex 
    // processing, and debugging pixel shaders requires REF.  The 
    // D3DXSHADER_FORCE_*_SOFTWARE_NOOPT flag improves the debug experience in the 
    // shader debugger.  It enables source level debugging, prevents instruction 
    // reordering, prevents dead code elimination, and forces the compiler to compile 
    // against the next higher available software target, which ensures that the 
    // unoptimized shaders do not exceed the shader model limitations.  Setting these 
    // flags will cause slower rendering since the shaders will be unoptimized and 
    // forced into software.  See the DirectX documentation for more information about 
    // using the shader debugger.
    DWORD dwShaderFlags = 0;
    #ifdef DEBUG_VS
        dwShaderFlags |= D3DXSHADER_SKIPOPTIMIZATION|D3DXSHADER_DEBUG;
    #endif

    LPD3DXBUFFER pCode = NULL;
    LPD3DXBUFFER pErrors = NULL;

    PersistentAssert(Utility::FileExists(_ShaderFile), String(_ShaderFile) + String(" not found."));

    // Assemble the vertex shader from the file
    hr = D3DXCompileShaderFromFile( _ShaderFile.CString(), NULL, NULL, "VShaderEntry",
                                    "vs_3_0", dwShaderFlags, &pCode,
                                    &pErrors, &_ConstantTable );
    
    if(pErrors)
    {
        char *ErrorMessage = (char *)pErrors->GetBufferPointer();
        DWORD ErrorLength = pErrors->GetBufferSize();

        ofstream file("ShaderDebug.txt");
        for(UINT i = 0; i < ErrorLength; i++)
        {
            file << ErrorMessage[i];
        }
        file.close();
    }

    Assert(!FAILED(hr), "D3DXCompileShaderFromFile failed.  See ShaderDebug.txt for details.");

    // Create the vertex shader
    hr = Device->CreateVertexShader( (DWORD*)pCode->GetBufferPointer(),
                                            &_Shader );

    if(pErrors)
    {
        pErrors->Release();
    }
    if(pCode)
    {
        pCode->Release();
    }
    Assert(!FAILED(hr), "CreateVertexShader failed");
}

void D3D9VertexShader::Init(const String &Filename, GraphicsDevice &GD)
{
    _ShaderFile = Filename;
    Reset(GD.CastD3D9().GetDevice());
}

void D3D9VertexShader::Init(const String &Filename, LPDIRECT3DDEVICE9 Device)
{
    _ShaderFile = Filename;
    Reset(Device);
}

void D3D9VertexShader::Set()
{
    _Decl.Set();
    D3DValidate( _Device->SetVertexShader(_Shader), "SetVertexShader");
}

#endif