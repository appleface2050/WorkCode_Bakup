/*
FrameController.h
Written by Matthew Fisher

Abstract class for an object which controls all rendering each frame.
*/

class FrameController
{
public:
    virtual void FreeMemory(WindowObjects &O) {}
    virtual void Init(WindowObjects &O) = 0;    //called only one at the beginning of our application
    virtual void ReInit(WindowObjects &O) = 0;  //called after we lose focus, telling us to restore
                                                //our textures and other data that may have been list
    virtual void Render(WindowObjects &O) = 0;  //called each frame
    virtual void ReleaseObjects(WindowObjects &O) = 0; //called to release graphics card data
};
