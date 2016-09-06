/*
ParameterFile.cpp
Written by Matthew Fisher

the Parameter class loads a paramater file as a simple set of "Parameter=Option" lines.
*/
#include "..\\..\\Main.h"

ParameterFile::ParameterFile(const String &Filename)
{
    AddFile(Filename);
}
void ParameterFile::AddFile(const String &Filename)
{
    PersistentAssert(Utility::FileExists(Filename), "Parameter file not found");
    ifstream File(Filename.CString());
    Vector<String> Lines;
    Utility::GetFileLines(File, Lines);
    for(UINT i = 0; i < Lines.Length(); i++)
    {
        String &CurLine = Lines[i];
        if(CurLine.IsPrefix("#include "))
        {
            Vector<String> Partition;
            CurLine.Partition("#include ", Partition);
            PersistentAssert(Partition.Length() == 1, String("Invalid line in parameter file: ") + CurLine);
            String IncludeFilename = Partition[0];
            IncludeFilename.Partition("\"", Partition);
            PersistentAssert(Partition.Length() == 1, String("Invalid line in parameter file: ") + CurLine);
            AddFile(Partition[0]);
        }
        else if(CurLine.Length() > 2)
        {
            Vector<String> Partition;
            CurLine.Partition('=', Partition);
            PersistentAssert(Partition.Length() == 2, String("Invalid line in parameter file: ") + CurLine);
            Parameters.PushEnd(ParameterEntry(Partition[0], Partition[1]));
        }
    }
}

String ParameterFile::GetString(const String &ParameterName)
{
    for(UINT i = 0; i < Parameters.Length(); i++)
    {
        if(Parameters[i].Name.MakeLowercase() == ParameterName.MakeLowercase())
        {
            return Parameters[i].Value;
        }
    }
    PersistentSignalError(String("Parameter not found: ") + ParameterName);
    return String("Parameter not found");
}

bool ParameterFile::GetBoolean(const String &ParameterName)
{
    String Str = GetString(ParameterName);
    Str = Str.MakeLowercase();
    if(Str == "true")
    {
        return true;
    }
    else if(Str == "false")
    {
        return false;
    }
    else
    {
        SignalError(String("Invalid boolean value: ") + Str);
        return false;
    }
}

int ParameterFile::GetInteger(const String &ParameterName)
{
    return GetString(ParameterName).ConvertToInteger();
}

UINT ParameterFile::GetUnsignedInteger(const String &ParameterName)
{
    return GetString(ParameterName).ConvertToUnsignedInteger();
}

double ParameterFile::GetDouble(const String &ParameterName)
{
    return GetString(ParameterName).ConvertToDouble();
}

float ParameterFile::GetFloat(const String &ParameterName)
{
    return GetString(ParameterName).ConvertToFloat();
}

Vec3f ParameterFile::GetVec3(const String &ParameterName)
{
    String VecString = GetString(ParameterName);
    Vector<String> Elements;
    VecString.Partition(' ', Elements);
    Assert(Elements.Length() == 3, "Vector with invalid element count");
    Vec3f Result;
    for(UINT i = 0; i < 3; i++)
    {
        Result[i] = Elements[i].ConvertToFloat();
    }
    return Result;
}