/*
ParameterFile.h
Written by Matthew Fisher

the Parameter class loads a paramater file as a simple set of "Parameter=Option" lines.
*/
#pragma  once
struct ParameterEntry
{
    ParameterEntry()
    {

    }
    ParameterEntry(const String &_Name, const String &_Value)
    {
        Name = _Name;
        Value = _Value;
        if(Name.Last() == ' ')
        {
            Name.PopEnd();
        }
    }
    String Name;
    String Value;
};

class ParameterFile
{
public:
    ParameterFile(const String &Filename);
    String GetString(const String &ParameterName);
    int GetInteger(const String &ParameterName);
    bool GetBoolean(const String &ParameterName);
    UINT GetUnsignedInteger(const String &ParameterName);
    double GetDouble(const String &ParameterName);
    float GetFloat(const String &ParameterName);
    Vec3f GetVec3(const String &ParameterName);

private:
    void AddFile(const String &Filename);

    Vector<ParameterEntry> Parameters;
};

