#region License
/*
    Copyright (c) 2010, Paweł Hofman (CodeTitans)
    All Rights Reserved.

    Licensed under the Apache License version 2.0.
    For more information please visit:

    http://codetitans.codeplex.com/license
        or
    http://www.apache.org/licenses/


    For latest source code, documentation, samples
    and more information please visit:

    http://codetitans.codeplex.com/
*/
#endregion

using System.IO;
using System;

namespace CodeTitans.Helpers
{
    /// <summary>
    /// Interface allowing to read characters.
    /// </summary>
    internal interface IStringReader
    {
        /// <summary>
        /// Reads next character from the input source.
        /// Automaticaly updates current char, EOF and traced position.
        /// </summary>
        char ReadNext();

        /// <summary>
        /// Gets the last-read character.
        /// </summary>
        char CurrentChar
        { get; }

        /// <summary>
        /// Gets an indication if given source is empty.
        /// </summary>
        bool IsEmpty
        { get; }

        /// <summary>
        /// Gets an indication if end-of-file has been reached.
        /// </summary>
        bool IsEof
        { get; }

        /// <summary>
        /// Gets the current line.
        /// </summary>
        int Line
        { get; }

        /// <summary>
        /// Gets the character offset within the line.
        /// </summary>
        int LineOffset
        { get; }
    }

    /// <summary>
    /// Class that wraps reading of a string.
    /// </summary>
    internal class StringReaderWrapper : IStringReader
    {
        private readonly string _text;
        private int _line;
        private int _lineOffset;
        private readonly int _length;
        private int _readerOffset;

        public StringReaderWrapper(string text)
        {
            _text = text;
            _length = string.IsNullOrEmpty(text) ? 0 : text.Length;
            _line = 1;
            _lineOffset = 1;
            _readerOffset = -1;
        }

        public char ReadNext()
        {
            if (!IsEof)
                _readerOffset++;

            if (_readerOffset < _length)
            {

                if (_text[_readerOffset] == '\n')
                {
                    _lineOffset = 1;
                    _line++;
                }
                else
                {
                    _lineOffset++;
                }

                return _text[_readerOffset];
            }

            return '\0';
        }

        public char CurrentChar
        {
            get { return _readerOffset >= 0 && _readerOffset < _length ? _text[_readerOffset] : '\0'; }
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(_text); }
        }

        public bool IsEof
        {
            get { return _readerOffset >= _length; }
        }

        public int Line
        {
            get { return _line; }
        }

        public int LineOffset
        {
            get { return _lineOffset; }
        }
    }

    internal class TextReaderWrapper : IStringReader
    {
        private readonly TextReader _reader;
        private int _line;
        private int _offset;
        private char _currentChar;
        private bool _eof;

        public TextReaderWrapper(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            _reader = reader;
            _line = 1;
            _offset = 1;
            _currentChar = '\0';
            _eof = false;
        }

        public char ReadNext()
        {
            int data = _reader.Read();

            _eof = data == -1;

            if (!_eof)
            {
                _currentChar = (char)data;
                if (_currentChar == '\n')
                {
                    _offset = 1;
                    _line++;
                }
                else
                {
                    _offset++;
                }
            }
            else
            {
                _currentChar = '\0';
            }

            return _currentChar;
        }

        public char CurrentChar
        {
            get { return _currentChar; }
        }

        public bool IsEmpty
        {
            get { return _reader.Peek() == -1; }
        }

        public bool IsEof
        {
            get { return _eof; }
        }

        public int Line
        {
            get { return _line; }
        }

        public int LineOffset
        {
            get { return _offset; }
        }
    }
}
