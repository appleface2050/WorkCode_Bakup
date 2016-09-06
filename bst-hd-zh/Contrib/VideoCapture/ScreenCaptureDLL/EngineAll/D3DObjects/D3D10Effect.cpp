/*
D3D10Effect.cpp
Written by Matthew Fisher

The D3D10Effect class is a wrapper around ID3D10Effect.
*/
#include "..\\..\\Main.h"
#include "D3D10Effect.h"

#ifdef USE_D3D10

D3D10Effect::D3D10Effect()
{
    _Effect = NULL;
    _Technique = NULL;
    _Pass = NULL;
    _VertexLayout = NULL;
}

D3D10Effect::~D3D10Effect()
{
    FreeMemory();
}

void D3D10Effect::FreeMemory()
{
    if(_Effect)
    {
        _Effect->Release();
        _Effect = NULL;
    }
    if(_VertexLayout)
    {
        _VertexLayout->Release();
        _VertexLayout = NULL;
    }
    
    // Techniques are not IUnknown
    _Technique = NULL;
}

void D3D10Effect::Init(const String &Filename, GraphicsDevice &GD)
{
    DWORD ShaderFlags = D3D10_SHADER_PACK_MATRIX_COLUMN_MAJOR | D3D10_SHADER_ENABLE_STRICTNESS | D3D10_SHADER_DEBUG;
    ID3D10Blob *CompileResult = NULL;
    D3DX10CreateEffectFromFile( Filename.CString(), NULL, NULL, "fx_4_0", ShaderFlags, 0, GD.CastD3D10().GetDevice(), NULL,
                                NULL, &_Effect, &CompileResult, NULL );
    if(CompileResult)
    {
        Console::WriteLine(String("Effect compile result: ") + String((char *)CompileResult->GetBufferPointer()));
        CompileResult->Release();
    }
    PersistentAssert(_Effect != NULL, "D3DX10CreateEffectFromFile failed");

    _Technique = _Effect->GetTechniqueByName( "Render" );
    PersistentAssert(_Technique != NULL, "Render technique not found");

    _Pass = _Technique->GetPassByIndex(0);
    PersistentAssert(_Pass != NULL, "Pass not found");

    D3D10_PASS_DESC PassDesc;
    _Pass->GetDesc(&PassDesc);
    GD.CastD3D10().GetDevice()->CreateInputLayout(MeshVertexD3D10Declaration, MeshVertexD3D10DeclarationElements, PassDesc.pIAInputSignature,
                                                  PassDesc.IAInputSignatureSize, &_VertexLayout );
    PersistentAssert(_VertexLayout != NULL, "CreateInputLayout failed");
}

void D3D10Effect::Set(GraphicsDevice &GD)
{
    GD.CastD3D10().GetDevice()->IASetInputLayout( _VertexLayout );
    _Pass->Apply(0);
}

#endif