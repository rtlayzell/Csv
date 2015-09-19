using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace Csv.Tests
{
	[TestClass]
	public class CsvWriterTests
	{


		[TestMethod]
		public void Construct_WithStream()
		{
			using (var stream = new MemoryStream())
			using (var writer = new CsvWriter(stream))
			{
				Assert.AreEqual(0, writer.CurrentField);
				Assert.AreEqual(0, writer.CurrentRecord);
			}
		}

		[TestMethod]
		public void Construct_WithStreamAndEncoding()
		{
			using (var stream = new MemoryStream())
			using (var writer = new CsvWriter(stream, Encoding.ASCII))
			{
				Assert.AreEqual(Encoding.ASCII, writer.Encoding);
				Assert.AreEqual(0, writer.CurrentField);
				Assert.AreEqual(0, writer.CurrentRecord);
			}
		}

		[TestMethod]
		public void WriteField_Simple()
		{
			using (var stream = new MemoryStream())
			using (var writer = new CsvWriter(stream))
			{
				string actual = null;

				writer.WriteField("Field1");
				writer.Flush();

				Assert.AreEqual(1, writer.CurrentField);
				Assert.AreEqual(0, writer.CurrentRecord);

				actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual("Field1", actual);

				writer.WriteField("Field2");
				writer.Flush();

				Assert.AreEqual(2, writer.CurrentField);
				Assert.AreEqual(0, writer.CurrentRecord);

				actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual("Field1,Field2", actual);
			}
		}

		[TestMethod]
		public void Flush()
		{
			using (var stream = new MemoryStream())
			using (var writer = new CsvWriter(stream))
			{
				string actual = String.Empty;

				writer.Flush();

				actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual("", actual);

				writer.WriteField("Field1");


				actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual("", actual);

				writer.Flush();

				actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual("Field1", actual);

			}
		}

		[TestMethod]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close()
		{
			var stream = new MemoryStream();
			var writer = new CsvWriter(stream, Encoding.ASCII, 1024, false);

			writer.Close();
			stream.Close();

			writer.WriteField("Hello");
		}

		[TestMethod]
		public void WriteField_EscapedWhiteSpace()
		{
			using (var stream = new MemoryStream())
			using (var writer = new CsvWriter(stream))
			{
				string actual = null;

				writer.WriteField("  Field1");
				writer.WriteField("Field2  ");
				writer.WriteField("Field3\n");
				writer.WriteField("\nField4");
				writer.Flush();

				actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual("\"  Field1\",\"Field2  \",\"Field3\n\",\"\nField4\"", actual);

				writer.WriteField("Field5\t");
				writer.WriteField("\tField6");
				writer.WriteField("Field7"); // non-escaped
				writer.Flush();

				actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual("\"  Field1\",\"Field2  \",\"Field3\n\",\"\nField4\",\"Field5\t\",\"\tField6\",Field7", actual);
			}
		}

		public void WriteField_EscapedDelimiters()
		{

		}

		[TestMethod]
		public void WriteField_TrackingCurrentFieldAndRecord()
		{
			using (var stream = new MemoryStream())
			using (var writer = new CsvWriter(stream))
			{
				var builder = new StringBuilder();

				for (int i = 0; i < 10; ++i)
				{
					Assert.AreEqual(i, writer.CurrentRecord);
					for (int j = 0; j < 5; ++j)
					{
						Assert.AreEqual(j, writer.CurrentField);

						var s = String.Format("Field({0}:{1})", writer.CurrentRecord, writer.CurrentField);
						builder.Append(s + ",");
						writer.WriteField(s);
					}

					builder.Append(Environment.NewLine);
					writer.WriteRecord();
				}

				writer.Flush();

				var expected = builder.ToString()
					.Replace("," + Environment.NewLine, Environment.NewLine)
					.TrimEnd(Environment.NewLine.ToCharArray());


				var actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void WriteField_TypeOverloads()
		{

			using (var stream = new MemoryStream())
			using (var writer = new CsvWriter(stream))
			{
				char[] chArray = "Field".ToCharArray();
				object obj = new object();

				writer.WriteField(true);
				writer.WriteField(false);

				writer.WriteField('c');
				writer.WriteField(chArray);
				writer.WriteField(chArray, 0, chArray.Length - 2);

				writer.WriteField((int)0);
				writer.WriteField((int)Int32.MaxValue);
				writer.WriteField((int)Int32.MinValue);

				writer.WriteField((uint)0);
				writer.WriteField((uint)UInt32.MaxValue);
				writer.WriteField((uint)UInt32.MinValue);

				writer.WriteField((long)0);
				writer.WriteField((long)Int64.MaxValue);
				writer.WriteField((long)Int64.MinValue);

				writer.WriteField((ulong)0);
				writer.WriteField((ulong)UInt64.MaxValue);
				writer.WriteField((ulong)UInt64.MinValue);

				writer.WriteField((float)3.14f);
				writer.WriteField((double)3.1415);
				writer.WriteField((decimal)3.1415926m);

				writer.WriteField(obj);
				writer.Flush();

				var expected = String.Format(
					"{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}",
					Boolean.TrueString,
					Boolean.FalseString,
					'c', new string(chArray),
					new string(chArray, 0, chArray.Length - 2),
					((int)0).ToString(writer.FormatProvider),
					Int32.MaxValue.ToString(writer.FormatProvider),
					Int32.MinValue.ToString(writer.FormatProvider),
					((uint)0).ToString(writer.FormatProvider),
					UInt32.MaxValue.ToString(writer.FormatProvider),
					UInt32.MinValue.ToString(writer.FormatProvider),
					((long)0).ToString(writer.FormatProvider),
					Int64.MaxValue.ToString(writer.FormatProvider),
					Int64.MinValue.ToString(writer.FormatProvider),
					((ulong)0).ToString(writer.FormatProvider),
					UInt64.MaxValue.ToString(writer.FormatProvider),
					UInt64.MinValue.ToString(writer.FormatProvider),
					((float)3.14f).ToString(writer.FormatProvider),
					((double)3.1415).ToString(writer.FormatProvider),
					((decimal)3.1415926m).ToString(writer.FormatProvider),
					obj.ToString());

				var actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void WriteRecord_Simple()
		{
			using (var stream = new MemoryStream())
			using (var writer = new CsvWriter(stream))
			{
				writer.WriteRecord("Field1", "Field2", "Field3", "Field4");
				writer.Flush();

				Assert.AreEqual(0, writer.CurrentField);
				Assert.AreEqual(1, writer.CurrentRecord);

				var actual = writer.Encoding.GetString(stream.ToArray());
				Assert.AreEqual("Field1,Field2,Field3,Field4", actual);
			}
		}

		[TestMethod]
		public void WriteRecord_EscapedWhiteSpace()
		{

		}
	}
}
