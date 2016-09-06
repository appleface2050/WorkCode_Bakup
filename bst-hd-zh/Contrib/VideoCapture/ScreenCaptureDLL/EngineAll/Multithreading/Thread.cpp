/*
Thread.cpp
Written by Matthew Fisher

A thread is a single unit of code execution
*/
#include "..\\..\\Main.h"
#include "Thread.h"

Thread::Thread()
{
    _Handle = NULL;
}

Thread::~Thread()
{
    CloseThread();
}

void Thread::CloseThread()
{
    if(_Handle)
    {
        CloseHandle(_Handle);
        _Handle = NULL;
    }
}

void Thread::Begin(LPTHREAD_START_ROUTINE StartFunction, void *Context)
{
    //
    // without the CREATE_SUSPENDED flag, the thread will start immediately
    //
    _Handle = CreateThread(NULL, 0, StartFunction, Context, 0, NULL);
    PersistentAssert(_Handle != NULL, "CreateThread failed");
}
