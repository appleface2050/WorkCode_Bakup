/*
MatrixController.cpp
Written by Matthew Fisher

MatrixController manages the 3 transformations (world, view, projection) central to the graphics pipeline.
*/
#include "..\\..\\Main.h"
#include "MatrixController.h"

Matrix4 MatrixController::TotalMatrix()
{
    return World * View * Perspective;
}
