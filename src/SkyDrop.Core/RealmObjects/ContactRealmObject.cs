using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Realms;

namespace SkyDrop.Core.RealmObjects
{
	public class ContactRealmObject : RealmObject
	{
		public string Name { get; set; }

		[PrimaryKey]
		public string PublicKeyBase64 { get; set; }
	}
}

