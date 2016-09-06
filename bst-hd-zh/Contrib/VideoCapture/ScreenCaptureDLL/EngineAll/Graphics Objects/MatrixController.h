/*
MatrixController.h
Written by Matthew Fisher

MatrixController manages the 3 transformations (world, view, projection) central to the graphics pipeline.
*/
#pragma once
class MatrixController
{
public:
    MatrixController() {}
    MatrixController(const Matrix4 &Input)
    {
        World = Matrix4::Identity();
        View = Matrix4::Identity();
        Perspective = Input;
    }
    Matrix4 TotalMatrix();

    Matrix4 World, View, Perspective;
};
