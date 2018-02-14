using System;
using System.IO;
using System.Windows.Documents;
using System.Windows.Forms;

namespace Cs.FileHandler.RtfFile
{
    public class ReadRtfFileException : Exception
    {
        public ReadRtfFileException()
        {
        }

        public ReadRtfFileException(string message)
            : base(message)
        {
        }

        public ReadRtfFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ReadRtfFile
    {
        private TextRange _textRange;
        private MemoryStream _memoryStream;
        private FlowDocument _document = new FlowDocument();

        private string _file;
        private byte[] _buffer;

        public ReadRtfFile(string file)
        {
            try
            {
                _file = file;
                _buffer = File.ReadAllBytes(file);
                _memoryStream = new MemoryStream(_buffer);
                _textRange = new TextRange(_document.ContentStart, _document.ContentEnd);
                _textRange.Load(_memoryStream, DataFormats.Rtf);
            }
            catch (Exception ex)
            {
                throw new ReadRtfFileException(this.GetType().Name + " ctor", ex);
            }
        }

        public string ReportFile { get { return _file; } }
        public string Text { get { return _textRange.Text; } }
    }

}
