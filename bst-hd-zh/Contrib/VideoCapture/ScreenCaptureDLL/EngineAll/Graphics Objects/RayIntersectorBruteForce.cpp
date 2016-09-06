/*
RayIntersectorBruteForce.cpp
Written by Matthew Fisher

Brute force implementation of RayIntersector.
*/
#include "..\\..\\Main.h"
#include "RayIntersectorBruteForce.h"

void RayIntersectorBruteForce::Init(const Vector<const BaseMesh*> &Meshes)
{
    UINT FaceCountSum = 0, VertexCountSum = 0;
    for(UINT MeshIndex = 0; MeshIndex < Meshes.Length(); MeshIndex++)
    {
        FaceCountSum += Meshes[MeshIndex]->FaceCount();
        VertexCountSum += Meshes[MeshIndex]->VertexCount();
    }

    _Vertices.Allocate(VertexCountSum);
    _Indices.Allocate(FaceCountSum * 3);
    UINT CurVertexIndex = 0, CurIndexIndex = 0;

    for(UINT MeshIndex = 0; MeshIndex < Meshes.Length(); MeshIndex++)
    {
        const BaseMesh &CurMesh = *Meshes[MeshIndex];
        UINT BaseVertexIndex = CurVertexIndex;
        for(UINT VertexIndex = 0; VertexIndex < CurMesh.VertexCount(); VertexIndex++)
        {
            _Vertices[CurVertexIndex++] = CurMesh.Vertices()[VertexIndex].Pos;
        }
        for(UINT IndexIndex = 0; IndexIndex < CurMesh.IndexCount(); IndexIndex++)
        {
            _Indices[CurIndexIndex++] = CurMesh.Indices()[IndexIndex] + BaseVertexIndex;
        }
    }
}

void RayIntersectorBruteForce::FindIntersections(const Ray3D &R, Vector<Intersection> &IntersectPts) const
{
    const UINT TriangleCount = _Indices.Length() / 3;
    const UINT *Indices = _Indices.CArray();
    const Vec3f *Vertices = _Vertices.CArray();
    
    for(UINT TriangleIndex = 0; TriangleIndex < TriangleCount; TriangleIndex++)
    {
        const Vec3f &V0 = Vertices[Indices[TriangleIndex * 3 + 0]];
        const Vec3f &V1 = Vertices[Indices[TriangleIndex * 3 + 1]];
        const Vec3f &V2 = Vertices[Indices[TriangleIndex * 3 + 2]];

        Intersection CurIntersection;
        if(Math::RayIntersectTriangle(R.P0, R.D, V0, V1, V2, CurIntersection.Pt))
        {
            CurIntersection.FaceIndex = TriangleIndex;
            IntersectPts.PushEnd(CurIntersection);
        }
    }

    RayIntersectionSorter Sorter(R.P0);
    IntersectPts.Sort(Sorter);
}
