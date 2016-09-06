/*
Console.h
Written by Matthew Fisher

Functions for interacting with the console.
*/
#pragma  once
class Picker
{
public:
    static void FindHits(const D3D9Mesh &M, const Matrix4 &WorldViewProjectionTransform, const Vec3f &VecEye, const Vec2f &MousePoint,
                         Vector<float> &Hits, DWORD &FirstHitFaceIndex, float &u, float &v);

private:

};
