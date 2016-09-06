#include "AbstractImageSource.h"

using namespace CLROBS;

AbstractImageSource::AbstractImageSource()
{
    size = gcnew Vector2(0.0f, 0.0f);
}

 
Vector2^ AbstractImageSource::Size::get()
{ 
    return size; 
}
void AbstractImageSource::Size::set(Vector2^ size) 
{ 
    this->size = size; 
}

void AbstractImageSource::Preprocess() 
{
}

void AbstractImageSource::ExecuteJS(String^ lines)
{
}

void AbstractImageSource::Tick(float fSeconds) 
{
}

void AbstractImageSource::BeginScene() 
{
}

void AbstractImageSource::EndScene() 
{
}