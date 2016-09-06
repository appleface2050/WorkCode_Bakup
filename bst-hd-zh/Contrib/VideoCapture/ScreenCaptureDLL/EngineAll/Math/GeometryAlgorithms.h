/*
GeometryAlgorithms.h
Written by Matthew Fisher

Geometric algorithms that do not fit anywhere else.
*/
#pragma once

namespace GeometryAlgorithms
{
    struct RigidCorrespondenceEntry
    {
	    Vec3f Source, Target;
	    float Weight;
    };

    void ComputeRigidCorrespondenceTransform(Matrix4 &SourceToTargetTransform, const Vector<RigidCorrespondenceEntry> &Correspondences);
};