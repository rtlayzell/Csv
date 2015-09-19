using Csv.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csv
{
	public class CsvReader : IDisposable
	{
		private const int DefaultBufferSize = 1024;
		private const int DefaultFileBufferSize = 4096;

		private const char QuoteChar = '\"';
		private const char DelimChar = ',';
		private const char SpaceChar = ' ';
		private const char TabChar = '\t';
		private const char NewLineChar = '\n';

		private StreamReader _reader;
		private int _recordPos;
		private int _columnPos;
		private int _columnLen;

		private bool FirstRecord
		{
			get { return _recordPos == 0; }
		}

		public Encoding CurrentEncoding
		{
			get { return _reader.CurrentEncoding; }
		}

		public bool EndOfTable
		{
			get { return _reader.EndOfStream; }
		}


		/// <summary>
		/// Gets the number of columns in the CSV.
		/// </summary>
		public int ColumnCount
		{
			get
			{
				if (FirstRecord)
					throw new InvalidOperationException(Strings.ReadColumnCountUnknown);
				return _columnLen;
			}
		}

		/// <summary>
		/// Gets the current record the reader is positioned at.
		/// </summary>
		public int CurrentRecord
		{
			get { return _recordPos; }
		}

		/// <summary>
		/// Gets the current field the reader is positioned at.
		/// </summary>
		public int CurrentField
		{
			get { return _columnPos; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvReader"/> class with the specified stream,
		/// using the default encoding and buffer size.
		/// </summary>
		/// <param name="stream">The stream to read from.</param>
		public CsvReader(Stream stream)
			: this(stream, Encoding.UTF8, DefaultBufferSize, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvReader"/> class with the specified stream,
		/// using the specified encoding and default buffer size.
		/// </summary>
		/// <param name="stream">The stream to read from.</param>
		/// <param name="encoding">The character encoding used to interpret the stream.</param>
		public CsvReader(Stream stream, Encoding encoding)
			: this(stream, encoding, DefaultBufferSize, false)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvReader"/> class with the specified stream,
		/// using the specified encoding and buffer size.
		/// </summary>
		/// <param name="stream">The stream to read from.</param>
		/// <param name="encoding">The character encoding used to interpret the stream.</param>
		/// <param name="bufferSize">The buffer size, in bytes.</param>
		public CsvReader(Stream stream, Encoding encoding, int bufferSize)
			: this(stream, encoding, bufferSize, false)
		{

		}

		public CsvReader(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
		{
			_reader = new StreamReader(stream, encoding, false, bufferSize, leaveOpen);
			_recordPos = 0;
			_columnLen = 0;
			_columnPos = 0;
		}

		public string ReadField()
		{
			throw new NotImplementedException();
		}

		private string ReadEscapedField()
		{
			throw new NotImplementedException();
		}

		public string[] ReadRecord()
		{
			if (FirstRecord)
				return ReadFirstRecord();

			throw new NotImplementedException();
		}

		private string[] ReadFirstRecord()
		{
			throw new NotImplementedException();
		}

		private string NextField()
		{
			throw new NotImplementedException();
		}

		private void IncrementCurrentColumn()
		{
			if (FirstRecord)
				_columnLen += 1;
			_columnPos += 1;
		}

		private void IncrementCurrentRecord()
		{
			_recordPos += 1;
			_columnPos = 0;
		}

		public void Dispose()
		{
			if (_reader != null)
			{
				_reader.Dispose();
				_reader = null;

				_columnLen = 0;
				_columnPos = 0;
				_recordPos = 0;
			}
		}
	}
}
