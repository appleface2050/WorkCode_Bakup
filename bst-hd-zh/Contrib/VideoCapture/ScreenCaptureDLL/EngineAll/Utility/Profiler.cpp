/*
Profiler.cpp
Written by Matthew Fisher

Profiler is used to record the percent of time spent in code.
*/
#include "..\\..\\Main.h"

ProfilerEntry::ProfilerEntry()
{

}

ProfilerEntry::ProfilerEntry(const ProfilerEntry &Entry)
{
    Description = Entry.Description;
    Time = Entry.Time;
}

ProfilerEntry::ProfilerEntry(const String &_Description, float _Time)
{
    Description = _Description;
    Time = _Time;
}

ProfilerEntry& ProfilerEntry::operator = (const ProfilerEntry &Entry)
{
    Description = Entry.Description;
    Time = Entry.Time;
    return *this;
}

void Profiler::AddEntry(UINT Index, const String &Description)
{
#ifdef PROFILING_ENABLED
    if(_Entries.Length() < Index + 1)
    {
        _Entries.ReSize(Index + 1);
    }
    _Entries[Index] = ProfilerEntry(Description, 0.0f);
#endif
}

void Profiler::Reset(UINT StartStateIndex)
{
#ifdef PROFILING_ENABLED
    for(UINT Index = 0; Index < _Entries.Length(); Index++)
    {
        _Entries[Index].Time = 0.0f;
    }
    _PreviousTime = _Clock.Elapsed();
    _PreviousState = StartStateIndex;
#endif
}

void Profiler::ChangeState(UINT NewStateIndex)
{
#ifdef PROFILING_ENABLED
    float NewTime = _Clock.Elapsed();
    _Entries[_PreviousState].Time += NewTime - _PreviousTime;
    _PreviousState = NewStateIndex;
    _PreviousTime = NewTime;
#endif
}

String Profiler::Report()
{
#ifdef PROFILING_ENABLED
    ChangeState(_PreviousState);
    String Result;
    float TotalTime = 0.0f;
    for(UINT Index = 0; Index < _Entries.Length(); Index++)
    {
        TotalTime += _Entries[Index].Time;
    }
    Result = String("Total time: ") + String(TotalTime) + String("s\n");
    for(UINT Index = 0; Index < _Entries.Length(); Index++)
    {
        Result += _Entries[Index].Description + String(": ") + String(_Entries[Index].Time / TotalTime * 100.0f) + String("%, ") + String(_Entries[Index].Time) + String("s\n");
    }
    return Result;
#else
    return "Profiling disabled";
#endif
}