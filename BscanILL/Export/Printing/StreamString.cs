using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;

namespace BscanILL.Export.Printing
{
	public class StreamString : IDisposable
	{
		private Stream ioStream;
		private UnicodeEncoding streamEncoding;

		#region constructor
		public StreamString(Stream ioStream)
		{
			this.ioStream = ioStream;
			streamEncoding = new UnicodeEncoding();
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
		}
		#endregion


		#region enum MessageType
		public enum MessageType : byte
		{
			Success = 0x01,
			Error = 0x02,
			Description = 0x04,
			Progress = 0x08,
			PrintBatchXml = 0x10,
			PharosWindow = 0x20
		}
		#endregion


		#region class IMessageBase
		public interface IMessageBase
		{
		}
		#endregion

		#region class SuccessMessage
		public class SuccessMessage : IMessageBase
		{
			public SuccessMessage()
			{
			}
		}
		#endregion

		#region class ErrorMessage
		public class ErrorMessage : IMessageBase
		{
			public string Message;

			public ErrorMessage(string message)
			{
				this.Message = message;
			}
		}
		#endregion

		#region class DescriptionMessage
		public class DescriptionMessage : IMessageBase
		{
			public string Description;

			public DescriptionMessage(string description)
			{
				this.Description = description;
			}
		}
		#endregion

		#region class ProgressMessage
		public class ProgressMessage : IMessageBase
		{
			public double Progress;

			public ProgressMessage(double progress)
			{
				this.Progress = progress; 
			}
		}
		#endregion
		
		#region class PrintBatchXmlMessage
		[Serializable]
		public class PrintBatchXmlMessage : IMessageBase
		{
			public string Xml;

			public PrintBatchXmlMessage(string xml)
			{
				this.Xml = xml;
			}
		}
		#endregion

		#region class PharosWindowMessage
		[Serializable]
		public class PharosWindowMessage : IMessageBase
		{
			public PharosWindowType WindowType;
			public string			PharosUserId = null;
			public string			PharosUserName = null;
			public string			PharosJobName = null;

			public PharosWindowMessage()
			{
				this.WindowType = PharosWindowType.UserId;
			}

			public PharosWindowMessage(string pharosUserId)
			{
				this.WindowType = PharosWindowType.UserId;
				this.PharosUserId = pharosUserId;
			}

			public PharosWindowMessage(string pharosUserName, string pharosJobName)
			{
				this.WindowType = PharosWindowType.UsernameJobname;
				this.PharosUserName = pharosUserName;
				this.PharosJobName = pharosJobName;
			}

			public override string ToString()
			{
				if (this.WindowType == PharosWindowType.UserId)
					return string.Format("User ID Window Type, UserId: {0}", this.PharosUserId);
				else
					return string.Format("User and Job Name Window Type, Username: {0}, Job Name: {1}", this.PharosUserName, this.PharosJobName);
			}
		}
		#endregion

		#region Read()
		public IMessageBase Read()
		{
			int bufferLength = ioStream.ReadByte() * 256 + ioStream.ReadByte();
			MessageType messageType = (MessageType)(ioStream.ReadByte() * 256 + ioStream.ReadByte());

			switch (messageType)
			{
				case MessageType.Success:
					{
						byte[] inBuffer = new byte[bufferLength];
						ioStream.Read(inBuffer, 0, bufferLength);

						return new SuccessMessage();
					}
				case MessageType.Error:
					{
						byte[] inBuffer = new byte[bufferLength];
						ioStream.Read(inBuffer, 0, bufferLength);

						return new ErrorMessage(streamEncoding.GetString(inBuffer));
					}
				case MessageType.Description:
					{
						byte[] inBuffer = new byte[bufferLength];
						ioStream.Read(inBuffer, 0, bufferLength);

						return new DescriptionMessage(streamEncoding.GetString(inBuffer));
					}
				case MessageType.Progress:
					{
						byte[] inBuffer = new byte[bufferLength];
						ioStream.Read(inBuffer, 0, bufferLength);

						return new ProgressMessage(BitConverter.ToDouble(inBuffer, 0));
					}
				case MessageType.PrintBatchXml:
					{
						byte[] inBuffer = new byte[bufferLength];
						ioStream.Read(inBuffer, 0, bufferLength);

						return new PrintBatchXmlMessage(streamEncoding.GetString(inBuffer));
					}
				case MessageType.PharosWindow:
					{
						byte[]				inBuffer = new byte[bufferLength];
						PharosWindowMessage msg;
						
						ioStream.Read(inBuffer, 0, bufferLength);

						System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

						using (MemoryStream stream = new MemoryStream(inBuffer))
						{
							msg = (PharosWindowMessage)formatter.Deserialize(stream);
						}

						return msg;
					}
				default: throw new Exception("Unexpected Message Type!");
			}
		}
		#endregion

		#region WriteDescription()
		public int WriteDescription(string outString)
		{
			byte[]		outBuffer = streamEncoding.GetBytes(outString);
			int			length = Math.Min((int)UInt16.MaxValue, outBuffer.Length);

			ioStream.WriteByte((byte)(length / 256));
			ioStream.WriteByte((byte)(length & 255));
			ioStream.WriteByte((byte)0);
			ioStream.WriteByte((byte)MessageType.Description);
			ioStream.Write(outBuffer, 0, length);
			ioStream.Flush();

			return outBuffer.Length + 4;
		}
		#endregion

		#region WritePrintBatchXml()
		public int WritePrintBatchXml(string outString)
		{
			byte[] outBuffer = streamEncoding.GetBytes(outString);
			int length = Math.Min((int)UInt16.MaxValue, outBuffer.Length);

			ioStream.WriteByte((byte)(length / 256));
			ioStream.WriteByte((byte)(length & 255));
			ioStream.WriteByte((byte)0);
			ioStream.WriteByte((byte)MessageType.PrintBatchXml);
			ioStream.Write(outBuffer, 0, length);
			ioStream.Flush();

			return outBuffer.Length + 4;
		}
		#endregion

		#region WritePharosWindow()
		public int WritePharosWindow(BscanILL.Export.Printing.StreamString.PharosWindowMessage msg)
		{
			System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			byte[] buffer;

			using (MemoryStream stream = new MemoryStream())
			{
				formatter.Serialize(stream, msg);

				stream.Seek(0, SeekOrigin.Begin);
				buffer = new byte[stream.Length];
				stream.Read(buffer, 0, (int) stream.Length);
			}

			int length = Math.Min((int)UInt16.MaxValue, buffer.Length);

			ioStream.WriteByte((byte)(length / 256));
			ioStream.WriteByte((byte)(length & 255));
			ioStream.WriteByte((byte)0);
			ioStream.WriteByte((byte)MessageType.PharosWindow);
			ioStream.Write(buffer, 0, length);
			ioStream.Flush();

			return length + 4;
		}
		#endregion

		#region WriteSuccess()
		public int WriteSuccess()
		{
			int length = 0;

			ioStream.WriteByte((byte)(length / 256));
			ioStream.WriteByte((byte)(length & 255));
			ioStream.WriteByte((byte)0);
			ioStream.WriteByte((byte)MessageType.Success);
			ioStream.Flush();

			return 4;
		}
		#endregion

		#region WriteError()
		public int WriteError(string message)
		{
			byte[] outBuffer = streamEncoding.GetBytes(message);
			int length = Math.Min((int)UInt16.MaxValue, outBuffer.Length);

			ioStream.WriteByte((byte)(length / 256));
			ioStream.WriteByte((byte)(length & 255));
			ioStream.WriteByte((byte)0);
			ioStream.WriteByte((byte)MessageType.Error);
			ioStream.Write(outBuffer, 0, length);
			ioStream.Flush();

			return length + 4;
		}
		#endregion

		#region WriteProgress()
		public int WriteProgress(double progress)
		{
			byte[] outBuffer = BitConverter.GetBytes(progress);
			int length = Math.Min((int)UInt16.MaxValue, outBuffer.Length);

			ioStream.WriteByte((byte)(length / 256));
			ioStream.WriteByte((byte)(length & 255));
			ioStream.WriteByte((byte)0);
			ioStream.WriteByte((byte)MessageType.Progress);
			ioStream.Write(outBuffer, 0, length);
			ioStream.Flush();

			return length + 4;
		}
		#endregion

	}
}
