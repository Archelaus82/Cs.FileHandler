using System;
using System.IO;
using System.Reflection;

namespace FILEHANDLER.TxtFile
{
    public class WriteTxtFileException : Exception
    {
        public WriteTxtFileException()
        {
        }

        public WriteTxtFileException(string message)
            : base(message)
        {
        }

        public WriteTxtFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class WriteTxtFile
    {
        public WriteTxtFile() { }

        public WriteTxtFile(string filename, string text)
        {
            try
            {
                File.WriteAllText(filename, text);
            }
            catch (Exception ex)
            {
                throw new WriteTxtFileException(this.GetType().Name + " ctor", ex);
            }
        }

        public WriteTxtFile(string filename, string[] lines)
        {
            try
            {
                File.WriteAllLines(filename, lines);
            }
            catch (Exception ex)
            {
                throw new WriteTxtFileException(this.GetType().Name + " ctor", ex);
            }
        }

        public void WriteToFile(string filename, bool appendToFile, string[] lines)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(filename, appendToFile))
                {
                    foreach (string s in lines)
                    {
                        file.WriteLine(s);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new WriteTxtFileException(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public void WriteToFile(string filename, bool appendToFile, string line)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(filename, appendToFile))
                {
                    file.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                throw new WriteTxtFileException(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
    }
}
