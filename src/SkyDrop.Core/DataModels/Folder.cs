using System;
using System.Collections.Generic;

namespace SkyDrop.Core.DataModels
{
	public class Folder
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public List<string> SkyLinks { get; set; }
	}
}

