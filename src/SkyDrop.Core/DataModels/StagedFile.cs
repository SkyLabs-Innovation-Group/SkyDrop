using System;
namespace SkyDrop.Core.DataModels
{
    public class StagedFile
    {
        public string Filename { get; set; }
        public byte[] Data { get; set; }
    }
}
