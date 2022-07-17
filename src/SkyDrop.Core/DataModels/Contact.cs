using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace SkyDrop.Core.DataModels
{
	public class Contact
	{
		public string Name { get; set; }

		public X25519PublicKeyParameters PublicKey { get; set; }
	}
}

