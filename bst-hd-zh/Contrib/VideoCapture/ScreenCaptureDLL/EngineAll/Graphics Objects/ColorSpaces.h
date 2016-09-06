/*
ColorSpaces.h
Written by Matthew Fisher

Color space transformations.  Represents colors as 3-vectors
*/
#pragma once
Vec3f RGBToHSL(const Vec3f &RGB);
Vec3f HSLToRGB(const Vec3f &HSL);
