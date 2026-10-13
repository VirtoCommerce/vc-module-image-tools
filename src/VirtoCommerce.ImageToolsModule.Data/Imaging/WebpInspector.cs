using System;
using System.Buffers.Binary;

namespace VirtoCommerce.ImageToolsModule.Data.Imaging;

/// <summary>
/// Reads WebP container metadata that the decoder does not expose.
/// </summary>
public static class WebpInspector
{
    private const int RiffHeaderSize = 12;
    private const int ChunkHeaderSize = 8;

    /// <summary>
    /// True when the first image bitstream in a WebP file is lossless (VP8L).
    /// </summary>
    public static bool IsLossless(ReadOnlySpan<byte> content)
    {
        if (content.Length < RiffHeaderSize || !content[..4].SequenceEqual("RIFF"u8) || !content[8..12].SequenceEqual("WEBP"u8))
        {
            return false;
        }

        var position = RiffHeaderSize;

        while (position + ChunkHeaderSize <= content.Length)
        {
            var fourCc = content.Slice(position, 4);
            var size = BinaryPrimitives.ReadUInt32LittleEndian(content.Slice(position + 4, 4));

            if (fourCc.SequenceEqual("VP8L"u8))
            {
                return true;
            }

            if (fourCc.SequenceEqual("VP8 "u8))
            {
                return false;
            }

            if (fourCc.SequenceEqual("ANMF"u8))
            {
                // Animation frame: 16 bytes of frame header, then the frame's own chunks
                position += ChunkHeaderSize + 16;
                continue;
            }

            // Chunks are padded to an even size
            var next = (long)position + ChunkHeaderSize + size + (size & 1);
            if (next > content.Length)
            {
                return false;
            }

            position = (int)next;
        }

        return false;
    }
}
