/*
Camera.h
Written by Matthew Fisher

Camera represents a 3-D free floating camera.  The simplest 3D description of a camera
is an eye location and 3 perpindicular vectors (the look vector, the up vector, and the
cross product of these two, which would be the vector pointing to the left of the viewer.)
Although this could use quaternions, it is easier to keep everything as the 4 vectors
(eye, look, up, left) described above.
*/
#pragma once
class Camera
{
public:
    Camera();
    void Reset(const Vec3f &Eye, const Vec3f &Up, const Vec3f &VecLookAt);    //resets tha camera to the given configuration
    void Reset(float Yaw, float Pitch, const Vec3f &Eye);                //resets tha camera to have the given yaw, pitch, and roll
    
    void Update();                                                //updates the camera's matView Matrix4
    void Update(MatrixController &MC);                            //Update(), but loads into the view Matrix4 of MC
    void Update(MatrixController &MC, GraphicsDevice &GD);        //Update(MC), but loads MC into GD
    
    void PlanarMouse(InputManager &Input, float SlideFactor, float DepthFactor);
    void Mouse(float Factor);                                    //moves the camera's view direction by the mouse
    void WindowKeyboard(WindowObjects &O, float FactorMove, float FactorLook);    //adjusts the camera based upon keyboard input (see Camera.cpp)
    void Keyboard(WindowObjects &O, float Factor);                                //adjusts the camera based upon keyboard input (see Camera.cpp)
    void LookUp(float Theta);
    void LookRight(float Theta);
    void Roll(float Theta);
    void Move(float Distance);
    void Strafe(float Distance);

    __forceinline Plane ViewPlane()
    {
        return Plane::ConstructFromPointNormal(_VecEye, _VecLookDir);
    }

    __forceinline Matrix4& ViewMatrix()
    {
        return _ViewMatrix;
    }
    __forceinline const Matrix4& ViewMatrix() const
    {
        return _ViewMatrix;
    }
    __forceinline Vec3f& VecEye()
    {
        return _VecEye;
    }
    __forceinline const Vec3f& VecEye() const
    {
        return _VecEye;
    }
    __forceinline Vec3f& VecLookDir()
    {
        return _VecLookDir;
    }
    __forceinline const Vec3f& VecLookDir() const
    {
        return _VecLookDir;
    }
    __forceinline Vec3f& VecUp()
    {
        return _VecUp;
    }
    __forceinline const Vec3f& VecUp() const
    {
        return _VecUp;
    }
    __forceinline Vec3f& VecRight()
    {
        return _VecRight;
    }
    __forceinline const Vec3f& VecRight() const
    {
        return _VecRight;
    }
    __forceinline Vec3f& AbsoluteUp()
    {
        return _AbsoluteUp;
    }
    __forceinline const Vec3f& AbsoluteUp() const
    {
        return _AbsoluteUp;
    }

private:
    Vec3f _VecEye, _VecLookDir, _VecUp, _VecRight, _AbsoluteUp;
    Matrix4 _ViewMatrix;
};
