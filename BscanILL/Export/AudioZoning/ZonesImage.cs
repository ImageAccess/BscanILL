using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace BscanILL.Export.AudioZoning
{
	public class ZonesImage
	{
		public BscanILL.Hierarchy.IllPage IllPage;
		public readonly FileInfo File;
		public List<BscanILL.Export.AudioZoning.Zone> Zones;

		public delegate void ZoneHnd(BscanILL.Export.AudioZoning.ZonesImage image, BscanILL.Export.AudioZoning.Zone zone);
		public delegate void VoidHnd();
		public event ZoneHnd ZoneAdding;
		public event ZoneHnd ZoneAdded;
		public event ZoneHnd ZoneRemoving;
		public event ZoneHnd ZoneRemoved;
		public event VoidHnd Clearing;
		public event VoidHnd Cleared;

		public ZonesImage(BscanILL.Hierarchy.IllPage illPage, FileInfo file, List<BscanILL.Export.AudioZoning.Zone> zones)
		{
			this.IllPage = illPage;
			this.File = file;
			this.Zones = zones;
		}

		/// <summary>
		/// creates instance with 1 default zone <0,0,1,1>
		/// </summary>
		/// <param name="file"></param>
		public ZonesImage(BscanILL.Hierarchy.IllPage illPage, FileInfo file)
		{
			this.IllPage = illPage;
			this.File = file;
			this.Zones = new List<Zone>();

			this.Zones.Add(new Zone(0, 0, 1, 1));
		}

		public void AddZone(Zone zone)
		{
			if (ZoneAdding != null)
				ZoneAdding(this, zone);

			this.Zones.Add(zone);

			if (ZoneAdded != null)
				ZoneAdded(this, zone);
		}

		public void RemoveZone(Zone zone)
		{
			if (this.Zones.Contains(zone))
			{
				if (ZoneRemoving != null)
					ZoneRemoving(this, zone);

				this.Zones.Remove(zone);

				if (ZoneRemoved != null)
					ZoneRemoved(this, zone);
			}
			else
				throw new Exception("Zone is not a part of of zones collection!");
		}

		/// <summary>
		/// Deletes all zones. Use ResetZones() to clear all zones and create 1 default zone instead.
		/// </summary>
		public void Clear()
		{
			if (Clearing != null)
				Clearing();

			this.Zones.Clear();

			if (Cleared != null)
				Cleared();
		}

		/// <summary>
		/// Deletes all zones and creates 1 empty zone instead.
		/// </summary>
		public void ResetZones()
		{
			Clear();
			AddZone(new Zone(0, 0, 1, 1));
		}

	}
}
