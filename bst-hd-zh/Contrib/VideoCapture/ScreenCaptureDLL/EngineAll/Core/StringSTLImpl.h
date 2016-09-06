/*
String.h
Written by Matthew Fisher

String class and related conversions.
*/

#pragma once

class StringSTLImpl
{
public:
    struct LexicographicComparison
    {
        bool operator()(const StringSTLImpl &L, const StringSTLImpl &R) const
        {
            return strcmp(L.CString(), R.CString()) < 0;
        }
    };

    StringSTLImpl()
    {
        FreeMemory();
    }

    ~StringSTLImpl()
    {
        
    }

    //
    // All constructors are made explicit to avoid undesired auto-casting.
    //
    StringSTLImpl(const StringSTLImpl &S)
    {
        FreeMemory();
        *this = S;
    }
    explicit StringSTLImpl(UINT Value)
    {
        FreeMemory();
        *this = Value;
    }
    explicit StringSTLImpl(UINT64 Value)
    {
        FreeMemory();
        *this = Value;
    }
    explicit StringSTLImpl(DWORD Value)
    {
        FreeMemory();
        *this = int(Value);
    }
    explicit StringSTLImpl(int Value)
    {
        FreeMemory();
        *this = Value;
    }
    explicit StringSTLImpl(float Value)
    {
        FreeMemory();
        *this = Value;
    }
    explicit StringSTLImpl(double Value)
    {
        FreeMemory();
        *this = float(Value);
    }
    StringSTLImpl(const char *Text)
    {
        FreeMemory();
        *this = Text;
    }
    explicit StringSTLImpl(char Character)
    {
        FreeMemory();
        *this = Character;
    }
    explicit StringSTLImpl(bool B)
    {
        FreeMemory();
        if(B)
        {
            *this = "true";
        }
        else
        {
            *this = "false";
        }
    }

    //
    // Memory
    //
    __forceinline void FreeMemory()
    {
        _Data.Allocate(1);
        _Data[0] = '\0';
    }

    __forceinline void ReSize(UINT NewLength)
    {
        _Data.ReSize(NewLength + 1);
        _Data.Last() = '\0';
    }

    //
    // Accessors
    //
    __forceinline char* CString()
    {
        return _Data.CArray();
    }

    __forceinline const char* CString() const
    {
        return _Data.CArray();
    }

    __forceinline UINT Length() const
    {
        return _Data.Length() - 1;
    }

    __forceinline char Last() const
    {
        return _Data[_Data.Length() - 2];
    }

    __forceinline char& operator [] (UINT k)
    {
        return _Data[k];
    }

    __forceinline char& operator [] (int k) 
    {
        return _Data[k];
    }

    __forceinline const char& operator [] (UINT k) const
    {
        return _Data[k];
    }

    __forceinline const char& operator [] (int k) const
    {
        return _Data[k];
    }

    //
    // Conversions
    //
    int ConvertToInteger() const
    {
        return atoi(CString());
    }

    ULONG ConvertToUnsignedInteger() const
    {
        return ULONG(_atoi64(CString()));
    }

    LONGLONG ConvertToLongInteger() const
    {
        return _atoi64(CString());
    }

    double ConvertToDouble() const
    {
        return atof(CString());
    }

    bool ConvertToBoolean() const;

    float ConvertToFloat() const
    {
        return float(atof(CString()));
    }

    //
    // Query
    //
    bool ExactMatchAtOffset(const StringSTLImpl &Find, UINT Offset) const;
    bool Contains(const StringSTLImpl &Find) const;
    bool IsNumeric() const;
    bool IsSuffix(const StringSTLImpl &EndCanidate) const;
    bool IsPrefix(const StringSTLImpl &StartCanidate) const;
    void Partition(char Seperator, Vector<StringSTLImpl> &Output) const;
    Vector<StringSTLImpl> Partition(char Seperator) const;
    void PartitionAboutIndex(UINT Index, StringSTLImpl &Left, StringSTLImpl &Right) const;
    void Partition(const StringSTLImpl &Seperator, Vector<StringSTLImpl> &Output) const;
    int FindFirstIndex(char Seperator) const;
    int FindLastIndex(char Seperator) const;
    StringSTLImpl FindAndReplace(const StringSTLImpl &Find, const StringSTLImpl &Replace) const;
    StringSTLImpl MakeLowercase() const;
    StringSTLImpl RemoveSuffix(const StringSTLImpl &EndCandidate) const;
    UINT Hash() const;
    
    //
    // Assignment
    //
    StringSTLImpl& operator = (const char *StringSTLImpl);
    StringSTLImpl& operator = (char Character);
    StringSTLImpl& operator = (float Value);
    StringSTLImpl& operator = (int Value);
    StringSTLImpl& operator = (UINT Value);
    StringSTLImpl& operator = (UINT64 Value);
    StringSTLImpl& operator = (const StringSTLImpl &S);

    //
    // Modifiers
    //
    StringSTLImpl& operator += (const StringSTLImpl &S);
    __forceinline StringSTLImpl& operator += (char C)
    {
        PushEnd(C);
        return *this;
    }

    __forceinline void PushEnd(char C)
    {
        _Data.Last() = C;
        _Data.PushEnd('\0');
    }

    __forceinline void PopEnd()
    {
        _Data.PopEnd();
        _Data.Last() = '\0';
    }

    void PopFront()
    {
        _Data.PopFront();
    }

    //
    // Formatting
    //
    static StringSTLImpl UnsignedIntAsHex(UINT Value);
    static StringSTLImpl ZeroPad(const StringSTLImpl &S, UINT ZeroPadding);

private:
    void ResizeToCStringLength();

    Vector<char> _Data;
};

//
// String Comparison
//
bool operator == (const StringSTLImpl &L, const StringSTLImpl &R);
bool operator == (const char *L, const StringSTLImpl &R);
bool operator == (const StringSTLImpl &R, const char *L);
__forceinline bool operator != (const StringSTLImpl &L, const StringSTLImpl &R) {return !(L == R);}
__forceinline bool operator != (const char *L, const StringSTLImpl &R) {return !(L == R);}
__forceinline bool operator != (const StringSTLImpl &R, const char *L) {return !(L == R);}

//
// String Operations
//
StringSTLImpl operator + (const StringSTLImpl &L, const StringSTLImpl &R);
StringSTLImpl operator + (const char *L, const StringSTLImpl &R);
StringSTLImpl operator + (const StringSTLImpl &R, const char *L);
ostream& operator << (ostream &os, const StringSTLImpl &S);

