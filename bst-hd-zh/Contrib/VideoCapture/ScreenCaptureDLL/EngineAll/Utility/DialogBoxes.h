/*
DialogBoxes.h
Written by Matthew Fisher
*/
#pragma  once
class DialogBoxes
{
public:
    static bool Load(String &Result, const String &FileTypeDescription, const String &FileTypeExtension);
    static bool Load(String &Result, const String &Filter);
};