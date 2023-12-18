using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BscanILL.Misc //RapidoDLLWrapper
{

	public enum RapidoFileTypes
	{
		None,
		Jpeg,
		Tiff,
		Png,
		Bmp,
		Pdf,
		Zip
	}

	/*
	 *   loading of multiple dlls that implement 'ILoader' interface with 'Message' method and calling the method using all the dynamically loaded dlls
	 * 
	 * 
		  string extensionPath = "location of the dynamically loaded dlss"
		  var pluginFiles = Directory.GetFiles( extensionPath, "*.dll")

		 // Load the assembly info

		 var loaders = ( 
						 from file in pluginFiles
						 let asm = Assembly.LoadFile(file)
						 from type in asm.GetTypes()
						 where typeof(ILoader).IsAssignableFrom(type)
						 select (ILoader)Activator.CreateInstance(type)
					   ).ToArray();

		// Call method of each loader
		foreach( var loader in loaders)
		{
			Console.WriteLine("{0}" , loader.Message());
		}

		Console.ReadLine();

	 */


	/*
	 *  another way how to load library dynamically is using kernel32.dll 's LoadLibrary(), FreeLibrary, GetProcAddress methods
	  
    // import necessary API as shown in other examples
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LoadLibrary(string lib);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern void FreeLibrary(IntPtr module);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr module, string proc);

    // declare a delegate with the required signature
    private delegate int AddDelegate(int a, int b);

    private static void Main()
    {
        // load the dll
        IntPtr module = LoadLibrary("myDLL.dll");
        if (module == IntPtr.Zero) // error handling
        {
            Console.WriteLine($"Could not load library: {Marshal.GetLastWin32Error()}");
            return;
        }

        // get a "pointer" to the method
        IntPtr method = GetProcAddress(module, "add");
        if (method == IntPtr.Zero) // error handling
        {
            Console.WriteLine($"Could not load method: {Marshal.GetLastWin32Error()}");
            FreeLibrary(module);  // unload library
            return;
        }
            
        // convert "pointer" to delegate
        AddDelegate add = (AddDelegate)Marshal.GetDelegateForFunctionPointer(method, typeof(AddDelegate));
    
        // use function    
        int result = add(750, 300);
        
        // unload library   
        FreeLibrary(module);
    }	  
	 
	
	*/

	public static class DllsLoader
	{
		static bool					_isAlreadyLoaded = false;
		static readonly object		_staticThreadLocker = new object();

		static object objRapidoExLibrisWrapper = null;

		static PropertyInfo propApiKey = null;
		static PropertyInfo propDocID = null;
		static PropertyInfo propFileInfo = null;
		static PropertyInfo propFileType = null;		

		static PropertyInfo propRetrievalRequestID = null;
		static PropertyInfo propRetrievalExternalRequestID = null;
		static PropertyInfo propRequestStatus = null;
		static PropertyInfo propLastError = null;		

		static MethodInfo methodClear = null;
		static MethodInfo methodPostArticle = null;

		static Delegate delegadeForRapido = null;
		static EventInfo eventProgressChanged = null;		

		public static bool IsRapidoLoaded
        {
			get { return _isAlreadyLoaded;  }
        }

		public static PropertyInfo ApiKey
		{
			get { return propApiKey; }
		}

		public static PropertyInfo DocID
		{
			get { return propDocID; }
		}

		public static PropertyInfo FileInfo
		{
			get { return propFileInfo; }
		}
		
		public static PropertyInfo FileType
		{
			get { return propFileType; }
		}

		public static PropertyInfo RetrievalRequestID
		{
			get { return propRetrievalRequestID; }
		}

		public static PropertyInfo RetrievalExternalRequestID
		{
			get { return propRetrievalExternalRequestID; }
		}

		public static PropertyInfo RequestStatus
		{
			get { return propRequestStatus; }
		}
		
		public static PropertyInfo LastError
		{
			get { return propLastError; }
		}

		public static MethodInfo Clear
		{
			get { return methodClear; }
		}

		public static MethodInfo PostArticle
		{
			get { return methodPostArticle; }
		}
		
		public static Delegate DelegadeForRapido
		{
			get { return delegadeForRapido; }
		}

		public static EventInfo ProgressChanged
		{
			get { return eventProgressChanged; }
		}

		public static object RapidoObject
		{
			get { return objRapidoExLibrisWrapper; }
		}

		public static void LoadLibrariesIntern( object caller, string callBackEventMethodName )
		{
			lock (_staticThreadLocker)
			{
				if (_isAlreadyLoaded == false)
				{
					try
					{
						DirectoryInfo dir = new FileInfo(System.Reflection.Assembly.GetCallingAssembly().Location).Directory;

						/*
											if (IntPtr.Size == 8)
											{
												NativeMethods.LoadLibrary(Path.Combine(dir.FullName, @"x64\EDSDK.dll"));
												NativeMethods.LoadLibrary(Path.Combine(dir.FullName, @"x64\EdsImage.dll"));
											}
											else
											{
												NativeMethods.LoadLibrary(Path.Combine(dir.FullName, @"x86\EDSDK.dll"));
												NativeMethods.LoadLibrary(Path.Combine(dir.FullName, @"x86\EdsImage.dll"));
											}
						*/

						if( ! File.Exists( Path.Combine(dir.FullName, @"RapidoWrapper.dll") ) )
						{
							//throw new Exception("File " + Path.Combine(dir.FullName, @"RapidoWrapper.dll") + " Not Found!");
							throw new Exception("File Not Found!");
						}


						var assembly = Assembly.LoadFile(Path.Combine(dir.FullName, @"RapidoWrapper.dll"));
						var typeRapidoExLibrisWrapper = assembly.GetType("RapidoWrapper.RapidoExLibrisWrapper");   // ("namespace.class")					
						System.Type delegateTypeRapido = null;


						objRapidoExLibrisWrapper = Activator.CreateInstance(typeRapidoExLibrisWrapper);

						propApiKey = typeRapidoExLibrisWrapper.GetProperty("ApiKey");
						propDocID = typeRapidoExLibrisWrapper.GetProperty("DocID");
						propFileInfo = typeRapidoExLibrisWrapper.GetProperty("FilePath");
						propFileType = typeRapidoExLibrisWrapper.GetProperty("FileType");
						propRetrievalRequestID = typeRapidoExLibrisWrapper.GetProperty("RetrievalRequestID");
						propRetrievalExternalRequestID = typeRapidoExLibrisWrapper.GetProperty("RetrievalExternalRequestID");
						propRequestStatus = typeRapidoExLibrisWrapper.GetProperty("RequestStatus");
						propLastError = typeRapidoExLibrisWrapper.GetProperty("LastError");

						eventProgressChanged = typeRapidoExLibrisWrapper.GetEvent("ProgressChanged");
						//delegateTypeRapido = RapidoDLLWrapper.DllsLoader.eventProgressChanged.EventHandlerType;
						delegateTypeRapido = DllsLoader.eventProgressChanged.EventHandlerType;

						//!!! rapidoWrapper_ProgressChanged must be pubic to get myCallback set!!!
						//var myCallback = typeof(Form1).GetMethod("rapidoWrapper_ProgressChanged");  
						//delegadeForRapido = Delegate.CreateDelegate(delegateTypeRapido, this, myCallback);

						//!!! this works even if rapidoWrapper_ProgressChanged is private unlike code above
						//delegadeForRapido = Delegate.CreateDelegate(delegateTypeRapido, caller, "rapidoWrapper_ProgressChanged");
						if(( callBackEventMethodName !=null ) && (callBackEventMethodName.Length > 0))
                        {
							delegadeForRapido = Delegate.CreateDelegate(delegateTypeRapido, caller, callBackEventMethodName);
						}

						//var method = typeRapidoExLibrisWrapper.GetMethod("PostArticle");
						methodPostArticle = typeRapidoExLibrisWrapper.GetMethod("PostArticle");

						methodClear = typeRapidoExLibrisWrapper.GetMethod("Clear");

						//_isAlreadyLoaded = true;
					}

					catch ( Exception ex)
                    {
						throw new Exception( "Unable to Load Rapido Library! " + ex.Message + " Please Contact DLSG's Tech Support!");
                    }
				}
			}
		}

		public static void LoadLibraries( object caller, string apiKey , string callBackEventMethodName)
		{
			lock (_staticThreadLocker)
			{
				if (_isAlreadyLoaded == false)
				{
					LoadLibrariesIntern( caller , callBackEventMethodName);

					//RapidoDLLWrapper.DllsLoader.ApiKey.SetValue(RapidoDLLWrapper.DllsLoader.RapidoObject, apiKey);
					if (RapidoObject != null)
					{
						ApiKey.SetValue(RapidoObject, apiKey);
						var objAK = ApiKey.GetValue(RapidoObject);
						if ((objAK != null) && (((string)objAK).Length > 0))
						{
							_isAlreadyLoaded = true;
						}
					}
				}
			}
		}		

	}
}
