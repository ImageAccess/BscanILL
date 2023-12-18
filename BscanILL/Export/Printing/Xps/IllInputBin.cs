using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BscanILL.Export.Printing.Xps
{
	class IllInputBin : IInputBin
	{
		string displayName;
		string value;

		public IllInputBin(string displayName, string value)
		{
			this.displayName = displayName;
			this.value = value;
		}

		public string DisplayName { get { return this.displayName; } }
		public string Key { get { return this.value; } }


		#region GetAllInputBins()
		public static List<IInputBin> GetAllInputBins(System.Printing.PrintQueue printQueue)
		{
			List<IInputBin> inputBins = new List<IInputBin>();

			System.Xml.Schema.XmlSchema schema = new System.Xml.Schema.XmlSchema();

			// get PrintCapabilities of the printer
			System.IO.MemoryStream printerCapXmlStream = printQueue.GetPrintCapabilitiesAsXml();

			// read the JobInputBins out of the PrintCapabilities
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(printerCapXmlStream);

			// create NamespaceManager and add PrintSchemaFrameWork-Namespace (should be on DocumentElement of the PrintTicket)
			// Prefix: psf NameSpace: xmlDoc.DocumentElement.NamespaceURI = "http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework"
			XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
			manager.AddNamespace(xmlDoc.DocumentElement.Prefix, xmlDoc.DocumentElement.NamespaceURI);

			// and select all nodes of the bins
			XmlNodeList nodeList = xmlDoc.SelectNodes("//psf:Feature[@name='psk:JobInputBin']/psf:Option", manager);
			foreach (XmlNode node in nodeList)
			{
				if (node.LastChild != null && node.Attributes["name"] != null)
					inputBins.Add(new IllInputBin(node.LastChild.InnerText, node.Attributes["name"].Value));
			}

			nodeList = xmlDoc.SelectNodes("//psf:Feature[@name='psk:PageInputBin']/psf:Option", manager);
			foreach (XmlNode node in nodeList)
			{
				if (node.LastChild != null && node.Attributes["name"] != null)
					inputBins.Add(new IllInputBin(node.LastChild.InnerText, node.Attributes["name"].Value));
			}

			nodeList = xmlDoc.SelectNodes("//psf:Feature[@name='psk:DocumentInputBin']/psf:Option", manager);
			foreach (XmlNode node in nodeList)
				if (node.LastChild != null && node.Attributes["name"] != null)
					inputBins.Add(new IllInputBin(node.LastChild.InnerText, node.Attributes["name"].Value));

			return inputBins;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			return this.DisplayName;
		}
		#endregion

	}
}
