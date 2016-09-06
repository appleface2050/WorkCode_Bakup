/*
D3D10Effect.h
Written by Matthew Fisher

The D3D10Effect class is a wrapper around ID3D10Effect.
*/
#pragma once

#ifdef USE_D3D10

class D3D10Effect
{
public:
    D3D10Effect();
    ~D3D10Effect();
    void FreeMemory();

    void Init(const String &Filename, GraphicsDevice &GD);
    void Set(GraphicsDevice &GD);

    ID3D10EffectVariable* GetVariable(const char *Name)
    {
        ID3D10EffectVariable* Result = _Effect->GetVariableByName( Name );
        Assert(Result != NULL, "Variable not found");
        return Result;
    }
    void SetMatrix(const char *Name, Matrix4 &M)
    {
        GetVariable(Name)->AsMatrix()->SetMatrix(&(M[0][0]));
        //GetVariable(Name)->AsMatrix()->SetMatrixTranspose(&(M[0][0]));
    }
    void SetFloat(const char *Name, float Value)
    {
        GetVariable(Name)->AsScalar()->SetFloat(Value);
    }
    void SetVec3(const char *Name, Vec3f &V)
    {
        GetVariable(Name)->AsScalar()->SetFloatArray(&V[0], 0, 3);
    }
    void SetVec4(const char *Name, Vec4f &V)
    {
        GetVariable(Name)->AsScalar()->SetFloatArray(&V[0], 0, 4);
    }
    void SetTexture2D(const char *Name, D3D10Texture &Tex)
    {
        GetVariable(Name)->AsShaderResource()->SetResource(Tex.View());
    }

private:
    ID3D10Effect*            _Effect;
    ID3D10EffectTechnique*   _Technique;
    ID3D10EffectPass*        _Pass;
    ID3D10InputLayout*       _VertexLayout;
};

#endif