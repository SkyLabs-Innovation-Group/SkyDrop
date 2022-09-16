using System;
using Realms;

namespace SkyDrop.Core.RealmObjects
{
	public class EncryptionKeyPairRealmObject : RealmObject
	{
		public string PrivateKeyBase64 { get; set; }

		public string PublicKeyBase64 { get; set; }

		public string Id { get; set; }

		public string Name { get; set; }
	}
}

