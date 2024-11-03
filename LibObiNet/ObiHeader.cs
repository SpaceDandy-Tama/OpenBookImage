using System;
using System.IO;

namespace LibObiNet
{
    public class ObiHeader
    {
        public const int HeaderSize = 10;

        public readonly ushort Width;
        public readonly ushort Height;
        public readonly LibObiNet.PixelFormat PixelFormat;
        public readonly ObiFlags Flags;
        public readonly uint Size;

        public bool IsRLE => (Flags & ObiFlags.RLE) != 0;

        public ObiHeader(ushort width, ushort height, LibObiNet.PixelFormat pixelFormat, ObiFlags flags, uint size)
        {
            Width = width;
            Height = height;
            PixelFormat = pixelFormat;
            Flags = flags;
            Size = size;
        }

        public ObiHeader(ObiHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            Width = header.Width;
            Height = header.Height;
            PixelFormat = header.PixelFormat;
            Flags = header.Flags;
            Size = header.Size;
        }

        public static ObiHeader FromHeaderBytes(byte[] headerBytes)
        {
            if(headerBytes == null)
            {
                throw new ArgumentNullException(nameof(headerBytes));
            }

            using(MemoryStream ms = new MemoryStream(headerBytes))
            {
                return ObiHeader.FromStream(ms);
            }
        }

        private static ObiHeader FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (stream.Length < ObiHeader.HeaderSize) // Check if the stream is long enough to read the header
            {
                throw new InvalidDataException("Stream does not contain enough data to read an OBI header.");
            }

            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.BaseStream.Position = 0;
                ushort width = reader.ReadUInt16(); // Read width (2 bytes)
                ushort height = reader.ReadUInt16(); // Read height (2 bytes)
                LibObiNet.PixelFormat pixelFormat = (LibObiNet.PixelFormat)reader.ReadByte(); // Read bits per pixel (1 byte)
                ObiFlags flags = (ObiFlags)reader.ReadByte(); // Read flags (1 byte)
                uint size = reader.ReadUInt32(); // Read size of data in bytes (4 bytes)
                return new ObiHeader(width, height, pixelFormat, flags, size);
            }
        }
    }
}