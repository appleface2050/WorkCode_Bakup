/*
PrimitiveRender.cpp
Written by Matthew Fisher

Rendering functions for lines and polygons in software.  Not very efficent.
DrawLine uses integer arithmetic but DrawPolygon doesn't.  I decide not to comment
these because I intend to rewrite them, but they work good enough under most conditions.
*/
#include "..\\..\\Main.h"
#include "PrimitiveRender.h"

void PrimitiveRender::DrawSquare(Bitmap &Bmp, const Vec2i &Coords, int Radius, RGBColor InteriorColor, RGBColor BorderColor)
{
    DrawRect(Bmp, Rectangle2i::ConstructFromCenterVariance(Coords, Vec2i(Radius, Radius)), InteriorColor, BorderColor);
}

void AliasRender::DrawRect(Bitmap &Bmp, const Rectangle2i &Rect, RGBColor InteriorColor, RGBColor BorderColor)
{
    const int Height = Bmp.Height();
    const int Width = Bmp.Width();
    for(int y = Rect.Min.y; y <= Rect.Max.y; y++)
    {
        if(y >= 0 && y < Height)
        {
            for(int x = Rect.Min.x; x <= Rect.Max.x; x++)
            {
                if(x >= 0 && x < Width)
                {
                    if(x == Rect.Min.x || x == Rect.Max.x || y == Rect.Min.y || y == Rect.Max.y)
                    {
                        Bmp[y][x] = BorderColor;
                    }
                    else
                    {
                        Bmp[y][x] = InteriorColor;
                    }
                }
            }
        }
    }
}

void AliasRender::DrawTransparentRect(Bitmap &Bmp, const Rectangle2i &Rect, RGBColor InteriorColor, RGBColor BorderColor)
{
    const int Height = Bmp.Height();
    const int Width = Bmp.Width();
    for(int y = Rect.Min.y; y <= Rect.Max.y; y++)
    {
        if(y >= 0 && y < Height)
        {
            for(int x = Rect.Min.x; x <= Rect.Max.x; x++)
            {
                if(x >= 0 && x < Width)
                {
                    if(x == Rect.Min.x || x == Rect.Max.x || y == Rect.Min.y || y == Rect.Max.y)
                    {
                        Bmp[y][x] = BorderColor;
                    }
                    else
                    {
                        RGBColor &C = Bmp[y][x];
                        C = RGBColor(int(C.r) * InteriorColor.r / 255, int(C.g) * InteriorColor.g / 255, int(C.b) * InteriorColor.b / 255);
                    }
                }
            }
        }
    }
}

void AliasRender::DrawCircle(Bitmap &Bmp, const Vec2i &Coords, int Radius, RGBColor Color)
{
    const int Height = Bmp.Height();
    const int Width = Bmp.Width();
    const int RadiusSq = Radius * Radius;
    for(int y = -Radius; y <= Radius; y++)
    {
        int BmpY = y + Coords.y;
        if(BmpY >= 0 && BmpY < Height)
        {
            int ySq = y * y;
            for(int x = -Radius; x <= Radius; x++)
            {
                int BmpX = x + Coords.x;
                if(BmpX >= 0 && BmpX < Width && ySq + x * x < RadiusSq)
                {
                    Bmp[BmpY][BmpX] = Color;
                }
            }
        }
    }
}

void AliasRender::DrawLine(Bitmap &B, int x1, int y1, int x2, int y2, const RGBColor &Color)
{
    int YSign = 1;

    if(x2 < x1)
    {
        Utility::Swap(x1, x2);
        Utility::Swap(y1, y2);
    }

    if(y2 < y1)
    {
        YSign = -1;
        if(x2 - x1 + y2 - y1 < 0)
            DrawLineR2(B, x1, y1, x2, y2, Color, YSign);
        else
            DrawLineR1(B, x1, y1, x2, y2, Color, YSign);
    } else {
        if(x2 - x1 + y1 - y2 < 0)
            DrawLineR2(B, x1, y1, x2, y2, Color, YSign);
        else
            DrawLineR1(B, x1, y1, x2, y2, Color, YSign);
    }
}

void AliasRender::DrawLineR1(Bitmap &B, int x1, int y1, int x2, int y2, const RGBColor &Color, int YSign)
{
    int y = y1;
    int dy = (y2 - y1) * YSign;
    int dxdy = (y2 - y1) * YSign + x1 - x2;
    int F = dxdy;
    int x;
    for(x = x1;x <= x2;x++)
    {
        if(x >= 0 && x < int(B.Width()) && y >= 0 && y < int(B.Height()))
            B[y][x] = Color;
        if( F < 0 )
            F += dy;
        else
        {
            y += YSign;
            F += dxdy;
        }
    }
}

void AliasRender::DrawLineR2(Bitmap &B, int x1, int y1, int x2, int y2, const RGBColor &Color, int YSign)
{
    int x = x1;
    int dx = x2-x1;
    int dxdy = x2-x1+(y1-y2)*YSign;
    int F = x2-x1+(y1-y2)*YSign;
    int y;

    if(YSign == 1)
    {
        for(y = y1;y <= y2;y++)
        {
            if(x >= 0 && x < int(B.Width()) && y >= 0 && y < int(B.Height()))
                B[y][x] = Color;
            if( F < 0 )
                F += dx;
            else
            {
                x++;
                F += dxdy;
            }
        }
    } else {
        for(y = y1;y >= y2;y--)
        {
            if(x >= 0 && x < int(B.Width()) && y >= 0 && y < int(B.Height()))
                B[y][x] = Color;
            if( F < 0 )
                F += dx;
            else
            {
                x++;
                F += dxdy;
            }
        }
    }
}

void AliasRender::DrawPolygon(Bitmap &B, ZBuffer &Z, MeshVertex *V, int vc)
{
    bool ActiveEdges[50];
    bool FirstHitFound, SecondHitFound;
    //edge n = vertex n -> vertex n+1

    int i,ip1,x,XStart,XEnd,XIntersect;
    MeshVertex VStart,VEnd,VIntersect,CurV;
    float Ub,s,CurZ;
    RGBColor CurColor;

    for(i=0;i<vc;i++)
        ActiveEdges[i] = true;

    int MinY = int(V[0].Pos.y),MaxY = int(V[0].Pos.y),CurY;
    for(i=1;i<vc;i++)
    {
        CurY = int(V[i].Pos.y);
        if(CurY < MinY)
        {
            MinY = CurY;
        }
        if(CurY > MaxY) MaxY = CurY;
    }

    if(MinY < -1) MinY = -1;
    if(MaxY > int(B.Height())-1) MaxY = int(B.Height())-1;

    for(CurY=MinY+1;CurY<=MaxY;CurY++)
    {
        FirstHitFound = false;
        SecondHitFound = false;
        for(i=0;i<vc;i++)
        {
            if(ActiveEdges[i])
            {
                ip1 = i + 1;
                if(ip1 == vc) ip1 = 0;
                if(V[i].Pos.y == V[ip1].Pos.y)
                {
                    
                } else {
                    Ub = float(CurY - V[i].Pos.y) / float(V[ip1].Pos.y - V[i].Pos.y);
                    if(Ub >= 0.0f && Ub < 1.0f)
                    {
                        XIntersect = int(V[i].Pos.x + Ub * (V[ip1].Pos.x - V[i].Pos.x) + 0.5f);

                        VIntersect.Pos.z = V[i].Pos.z + Ub * (V[ip1].Pos.z - V[i].Pos.z);
                        VIntersect.Color.r = unsigned char(V[i].Color.r + int(Ub * (V[ip1].Color.r - V[i].Color.r)));
                        VIntersect.Color.g = unsigned char(V[i].Color.g + int(Ub * (V[ip1].Color.g - V[i].Color.g)));
                        VIntersect.Color.b = unsigned char(V[i].Color.b + int(Ub * (V[ip1].Color.b - V[i].Color.b)));

                        if(FirstHitFound)
                        {
                            if(!SecondHitFound)
                            {
                                SecondHitFound = true;
                                XEnd = XIntersect;
                                VEnd = VIntersect;
                            } else {
                                if(XIntersect != XEnd)
                                {
                                    XEnd = XIntersect;
                                    VEnd = VIntersect;
                                }
                            }
                        } else {
                            FirstHitFound = true;
                            XStart = XIntersect;
                            VStart = VIntersect;
                        }
                    }
                }
            }
        }
        
        if(SecondHitFound)
        {
            if(XStart > XEnd)
            {
                Utility::Swap(XStart, XEnd);
                Utility::Swap(VStart, VEnd);
            }
            for(x=XStart;x<XEnd;x++)
            {
                if(x >= 0 && x < int(B.Width()))
                {
                    //s = Math::LinearMap(XStart-1, XEnd, 0.0f, 1.0f, x);
                    s = float(x-(XStart-1))/float(XEnd-(XStart-1));
                    CurZ = VStart.Pos.z + s * (VEnd.Pos.z - VStart.Pos.z);
                    if(CurZ <= Z[CurY][x])
                    {
                        Z[CurY][x] = CurZ;

                        CurColor.r = unsigned char(VStart.Color.r + int(s * (VEnd.Color.r - VStart.Color.r)));
                        CurColor.g = unsigned char(VStart.Color.g + int(s * (VEnd.Color.g - VStart.Color.g)));
                        CurColor.b = unsigned char(VStart.Color.b + int(s * (VEnd.Color.b - VStart.Color.b)));
                        B[CurY][x] = CurColor;
                    }
                }
            }
        }
    }
}
