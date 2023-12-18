using System;


namespace Scanners
{
	/// <summary>
	/// Summary description for ScannersEx.
	/// </summary>
	public class ScannersEx : Exception
	{
		public readonly AlertType	Type = AlertType.Error;


		#region constructor
		public ScannersEx(string message)
			: base(message)
		{
		}

		public ScannersEx(Exception ex)
			: base(ex.Message, ex)
		{
		}

		public ScannersEx(string message, AlertType type)
			: base(message)
		{
			this.Type = type;
		}

		public ScannersEx(Exception ex, AlertType type)
			: base(ex.Message, ex)
		{
			this.Type = type;
		}
		#endregion


		#region AlertType
		public enum AlertType
		{
			Information,
			Question,
			Warning,
			Error
		}
		#endregion

	}
}
