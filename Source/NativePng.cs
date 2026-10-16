using System.IO.Compression;
using System.Buffers.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace StarlightSyndicateDemo;
internal static class NativePng
{
 public static void Write(Texture2D texture,Stream output){var pixels=new Color[texture.Width*texture.Height];texture.GetData(pixels);output.Write(new byte[]{137,80,78,71,13,10,26,10});var header=new byte[13];BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0,4),texture.Width);BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4,4),texture.Height);header[8]=8;header[9]=6;Chunk(output,"IHDR",header);using var packed=new MemoryStream();using(var zip=new ZLibStream(packed,CompressionLevel.Optimal,true)){for(int y=0;y<texture.Height;y++){zip.WriteByte(0);for(int x=0;x<texture.Width;x++){var c=pixels[y*texture.Width+x];zip.WriteByte(c.R);zip.WriteByte(c.G);zip.WriteByte(c.B);zip.WriteByte(c.A);}}}Chunk(output,"IDAT",packed.ToArray());Chunk(output,"IEND",[]);}
 static void Chunk(Stream output,string name,byte[] data){byte[] type=System.Text.Encoding.ASCII.GetBytes(name);Span<byte> size=stackalloc byte[4];BinaryPrimitives.WriteInt32BigEndian(size,data.Length);output.Write(size);output.Write(type);output.Write(data);uint crc=0xffffffff;foreach(byte b in type.Concat(data)){crc^=b;for(int i=0;i<8;i++)crc=(crc&1)!=0?0xedb88320^(crc>>1):crc>>1;}BinaryPrimitives.WriteUInt32BigEndian(size,~crc);output.Write(size);}
}
