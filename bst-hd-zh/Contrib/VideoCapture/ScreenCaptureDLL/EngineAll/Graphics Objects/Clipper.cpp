/*
Clipper.cpp
Written by Matthew Fisher

Clipper is used to take an arbitrary polygon and clip it against the viewing frustrum.
*/
#include "..\\..\\Main.h"
#include "Clipper.h"

Clipper::Clipper()
{
    
}

void Clipper::Init(float Width, float Height)
{
    //
    // the 6 standard view planes in screen coordinates
    //
    ViewPlanes[0] = Plane::ConstructFromPointNormal(Vec3f::Origin, Vec3f::eX);
    ViewPlanes[1] = Plane::ConstructFromPointNormal(Vec3f::Origin, Vec3f::eY);
    ViewPlanes[2] = Plane::ConstructFromPointNormal(Vec3f::Origin, Vec3f::eZ);

    ViewPlanes[3] = Plane::ConstructFromPointNormal(Vec3f(Width,Height,1.0f), -Vec3f::eX);
    ViewPlanes[4] = Plane::ConstructFromPointNormal(Vec3f(Width,Height,1.0f), -Vec3f::eY);
    ViewPlanes[5] = Plane::ConstructFromPointNormal(Vec3f(Width,Height,1.0f), -Vec3f::eZ);
}

void Clipper::Clip(MeshVertex *Polygon, int &vc)
{
    for(UINT i = 0; i < 6; i++)
    {
        if(vc)
        {
            //
            // clip against each view plane seperately
            //
            Clip(Polygon, vc, ViewPlanes[i]);
        }
    }
}

void Clipper::Clip(MeshVertex *Polygon, int &vc, Plane &P)
{
    int i;
    bool AllInside = true;
    bool AllOutside = true;

    for(i=0;i<vc;i++)    //for each point
    {
        if(Plane::DotCoord(P, Polygon[i].Pos) < 0.0f) Inside[i] = false;    //determine if we're inside or outside
        else Inside[i] = true;

        if(Inside[i]) AllOutside = false;
        else AllInside = false;
    }

    if(AllOutside)    //if we're all outside,
    {
        vc = 0;        //render nothing
        return;
    }
    if(AllInside) return;    //if we're all inside this clipping plane does nothing.

    //if we're here, we overlap the clipping plane and need to clip.

    int Break1=-1, Break2=-1, ip1;
    MeshVertex Vtx1, Vtx2;
    float s;

    for(i=0;i<vc;i++)
    {
        ip1 = i+1;
        if(ip1 == vc) ip1 = 0;

        if(Inside[i] != Inside[ip1])    //if we crossed over here,
        {
            if(Inside[ip1])
            {
                s = P.IntersectLineRatio(Polygon[i].Pos,Polygon[ip1].Pos);    //compute the intersection with the clipping plane
                MeshVertex::Interpolate(Polygon[i],Polygon[ip1], Vtx1, s);                //create a new vertex at this interpolation value
                Break1 = i;            //mark this crossing (from outside to inside)
            }
            else
            {
                s = P.IntersectLineRatio(Polygon[i].Pos,Polygon[ip1].Pos);    //same as above
                MeshVertex::Interpolate(Polygon[i],Polygon[ip1], Vtx2, s);
                Break2 = i;            //mark this crossing (from inside to outside)
            }
        }
    }

    if(Break1 == -1 || Break2 == -1) return;    //if we don't have two breaks, we're confused, so do no clipping.

    //the following reconstructs the clipped polygon given the breakpoints calculated above.
    int NewVC = 0;
    NewVtx[NewVC++] = Vtx1;

    i=(Break1 + 1) % vc;
    bool Exit = false;

    while(!Exit)
    {
        NewVtx[NewVC++] = Polygon[i];

        if(i == Break2) Exit = true;
        i++;
        i %= vc;
    }

    NewVtx[NewVC++] = Vtx2;

    vc = NewVC;                                            //load the new vertices
    memcpy(Polygon, NewVtx, sizeof(MeshVertex)*NewVC);    //copy from temporary memory into the passed in vertices
}