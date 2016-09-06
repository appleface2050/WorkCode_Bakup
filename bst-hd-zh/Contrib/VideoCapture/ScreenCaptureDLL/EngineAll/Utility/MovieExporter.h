/*
MovieExporter.h
Written by Matthew Fisher

MovieExporter manages screen capture and exports as a sequence of images for compilation into a movie.
*/
#pragma  once
class MovieExporter
{
public:
    void Init(const String &BaseFilename, const String &ImageListFilename, const String &MaskFilename);
    void ExportScreen(GraphicsDevice &GD, WindowManager &WM);

private:
    Bitmap _Mask;
    UINT _CurImageIndex;
    String _BaseFilename;
    ofstream _ImageList;
};
