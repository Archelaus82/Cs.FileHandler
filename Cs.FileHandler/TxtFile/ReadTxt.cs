using System;
using System.Collections.Generic;
using System.IO;

namespace FILEHANDLER.TxtFile
{
    public class ReadTxtFileException : Exception
    {
        public ReadTxtFileException()
        {
        }

        public ReadTxtFileException(string message)
            : base(message)
        {
        }

        public ReadTxtFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ReadTxtFile
    {
        int _lastIndex = 0;
        int _index = 0;
        List<string> _lines = new List<string>();

        public ReadTxtFile(string location, string filename)
        {
            try
            {
                _ReadLines(location, filename);
                _lastIndex = _lines.Count - 1;
            }
            catch (Exception ex)
            {
                throw new ReadTxtFileException(this.GetType().Name + " ctor", ex);
            }
        }

        public string GetNextLine
        {
            get
            {
                if (_index == _lastIndex)
                    throw new Exception("File contains no more lines");
                _index++;
                return _lines[_index];
            }
        }
        public string GetPrevLine
        {
            get
            {
                if (_index == _lastIndex)
                    throw new Exception("Begining of file");
                _index--;
                return _lines[_index];
            }
        }
        public string GetLastLine
        {
            get
            {
                _index = _lastIndex;
                return _lines[_index];
            }
        }
        public string GetFirstLine
        {
            get
            {
                _index = 0;
                return _lines[_index];
            }
        }

        public List<string> AllLines
        {
            get { return _lines; }
        }

        private void _ReadLines(string location, string filename)
        {
            try
            {
                using (StreamReader sr = new StreamReader(Path.Combine(location, filename)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        _lines.Add(line);
                }
            }
            catch (Exception ex)
            {
                throw new ReadTxtFileException("Read lines failed", ex);
            }
        }

        public string GetFirstLineContaining(string searchString)
        {
            try
            {
                foreach (string line in _lines)
                {
                    if (line.Contains(searchString))
                        return line;
                }

                return "";
            }
            catch (Exception ex)
            {
                throw new ReadTxtFileException("Get first line containing failed", ex);
            }
        }

        public string GetLine(int index)
        {
            try
            {
                if (index > _lastIndex || index < 0)
                    throw new Exception("Index [" + index + "] outside of bounds [0-" + _lastIndex + "]");

                return _lines[index];
            }
            catch (Exception ex)
            {
                throw new ReadTxtFileException("Get line failed", ex);
            }
        }

        public bool TryGetPrevLine(out string line)
        {
            try
            {
                line = "";
                if (_index == 0)
                    return false;
                else
                    line = GetPrevLine;

                return true;
            }
            catch (Exception ex)
            {
                throw new ReadTxtFileException("Try to get previous line failed", ex);
            }
        }

        public bool TryGetNextLine(out string line)
        {
            try
            {
                line = "";
                if (_index == _lastIndex)
                    return false;
                else
                    line = GetNextLine;

                return true;
            }
            catch (Exception ex)
            {
                throw new ReadTxtFileException("Try get next line failed", ex);
            }
        }
    }
}
