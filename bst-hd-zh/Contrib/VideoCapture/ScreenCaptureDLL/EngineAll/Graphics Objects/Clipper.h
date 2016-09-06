/*
Clipper.h
Written by Matthew Fisher

Clipper is used to take an arbitrary polygon and clip it against the viewing frustrum.
*/
#pragma once

class Clipper {
public:
    Clipper();
    void Init(float Width, float Height);                //initalizes the ViewPlanes

    void Clip(MeshVertex *Polygon, int &vc);            //clips the polygon against all 6 ViewPlanes.  The result is stored in the same space as the passed in polygon.
    void Clip(MeshVertex *Polygon, int &vc, Plane &P);  //clips the polygon against plane P.  The result is stored in the same space as the passed in polygon.

    Plane ViewPlanes[6]; //the 6 clipping planes

    //
    // temporary storage
    //
    bool Inside[100];
    MeshVertex NewVtx[10];
};