/*
GUISlider.h
Written by Matthew Fisher

GUISlider is a draggable slider GUIElement.
*/
#pragma  once
class GUISlider : public GUIElement
{
public:
    GUISlider() {}
    GUISlider(const Rectangle2f &Dimensions, const String &Name, float LowRange, float HighRange, UINT SliderUpdateEventId)
    {
        _Dimensions = Dimensions;
        _SliderUpdateEventId = SliderUpdateEventId;
        _Name = Name;
        _LowRange = LowRange;
        _HighRange = HighRange;
        _Value = (_LowRange + _HighRange) * 0.5f;
    }

    void MouseDown(const Vec3f &MousePos, GUIController &Parent);
    void MouseDrag(const Vec3f &MousePos, GUIController &Parent);
    void MouseUp(const Vec3f &MousePos, GUIController &Parent);
    float GetFloatValue();
    
    void Render(GraphicsDevice &GD, WindowManager &WM);

private:
    void UpdateValue(const Vec3f &MousePos, GUIController &Parent);

    UINT _SliderUpdateEventId;
    Mesh _SliderMesh, _MarkerMesh;
    float _LowRange;
    float _HighRange;
    float _Value;
};