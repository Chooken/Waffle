using System;
using System.Collections;

namespace MagicGame.Scripts.BookEditor;

public class TextEditor
{
    public Action<int> LineUpdated;
    
    private GapBuffer<char> _characterBuffer = new ();
    private GapBuffer<LineInfo> _lineBuffer = new (64);

    private int maxLineLength;

    public TextEditor(int lineLength)
    {
        _lineBuffer.Add(new LineInfo());
        maxLineLength = lineLength;
    }

    public int CursorPosition => _characterBuffer.CursorPosition;

    public (int line, int index) GetCursorPosition()
    {
        int line = _lineBuffer.CursorPosition - 1;
        int linePosition = GetLinePosition(line);

        return (line, _characterBuffer.CursorPosition - linePosition);
    }
    
    public int GetCharacterLine(int character)
    {
        int line = 0;
        int linePosition = 0;
            
        for (int i = 0; i < _lineBuffer.Count; i++)
        {
            linePosition += _lineBuffer[i].Length;

            if (character <= linePosition)
            {
                line = i;
                break;
            }
        }

        return line;
    }
    
    public int GetLinePosition(int line)
    {
        int linePosition = 0;
            
        for (int i = 0; i < line; i++)
        {
            linePosition += _lineBuffer[i].Length;
        }

        return linePosition;
    }

    public void InsertNewLine()
    {
        int linePosition = GetLinePosition(_lineBuffer.CursorPosition - 1);

        // Figure out how long the old and new lines should be
        int newLength = _characterBuffer.CursorPosition - linePosition;
        int remainder = linePosition
                        + _lineBuffer[_lineBuffer.CursorPosition - 1].Length
                        - _characterBuffer.CursorPosition;

        // Apply the change
        _lineBuffer[_lineBuffer.CursorPosition - 1].Length = newLength;
        _lineBuffer.Add(new LineInfo()
        {
            Length = remainder,
        });
        
        LineUpdated.Invoke(_lineBuffer.CursorPosition - 2);
        
        return;
    }
    
    public void InsertCharacter(char character)
    {
        if (character == '\n')
        {
            InsertNewLine();
        }

        if (_lineBuffer[_lineBuffer.CursorPosition - 1].Length >= maxLineLength)
        {
            return;
        }

        _lineBuffer[_lineBuffer.CursorPosition - 1].Length += 1;
        _characterBuffer.Add(character);
        LineUpdated.Invoke(_lineBuffer.CursorPosition - 1);
    }

    public void DeleteCharacter()
    {
        if (_characterBuffer.CursorPosition == 0)
            return;

        if (_lineBuffer[_lineBuffer.CursorPosition - 1].Length == 0)
        {
            _lineBuffer.Delete();
            return;
        }

        int linePosition = GetLinePosition(_lineBuffer.CursorPosition - 1);

        if (linePosition == _characterBuffer.CursorPosition)
        {
            return;
        }
        
        _characterBuffer.Delete();
        _lineBuffer[_lineBuffer.CursorPosition - 1].Length -= 1;
        LineUpdated.Invoke(_lineBuffer.CursorPosition - 1);
    }

    public void MoveCursor(int line, int character)
    {
        int linePosition = GetLinePosition(line);
        
        _characterBuffer.SetCursorPosition(linePosition + character);
        _lineBuffer.SetCursorPosition(line);
    }

    public void MoveCursorUp()
    {
        int linePosition = GetLinePosition(_lineBuffer.CursorPosition - 1);

        if (linePosition == _characterBuffer.CursorPosition)
        {
            if (_lineBuffer.CursorPosition == 1)
                return;
            
            _lineBuffer.MoveCursorLeft(1);
            return;
        }
        
        _characterBuffer.MoveCursorLeft(1);
    }
    
    public void MoveCursorDown()
    {
        int linePosition = GetLinePosition(_lineBuffer.CursorPosition);

        if (linePosition == _characterBuffer.CursorPosition)
        {
            _lineBuffer.MoveCursorRight(1);
            return;
        }
        
        _characterBuffer.MoveCursorRight(1);
    }

    public void MoveCursorLeft()
    {
        if (_lineBuffer.CursorPosition == 1)
        {
            return;
        }
        
        int linePosition = GetLinePosition(_lineBuffer.CursorPosition - 1);
        int index = _characterBuffer.CursorPosition - linePosition;
        
        _lineBuffer.MoveCursorLeft(1);
        linePosition = GetLinePosition(_lineBuffer.CursorPosition - 1);
        linePosition += Math.Min(index, _lineBuffer[_lineBuffer.CursorPosition - 1].Length);
        _characterBuffer.SetCursorPosition(linePosition);
    }

    public void MoveCursorRight()
    {
        int linePosition = GetLinePosition(_lineBuffer.CursorPosition - 1);
        int index = _characterBuffer.CursorPosition - linePosition;
        
        _lineBuffer.MoveCursorRight(1);
        linePosition = GetLinePosition(_lineBuffer.CursorPosition - 1);
        linePosition += Math.Min(index, _lineBuffer[_lineBuffer.CursorPosition - 1].Length);
        _characterBuffer.SetCursorPosition(linePosition);
    }

    public bool TryGetCharAt(int line, int position, out char character)
    {
        character = char.MinValue;
        
        int linePosition = GetLinePosition(line);

        if (_characterBuffer.Count <= linePosition + position)
            return false;
        
        character = _characterBuffer.Get(linePosition + position);

        return true;
    }

    public IEnumerator<char> GetEnumerator() => _characterBuffer.GetEnumerator();

    public IEnumerator<char> GetLineEnumerator(int line)
    {
        if (line >= _lineBuffer.Count)
            return null;
        
        int start = GetLinePosition(line);
        int end = start + _lineBuffer[line].Length;

        return _characterBuffer.GetRangeEnumerator(start..end);
    }
}

public struct LineInfo
{
    public int Length;
}

public class GapBuffer<T> : IEnumerable<T>
{
    public int CursorPosition => _gapStart;
    public int Count => _size;
    public int GapSize => _gapEnd - _gapStart;
    
    private T[] _data;

    private int _idealGapSize = 10;
    
    private int _gapStart = 0;
    private int _gapEnd = 10;

    private int _size = 0;

    public GapBuffer(int initialCapacity = 128)
    {
        _data = new T[initialCapacity];
    }

    public void SetIdealGapSize(int size)
    {
        _idealGapSize = size;
    }

    public void SetCursorPosition(int position)
    {
        if (position == _gapStart)
            return;
        
        if (position < _gapStart)
            MoveCursorLeft(_gapStart - position);
        else 
            MoveCursorRight(position - _gapStart);
    }

    public void MoveCursorLeft(int spaces)
    {
        if (spaces <= 0)
        {
            return;
        }

        if (_gapStart - spaces < 0)
        {
            spaces = _gapStart;
        }
        
        Array.Copy(_data, _gapStart - spaces, _data, _gapEnd - spaces, spaces);

        _gapStart -= spaces;
        _gapEnd -= spaces;
    }

    public void MoveCursorRight(int spaces)
    {
        if (spaces <= 0)
        {
            return;
        }

        if (_gapStart + spaces >= _size)
        {
            spaces = _size - _gapStart;
        }
        
        Array.Copy(_data, _gapEnd, _data, _gapStart, spaces);
        
        _gapStart += spaces;
        _gapEnd += spaces;
    }

    public void Add(T value)
    {
        if (_gapStart == _gapEnd)
        {
            GrowGap();
        }

        _data[_gapStart] = value;
        _gapStart++;
        _size++;
    }

    public void Delete()
    {
        if (_gapStart == 0)
            return;
        
        _gapStart--;
        _size--;
    }

    public void GrowGap()
    {
        if (GapSize == _idealGapSize)
            return;
        
        if (_gapStart + _idealGapSize >= _data.Length)
            GrowBuffer();
        
        if (_size - _gapEnd != 0)
        {
            int dif = _idealGapSize - GapSize;
            Array.Copy(_data, _gapEnd, _data, _gapEnd + dif, _size - _gapEnd);
        }

        _gapEnd = _gapStart + _idealGapSize;
    }

    private void GrowBuffer()
    {
        Array.Resize(ref _data, _data.Length * 2);
    }

    public ref T this[int index] => ref Get(index);

    public ref T Get(int index)
    {
        if (index < _gapStart)
        {return ref _data[index];
        }

        return ref _data[index + GapSize];
    }

    public IEnumerator<T> GetRangeEnumerator(Range range)
    {
        int start = range.Start.Value;
        int end = range.End.Value;

        if (start < 0 || start >= _data.Length ||
            end < 0 || end >= _data.Length)
        {
            yield break;
        }

        for (int i = range.Start.Value; i < range.End.Value; i++)
        {
            if (i >= _gapStart)
            {
                yield return _data[i + GapSize];
                continue;
            }

            yield return _data[i];
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _size; i++)
        {
            if (i >= _gapStart)
            {
                yield return _data[i + GapSize];
                continue;
            }

            yield return _data[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}