/*
Profiler.h
Written by Matthew Fisher

Profiler is used to record the percent of time spent in code.
*/
#pragma  once
struct ProfilerEntry
{
    ProfilerEntry();
    ProfilerEntry(const String &_Description, float _Time);
    ProfilerEntry(const ProfilerEntry &Entry);

    ProfilerEntry& operator = (const ProfilerEntry &Entry);

    String Description;
    float Time;
};

class Profiler
{
public:
    void AddEntry(UINT Index, const String &Description);
    void Reset(UINT StartState);
    void ChangeState(UINT Index);
    String Report();

private:
    Vector<ProfilerEntry> _Entries;
    Clock _Clock;
    float _PreviousTime;
    UINT _PreviousState;
};
