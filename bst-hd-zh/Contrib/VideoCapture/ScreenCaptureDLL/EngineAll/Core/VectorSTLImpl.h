/*
Vector.h
Written by Matthew Fisher

Template vector library.  Extends functionality of std::vector
*/

#pragma once
#include "..\\..\\Main.h"

template <class type> class Vector
{
public:
    Vector();
    explicit Vector(UINT _Size);
    Vector(const Vector<type> &V);

    ~Vector();
    

    //
    // Memory
    //
    void FreeMemory();

    //
    // Windows has claimed the nice name "ZeroMemory" for its own uses
    //
    void ZeroMem();

    Vector<type>& operator = (const Vector<type> &V);

    void ReSize(UINT Size);
    void Reserve(UINT Size);
    void Allocate(UINT Size);

    //
    // Accessors
    //
    __forceinline type& operator [] (UINT k)
    {
#ifdef VECTOR_DEBUG
        Assert(k < int(Data.size()), "Out-of-bounds vector access");
#endif
        return Data[k];
    }
    __forceinline type& operator [] (int k) 
    {
#ifdef VECTOR_DEBUG
        Assert(k >= 0 && k < int(Data.size()), "Out-of-bounds vector access");
#endif
        return Data[k];
    }
    __forceinline const type& operator [] (UINT k) const
    {
#ifdef VECTOR_DEBUG
        Assert(k < int(Data.size()), "Out-of-bounds vector access");
#endif
        return Data[k];
    }
    __forceinline const type& operator [] (int k) const
    {
#ifdef VECTOR_DEBUG
        Assert(k >= 0 && k < int(Data.size()), "Out-of-bounds vector access");
#endif
        return Data[k];
    }
    __forceinline UINT Length() const
    {
        return UINT(Data.size());
    }
    __forceinline type* CArray()
    {
#ifdef VECTOR_DEBUG
        Assert(Data.size() != 0, "CArray called on zero-length vector");
#endif
        return &(Data[0]);
    }
    __forceinline const type* CArray() const
    {
#ifdef VECTOR_DEBUG
        Assert(Data.size() != 0, "CArray called on zero-length vector");
#endif
        return &(Data[0]);
    }
    __forceinline type& RandomElement()
    {
        Assert(Length() > 0, "RandomElement called with no elements.");
        return (*this)[rand() % Length()];
    }
    __forceinline const type& RandomElement() const
    {
        Assert(Length() > 0, "RandomElement called with no elements.");
        return (*this)[rand() % Length()];
    }
    __forceinline type& Last()
    {
        Assert(Length() > 0, "Last called with no elements.");
        return (*this)[Length() - 1];
    }
    __forceinline const type& Last() const
    {
        Assert(Length() > 0, "Last called with no elements.");
        return (*this)[Length() - 1];
    }
    __forceinline type& First()
    {
        Assert(Length() > 0, "First called with no elements.");
        return (*this)[0];
    }
    __forceinline const type& First() const
    {
        Assert(Length() > 0, "Last called with no elements.");
        return (*this)[0];
    }

    //
    // Modifiers
    //
    void Append(const Vector<type> &V);
    void PushEnd(const type &t);
    void PushEnd();
    void PopEnd();
    void PopFront();
    void RemoveSlow(UINT Index);
    void RemoveSwap(UINT Index);
    void Randomize();
    void Sort();
    void Scale(const type &t);
    template<class orderingType> void Sort(orderingType Function)
    {
        std::sort(CArray(), CArray() + Length(), Function);
    }
    void Clear(const type &T);

    //
    // Query
    //
    type Sum() const;
    const type& MaxValue() const;
    UINT MaxIndex() const;
    const type& MinValue() const;
    UINT MinIndex() const;
    bool Contains(const type &t);
    UINT Hash32() const;
    UINT64 Hash64() const;

protected:
    vector<type> Data;
};

//#include "VectorSTLImpl.cpp"

/*
Vector.cpp
Written by Matthew Fisher
*/
template <class type> Vector<type>::Vector()
{

}

template <class type> Vector<type>::Vector(UINT _Size)
{
  Data.resize(_Size);
}

template <class type> Vector<type>::Vector(const Vector<type> &V)
{
  Data = V.Data;
}

template <class type> Vector<type>::~Vector()
{
  FreeMemory();
}

template <class type> void Vector<type>::FreeMemory()
{
  if(Length() != 0)
  {
    Data.resize(0);
    Data.clear();
  }
}

template <class type> void Vector<type>::ZeroMem()
{
  memset(CArray(), 0, Length() * sizeof(type));
}

template <class type> Vector<type>& Vector<type>::operator = (const Vector<type> &V)
{
  Data = V.Data;
  return *this;
}

template <class type> void Vector<type>::ReSize(UINT Size)
{
  Data.resize(Size);
}
template <class type> void Vector<type>::Reserve(UINT Size)
{
  Data.reserve(Size);
}

template <class type> void Vector<type>::Allocate(UINT Size)
{
  Data.clear();
  Data.resize(Size);
}

template <class type> void Vector<type>::Clear(const type &T)
{
  UINT Length = Data.size();
  type *CPtr = CArray();
  for(UINT i = 0; i < Length; i++)
  {
    CPtr[i] = T;
  }
}

template <class type> type Vector<type>::Sum() const
{
  const UINT Length = Data.size();
  const type *CPtr = CArray();
  type Result = CPtr[0];
  for(UINT Index = 1; Index < Length; Index++)
  {
    Result += CPtr[Index];
  }
  return Result;
}

template <class type> UINT Vector<type>::MaxIndex() const
{
  const UINT Length = Data.size();
  const type *CPtr = CArray();
  UINT LargestIndexSeen = 0;
  for(UINT Index = 1; Index < Length; Index++)
  {
    if(CPtr[Index] > CPtr[LargestIndexSeen])
    {
      LargestIndexSeen = Index;
    }
  }
  return LargestIndexSeen;
}

template <class type> const type& Vector<type>::MaxValue() const
{
  return Data[MaxIndex()];
}

template <class type> UINT Vector<type>::MinIndex() const
{
  const UINT Length = Data.size();
  const type *CPtr = CArray();
  UINT SmallestIndexSeen = 0;
  for(UINT Index = 1; Index < Length; Index++)
  {
    if(CPtr[Index] < CPtr[SmallestIndexSeen])
    {
      SmallestIndexSeen = Index;
    }
  }
  return SmallestIndexSeen;
}

template <class type> const type& Vector<type>::MinValue() const
{
  return Data[MinIndex()];
}

template <class type> void Vector<type>::Sort()
{
  std::sort(CArray(), CArray() + Length());
}

//template <class type, class orderingType> void Vector<type>::Sort(orderingType Function)


template <class type> UINT Vector<type>::Hash32() const
{
  return Utility::Hash32((const BYTE *)CArray(), sizeof(type) * Length());
}

template <class type> UINT64 Vector<type>::Hash64() const
{
  return Utility::Hash64((const BYTE *)CArray(), sizeof(type) * Length());
}

template <class type> void Vector<type>::Append(const Vector<type> &V)
{
  Data.reserve(V.Length() + Length());
  for(UINT Index = 0; Index < V.Length(); Index++)
  {
    Data.push_back(V[Index]);
  }
}

template <class type> void Vector<type>::PushEnd(const type &t)
{
  Data.push_back(t);
}

template <class type> void Vector<type>::PushEnd()
{
  Data.resize(Data.size() + 1);
}

template <class type> void Vector<type>::PopEnd()
{
#ifdef VECTOR_DEBUG
  Assert(Data.size() != 0, "PopEnd called on zero-length vector");
#endif
  Data.pop_back();
}

template <class type> void Vector<type>::RemoveSlow(UINT Index)
{
#ifdef VECTOR_DEBUG
  Assert(Index < Length(), "Remove called on invalid index");
#endif
  for(UINT i = Index; i < Length() - 1; i++)
  {
    Data[i] = Data[i + 1];
  }
  PopEnd();
}

template <class type> void Vector<type>::Scale(const type &t)
{
  const UINT Length = Data.size();
  type *CPtr = CArray();
  for(UINT Index = 0; Index < Length; Index++)
  {
    CPtr[Index] *= t;
  }
}

template <class type> void Vector<type>::RemoveSwap(UINT Index)
{
#ifdef VECTOR_DEBUG
  Assert(Index < Length(), "Remove called on invalid index");
#endif
  Utility::Swap(Data[Index], Data[Length() - 1]);
  PopEnd();
}

template <class type> bool Vector<type>::Contains(const type &t)
{
  UINT Len = Length();
  for(UINT Index = 0; Index < Len; Index++)
  {
    if(Data[Index] == t)
    {
      return true;
    }
  }
  return false;
}

template <class type> void Vector<type>::Randomize()
{
  UINT Len = Length();
  for(UINT i = 0; i < Len; i++)
  {
    UINT RandomNumber = UINT(rand());
    Utility::Swap(Data[i], Data[i + RandomNumber % (Len - i)]);
  }
}

template <class type> void Vector<type>::PopFront()
{
  RemoveSlow(0);
}


