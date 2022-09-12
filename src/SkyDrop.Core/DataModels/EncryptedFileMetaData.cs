using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace SkyDrop.Core.DataModels
{
    public class EncryptedFileMetaData
    {
        public ushort HeaderFormatIdentifier;
        public ushort RecipientsCount;
        public Guid SenderId;
        public Dictionary<Guid, byte[]> RecipientKeys;

        private const int headerFormatIdentifierSizeBytes = 2;
        private const int recipientsCountSizeBytes = 2;
        private const int senderIdSizeBytes = 16;
        private const int recipientIdSizeBytes = 16;
        private const int recipientKeySizeBytes = 64;

        public static (EncryptedFileMetaData metaData, byte[] encryptedContent) GetEncryptedFileMetaData(byte[] encryptedFile)
        {
            var headerFormatIdentifierBytes = encryptedFile.Take(headerFormatIdentifierSizeBytes).ToArray(); //first 2 bytes
            var headerFormatIdentifier = BinaryPrimitives.ReadUInt16BigEndian(headerFormatIdentifierBytes);

            //check this is the headerFormatIdentifier value we expect (there is currently only 1)
            if (headerFormatIdentifier != 1)
                throw new Exception("Unexpected file format");

            var recipientsCountBytes = encryptedFile.Skip(headerFormatIdentifierSizeBytes).Take(recipientsCountSizeBytes).ToArray(); //second 2 bytes
            var recipientsCount = BinaryPrimitives.ReadUInt16BigEndian(recipientsCountBytes);

            var metadataSizeBytes = headerFormatIdentifierSizeBytes + recipientsCountSizeBytes + senderIdSizeBytes + (recipientIdSizeBytes + recipientKeySizeBytes) * recipientsCount;

            var metaDataBytes = encryptedFile.Take(metadataSizeBytes).ToArray();
            var encryptedContent = encryptedFile.Skip(metadataSizeBytes).ToArray();

            var senderIdBytes = metaDataBytes.Skip(headerFormatIdentifierSizeBytes).Skip(recipientsCountSizeBytes).Take(senderIdSizeBytes).ToArray();
            var senderId = new Guid(senderIdBytes);

            var recipientsListBytes = metaDataBytes.Skip(headerFormatIdentifierSizeBytes).Skip(recipientsCountSizeBytes).Skip(senderIdSizeBytes).ToArray();
            var recipientKeys = DecodeRecipientsList(recipientsListBytes, recipientsCount);

            var metaData = new EncryptedFileMetaData { HeaderFormatIdentifier = headerFormatIdentifier, RecipientsCount = recipientsCount, SenderId = senderId, RecipientKeys = recipientKeys };

            return (metaData, encryptedContent);
        }

        private static Dictionary<Guid, byte[]> DecodeRecipientsList(byte[] recipientsList, int recipientsCount)
        {
            var recipientKeys = new Dictionary<Guid, byte[]>();
            int readIndex = 0;
            for(int i = 0; i < recipientsCount; i++)
            {
                var recipientId = new Guid(recipientsList.Skip(readIndex).Take(recipientIdSizeBytes).ToArray());
                readIndex += recipientIdSizeBytes;

                var recipientKey = recipientsList.Skip(readIndex).Take(recipientKeySizeBytes).ToArray();
                readIndex += recipientKeySizeBytes;

                recipientKeys.Add(recipientId, recipientKey);
            }

            return recipientKeys;
        }
    }
}

