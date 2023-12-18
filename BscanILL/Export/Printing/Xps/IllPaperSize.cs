using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Printing;
using System.IO;

namespace BscanILL.Export.Printing.Xps
{
	class IllPaperSize : IPaperSize
	{
		const string FEATURENODE = "psf:Feature";
		const string PAPERSIZEATTRIBUTE = "psk:PageMediaSize";
		const string PAPEROPTIONNODE = "psf:Option";
		const string SCOREDPROPERTYNODE = "psf:ScoredProperty";
		const string WIDTHATTRIBUTE = "psk:MediaSizeWidth";
		const string HEIGHTATTRIBUTE = "psk:MediaSizeHeight";
		const string VALUENODE = "psf:Value";
		const string PROPERTNODE = "psf:Property";
		const string DISPLAYNAMEATTRIBUTE = "psk:DisplayName";
		const string NAMEATTRIBUTE = "name";

		double? width = null;
		double? height = null;

		readonly string key;
		readonly string displayName;
		readonly PageMediaSizeName pageMediaSizeName = PageMediaSizeName.Unknown;


		#region constructor
		public IllPaperSize()
		{
		}

		public IllPaperSize(string paperKey, string paperDisplayName)
			: this(paperKey, paperDisplayName, null, null)
		{
		}

		public IllPaperSize(string paperKey, string paperDisplayName, double? paperWidth, double? paperHeight)
		{
			this.key = paperKey;
			this.displayName = paperDisplayName;
			this.width = paperWidth;
			this.height = paperHeight;
			this.pageMediaSizeName = GetPageMediaSizeName(this.key);
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		double? Width			{ get { return (this.width != null) ? (this.width / 25.4) * 96 / 1000 : null; } }
		double? Height			{ get { return (this.height != null) ? (this.height / 25.4) * 96 / 1000 : null; } }
		public double? WidthInMM		{ get { return (this.Width.HasValue) ? (WidthInInches * 25.4) : null; } }
		public double? HeightInMM		{ get { return (this.Height.HasValue) ? (HeightInInches * 25.4) : null; } }
		public double? WidthInInches	{ get { return (this.Width.HasValue) ? (this.Width / 96) : null; } }
		public double? HeightInInches	{ get { return (this.Height.HasValue) ? (this.Height / 96) : null; } }

		public string			Key { get { return this.key; } }
		public string			DisplayName { get { return this.displayName; } }
		public PageMediaSize	PageMediaSize { get { return new PageMediaSize(pageMediaSizeName, this.Width.Value, this.Height.Value); } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region GetPaperSizes()
		public static List<IPaperSize> GetPaperSizes(PrintQueue printQueue)
		{
			List<IPaperSize> lstPaperSizes = new List<IPaperSize>();

			using (MemoryStream memoryStream = printQueue.GetPrintCapabilitiesAsXml())
			{
				using (System.Xml.XmlReader xmlReader = new System.Xml.XmlTextReader(memoryStream))
				{

					try
					{
						while (xmlReader.Read())
						{
							if (xmlReader.NodeType == System.Xml.XmlNodeType.Element && xmlReader.Name == FEATURENODE)
							{
								if (xmlReader.AttributeCount == 1)
									if (xmlReader.GetAttribute(NAMEATTRIBUTE) == PAPERSIZEATTRIBUTE)
										lstPaperSizes = ProcessAllPaperSizes(xmlReader.ReadSubtree());
							}
						}
					}
					catch (Exception ex)
					{
						throw ex;
					}
				}
			}

			return lstPaperSizes;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			return this.DisplayName;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region ProcessAllPaperSizes()
		private static List<IPaperSize> ProcessAllPaperSizes(System.Xml.XmlReader PaperSizeXmlString)
		{
			List<IPaperSize> lstPaperSizes = new List<IPaperSize>();

			try
			{
				while (PaperSizeXmlString.Read())
				{
					if (PaperSizeXmlString.NodeType == System.Xml.XmlNodeType.Element)
					{
						if (PaperSizeXmlString.Name == PAPEROPTIONNODE)
						{
							string currentKey = PaperSizeXmlString.GetAttribute(NAMEATTRIBUTE);
							lstPaperSizes.Add(ProcessPaperSize(currentKey, PaperSizeXmlString.ReadSubtree()));
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return lstPaperSizes;
		}
		#endregion

		#region ProcessPaperSize()
		private static IllPaperSize ProcessPaperSize(string currentPaperKey, System.Xml.XmlReader PaperSizeXmlString)
		{
			double? currentWidth = null, currentHeight = null;
			string currentName = "";
			string stringwidth, stringheight;

			try
			{
				while (PaperSizeXmlString.Read())
				{
					if (PaperSizeXmlString.NodeType == System.Xml.XmlNodeType.Element)
					{
						switch (PaperSizeXmlString.Name)
						{
							case SCOREDPROPERTYNODE:
								{
									if (PaperSizeXmlString.AttributeCount == 1)
									{
										switch (PaperSizeXmlString.GetAttribute(NAMEATTRIBUTE))
										{
											case WIDTHATTRIBUTE:
												{
													stringwidth = ProcessPaperValue(PaperSizeXmlString.ReadSubtree());
													
													if (string.IsNullOrEmpty(stringwidth))
														currentWidth = null;
													else
														currentWidth = double.Parse(stringwidth) ;

												} break;

											case HEIGHTATTRIBUTE:
												{
													stringheight = ProcessPaperValue(PaperSizeXmlString.ReadSubtree());
													
													if (string.IsNullOrEmpty(stringheight))
														currentHeight = null;
													else
														currentHeight = double.Parse(stringheight);
												} break;
										} break;
									}
								} break;
							case PROPERTNODE:
								{
									if (PaperSizeXmlString.AttributeCount == 1)
									{
										if (PaperSizeXmlString.GetAttribute(NAMEATTRIBUTE) == DISPLAYNAMEATTRIBUTE)
											currentName = ProcessPaperValue(PaperSizeXmlString.ReadSubtree());
									}
								} break;
						}
					}
				}

				return new IllPaperSize(currentPaperKey, currentName, currentWidth, currentHeight);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		#region ProcessPaperValue()
		private static string ProcessPaperValue(System.Xml.XmlReader valueXmlString)
		{
			try
			{
				while (valueXmlString.Read())
				{
					if (valueXmlString.NodeType == System.Xml.XmlNodeType.Element)
						if (valueXmlString.Name == VALUENODE)
							return valueXmlString.ReadElementContentAsString().Trim();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return string.Empty;
		}
		#endregion

		#region GetPageMediaSizeName()
		private static PageMediaSizeName GetPageMediaSizeName(string key)
		{
			if (key.IndexOf(':') >= 0 && key.IndexOf(':') < key.Length - 1)
				key = key.Substring(key.IndexOf(':') + 1);
			
			foreach (PageMediaSizeName name in Enum.GetValues(typeof(PageMediaSizeName)))
			{
				if (name.ToString().ToLower() == key.ToLower())
					return name;
			}

			return PageMediaSizeName.Unknown;
		}
		#endregion		
		
		#endregion
	
	}
}
