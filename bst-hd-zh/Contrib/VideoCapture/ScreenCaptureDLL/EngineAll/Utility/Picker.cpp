#include "..\\..\\Main.h"
#include "Picker.h"

#ifdef USE_D3D9
void Picker::FindHits(const D3D9Mesh &M, const Matrix4 &WorldViewProjectionTransform, const Vec3f &VecEye, const Vec2f &MousePoint,
                      Vector<float> &Hits, DWORD &FirstHitFaceIndex, float &u, float &v)
{
    Vec3f LookPtProjection(Math::LinearMap(0.0f, 1.0f, -1.0f, 1.0f, MousePoint.x),
                           Math::LinearMap(0.0f, 1.0f, 1.0f, -1.0f, MousePoint.y),
                           0.5f);
    Vec3f LookPtObject = WorldViewProjectionTransform.Inverse().TransformPoint(LookPtProjection);
    M.QueryHits(VecEye, LookPtObject - VecEye, Hits, FirstHitFaceIndex, u, v);
}
#endif