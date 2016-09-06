/*
Camera.cpp
Written by Matthew Fisher

Camera represents a 3-D free floating camera.  The simplest 3D description of a camera
is an eye location and 3 perpindicular vectors (the look vector, the up vector, and the
cross product of these two, which would be the vector pointing to the left of the viewer.)
Although this could use quaternions, it is easier to keep everything as the 4 vectors
(eye, look, up, left) described above.  See Camera.h for a description of each function.
*/
#include "..\\..\\Main.h"
#include "Camera.h"

Camera::Camera()
{
    _VecEye = Vec3f(0.0f,0.0f,0.0f);
    _VecUp = Vec3f(0.0f,1.0f,0.0f);
    _VecRight = Vec3f(1.0f,0.0f,0.0f);
    _VecLookDir = Vec3f(0.0f,0.0f,1.0f);
    _AbsoluteUp = Vec3f(0.0f,1.0f,0.0f);
    Update();
}

void Camera::Reset(float Yaw, float Pitch, const Vec3f &Eye)
{
    _VecEye = Eye;
    _VecUp = Vec3f(0.0f, 0.0f, 1.0f);
    _VecRight = Vec3f(0.0f , 1.0f, 0.0f);
    _VecLookDir = Vec3f(1.0f, 0.0f, 0.0f);
    _AbsoluteUp = Vec3f(0.0f, 0.0f, 1.0f);
    
    LookRight(Yaw);
    LookUp(Pitch);

    Update();
}

void Camera::Reset(const Vec3f &Eye, const Vec3f &Up, const Vec3f &VecLookAt)
{
    _VecEye = Eye;
    _AbsoluteUp = Up;
    _VecLookDir = Vec3f::Normalize(_VecEye - VecLookAt);
    _VecRight = Vec3f::Normalize(Vec3f::Cross(_AbsoluteUp, _VecLookDir));
    _VecUp = Vec3f::Normalize(Vec3f::Cross(_VecLookDir, _VecRight));

    Update();
}

void Camera::Update()
{
    _ViewMatrix = Matrix4::Camera(_VecEye, _VecLookDir, _VecUp, -_VecRight);
}

void Camera::Update(MatrixController &MC)
{
    Update();
    MC.View = _ViewMatrix;
}

void Camera::Update(MatrixController &MC, GraphicsDevice &GD)
{
    Update(MC);
    GD.LoadMatrix(MC);
}

void Camera::PlanarMouse(InputManager &Input, float SlideFactor, float DepthFactor)
{
    if(Input.Mouse().Buttons[MouseButtonLeft])
    {
        _VecEye -= _VecRight * (SlideFactor * Input.MouseDiff().Pos.x * 0.01f);
        _VecEye -= _VecUp * (SlideFactor * Input.MouseDiff().Pos.y * 0.01f);
    }
    _VecEye -= _VecLookDir * DepthFactor * float(Input.MouseDiff().Wheel) / 120.0f;
    if(Input.KeyCheck(KEY_PGUP))
    {
        _VecEye += _VecLookDir * DepthFactor;
    }
    if(Input.KeyCheck(KEY_PGDN))
    {
        _VecEye -= _VecLookDir * DepthFactor;
    }
    if(_VecEye.z < 0.025f)
    {
        _VecEye.z = 0.025f;
    }
}

void Camera::Mouse(float Factor)
{
    POINT P;
    GetCursorPos(&P);
    LookRight(Factor * (P.x - 320));        //x controls left-right
    LookUp(Factor * (P.y - 200));        //y controls up-down
    SetCursorPos(320, 200);
}

void Camera::Keyboard(WindowObjects &O, float Factor)
{
    //Unreal Tournament style keyboard setup
    if(O.GetInputManager().KeyCheck(KEY_W)) Move(-O.GetFrameTimer().SPF() * Factor);
    if(O.GetInputManager().KeyCheck(KEY_S)) Move(O.GetFrameTimer().SPF() * Factor);

    if(O.GetInputManager().KeyCheck(KEY_A)) Strafe(-O.GetFrameTimer().SPF() * Factor);
    if(O.GetInputManager().KeyCheck(KEY_D)) Strafe(O.GetFrameTimer().SPF() * Factor);
}

void Camera::WindowKeyboard(WindowObjects &O, float FactorMove, float FactorLook)
{
    //Unreal Tournament style keyboard setup
    if(O.GetInputManager().KeyCheck(KEY_W)) Move(-O.GetFrameTimer().SPF() * FactorMove);
    if(O.GetInputManager().KeyCheck(KEY_S)) Move(O.GetFrameTimer().SPF() * FactorMove);

    if(O.GetInputManager().KeyCheck(KEY_A)) Strafe(O.GetFrameTimer().SPF() * FactorMove);
    if(O.GetInputManager().KeyCheck(KEY_D)) Strafe(-O.GetFrameTimer().SPF() * FactorMove);

    //Use the numberpad to replace the mouse
    if(O.GetInputManager().KeyCheck(KEY_NUMPAD4)) LookRight(-O.GetFrameTimer().SPF() * FactorLook);
    if(O.GetInputManager().KeyCheck(KEY_NUMPAD6)) LookRight(O.GetFrameTimer().SPF() * FactorLook);

    if(O.GetInputManager().KeyCheck(KEY_NUMPAD8)) LookUp(-O.GetFrameTimer().SPF() * FactorLook);
    if(O.GetInputManager().KeyCheck(KEY_NUMPAD2)) LookUp(O.GetFrameTimer().SPF() * FactorLook);

    if(O.GetInputManager().KeyCheck(KEY_NUMPAD7)) Roll(O.GetFrameTimer().SPF() * FactorLook);
    if(O.GetInputManager().KeyCheck(KEY_NUMPAD9)) Roll(-O.GetFrameTimer().SPF() * FactorLook);
}

void Camera::LookUp(float Theta)
{
    Matrix4 Rotation = Matrix4::Rotation(_VecRight, Theta);
    _VecUp = Rotation.TransformPoint(_VecUp);
    _VecRight = Rotation.TransformPoint(_VecRight);
    _VecLookDir = Rotation.TransformPoint(_VecLookDir);
}

void Camera::LookRight(float Theta)
{
    Matrix4 Rotation = Matrix4::Rotation(_AbsoluteUp, Theta);
    _VecUp = Rotation.TransformPoint(_VecUp);
    _VecRight = Rotation.TransformPoint(_VecRight);
    _VecLookDir = Rotation.TransformPoint(_VecLookDir);
}

void Camera::Roll(float Theta)
{
    Matrix4 Rotation = Matrix4::Rotation(_VecLookDir, Theta);
    _VecUp = Rotation.TransformPoint(_VecUp);
    _VecRight = Rotation.TransformPoint(_VecRight);
    _VecLookDir = Rotation.TransformPoint(_VecLookDir);
}

void Camera::Move(float Distance)
{
    _VecEye += _VecLookDir * Distance;
}

void Camera::Strafe(float Distance)
{
    _VecEye += _VecRight * Distance;
}
