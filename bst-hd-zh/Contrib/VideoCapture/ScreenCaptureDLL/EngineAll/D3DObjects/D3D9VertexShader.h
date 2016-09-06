/*
D3D9VertexShader.h
Written by Matthew Fisher

D3D9VertexShader wraps the IDirect3DVertexShader9 object and an associated ID3DXConstantTable.
*/
#pragma once

#ifdef USE_D3D9

class D3D9VertexShader : public D3D9Object
{
public:
    D3D9VertexShader();
    ~D3D9VertexShader();

    void ReleaseMemory();
    void FreeMemory();
    void Reset(LPDIRECT3DDEVICE9 _Device);

    void Init(const String &Filename, LPDIRECT3DDEVICE9 _Device);
    void Init(const String &Filename, GraphicsDevice &GD);

    void Set();
    void SetMatrix(const char *Name, const Matrix4 &M)
    {
        D3DXMATRIX DM = M;
        D3DValidate( _ConstantTable->SetMatrix(_Device, Name, &DM), "SetMatrix" );
    }
    void SetFloat(const char *Name, float Value)
    {
        D3DValidate( _ConstantTable->SetFloat(_Device, Name, Value), "SetFloat" );
    }
    void SetVec3(const char *Name, const Vec3f &V)
    {
        const float V3[3] = {V.x, V.y, V.z};
        D3DValidate( _ConstantTable->SetFloatArray(_Device, Name, V3, 3), "SetFloatArray" );
    }
    void SetVec4(const char *Name, const Vec4f &V)
    {
        const float V4[4] = {V.x, V.y, V.z, V.w};
        D3DValidate( _ConstantTable->SetFloatArray(_Device, Name, V4, 4), "SetFloatArray" );
    }
    LPDIRECT3DDEVICE9 Device()
    {
        return _Device;
    }

private:
    String _ShaderFile;
    D3D9VertexDeclaration _Decl;
    LPDIRECT3DVERTEXSHADER9 _Shader;
    LPDIRECT3DDEVICE9 _Device;
    LPD3DXCONSTANTTABLE _ConstantTable;
};

#endif