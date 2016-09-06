/*
ColorSpaces.cpp
Written by Matthew Fisher

Color space transformations.  Represents colors as 3-vectors.
*/
#include "..\\..\\Main.h"
#include "ColorSpaces.h"

UINT MaxChannel(const Vec3f &V)
{
    if(V.x > V.y && V.x > V.z)
    {
        return 0;
    }
    else if(V.y > V.z)
    {
        return 1;
    }
    else
    {
        return 2;
    }
}

UINT MinChannel(const Vec3f &V)
{
    if(V.x < V.y && V.x < V.z)
    {
        return 0;
    }
    else if(V.y < V.z)
    {
        return 1;
    }
    else
    {
        return 2;
    }
}

Vec3f RGBToHSL(const Vec3f &RGB)
{
    Vec3f Result = Vec3f::Origin;
    float R = RGB.x;
    float G = RGB.y;
    float B = RGB.z;

    UINT Max = MaxChannel(RGB);
    UINT Min = MinChannel(RGB);
    float MaxDifference = RGB[Max] - RGB[Min];
    
    //
    // Luminance
    //
    Result.z = 0.5f * (RGB[Max] + RGB[Min]);

    //
    // Hue
    //
    if(MaxDifference > 0.0f)
    {
        float HueFactor = (60.0f / 360.0f) / MaxDifference;
        if(Max == 0)
        {
            if(G >= B)
            {
                Result.x = HueFactor * (G - B) + 0.0f / 360.0f;
            }
            else
            {
                Result.x = HueFactor * (G - B) + 360.0f / 360.0f;
            }
        }
        else if(Max == 1)
        {
            Result.x = HueFactor * (B - R) + 120.0f / 360.0f;
        }
        else
        {
            Result.x = HueFactor * (R - G) + 240.0f / 360.0f;
        }
    }

    //
    // Saturation
    //
    if(Result.z > 0.0f && Max != Min)
    {
        if(Result.z <= 0.5f)
        {
            Result.y = 0.5f * MaxDifference / Result.z;
        }
        else
        {
            Result.y = 0.5f * MaxDifference / (1.0f - Result.z);
        }
    }
    return Result;
}

Vec3f HSLToRGB(const Vec3f &HSL)
{
    float H = HSL.x;
    float S = HSL.y;
    float L = HSL.z;

    if(S == 0.0f)
    {
        return Vec3f(L, L, L);
    }

    float Q;
    if(L < 0.5f)
    {
        Q = L * (1.0f + S);
    }
    else
    {
        Q = L + S - (L * S);
    }
    float P = 2.0f * L - Q;
    Vec3f T(H + 1.0f / 3.0f,
           H,
           H - 1.0f / 3.0f);
    for(UINT i = 0; i < 3; i++)
    {
        if(T[i] < 0.0f)
        {
            T[i] += 1.0f;
        }
        else if(T[i] > 1.0f)
        {
            T[i] -= 1.0f;
        }
    }

    Vec3f Result;
    for(UINT i = 0; i < 3; i++)
    {
        float Tc = T[i];
        if(Tc < 1.0f / 6.0f)
        {
            Result[i] = P + ((Q - P) * 6.0f * Tc);
        }
        else if(Tc < 0.5f)
        {
            Result[i] = Q;
        }
        else if(Tc < 2.0f / 3.0f)
        {
            Result[i] = P + ((Q - P) * (2.0f / 3.0f - Tc) * 6.0f);
        }
        else
        {
            Result[i] = P;
        }
    }
    return Result;
}