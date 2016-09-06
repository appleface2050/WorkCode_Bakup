/*
Vector.h
Written by Matthew Fisher

Template vector library.  Extends functionality of std::vector
*/

#pragma once
#include "..\\..\\Main.h"
#include "Asserts.h"

//
// Intellisense for custom types
//
//http://thejefffiles.com/blog/autoexpdat-your-key-to-better-debugging-part-1/

#pragma warning(disable : 4700)

template <class T> class Vector
{
public:
    Vector()
    {
        _Data = NULL;
        _Length = 0;
        _Capacity = 0;
    }

    explicit Vector(unsigned int Size)
    {
        _Data = new T[Size];
        _Length = Size;
        _Capacity = Size;
    }

    Vector(const Vector<T> &V)
    {
        const unsigned int Length = V._Length;
        _Length = Length;
        _Capacity = Length;
        if(Length == 0)
        {
            _Data = NULL;
        }
        else
        {
            _Data = new T[Length];
            for(unsigned int Index = 0; Index < Length; Index++)
            {
                _Data[Index] = V._Data[Index];
            }
        }
    }

    ~Vector()
    {
        if(_Data != NULL)
        {
            delete[] _Data;
        }
    }
    
    //
    // Memory
    //
    void FreeMemory()
    {
        if(_Data != NULL)
        {
            delete[] _Data;
            _Data = NULL;
        }
        _Length = 0;
        _Capacity = 0;
    }

    void DeleteMemory()
    {
        if(_Data != NULL)
        {
            for(UINT Index = 0; Index < _Length; Index++)
            {
                delete _Data[Index];
            }
            delete[] _Data;
            _Data = NULL;
        }
        _Length = 0;
        _Capacity = 0;
    }

    //
    // Windows has claimed the nice name "ZeroMemory" for its own uses
    //
    void ZeroMem()
    {
        if(_Length > 0)
        {
            memset(_Data, 0, _Length * sizeof(T));
        }
    }

    void operator = (const Vector<T> &V)
    {
        const unsigned int Length = V._Length;
        _Length = Length;
        _Capacity = Length;
        if(_Data != NULL)
        {
            delete[] _Data;
        }
        if(Length == 0)
        {
            _Data = NULL;
        }
        else
        {
            _Data = new T[Length];
            for(unsigned int Index = 0; Index < Length; Index++)
            {
                _Data[Index] = V._Data[Index];
            }
        }
    }

    void Allocate(unsigned int Size)
    {
        if(Size == 0)
        {
            FreeMemory();
        }
        /*else if(_Length == Size)
        {
            if(typeid(T) != typeid(int) &&
               typeid(T) != typeid(unsigned int) &&
               typeid(T) != typeid(double) &&
               typeid(T) != typeid(float) &&
               typeid(T) != typeid(char) &&
               typeid(T) != typeid(unsigned char))
            {
                T Value;
                for(unsigned int Index = 0; Index < Size; Index++)
                {
                    _Data[Index] = Value;
                }
            }
        }*/
        else
        {
            if(_Data != NULL)
            {
                delete[] _Data;
            }
            _Data = new T[Size];
            _Length = Size;
            _Capacity = Size;
        }
    }

    void Allocate(unsigned int Size, const T &ClearValue)
    {
        Allocate(Size);
        Clear(ClearValue);
    }

    void ReSize(unsigned int Size)
    {
        if(Size == 0)
        {
            FreeMemory();
        }
        else if(Size <= _Capacity)
        {
            _Length = Size;
            if(Size == 0)
            {
                if(_Data != NULL)
                {
                    delete[] _Data;
                    _Capacity = 0;
                }
            }
        }
        else
        {
            unsigned int NewCapacity = Size * 3 / 2 + 4;
            T *NewData = new T[NewCapacity];
            const unsigned int Length = _Length;
            for(unsigned int Index = 0; Index < Length; Index++)
            {
                NewData[Index] = _Data[Index];
            }
            if(_Data)
            {
                delete[] _Data;
            }
            _Data = NewData;
            _Length = Size;
            _Capacity = NewCapacity;
        }
    }

    void Reserve(unsigned int Size)
    {
        if(Size > _Capacity)
        {
            T *NewData = new T[Size];
            if(_Data != NULL)
            {
                const unsigned int Length = _Length;
                for(unsigned int Index = 0; Index < Length; Index++)
                {
                    NewData[Index] = _Data[Index];
                }
                delete[] _Data;
            }
            _Data = NewData;
            _Capacity = Size;
        }
    }

    //
    // Accessors
    //
    __forceinline T& operator [] (unsigned int k)
    {
#ifdef VECTOR_DEBUG
        if(k >= _Length)
        {
            SignalError("Out-of-bounds vector access");
        }
#endif
        return _Data[k];
    }
    __forceinline T& operator [] (int k) 
    {
#ifdef VECTOR_DEBUG
        if(k < 0 || k >= int(_Length))
        {
            SignalError("Out-of-bounds vector access");
        }
#endif
        return _Data[k];
    }
    __forceinline const T& operator [] (unsigned int k) const
    {
#ifdef VECTOR_DEBUG
        if(k >= _Length)
        {
            SignalError("Out-of-bounds vector access");
        }
#endif
        return _Data[k];
    }
    __forceinline const T& operator [] (int k) const
    {
#ifdef VECTOR_DEBUG
        if(k < 0 || k >= int(_Length))
        {
            SignalError("Out-of-bounds vector access");
        }
#endif
        return _Data[k];
    }
    __forceinline unsigned int Length() const
    {
        return _Length;
    }
    __forceinline T* CArray()
    {
#ifdef VECTOR_DEBUG
        if(_Length == 0)
        {
            SignalError("CArray called on zero-length vector");
        }
#endif
        return _Data;
    }
    __forceinline const T* CArray() const
    {
#ifdef VECTOR_DEBUG
        if(_Length == 0)
        {
            SignalError("CArray called on zero-length vector");
        }
#endif
        return _Data;
    }
    __forceinline T& RandomElement()
    {
        Assert(_Length > 0, "RandomElement called with no elements");
        return _Data[rand() % _Length];
    }
    __forceinline const T& RandomElement() const
    {
        Assert(_Length > 0, "RandomElement called with no elements");
        return _Data[rand() % _Length];
    }
    __forceinline T& Last()
    {
        Assert(_Length > 0, "Last called with no elements");
        return _Data[_Length - 1];
    }
    __forceinline const T& Last() const
    {
        Assert(_Length > 0, "Last called with no elements");
        return _Data[_Length - 1];
    }
    __forceinline T& First()
    {
        Assert(_Length > 0, "First called with no elements");
        return _Data[0];
    }
    __forceinline const T& First() const
    {
        Assert(_Length > 0, "First called with no elements");
        return _Data[0];
    }

    //
    // Modifiers
    //
    void Append(const Vector<T> &V);
    void PushEnd(const T &t);
    void PushEnd();
    void PopEnd();
    void PopFront();
    void RemoveSlow(unsigned int Index);
    void RemoveSwap(unsigned int Index);
    void Randomize();
    void Sort();
    void Scale(const T &t);
    template<class orderingType> void Sort(orderingType Function)
    {
        if(_Length >= 2)
        {
            std::sort(_Data, _Data + _Length, Function);
        }
    }
    void Clear(const T &T);

    //
    // Query
    //
    T Sum() const;
    const T& MaxValue() const;
    unsigned int MaxIndex() const;
    const T& MinValue() const;
    unsigned int MinIndex() const;
    bool Contains(const T &t);
    unsigned int Hash32() const;
    unsigned __int64 Hash64() const;

    //
    // File
    //
    void SaveToASCIIFile(const String &Filename)
    {
        ofstream File(Filename.CString());
        PersistentAssert(!File.fail(), "Failed to open file");
        File << _Length << endl;
        for(unsigned int Index = 0; Index < _Length; Index++)
        {
            File << _Data[Index] << '\n';
        }
    }

    void LoadFromASCIIFile(const String &Filename)
    {
        ifstream File(Filename.CString());
        PersistentAssert(!File.fail(), "Failed to open file");
        unsigned int Length;
        File >> Length;
        Allocate(Length);
        for(unsigned int Index = 0; Index < Length; Index++)
        {
            File >> _Data[Index];
        }
    }

protected:
    //vector<T> Data;
    T *_Data;
    unsigned int _Length;
    unsigned int _Capacity;
};

#pragma warning(default : 4700)

//#include "Vector.cpp"
/*
Vector.cpp
Written by Matthew Fisher
*/

#pragma once

#include "Vector.h"

template <class T> void Vector<T>::Clear(const T &Value)
{
  const unsigned int Length = _Length;
  T *CPtr = _Data;
  for(unsigned int i = 0; i < Length; i++)
  {
    CPtr[i] = Value;
  }
}

#pragma warning(disable : 4700)

template <class T> T Vector<T>::Sum() const
{
  const unsigned int Length = _Length;
  if(_Length == 0)
  {
    T Result;
    return Result;
  }
  else
  {
    const T *CPtr = _Data;
    T Result = CPtr[0];
    for(unsigned int Index = 1; Index < Length; Index++)
    {
      Result += CPtr[Index];
    }
    return Result;
  }
}

template <class T> unsigned int Vector<T>::MaxIndex() const
{
  Assert(_Length > 0, "MaxIndex called on zero-length vector");
  const unsigned int Length = _Length;
  const T *CPtr = _Data;
  unsigned int LargestIndexSeen = 0;
  for(unsigned int Index = 1; Index < Length; Index++)
  {
    if(CPtr[Index] > CPtr[LargestIndexSeen])
    {
      LargestIndexSeen = Index;
    }
  }
  return LargestIndexSeen;
}

template <class T> const T& Vector<T>::MaxValue() const
{
  return _Data[MaxIndex()];
}

template <class T> unsigned int Vector<T>::MinIndex() const
{
  Assert(_Length > 0, "MinIndex called on zero-length vector");
  const unsigned int Length = _Length;
  const T *CPtr = _Data;
  unsigned int SmallestIndexSeen = 0;
  for(unsigned int Index = 1; Index < Length; Index++)
  {
    if(CPtr[Index] < CPtr[SmallestIndexSeen])
    {
      SmallestIndexSeen = Index;
    }
  }
  return SmallestIndexSeen;
}

template <class T> const T& Vector<T>::MinValue() const
{
  return _Data[MinIndex()];
}

template <class T> void Vector<T>::Sort()
{
  std::sort(_Data, _Data + _Length);
}

template <class T> unsigned int Vector<T>::Hash32() const
{
  return Utility::Hash32((const BYTE *)_Data, sizeof(T) * _Length);
}

template <class T> unsigned __int64 Vector<T>::Hash64() const
{
  return Utility::Hash64((const BYTE *)_Data, sizeof(T) * _Length);
}

template <class T> void Vector<T>::Append(const Vector<T> &V)
{
  const unsigned int Length = _Length;
  const unsigned int VLength = V._Length;
  ReSize(Length + VLength);
  for(unsigned int Index = 0; Index < VLength; Index++)
  {
    _Data[Index + Length] = V._Data[Index];
  }
}

template <class T> void Vector<T>::PushEnd(const T &t)
{
  if(_Length >= _Capacity)
  {
    Reserve(_Length * 2 + 4);
  }
  _Data[_Length] = t;
  _Length++;
}

template <class T> void Vector<T>::PushEnd()
{
  ReSize(_Length + 1);
}

template <class T> void Vector<T>::PopEnd()
{
#ifdef VECTOR_DEBUG
  Assert(_Length != 0, "PopEnd called on zero-length vector");
#endif
  _Length--;
}

template <class T> void Vector<T>::RemoveSlow(unsigned int Index)
{
#ifdef VECTOR_DEBUG
  Assert(Index < _Length, "Remove called on invalid index");
#endif
  for(unsigned int i = Index; i < _Length - 1; i++)
  {
    _Data[i] = _Data[i + 1];
  }
  _Length--;
}

template <class T> void Vector<T>::Scale(const T &t)
{
  const unsigned int Length = _Length;
  T *CPtr = CArray();
  for(unsigned int Index = 0; Index < Length; Index++)
  {
    CPtr[Index] *= t;
  }
}

template <class T> void Vector<T>::RemoveSwap(unsigned int Index)
{
#ifdef VECTOR_DEBUG
  Assert(Index < _Length, "Remove called on invalid index");
#endif
  Utility::Swap(_Data[Index], _Data[_Length - 1]);
  _Length--;
}

template <class T> bool Vector<T>::Contains(const T &t)
{
  const unsigned int Length = _Length;
  for(unsigned int Index = 0; Index < Length; Index++)
  {
    if(_Data[Index] == t)
    {
      return true;
    }
  }
  return false;
}

template <class T> void Vector<T>::Randomize()
{
  unsigned int Length = _Length;
  for(unsigned int i = 0; i < Length; i++)
  {
    unsigned int RandomNumber = unsigned int(rand());
    Utility::Swap(_Data[i], _Data[i + RandomNumber % (Length - i)]);
  }
}

template <class T> void Vector<T>::PopFront()
{
  RemoveSlow(0);
}

