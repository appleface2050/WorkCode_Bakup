/*
Grid.h
Written by Matthew Fisher

Arbitrary-type grid structure.
*/

#pragma once
#include "..\\..\\Main.h"

template <class type> class Grid
{
public:
    Grid();
    Grid(unsigned int Rows, unsigned int Cols);
    Grid(const Grid<type> &G);

    ~Grid();

    //
    // Memory
    //
    void FreeMemory();
    Grid<type>& operator = (const Grid<type> &G);
    void Allocate(unsigned int Rows, unsigned int Cols);
    void Allocate(const Vec2i &Dimensions)
    {
        Allocate(Dimensions.y, Dimensions.x);
    }
    
    //
    // Accessors
    //
    __forceinline type& GetElement(unsigned int Row, unsigned int Col)
    {
#ifdef VECTOR_DEBUG
        Assert( (Row < _Rows) && (Col < _Cols), "Out-of-bounds vector access");
#endif
        return _Data[Row * _Cols + Col];
    }
    __forceinline const type& GetElement(unsigned int Row, unsigned int Col) const
    {
#ifdef VECTOR_DEBUG
        Assert( (Row < _Rows) && (Col < _Cols), "Out-of-bounds vector access");
#endif
        return _Data[Row * _Cols + Col];
    }
    __forceinline unsigned int Rows() const
    {
        return _Rows;
    }
    __forceinline unsigned int Cols() const
    {
        return _Cols;
    }
    __forceinline Vec2i Dimensions() const
    {
        return Vec2i(_Cols, _Rows);
    }
    __forceinline bool Square() const
    {
        return (_Rows == _Cols);
    }
    __forceinline type* CArray()
    {
        return _Data;
    }
    __forceinline const type* CArray() const
    {
        return _Data;
    }

    //
    // Query
    //
    __forceinline bool ValidCoordinates(int Row, int Col) const
    {
        return (Row >= 0 && Row < int(_Rows) && Col >= 0 && Col < int(_Cols));
    }
    void ExtractRow(unsigned int Row, Vector<type> &Result)
    {
        const type *CPtr = _Data;
        Result.Allocate(_Cols);
        for(unsigned int Col = 0; Col < _Cols; Col++)
        {
            Result[Col] = CPtr[Row * _Cols + Col];
        }
    }
    void ExtractCol(unsigned int Col, Vector<type> &Result)
    {
        const type *CPtr = _Data;
        Result.Allocate(_Rows);
        for(unsigned int Row = 0; Row < _Rows; Row++)
        {
            Result[Row] = CPtr[Row * _Cols + Col];
        }
    }

    //
    // Modifiers
    //
    void Clear(const type &T);

    //
    // File
    //
    void SaveToASCIIFile(const String &Filename)
    {
        ofstream File(Filename.CString());
        PersistentAssert(!File.fail(), "Failed to open file");
        File << _Rows << '\t' << _Cols << endl;
        for(unsigned int Row = 0; Row < _Rows; Row++)
        {
            for(unsigned int Col = 0; Col < _Cols; Col++)
            {
                File << _Data[Row * _Cols + Col] << '\t';
            }
            File << endl;
        }
    }

protected:
    type *_Data;
    unsigned int _Rows, _Cols;
};

//#include "Grid.cpp"
/*
Grid.cpp
Written by Matthew Fisher
*/


template <class type> void Grid<type>::Allocate(unsigned int Rows, unsigned int Cols)
{
  _Rows = Rows;
  _Cols = Cols;
  if(_Data)
  {
    delete[] _Data;
  }
  _Data = new type[Rows * Cols];
}

template <class type> void Grid<type>::Clear(const type &T)
{
  const unsigned int TotalEntries = _Rows * _Cols;
  for(unsigned int Index = 0; Index < TotalEntries; Index++)
  {
    _Data[Index] = T;
  }
}

template <class type> Grid<type>::Grid()
{
  _Rows = 0;
  _Cols = 0;
  _Data = NULL;
}

template <class type> Grid<type>::Grid(unsigned int Rows, unsigned int Cols)
{
  _Rows = Rows;
  _Cols = Cols;
  _Data = new type[Rows * Cols];
}

template <class type> Grid<type>::Grid(const Grid<type> &G)
{
  _Rows = G._Rows;
  _Cols = G._Cols;

  const unsigned int TotalEntries = _Rows * _Cols;
  _Data = new type[TotalEntries];
  for(unsigned int Index = 0; Index < TotalEntries; Index++)
  {
    _Data[Index] = G._Data[Index];
  }
}

template <class type> Grid<type>::~Grid()
{
  FreeMemory();
}

template <class type> void Grid<type>::FreeMemory()
{
  _Rows = 0;
  _Cols = 0;
  if(_Data != NULL)
  {
    delete[] _Data;
    _Data = NULL;
  }
}

template <class type> Grid<type>& Grid<type>::operator = (const Grid<type> &G)
{
  if(_Data)
  {
    delete[] _Data;
  }
  _Rows = G._Rows;
  _Cols = G._Cols;

  const unsigned int TotalEntries = _Rows * _Cols;
  _Data = new type[TotalEntries];
  for(unsigned int Index = 0; Index < TotalEntries; Index++)
  {
    _Data[Index] = G._Data[Index];
  }

  return *this;
}

