using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LOGGER;

namespace QC.ADDONS.FILEHANDLER
{
    public class NoFieldException : Exception
    {
        public NoFieldException(string message) : base(message)
        {

        }
    }

    public class NoRowException : Exception
    {
        public NoRowException(string message) : base(message)
        {

        }
    }

    public class TextFieldParser
    {
        #region Members

        static Logger _logger = new Logger(typeof(TextFieldParser));
        char _fieldDelimiter;
        bool _hasHeaderRow;
        string[] _fieldHeaders;
        bool _filedHeadersSet = false;
        Dictionary<int, string[]> _lines;

        #endregion

        public TextFieldParser(char fieldDelimiter, bool hasHeaderRow)
        {
            _fieldDelimiter = fieldDelimiter;
            _hasHeaderRow = hasHeaderRow;
            _lines = new Dictionary<int, string[]>();
        }

        #region Accessors

        public char FiledDelimiter {  get { return _fieldDelimiter;  } }
        public string[] FieldHeaders {  get { return _fieldHeaders; } }
        public int RowCount { get { return _lines.Keys.Count; } }

        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the header values.
        /// </summary>
        /// <param name="fieldHeaders">array of strings split by a delimited value</param>
        public void SetFieldHeaders(string[] fieldHeaders)
        {
            _fieldHeaders = fieldHeaders;                                               // Set _fieldHeaders to fieldHeaders
            _filedHeadersSet = true;
        }

        /// <summary>
        /// Gets items by row if the row exists
        /// </summary>
        /// <param name="row">the integer of the row number</param>
        /// <returns></returns>
        public string[] GetRow(int row)
        {
            if (!_RowExists(row))                                                            // if not _RowExists(row)
            {
                throw new NoRowException(string.Format("Row [{0}] does not exist", row));    // throw NoRowException
            }
            return _lines[row];                                                             // return _lines[row]
        }

        /// <summary>
        /// Get items by column number if the column exists
        /// </summary>
        /// <param name="col">integer of the column number</param>
        /// <returns>array of strings found in the column</returns>
        public string[] GetColumn(int col)
        {
            List<string> colData = new List<string>();                                              // Define string list; colData as new lis
            foreach (KeyValuePair<int, string[]> kvp in _lines)                                                            // Foreach line in _lines
            {
                int rowIndex = kvp.Key;
                string[] line = kvp.Value;
                if (!_ColumnExists(rowIndex, col))                                                             // if not ColumnExists(number)
                {
                    throw new NoFieldException(string.Format("Column [{0}] does not exist", col));  // throw NoFieldException
                }
                colData.Add(line[col]);                                                       // Add line[number] to colData
            }
            return colData.ToArray();                                                               // return colData
        }

        /// <summary>
        /// Get column by header value if the column exists
        /// </summary>
        /// <param name="header">string of the header value by column</param>
        /// <returns>array of strings found in the column</returns>
        public string[] GetColumn(string header)
        {
            if (!_FieldExists(header))                                                               // if not FieldExists(header)
            {
                throw new NoFieldException(string.Format("Field [{0}] does not exist", header));    // throw NoFieldException
            }

            int colIndex = _fieldHeaders.ToList().IndexOf(header);                                  // Define int; colIndex as _fieldHeaders.IndexOf(header)

            return GetColumn(colIndex);                                                             // return GetColumn(colIndex)
        }

        /// <summary>
        /// Get string item by row and column if the row and column exist
        /// </summary>
        /// <param name="row">integer of the row number</param>
        /// <param name="col">integer of the column number</param>
        /// <returns>string of the item</returns>
        public string Get(int row, int col)
        {
            if (!_RowExists(row))                                                                    // if not _RowExists
            {
                throw new NoRowException(string.Format("Row [{0}] does not exist", row));           // throw NoRowException
            }

            if (!_ColumnExists(row, col))                                                                 // if not _ColExists
            {
                throw new NoFieldException(string.Format("Column [{0}] does not exist", col));      // throw NoFieldException
            }

            return GetRow(row)[col];                                                                // return GetRow(row)[col]
        }

        /// <summary>
        /// Gets string of item by column header and row number
        /// </summary>
        /// <param name="row">integer of the row number</param>
        /// <param name="header">string of the header value by column</param>
        /// <returns>string of the item</returns>
        public string Get(int row, string header)
        {
            if (!_FieldExists(header)) // if not FieldExists(header)
            {
                throw new NoFieldException(string.Format("Field [{0}] does not exist", header));    // throw NoFieldException
            }

            int colIndex = _fieldHeaders.ToList().IndexOf(header);                                  // Define int; colIndex as _fieldHeaders.IndexOf(header)

            return Get(row, colIndex);                                                              // return Get(row, colIndex)
        }

        /// <summary>
        /// Adds new empty row by size of width
        /// 
        /// If parser has filed headers set, column width is set to the length
        /// of the filed headers array.
        /// </summary>
        public int AddEmptyRow(int width = 0)
        {
            if (_hasHeaderRow)
            {
                if (!_filedHeadersSet)
                    throw new Exception("Field headers not set");
                if (width != 0)
                    throw new ArgumentException("Parser has field headers. Width value should not be specified.");
                width = _fieldHeaders.Length;
            }
            else if (width == 0)
                throw new ArgumentException("Must specify column width");

            int rowIndex = _lines.Keys.Count;                                                       // Define int; rowIndex as _lines.Keys() length
            _lines[rowIndex] = new string[width];                                                   // _lines[rowIndex] to new string array length of width

            return rowIndex;
        }

        /// <summary>
        /// Add new row with values
        /// </summary>
        /// <param name="fieldValues">String array of data to be added to row</param>
        public void AddRow(string[] fieldValues)
        {
            int rowIndex = _lines.Keys.Count;                                                       // Define int; rowIndex as _lines.Keys() length
            // add quotes to sub-delimited values
            for (int i = 0; i < fieldValues.Length; i++)
            {
                if (fieldValues[i] == null)
                {
                    fieldValues[i] = "";
                    continue;
                }

                if (fieldValues[i].Contains(_fieldDelimiter) &&
                    !fieldValues[i].Contains('"'))
                    fieldValues[i] = String.Format("\"{0}\"", fieldValues[i]);
            }

            _lines[rowIndex] = fieldValues;                                                       // _lines[rowIndex] to fieldValues
            fieldValues = null;  // clear array
        }

        /// <summary>
        /// Sets existing row data values
        /// </summary>
        /// <param name="row">integer of the row number</param>
        /// <param name="fieldValues">String array of data to be added to row</param>
        public void SetRow(int row, string[] fieldValues)
        {
            if (!_RowExists(row))                                                                    // if not _RowExists
            {
                throw new NoRowException(string.Format("Row [{0}] does not exist", row));           // throw NoRowException
            }

            _lines[row] = fieldValues;                                                              // Set _lines[row] to fieldValues
            fieldValues = null;  // clear array
        }

        /// <summary>
        /// Set value of existing item by row number and column number
        /// </summary>
        /// <param name="row">integer of the row number</param>
        /// <param name="col">integer of the column number</param>
        /// <param name="value">value of data to be set to existing item</param>
        public void Set(int row, int col, string value)
        {
            if (!_RowExists(row))                                                                    // if not _RowExists
            {
                throw new NoRowException(string.Format("Row [{0}] does not exist", row));           // throw NoRowException
            }

            if (!_ColumnExists(row, col))                                                                 // if not _ColExists
            {
                throw new NoFieldException(string.Format("Column [{0}] does not exist", col));      // throw NoFieldException
            }

            string[] values = _lines[row];                                                               // Set _lines[row][col] to value
            values[col] = value;
            SetRow(row, values);
            values = null;  // clear array
        }

        /// <summary>
        /// Set the value of existing item by row number and column header value
        /// </summary>
        /// <param name="row">integer of the row number</param>
        /// <param name="header">string of the header value by column</param>
        /// <param name="value">value of data to be set to existing item</param>
        public void Set(int row, string header, string value)
        {
            if (!_FieldExists(header))                                                              // if not FieldExists(header)
            {
                throw new NoFieldException(string.Format("Field [{0}] does not exist", header));    // throw NoFieldException
            }

            int colIndex = _fieldHeaders.ToList().IndexOf(header);                                  // Define int; colIndex as _fieldHeaders.IndexOf(header)

            Set(row, colIndex, value);                                                              // Set(rwo, colIndex, value)
        }

        /// <summary>
        /// Read in the data from a file by line and parse the lines
        /// by _fielDelimiter value
        /// </summary>
        /// <param name="directory">Directory where the file to be read is located</param>
        /// <param name="filename">the name of the file to be read.</param>
        public void Read(string directory, string filename)
        {
            try
            {
                string file = Path.Combine(directory, filename);                                        // Define string; file as Path.Combine(directory, filename)
                string[] lines = File.ReadAllLines(file);                                               // Read all lines

                for (int i = 0; i < lines.Length; i++)                                                  // For i = 0 less than lines count increment by 1
                {
                    //string[] items = lines[i].Split(_fieldDelimiter);                                   // Define string array; items as line split using _fieldDelimiter
                    List<string> values = new List<string>();
                    int lastIndex = 0;
                    bool insideQuotes = false;
                    for (int j = 0; j < lines[i].Length; j++)
                    {
                        if (lines[i][j].ToString() == "\"")
                            insideQuotes = !insideQuotes;


                        if (lines[i][j] == _fieldDelimiter)
                        {
                            string subValue;
                            if (!insideQuotes)
                            {
                                // first file is empty
                                if (j == 0)
                                {
                                    values.Add("");  // add empty value
                                    lastIndex++;
                                    continue;
                                }

                                // field is empty
                                if (lastIndex == j)
                                {
                                    values.Add("");  // add empty value
                                    lastIndex++;
                                    continue;
                                }

                                subValue = lines[i].Substring(lastIndex, j - lastIndex);
                                values.Add(subValue);
                                lastIndex = j + 1;
                            }
                        }
                    }

                    if (i.Equals(0) && _hasHeaderRow)                                                                     // if i is 0  set headers if necessary
                    {                                                                                       // if _hasHeader                        
                        _fieldHeaders = values.ToArray();                                                      // _fieldHeaders to items
                    }
                    else                                                                            // else
                    {
                        AddRow(values.ToArray());                                                           // SetRow(i,  items)
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(""), ex);
            }
        }

        /// <summary>
        /// Write data to a file
        /// </summary>
        /// <param name="directory">The directory where the file is to be written to.</param>
        /// <param name="filename">The name of the file being written to.</param>
        public void Write(string directory, string filename)
        {
            string file = Path.Combine(directory, filename); // Define string; file as Path.Combine(directory, filename)
            if (!Directory.Exists(directory)) // if directory does not exist
            {
                Directory.CreateDirectory(directory); // Create directory
            }


            using (StreamWriter sw = new StreamWriter(file)) // Create new StreamWriter sw inside ‘using’
            {
                if (_hasHeaderRow) // if _hasHeaders
                {
                    sw.WriteLine(String.Join(_fieldDelimiter.ToString(), _fieldHeaders)); // sw.Writeline(String.Join(_fieldDelimiter, _fieldHeaders))
                }

                foreach (KeyValuePair<int, string[]> kvp in _lines) // Foreach KeyValuePair int,string list kvp in _lines
                {
                    sw.WriteLine(String.Join(_fieldDelimiter.ToString(), kvp.Value));
                }
            }
            _logger.Info("Wrote file {0}", file);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Check if a row exists.
        /// </summary>
        /// <param name="row">the integer of the row number</param>
        /// <returns>True = the row exists, False = the row does not exist.</returns>
        bool _RowExists(int row)
        {
            if(_lines.Keys.Contains(row))                                           // if _lines.Keys()  contains row
                return true;                                                        // return true      	
            
            return false;                                                           // return false
        }

        /// <summary>
        /// Check if a column exists.
        /// </summary>
        /// <param name="col">the integer of the column number</param>
        /// <returns>True = the column exists, Fales = the column does not exist</returns>
        bool _ColumnExists(int row, int col)
        {
            if(_lines[row].Length >= col)                                          // if line count greater than or equal to  number
                return true;                                                        // return true
            
            return false;                                                           // return false
        }

        /// <summary>
        /// Check if a header field value exists.
        /// </summary>
        /// <param name="header">string value of the header field</param>
        /// <returns>True = the header value exists, False = the header value does not exist.</returns>
        bool _FieldExists(string header)  
        {
            if(_fieldHeaders.Contains(header))                                      // if _fieldHeaders contains header
                return true;                                                        // return true
            
            return false;                                                           // return false
        }

        #endregion    
    }
}
