/*
String.cpp
Written by Matthew Fisher

Implementation of StringSTLImpl class.
*/
#include "..\\..\\Main.h"
#include "StringSTLImpl.h"

StringSTLImpl StringSTLImpl::UnsignedIntAsHex(UINT Value)
{
    StringSTLImpl S;
    S.ReSize(20);
    sprintf(S.CString(), "%x", Value);
    S.ResizeToCStringLength();
    return S;
}

StringSTLImpl StringSTLImpl::ZeroPad(const StringSTLImpl &S, UINT ZeroPadding)
{
    StringSTLImpl Prefix;
    for(UINT i = S.Length(); i < ZeroPadding; i++)
    {
        Prefix.PushEnd('0');
    }
    return Prefix + S;
}

bool StringSTLImpl::ConvertToBoolean() const
{
    if(*this == "true")
    {
        return true;
    }
    if(*this == "false")
    {
        return false;
    }
    SignalError("Invalid StringSTLImpl -> boolean conversion");
    return false;
}

void StringSTLImpl::ResizeToCStringLength()
{
    for(UINT i = 0; i < _Data.Length(); i++)
    {
        if(_Data[i] == '\0')
        {
            _Data.ReSize(i + 1);
            return;
        }
    }
    SignalError("No null terminator found in StringSTLImpl::ResizeToCStringLength");
}

StringSTLImpl& StringSTLImpl::operator = (float Value)
{
    ostringstream S(ostringstream::out);
    S << Value;
    *this = S.str().c_str();
    return *this;
}

StringSTLImpl& StringSTLImpl::operator = (char Character)
{
    _Data.ReSize(2);
    _Data[0] = Character;
    _Data[1] = '\0';
    return *this;
}

StringSTLImpl& StringSTLImpl::operator = (int Value)
{
    ostringstream S(ostringstream::out);
    S << Value;
    *this = S.str().c_str();
    return *this;
}

StringSTLImpl& StringSTLImpl::operator = (UINT Value)
{
    ostringstream S(ostringstream::out);
    S << Value;
    *this = S.str().c_str();
    return *this;
}

StringSTLImpl& StringSTLImpl::operator = (UINT64 Value)
{
    ostringstream S(ostringstream::out);
    S << Value;
    *this = S.str().c_str();
    return *this;
}

StringSTLImpl& StringSTLImpl::operator = (const char *String)
{
    UINT NewLength = UINT(strlen(String)) + 1;
    _Data.ReSize(NewLength);
    memcpy(CString(), String, NewLength);
    return *this;
}

StringSTLImpl& StringSTLImpl::operator = (const StringSTLImpl &S)
{
    _Data = S._Data;
    return *this;
}

StringSTLImpl& StringSTLImpl::operator += (const StringSTLImpl &S)
{
    UINT BaseLength = Length();
    _Data.ReSize(BaseLength + S.Length() + 1);
    for(UINT i = 0; i < S.Length(); i++)
    {
        _Data[i + BaseLength] = S[i];
    }
    _Data.Last() = '\0';
    return *this;
}

StringSTLImpl StringSTLImpl::MakeLowercase() const
{
    StringSTLImpl Result = *this;
    for(UINT i = 0; i < Result.Length(); i++)
    {
        Result[i] = tolower(Result[i]);
    }
    return Result;
}

void StringSTLImpl::Partition(char Seperator, Vector<StringSTLImpl> &Output) const
{
    Output.FreeMemory();
    StringSTLImpl CurEntry;
    for(UINT CurIndex = 0; CurIndex < Length(); CurIndex++)
    {
        if((*this)[CurIndex] == Seperator)
        {
            if(CurEntry.Length() > 0)
            {
                Output.PushEnd(CurEntry);
                CurEntry.FreeMemory();
            }
        }
        else
        {
            CurEntry.PushEnd((*this)[CurIndex]);
        }
    }
    if(CurEntry.Length() > 0)
    {
        Output.PushEnd(CurEntry);
    }
}

Vector<StringSTLImpl> StringSTLImpl::Partition(char Seperator) const
{
    Vector<StringSTLImpl> Result;
    Partition(Seperator, Result);
    return Result;
}

int StringSTLImpl::FindFirstIndex(char Seperator) const
{
    UINT L = Length();
    for(UINT CurIndex = 0; CurIndex < L; CurIndex++)
    {
        if((*this)[CurIndex] == Seperator)
        {
            return CurIndex;
        }
    }
    return -1;
}

int StringSTLImpl::FindLastIndex(char Seperator) const
{
    UINT L = Length();
    for(int CurIndex = L - 1; CurIndex >= 0; CurIndex--)
    {
        if((*this)[CurIndex] == Seperator)
        {
            return CurIndex;
        }
    }
    return -1;
}

void StringSTLImpl::PartitionAboutIndex(UINT Index, StringSTLImpl &Left, StringSTLImpl &Right) const
{
    UINT L = Length();
    Assert(Index < L, "Index invalid in StringSTLImpl::PartitionAboutIndex");
    Left.ReSize(Index);
    Right.ReSize(L - Index - 1);
    for(UINT LeftIndex = 0; LeftIndex < Index; LeftIndex++)
    {
        Left[LeftIndex] = (*this)[LeftIndex];
    }
    for(UINT RightIndex = 0; RightIndex < Right.Length(); RightIndex++)
    {
        Right[RightIndex] = (*this)[Index + 1 + RightIndex];
    }
}

void StringSTLImpl::Partition(const StringSTLImpl &Seperator, Vector<StringSTLImpl> &Output) const
{
    Assert(Seperator.Length() >= 1, "empty seperator");
    Output.FreeMemory();
    StringSTLImpl CurEntry;
    for(UINT CurIndex = 0; CurIndex < Length(); CurIndex++)
    {
        bool IsSeperator = true;
        for(UINT TestIndex = 0; TestIndex < Seperator.Length() && CurIndex + TestIndex < Length() && IsSeperator; TestIndex++)
        {
            if((*this)[CurIndex + TestIndex] != Seperator[TestIndex])
            {
                IsSeperator = false;
            }
        }

        if(IsSeperator)
        {
            if(CurEntry.Length() > 0)
            {
                Output.PushEnd(CurEntry);
                CurEntry.FreeMemory();
            }
            CurIndex += Seperator.Length() - 1;
        }
        else
        {
            CurEntry.PushEnd((*this)[CurIndex]);
        }
    }
    if(CurEntry.Length() > 0)
    {
        Output.PushEnd(CurEntry);
    }
}

bool StringSTLImpl::ExactMatchAtOffset(const StringSTLImpl &Find, UINT Offset) const
{
    UINT MatchLength = 0;
    for(UINT Index = 0; Index + Offset < Length() && Index < Find.Length(); Index++)
    {
        if((*this)[Index + Offset] == Find[Index])
        {
            MatchLength++;
            if(MatchLength == Find.Length())
            {
                return true;
            }
        }
    }
    return false;
}

bool StringSTLImpl::Contains(const StringSTLImpl &Find) const
{
    for(UINT Index = 0; Index < Length(); Index++)
    {
        if(ExactMatchAtOffset(Find, Index))
        {
            return true;
        }
    }
    return false;
}

StringSTLImpl StringSTLImpl::FindAndReplace(const StringSTLImpl &Find, const StringSTLImpl &Replace) const
{
    StringSTLImpl Result;
    for(UINT Index = 0; Index < Length(); Index++)
    {
        if(ExactMatchAtOffset(Find, Index))
        {
            Result += Replace;
            Index += Find.Length() - 1;
        }
        else
        {
            Result += (*this)[Index];
        }
    }
    return Result;
}

bool StringSTLImpl::IsNumeric() const
{
    return (Length() > 0 &&
           (ConvertToDouble() != 0.0 ||
            _Data[0] == '0' ||
            _Data[0] == '.' ||
            _Data[0] == '-'));
}

bool StringSTLImpl::IsSuffix(const StringSTLImpl &EndCanidate) const
{
    if(Length() < EndCanidate.Length())
    {
        return false;
    }
    for(UINT i = 0; i < EndCanidate.Length(); i++)
    {
        if((*this)[Length() - EndCanidate.Length() + i] != EndCanidate[i])
        {
            return false;
        }
    }
    return true;
}

StringSTLImpl StringSTLImpl::RemoveSuffix(const StringSTLImpl &EndCandidate) const
{
    Assert(IsSuffix(EndCandidate), "Removing invalid suffix");
    StringSTLImpl Result = *this;
    for(UINT i = 0; i < EndCandidate.Length(); i++)
    {
        Result.PopEnd();
    }
    return Result;
}

bool StringSTLImpl::IsPrefix(const StringSTLImpl &StartCanidate) const
{
    if(Length() < StartCanidate.Length())
    {
        return false;
    }
    for(UINT i = 0; i < StartCanidate.Length(); i++)
    {
        if((*this)[i] != StartCanidate[i])
        {
            return false;
        }
    }
    return true;
}

UINT StringSTLImpl::Hash() const
{
    return Utility::Hash32((const BYTE *)_Data.CArray(), _Data.Length());
}

//
// Overloaded operators
//

StringSTLImpl operator + (const StringSTLImpl &L, const StringSTLImpl &R)
{
    StringSTLImpl Ret = L;
    for(UINT i = 0; i < R.Length(); i++)
    {
        Ret.PushEnd(R[i]);
    }
    return Ret;
}

StringSTLImpl operator + (const char *L, const StringSTLImpl &R)
{
    StringSTLImpl Ret = L;
    for(UINT i = 0; i < R.Length(); i++)
    {
        Ret.PushEnd(R[i]);
    }
    return Ret;
}

StringSTLImpl operator + (const StringSTLImpl &L, const char *R)
{
    UINT RLen = UINT(strlen(R));
    StringSTLImpl Ret = L;
    for(UINT i = 0; i < RLen; i++)
    {
        Ret.PushEnd(R[i]);
    }
    return Ret;
}

ostream& operator << (ostream &os, const StringSTLImpl &S)
{
    if(S.Length() > 0)
    {
        os.write(S.CString(), S.Length());
    }
    return os;
}

bool operator == (const StringSTLImpl &L, const StringSTLImpl &R)
{
    UINT LLen = L.Length();
    UINT RLen = R.Length();
    if(LLen != RLen)
    {
        return false;
    }
    for(UINT i = 0; i < LLen; i++)
    {
        if(L[i] != R[i])
        {
            return false;
        }
    }
    return true;
}

bool operator == (const char *L, const StringSTLImpl &R)
{
    UINT LLen = UINT(strlen(L));
    UINT RLen = R.Length();
    if(LLen != RLen)
    {
        return false;
    }
    for(UINT i = 0; i < LLen; i++)
    {
        if(L[i] != R[i])
        {
            return false;
        }
    }
    return true;
}

bool operator == (const StringSTLImpl &R, const char *L)
{
    return (L == R);
}
