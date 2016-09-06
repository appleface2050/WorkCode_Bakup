/*
UVATlas.h
Written by Matthew Fisher

UVAtlas is used to break a mesh up into regions topologically equivalent to a disk, and pack these regions
into a planar texture.
*/
#pragma  once
#ifdef USE_D3D9

struct UVAtlasCell
{
    __forceinline Vec3f ComputePosition(BaseMesh &M) const
    {
        Vec3f &V0 = M.Vertices()[M.Indices()[FaceIndex * 3 + 0]].Pos;
        Vec3f &V1 = M.Vertices()[M.Indices()[FaceIndex * 3 + 1]].Pos;
        Vec3f &V2 = M.Vertices()[M.Indices()[FaceIndex * 3 + 2]].Pos;
        return V0 * BarycentricCoord.x + V1 * BarycentricCoord.y + V2 * (1.0f - BarycentricCoord.x - BarycentricCoord.y);
    }

    __forceinline Vec3f ComputeNormal(BaseMesh &M) const
    {
        Vec3f &N0 = M.Vertices()[M.Indices()[FaceIndex * 3 + 0]].Normal;
        Vec3f &N1 = M.Vertices()[M.Indices()[FaceIndex * 3 + 1]].Normal;
        Vec3f &N2 = M.Vertices()[M.Indices()[FaceIndex * 3 + 2]].Normal;
        return Vec3f::Normalize(N0 * BarycentricCoord.x + N1 * BarycentricCoord.y + N2 * (1.0f - BarycentricCoord.x - BarycentricCoord.y));
    }


    UINT FaceIndex;
    Vec2f BarycentricCoord;

    //
    // 0 - invalid point
    // 1 - inside triangle
    // 2 - inside gutter
    // 4 - inside gutter, evaluated as a full sample
    //
    BYTE TextelClass;
};

class UVAtlas
{
public:
    UVAtlas();
    ~UVAtlas();
    void FreeMemory();

    void Create(const BaseMesh &MeshIn, BaseMesh &MeshOut, UINT TextureWidth, UINT TextureHeight, bool UseProvidedTexCoords, const String &FastLoadDirectory);
    void ApplyToTexture(LPDIRECT3DTEXTURE9 pTexture);
    void MakeDescriptiveBitmap(Bitmap &Bmp);
    __forceinline const Vector<UINT>& VertexRemapArray() const
    {
        return _VertexRemapArray;
    }
    __forceinline const Grid<UVAtlasCell>& Cells() const
    {
        return _Cells;
    }

private:
    LPD3DXTEXTUREGUTTERHELPER _GutterHelper;
    Grid<UVAtlasCell> _Cells;
    Vector<UINT> _VertexRemapArray;
};

#endif