/*
Mutex.h
Written by Matthew Fisher

A mutex is an object that grants a thread exclusive access to a given section of code.
*/

//
// Note: CRITICAL_SECTION stores a ref count each time EnterCriticalSection is called,
// and LeaveCriticalSection must be called once for each EnterCriticalSection.
//
#pragma  once

struct Mutex
{
public:
    Mutex()
    {
        InitializeCriticalSection(&_CriticalSection);
    }
    ~Mutex()
    {
        DeleteCriticalSection(&_CriticalSection);
    }
    __forceinline void Acquire()
    {
        EnterCriticalSection(&_CriticalSection);
    }
    __forceinline void Release()
    {
        LeaveCriticalSection(&_CriticalSection);
    }

private:
    CRITICAL_SECTION _CriticalSection;
};
