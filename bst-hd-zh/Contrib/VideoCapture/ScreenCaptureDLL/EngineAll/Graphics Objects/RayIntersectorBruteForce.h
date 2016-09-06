/*
RayIntersectorBruteForce.h
Written by Matthew Fisher

Brute force implementation of RayIntersector.
*/
#pragma once

class RayIntersectorBruteForce : public RayIntersector
{
public:
    void Init(const Vector<const BaseMesh*> &M);
    void FindIntersections(const Ray3D &R, Vector<Intersection> &IntersectPts) const;

private:
    Vector<Vec3f> _Vertices;
    Vector<UINT> _Indices;
};
