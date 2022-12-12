using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace SkyDrop.Core.DataModels
{
    public class EncryptedFileMetaData
    {
        private const int HeaderFormatIdentifierSizeBytes = 2;
        private const int RecipientsCountSizeBytes = 2;
        private const int SenderIdSizeBytes = 16;
        private const int RecipientIdSizeBytes = 16;
        private const int RecipientKeySizeBytes = 64;
        public ushort HeaderFormatIdentifier;
        public Dictionary<Guid, byte[]> RecipientKeys;
        public ushort RecipientsCount;
        public Guid SenderId;

        public static (EncryptedFileMetaData metaData, byte[] encryptedData) GetEncryptedFileMetaData(
            byte[] encryptedFile)
        {
            var headerFormatIdentifierBytes =
                encryptedFile.Take(HeaderFormatIdentifierSizeBytes).ToArray(); //first 2 bytes
            var headerFormatIdentifier = BinaryPrimitives.ReadUInt16BigEndian(headerFormatIdentifierBytes);

            //check this is the headerFormatIdentifier value we expect (there is currently only 1)
            if (headerFormatIdentifier != 1)
                throw new Exception("Unexpected file format");

            var recipientsCountBytes = encryptedFile.Skip(HeaderFormatIdentifierSizeBytes)
                .Take(RecipientsCountSizeBytes).ToArray(); //second 2 bytes
            var recipientsCount = BinaryPrimitives.ReadUInt16BigEndian(recipientsCountBytes);

            var metadataSizeBytes = HeaderFormatIdentifierSizeBytes + RecipientsCountSizeBytes + SenderIdSizeBytes +
                                    (RecipientIdSizeBytes + RecipientKeySizeBytes) * recipientsCount;

            var metaDataBytes = encryptedFile.Take(metadataSizeBytes).ToArray();
            var encryptedData = encryptedFile.Skip(metadataSizeBytes).ToArray();

            var senderIdBytes = metaDataBytes.Skip(HeaderFormatIdentifierSizeBytes).Skip(RecipientsCountSizeBytes)
                .Take(SenderIdSizeBytes).ToArray();
            var senderId = new Guid(senderIdBytes);

            var recipientsListBytes = metaDataBytes.Skip(HeaderFormatIdentifierSizeBytes).Skip(RecipientsCountSizeBytes)
                .Skip(SenderIdSizeBytes).ToArray();
            var recipientKeys = DecodeRecipientsList(recipientsListBytes, recipientsCount);

            var metaData = new EncryptedFileMetaData
            {
                HeaderFormatIdentifier = headerFormatIdentifier, RecipientsCount = recipientsCount, SenderId = senderId,
                RecipientKeys = recipientKeys
            };

            return (metaData, encryptedData);
        }

        private static Dictionary<Guid, byte[]> DecodeRecipientsList(byte[] recipientsList, int recipientsCount)
        {
            var recipientKeys = new Dictionary<Guid, byte[]>();
            var readIndex = 0;
            for (var i = 0; i < recipientsCount; i++)
            {
                var recipientId = new Guid(recipientsList.Skip(readIndex).Take(RecipientIdSizeBytes).ToArray());
                readIndex += RecipientIdSizeBytes;

                var recipientKey = recipientsList.Skip(readIndex).Take(RecipientKeySizeBytes).ToArray();
                readIndex += RecipientKeySizeBytes;

                recipientKeys.Add(recipientId, recipientKey);
            }

            return recipientKeys;
        }
    }
}