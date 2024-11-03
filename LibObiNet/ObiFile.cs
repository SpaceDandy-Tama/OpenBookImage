using System;
using System.Drawing;
using System.IO;

namespace LibObiNet
{
    public class ObiFile
    {
        public ObiHeader Header { get; private set; }

        public byte[] Data { get; private set; }

        public ObiFile(ObiHeader header, byte[] data)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public ObiFile(ObiFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            Header = new ObiHeader(file.Header);
            Data = new byte[Header.Size];
            Buffer.BlockCopy(file.Data, 0, Data, 0, Data.Length);
        }

        public ObiFile(byte[] rawObi)
        {
            if (rawObi == null)
            {
                throw new ArgumentNullException(nameof(rawObi));
            }

            using (MemoryStream stream = new MemoryStream(rawObi))
            {
                LoadFromStream(stream, out ObiHeader tempHeader, out byte[] tempData);
                Header = tempHeader;
                Data = tempData;
            }
        }

        public ObiFile(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            LoadFromStream(stream, out ObiHeader tempHeader, out byte[] tempData);
            Header = tempHeader;
            Data = tempData;
        }

        public ObiFile(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} does not exist.");
            }
            if (!Path.GetExtension(filePath).Equals(".obi", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"{filePath} is not a valid OBI file (Open Book Image). Expected file extension: .obi");
            }

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                LoadFromStream(stream, out ObiHeader tempHeader, out byte[] tempData);
                Header = tempHeader;
                Data = tempData;
            }
        }

        public ObiFile(Bitmap bitmap, PixelFormat pixelFormat, bool useRLE)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            ushort width = (ushort)bitmap.Width;
            ushort height = (ushort)bitmap.Height;
            ObiFlags flags = useRLE ? ObiFlags.RLE : ObiFlags.None;
            Data = ObiUtils.GetDataFromBitmap(width, height, pixelFormat, flags, bitmap);
            Header = new ObiHeader(width, height, pixelFormat, flags, (uint)Data.Length);
        }

        private static void LoadFromStream(Stream stream, out ObiHeader header, out byte[] data)
        {
            byte[] headerBytes = new byte[ObiHeader.HeaderSize];
            stream.Read(headerBytes, 0, headerBytes.Length);
            header = ObiHeader.FromHeaderBytes(headerBytes);
            data = new byte[header.Size];

            int bytesRead = stream.Read(data, 0, data.Length);
            if (bytesRead != data.Length)
            {
                throw new InvalidDataException("Could not read the expected amount of data from the file.");
            }
        }

        public void Save(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException();
            }

            string directory = Path.GetDirectoryName(Path.GetFullPath(filePath));

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer =  new BinaryWriter(fs))
                {
                    writer.BaseStream.Position = 0;
                    writer.Write((ushort)Header.Width);
                    writer.Write((ushort)Header.Height);
                    writer.Write((byte)Header.PixelFormat);
                    writer.Write((byte)Header.Flags);
                    writer.Write((uint)Header.Size);
                    writer.Write(Data);
                }
            }
        }
    }
}