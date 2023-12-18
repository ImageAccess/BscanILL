using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace BscanILL.Misc
{
	class FileEncryption
	{
		
		// PUBLIC METHODS
		#region public methods

		#region Encrypt()
		internal static void Encrypt(FileInfo source, FileInfo destination, string password, string passwordSalt)
		{
			using (FileStream streamReader = source.OpenRead())
			{
				using (FileStream streamWriter = destination.OpenWrite())
				{
					using (Aes aes = new AesManaged())
					{
						Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(passwordSalt));
						aes.Key = deriveBytes.GetBytes(16);
						streamWriter.Write(BitConverter.GetBytes(aes.IV.Length), 0, sizeof(int));
						streamWriter.Write(aes.IV, 0, aes.IV.Length);

						using (CryptoStream cs = new CryptoStream(streamWriter, aes.CreateEncryptor(), CryptoStreamMode.Write))
						{
							//byte[] buffer = Encoding.Unicode.GetBytes("hovno");
							byte[] buffer = new byte[streamReader.Length];
							streamReader.Read(buffer, 0, buffer.Length);

							cs.Write(buffer, 0, buffer.Length);
							cs.FlushFinalBlock();
						}
					}
				}
			}
		}
		#endregion

		#region Decrypt()
		internal static void Decrypt(FileInfo source, FileInfo destination, string password, string passwordSalt)
		{            
			using (FileStream streamReader = source.OpenRead())
			{                
				using (FileStream streamWriter = destination.OpenWrite())
				{                    
					using (Aes aes = new AesManaged())
					{                        
						Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(passwordSalt));                        
						aes.Key = deriveBytes.GetBytes(16);                        
						aes.IV = ReadByteArray(streamReader);                        
						using (CryptoStream cs = new CryptoStream(streamReader, aes.CreateDecryptor(), CryptoStreamMode.Read))
						{                            
							int bytesRead;
							byte[] buffer = new byte[1024 * 1024];
							while ((bytesRead = cs.Read(buffer, 0, 1024 * 1024)) > 0)
							{
								streamWriter.Write(buffer, 0, bytesRead);
							}                         
						}
					}
				}
			}
		}
		#endregion

		#endregion

		// PRIVATE METHODS
		#region private methods

		#region ReadByteArray()
		private static byte[] ReadByteArray(Stream s)
		{
			byte[] rawLength = new byte[sizeof(int)];
			if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
			{
				throw new SystemException("Stream did not contain properly formatted byte array");
			}

			byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
			if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
			{
				throw new SystemException("Did not read byte array properly");
			}

			return buffer;
		}
		#endregion

		#endregion

	}
}
