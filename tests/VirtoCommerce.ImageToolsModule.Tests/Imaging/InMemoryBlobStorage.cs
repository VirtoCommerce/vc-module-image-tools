using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;

namespace VirtoCommerce.ImageToolsModule.Tests.Imaging;

/// <summary>
/// Blob storage backed by memory: serves added blobs and captures everything written.
/// </summary>
internal sealed class InMemoryBlobStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _sources = new();
    private readonly ConcurrentDictionary<string, MemoryStream> _written = new();

    public InMemoryBlobStorage()
    {
        var mock = new Mock<IBlobStorageProvider>();

        mock.Setup(x => x.OpenReadAsync(It.IsAny<string>()))
            .ReturnsAsync((string url) => _sources.TryGetValue(url, out var content)
                ? new MemoryStream(content, writable: false)
                : throw new FileNotFoundException("Blob not found", url));

        mock.Setup(x => x.OpenWriteAsync(It.IsAny<string>()))
            .ReturnsAsync((string url) => _written[url] = new MemoryStream());

        Provider = mock.Object;
    }

    public IBlobStorageProvider Provider { get; }

    public ICollection<string> WrittenUrls => _written.Keys;

    public void Add(string url, byte[] content)
    {
        _sources[url] = content;
    }

    /// <summary>
    /// Returns the bytes written to <paramref name="url"/>, or null when nothing was written.
    /// </summary>
    public byte[] GetWritten(string url)
    {
        // MemoryStream.ToArray works after the writer has disposed the stream
        return _written.TryGetValue(url, out var stream) ? stream.ToArray() : null;
    }
}
