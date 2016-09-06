/*
Intersect.h
Written by Matthew Fisher

Code for computing geometric object intersections.
*/
#pragma once
namespace Math
{
    bool RayIntersectRectangle(const Ray3D &Ray, const Rectangle3f &Rect);
    bool RayIntersectTriangle(const Vec3f &RayPt, const Vec3f &RayDir, const Vec3f &T0, const Vec3f &T1, const Vec3f &T2, Vec3f &IntersectPt);
    bool LineIntersectTriangle(const Vec3f &LinePt, const Vec3f &LineDir, const Vec3f &T0, const Vec3f &T1, const Vec3f &T2, Vec3f &IntersectPt);
    bool SegmentIntersectTriangle(const Vec3f &P0, const Vec3f &P1, const Vec3f &T0, const Vec3f &T1, const Vec3f &T2, Vec3f &IntersectPt);
    void ReorientBasis(Vec3f &V1, const Vec3f &V2, const Vec3f &V3);
}