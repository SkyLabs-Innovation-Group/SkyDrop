using System;
using System.Collections.Generic;
using Realms;

namespace SkyDrop.Core.RealmObjects
{
	public class FolderRealmObject : RealmObject
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string SkyLinks { get; set; }
	}
}

