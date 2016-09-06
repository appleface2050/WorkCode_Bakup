/*
WaveFile.h
Written by Matthew Fisher

A wave file defines a (possibly stereo) sound.
*/
#pragma once

class WaveFile
{
public:

  void LoadFromFile(LPWSTR strFileName, WAVEFORMATEX* pwfx, DWORD dwFlags );
  void LoadFromMemory(const Vector<DWORD> &Data, UINT SamplesPerSec);
  void LoadFromMemory(const Vector<double> &Data, UINT SamplesPerSec);
  void Save(const String &Filename);

  __forceinline const Vector<DWORD>& Data() const
  {
    return _Data;
  }
  __forceinline WAVEFORMATEX& Format()
  {
      return _Format;
  }

private:
  Vector<DWORD> _Data;
  WAVEFORMATEX _Format;
};
