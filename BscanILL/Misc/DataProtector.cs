using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace BscanILL.Misc
{
	public class DataProtector
	{
		private Store store;

		static private IntPtr NullPtr = ((IntPtr)((int)(0)));
		private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
		private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;

		#region DllImport
		[DllImport("Crypt32.dll", SetLastError = true,
			CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool CryptProtectData(
										  ref DATA_BLOB pDataIn,
										  String szDataDescr,
										  ref DATA_BLOB pOptionalEntropy,
										  IntPtr pvReserved,
										  ref CRYPTPROTECT_PROMPTSTRUCT
											pPromptStruct,
										  int dwFlags,
										  ref DATA_BLOB pDataOut);
		[DllImport("Crypt32.dll", SetLastError = true,
					CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool CryptUnprotectData(
										  ref DATA_BLOB pDataIn,
										  String szDataDescr,
										  ref DATA_BLOB pOptionalEntropy,
										  IntPtr pvReserved,
										  ref CRYPTPROTECT_PROMPTSTRUCT
											pPromptStruct,
										  int dwFlags,
										  ref DATA_BLOB pDataOut);
		[DllImport("kernel32.dll",
					CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		private unsafe static extern int FormatMessage(int dwFlags,
													   ref IntPtr lpSource,
													   int dwMessageId,
													   int dwLanguageId,
													   ref String lpBuffer,
														 int nSize,
													   IntPtr* Arguments);
		#endregion


		#region constructor
		public DataProtector(Store tempStore)
		{
			this.store = tempStore;
		}
		#endregion


		#region struct DATA_BLOB
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct DATA_BLOB
		{
			public int cbData;
			public IntPtr pbData;
		}
		#endregion

		#region struct CRYPTPROTECT_PROMPTSTRUCT
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct CRYPTPROTECT_PROMPTSTRUCT
		{
			public int cbSize;
			public int dwPromptFlags;
			public IntPtr hwndApp;
			public String szPrompt;
		}
		#endregion

		#region enum Store
		public enum Store
		{
			USE_MACHINE_STORE = 1,
			USE_USER_STORE
		};
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Encrypt()
		public byte[] Encrypt(byte[] plainText, byte[] optionalEntropy)
		{
			bool retVal = false;
			DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherTextBlob = new DATA_BLOB();
			DATA_BLOB entropyBlob = new DATA_BLOB();
			CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();

			InitPromptstruct(ref prompt);
			int dwFlags;

			try
			{
				try
				{
					int bytesSize = plainText.Length;
					plainTextBlob.pbData = Marshal.AllocHGlobal(bytesSize);

					if (IntPtr.Zero == plainTextBlob.pbData)
						throw new Exception("Unable to allocate plaintext buffer.");

					plainTextBlob.cbData = bytesSize;
					Marshal.Copy(plainText, 0, plainTextBlob.pbData, bytesSize);
				}
				catch (Exception ex)
				{
					throw new Exception("Exception marshalling data. " + ex.Message);
				}

				if (Store.USE_MACHINE_STORE == store)
				{//Using the machine store, should be providing entropy.
					dwFlags = CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN;
					//Check to see if the entropy is null

					if (null == optionalEntropy)
					{//Allocate something
						optionalEntropy = new byte[0];
					}

					try
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(optionalEntropy.Length); ;

						if (IntPtr.Zero == entropyBlob.pbData)
							throw new Exception("Unable to allocate entropy data buffer.");

						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, bytesSize);
						entropyBlob.cbData = bytesSize;
					}
					catch (Exception ex)
					{
						throw new Exception("Exception entropy marshalling data. " +
						ex.Message);
					}
				}
				else
				{//Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}

				retVal = CryptProtectData(ref plainTextBlob, "", ref entropyBlob, IntPtr.Zero, ref prompt, dwFlags, ref cipherTextBlob);

				if (false == retVal)
					throw new Exception("Encryption failed. " + GetErrorMessage(Marshal.GetLastWin32Error()));

				//Free the blob and entropy.
				if (IntPtr.Zero != plainTextBlob.pbData)
					Marshal.FreeHGlobal(plainTextBlob.pbData);

				if (IntPtr.Zero != entropyBlob.pbData)
					Marshal.FreeHGlobal(entropyBlob.pbData);
			}
			catch (Exception ex)
			{
				throw new Exception("Exception encrypting. " + ex.Message);
			}

			byte[] cipherText = new byte[cipherTextBlob.cbData];
			Marshal.Copy(cipherTextBlob.pbData, cipherText, 0, cipherTextBlob.cbData);
			Marshal.FreeHGlobal(cipherTextBlob.pbData);

			return cipherText;
		}
		#endregion

		#region Decrypt()
		public byte[] Decrypt(byte[] cipherText, byte[] optionalEntropy)
		{
			bool retVal = false;
			DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherBlob = new DATA_BLOB();
			CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();

			InitPromptstruct(ref prompt);

			try
			{
				try
				{
					int cipherTextSize = cipherText.Length;
					cipherBlob.pbData = Marshal.AllocHGlobal(cipherTextSize);

					if (IntPtr.Zero == cipherBlob.pbData)
						throw new Exception("Unable to allocate cipherText buffer.");

					cipherBlob.cbData = cipherTextSize;
					Marshal.Copy(cipherText, 0, cipherBlob.pbData, cipherBlob.cbData);
				}
				catch (Exception ex)
				{
					throw new Exception("Exception marshalling data. " + ex.Message);
				}

				DATA_BLOB entropyBlob = new DATA_BLOB();
				int dwFlags;

				if (Store.USE_MACHINE_STORE == store)
				{//Using the machine store, should be providing entropy.
					dwFlags = CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN;
					//Check to see if the entropy is null

					if (null == optionalEntropy)
					{//Allocate something
						optionalEntropy = new byte[0];
					}

					try
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(bytesSize);

						if (IntPtr.Zero == entropyBlob.pbData)
							throw new Exception("Unable to allocate entropy buffer.");

						entropyBlob.cbData = bytesSize;
						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, bytesSize);
					}
					catch (Exception ex)
					{
						throw new Exception("Exception entropy marshalling data. " +
											ex.Message);
					}
				}
				else
				{//Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}
				retVal = CryptUnprotectData(ref cipherBlob, null, ref entropyBlob, IntPtr.Zero, ref prompt, dwFlags, ref plainTextBlob);

				if (false == retVal)
					throw new Exception("Decryption failed. " + GetErrorMessage(Marshal.GetLastWin32Error()));

				//Free the blob and entropy.
				if (IntPtr.Zero != cipherBlob.pbData)
					Marshal.FreeHGlobal(cipherBlob.pbData);

				if (IntPtr.Zero != entropyBlob.pbData)
					Marshal.FreeHGlobal(entropyBlob.pbData);
			}
			catch (Exception ex)
			{
				throw new Exception("Exception decrypting. " + ex.Message);
			}

			byte[] plainText = new byte[plainTextBlob.cbData];

			Marshal.Copy(plainTextBlob.pbData, plainText, 0, plainTextBlob.cbData);
			Marshal.FreeHGlobal(plainTextBlob.pbData);

			return plainText;
		}
		#endregion

		#region GetString()
		public static string GetString(System.Security.SecureString ss)
		{
			IntPtr		ptr = Marshal.SecureStringToBSTR(ss);
			string		str = Marshal.PtrToStringAuto(ptr); // this now makes it insecure and held in regular managed memory
			
			Marshal.ZeroFreeBSTR(ptr);
			return str;
		}
		#endregion

		#region GetSecureString()
		public static SecureString GetSecureString(byte[] buffer)
		{
			if (buffer != null && buffer.Length > 0)
			{
				string salt = "KicPharosSupport678";
				byte[] saltBytes = Encoding.Unicode.GetBytes(salt);

				byte[] bytes = System.Security.Cryptography.ProtectedData.Unprotect(buffer, saltBytes, DataProtectionScope.LocalMachine);
				string str = Encoding.Unicode.GetString(bytes);

				SecureString secureString = new SecureString();

				foreach (char ch in str.ToCharArray())
					secureString.AppendChar(ch);

				secureString.MakeReadOnly();
				return secureString;
			}
			else
				return new SecureString();
		}
		#endregion

		#region GetArray()
		public static byte[] GetArray(SecureString secureString)
		{
			string salt = "KicPharosSupport678";
			byte[] saltBytes = Encoding.Unicode.GetBytes(salt);

			IntPtr unmanagedBytes = IntPtr.Zero;
			int length = secureString.Length * 2;
			byte[] bytes = new byte[length];

			try
			{
				unmanagedBytes = System.Runtime.InteropServices.Marshal.SecureStringToCoTaskMemUnicode(secureString);
				System.Runtime.InteropServices.Marshal.Copy(unmanagedBytes, bytes, 0, length);
			}
			finally
			{
				if (unmanagedBytes != IntPtr.Zero)
					System.Runtime.InteropServices.Marshal.ZeroFreeCoTaskMemUnicode(unmanagedBytes);
			}

			return ProtectedData.Protect(bytes, saltBytes, DataProtectionScope.LocalMachine);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region InitPromptstruct()
		private void InitPromptstruct(ref CRYPTPROTECT_PROMPTSTRUCT ps)
		{
			ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
			ps.dwPromptFlags = 0;
			ps.hwndApp = NullPtr;
			ps.szPrompt = null;
		}
		#endregion

		#region GetErrorMessage()
		private unsafe static String GetErrorMessage(int errorCode)
		{
			int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
			int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
			int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
			int messageSize = 255;
			String lpMsgBuf = "";
			int dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER |
			  FORMAT_MESSAGE_FROM_SYSTEM |
						  FORMAT_MESSAGE_IGNORE_INSERTS;
			IntPtr ptrlpSource = new IntPtr();
			IntPtr prtArguments = new IntPtr();
			int retVal = FormatMessage(dwFlags, ref ptrlpSource, errorCode, 0,
									   ref lpMsgBuf, messageSize,
										 &prtArguments);
			if (0 == retVal)
			{
				throw new Exception("Failed to format message for error code " +
									errorCode + ". ");
			}

			return lpMsgBuf;
		}
		#endregion

		#endregion
	}

}
