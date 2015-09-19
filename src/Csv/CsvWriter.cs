using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Csv.Resources;

namespace Csv
{
	public class CsvWriter : IDisposable
	{
		private const int DefaultBufferSize = 1024;
		private const int DefaultFileBufferSize = 4096;

		private const char QuoteChar = '\"';
		private const char DelimChar = ',';
		private const char SpaceChar = ' ';
		private const char TabChar = '\t';
		private const char NewLineChar = '\n';

		private TextWriter _writer;
		private int _columnLen;
		private int _columnPos;
		private int _recordPos;


		private static Encoding _defaultEncoding;

		/// <summary>
		/// Gets the default encoding used by <see cref="CsvWriter"/>.
		/// </summary>
		private static Encoding DefaultEncoding
		{
			get
			{
				return System.Threading.LazyInitializer
					.EnsureInitialized(ref _defaultEncoding,
					() => new UTF8Encoding(false, true));
			}
		}

		/// <summary>
		/// Gets a value indicating whether the writer is currently
		/// positioned at the first record in the CSV.
		/// </summary>
		private bool IsFirstRecord
		{
			get { return _recordPos == 0; }
		}

		/// <summary>
		/// Gets a value indicating that the writer is currently positioned
		/// at the first field of the record.
		/// </summary>
		private bool IsFirstField
		{
			get { return _columnPos == 0; }
		}

		public int CurrentField { get { return _columnPos; } }

		public int CurrentRecord { get { return _recordPos; } }


		/// <summary>
		/// Gets an object that controls formatting.
		/// </summary>
		public IFormatProvider FormatProvider
		{
			get { return _writer.FormatProvider; }
		}

		///// <summary>
		///// Gets the underlying stream that interfaces with a backing store.
		///// </summary>
		//public Stream BaseStream
		//{
		//	get { return _writer.BaseStream; }
		//}

		/// <summary>
		/// Gets the <see cref="System.Text.Encoding"/> in which the output is written.
		/// </summary>
		public Encoding Encoding
		{
			get { return _writer.Encoding; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class for the specified file,
		/// using the default encoding and buffer size.
		/// </summary>
		/// <param name="path">The complete file path to write to. path can be a file name.</param>
		public CsvWriter(string path)
			: this(path, DefaultEncoding, DefaultBufferSize)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class for the specified file,
		/// using the specified encoding and the default buffer size.
		/// </summary>
		/// <param name="path">The complete file path to write to. path can be a file name.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public CsvWriter(string path, Encoding encoding)
			: this(path, encoding, DefaultBufferSize)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class for the specified file,
		/// using the specified encoding and buffer size.
		/// </summary>
		/// <param name="path">The complete file path to write to. path can be a file name.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="bufferSize">The size of the buffer, in bytes.</param>
		public CsvWriter(string path, Encoding encoding, int bufferSize)
		{
			Stream stream = _CreateFile(path);
			_Init(stream, encoding, bufferSize, false);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class with the specified stream,
		/// using UTF8 Encoding and the default buffer size.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public CsvWriter(Stream stream)
			: this(stream, DefaultEncoding, DefaultBufferSize, false)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class for the specified stream,
		/// using the specified encoding and default buffer size.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="encoding">The character encoding to use.</param>
		public CsvWriter(Stream stream, Encoding encoding)
			: this(stream, encoding, DefaultBufferSize, false)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class for the specified stream,
		/// using the specified encoding and buffer size.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="bufferSize">The size of the buffer, in bytes.</param>
		public CsvWriter(Stream stream, Encoding encoding, int bufferSize)
			: this(stream, encoding, bufferSize, false)
		{

		}

		/// <summary>
		/// Inintializes a new instance of the <see cref="CsvWriter"/> class for the specified stream,
		/// using the specified encoding and buffer size. Optionally specify whether to leave the stream
		/// open after disposing of the writer instance.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <param name="bufferSize">The size of the buffer, in bytes</param>
		/// <param name="leaveOpen">true to leave stream open after the <see cref="CsvWriter"/> object has been disposed; otherwise, false.</param>
		public CsvWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
		{
			_Init(stream, encoding, bufferSize, leaveOpen);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class with the specified TextWriter,
		/// </summary>
		/// <param name="output">The writer to which output is written.</param>
		public CsvWriter(TextWriter output)
		{
			_writer = output;
			_columnLen = 0;
			_columnPos = 0;
			_recordPos = 0;
		}

		private static FileStream _CreateFile(string path)
		{
			return new FileStream(path, FileMode.Create, FileAccess.Write,
				FileShare.Read, DefaultFileBufferSize, FileOptions.SequentialScan);
		}

		private void _Init(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
		{
			_writer = new StreamWriter(stream, encoding, bufferSize, leaveOpen);
			_columnLen = 0;
			_columnPos = 0;
			_recordPos = 0;
		}

		/// <summary>
		/// Closes the current <see cref="CsvWriter"/> object and the current stream.
		/// </summary>
		/// <remarks>
		/// If leaveOpen was specified during construction of the writer, then this
		/// method will not close the underlying stream.
		/// </remarks>
		public void Close()
		{
			Dispose();
		}

		/// <summary>
		/// Clears all buffers for the current writer and causes any buffered data
		/// to be written to the underlying stream.
		/// </summary>
		public void Flush()
		{
			if (_writer == null)
				throw new ObjectDisposedException(ToString());

			_writer.Flush();
		}

		/// <summary>
		/// Writes the [text] representation of a Boolean value to current record in the CSV stream.
		/// </summary>
		/// <param name="value"></param>
		public void WriteField(bool value)
		{
			WriteField(value ? Boolean.TrueString : Boolean.FalseString);
		}

		public void WriteField(char value)
		{
			WriteField(new char[] { value });
		}

		/// <summary>
		/// Writes the text representation of a 4-byte signed integer to the current record in the CSV stream.
		/// </summary>
		/// <param name="value">The 4-byte signed integer to write.</param>
		public void WriteField(int value)
		{
			WriteField(value.ToString(_writer.FormatProvider));
		}

		/// <summary>
		/// Writes the text representation of a 4-byte unsigned integer to the current record in the CSV stream.
		/// </summary>
		/// <param name="value">The 4-byte unsigned integer to write.</param>
		public void WriteField(uint value)
		{
			WriteField(value.ToString(_writer.FormatProvider));
		}

		/// <summary>
		/// Writes the text representation of an 8-byte signed integer to the current record in the CSV stream.
		/// </summary>
		/// <param name="value">The 8-byte signed integer to write.</param>
		public void WriteField(long value)
		{
			WriteField(value.ToString(_writer.FormatProvider));
		}

		/// <summary>
		/// Writes the text representation of an 8-byte unsigned integer to the current record in the CSV stream.
		/// </summary>
		/// <param name="value">The 8-byte unsigned integer to write.</param>
		public void WriteField(ulong value)
		{
			WriteField(value.ToString(_writer.FormatProvider));
		}

		/// <summary>
		/// Writes the text representation of [a|an ...] to the current record in the CSV stream.
		/// </summary>
		/// <param name="value">The [...] to write.</param>
		public void WriteField(float value)
		{
			WriteField(value.ToString(_writer.FormatProvider));
		}

		/// <summary>
		/// Writes the text representation of [a|an ...] to the current record in the CSV stream.
		/// </summary>
		/// <param name="value">The [...] to write.</param>
		public void WriteField(double value)
		{
			WriteField(value.ToString(_writer.FormatProvider));
		}

		/// <summary>
		/// Writes the text representation of [a|an ...] to the current record in the CSV stream.
		/// </summary>
		/// <param name="value">The [...] to write.</param>
		public void WriteField(decimal value)
		{
			WriteField(value.ToString(_writer.FormatProvider));
		}

		/// <summary>
		/// Writes the text representation of [a|an ...] to the current record in the CSV stream.
		/// </summary>
		/// <param name="value">The [...] to write.</param>
		public void WriteField(object field)
		{
			if (field != null)
				WriteField(field.ToString());
			else
				WriteEmptyField();
		}
		private void WriteField(string field)
		{
			if (!String.IsNullOrEmpty(field))
				WriteField(field.ToCharArray());
			else
				WriteEmptyField();
		}

		public void WriteField(char[] buffer)
		{
			if (buffer != null)
				WriteField(buffer, 0, buffer.Length);
			else
				WriteEmptyField();
		}

		public void WriteField(char[] buffer, int index, int count)
		{
			if (_writer == null)
				throw new ObjectDisposedException(ToString());

			if (!IsFirstField)
			{
				// Write a delimiter for fields after the first.
				_writer.Write(DelimChar);
			}
			else if (!IsFirstRecord)
			{
				// First field of a new record, start it on a new line.
				_writer.Write(Environment.NewLine);
			}


			bool isQuoted = false;
			if (ShouldEscape(buffer, index, count))
			{
				_writer.Write(QuoteChar);
				isQuoted = true;
			}

			for (int i = 0; i < count; ++i)
			{
				_writer.Write(buffer[index + i]);
				if (buffer[index + i] == QuoteChar)
					_writer.Write(QuoteChar);
			}

			if (isQuoted)
			{
				_writer.Write(QuoteChar);
			}

			IncrementField();
		}

		/// <summary>
		/// Writes the comma separated text representation of a series of objects as a complete record to the CSV stream.
		/// </summary>
		/// <param name="fields">The objects to write to the stream.</param>
		public void WriteRecord(params object[] fields)
		{
			WriteRecord((IEnumerable<object>)fields);
		}

		/// <summary>
		/// Writes the comma separated text representation of a series of objects as a complete record to the CSV stream.
		/// </summary>
		/// <param name="fields">The objects to write to the stream.</param>
		public void WriteRecord(IEnumerable<object> fields)
		{
			if (fields != null && fields.Count() > 0)
			{
				foreach (var field in fields)
					WriteField(field);
			}

			EndCurrentRecord();
		}

		/// <summary>
		/// Ends the current record for writing and begins a new record.
		/// </summary>
		private void EndCurrentRecord()
		{
			if (_columnPos > 0)
			{
				_columnPos = 0;
				_recordPos += 1;
			}
		}

		private void WriteEmptyField()
		{
			if (!IsFirstField)
				_writer.Write(DelimChar);
			IncrementField();
		}

		private void IncrementField()
		{
			if (IsFirstRecord)
				_columnLen += 1;

			// TODO: Use strict column overflow settings here..
			if (_columnPos >= _columnLen)
			{
				throw new InvalidOperationException(
					String.Format(Strings.WriteColumnOverflow, _columnLen));
			}

			_columnPos += 1;
		}

		private bool ShouldEscape(char[] buffer, int index, int count)
		{
			char cbegin = buffer[index];
			char cend = buffer[index + count - 1];

			if (cbegin == SpaceChar || cbegin == TabChar ||
				cend == SpaceChar || cend == TabChar)
				return true;

			if (buffer.Any(x => x == DelimChar || x == QuoteChar || x == NewLineChar))
				return true;

			return false;
		}

		/// <summary>
		/// Releases all resources used by the <see cref="CsvWriter"/>
		/// </summary>
		public void Dispose()
		{
			if (_writer != null)
			{
				_writer.Dispose();
				_writer = null;
				_columnLen = 0;
				_columnPos = 0;
				_recordPos = 0;
			}
		}
	}
}