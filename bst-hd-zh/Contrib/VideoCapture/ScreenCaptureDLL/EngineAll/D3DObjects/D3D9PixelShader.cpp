/*
D3D9PixelShader.cpp
Written by Matthew Fisher

D3D9PixelShader wraps the IDirect3DPixelShader9 object and an associated ID3DXConstantTable.
*/
#include "..\\..\\Main.h"
#include "D3D9PixelShader.h"

#ifdef USE_D3D9

D3D9PixelShader::D3D9PixelShader()
{
    Shader = NULL;
    Device = NULL;
    ConstantTable = NULL;
}
D3D9PixelShader::~D3D9PixelShader()
{
    FreeMemory();
}

void D3D9PixelShader::FreeMemory()
{
    if(Shader)
    {
        Shader->Release();
        Shader = NULL;
    }
    if(ConstantTable)
    {
        ConstantTable->Release();
        ConstantTable = NULL;
    }
}

void D3D9PixelShader::ReleaseMemory()
{
    FreeMemory();
}

void D3D9PixelShader::Reset(LPDIRECT3DDEVICE9 _Device)
{
    FreeMemory();

    HRESULT hr;

    Device = _Device;
    Assert(Device != NULL, "Device == NULL");

    DWORD dwShaderFlags = 0;
    #ifdef DEBUG_PS
        dwShaderFlags |= D3DXSHADER_SKIPOPTIMIZATION | D3DXSHADER_DEBUG;
    #endif

    LPD3DXBUFFER pCode = NULL;
    LPD3DXBUFFER pErrors = NULL;

    // Assemble the vertex shader from the file
    hr = D3DXCompileShaderFromFile( ShaderFile.CString(), NULL, NULL, "PShaderEntry",
                                    "ps_3_0", dwShaderFlags, &pCode,
                                    &pErrors, &ConstantTable );
    
    String ErrorString;
    if(pErrors)
    {
        char *ErrorMessage = (char *)pErrors->GetBufferPointer();
        DWORD ErrorLength = pErrors->GetBufferSize();

        ofstream file("ShaderDebug.txt");
        for(UINT i = 0; i < ErrorLength; i++)
        {
            file << ErrorMessage[i];
            ErrorString += String(ErrorMessage[i]);
        }
        file.close();
    }

    PersistentAssert(!FAILED(hr), String("D3DXCompileShaderFromFile failed: ") + ErrorString);

    // Create the vertex shader
    hr = Device->CreatePixelShader( (DWORD*)pCode->GetBufferPointer(),
                                            &Shader );

    if(pErrors)
    {
        pErrors->Release();
    }
    if(pCode)
    {
        pCode->Release();
    }
    PersistentAssert(!FAILED(hr), "CreatePixelShader failed");
}

void D3D9PixelShader::Init(const String &Filename, LPDIRECT3DDEVICE9 _Device)
{
    ShaderFile = Filename;
    Reset(_Device);
}

void D3D9PixelShader::Init(const String &Filename, GraphicsDevice &GD)
{
    ShaderFile = Filename;
    Reset(GD.CastD3D9().GetDevice());
}

void D3D9PixelShader::Set()
{
    D3DValidate( Device->SetPixelShader(Shader), "SetPixelShader");
}

#endif