using System;

namespace RecipeRage.Modules.Cloud.Interfaces
{
    /// <summary>
    /// Metadata for cloud storage files
    /// Complexity Rating: 1
    /// </summary>
    [Serializable]
    public class CloudFileMetadata
    {

        /// <summary>
        /// Create a new file metadata object
        /// </summary>
        /// <param name="fileName"> Name of the file </param>
        /// <param name="size"> Size in bytes </param>
        /// <param name="lastModified"> Last modified time </param>
        /// <param name="provider"> Provider name </param>
        public CloudFileMetadata(string fileName, long size, DateTime lastModified, string provider)
        {
            FileName = fileName;
            Size = size;
            LastModified = lastModified;
            Provider = provider;
            Hash = string.Empty;
            FileId = string.Empty;
            IsLocal = false;
            IsSynced = false;
        }

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public CloudFileMetadata()
        {
            FileName = string.Empty;
            Size = 0;
            LastModified = DateTime.UtcNow;
            Provider = string.Empty;
            Hash = string.Empty;
            FileId = string.Empty;
            IsLocal = false;
            IsSynced = false;
        }

        /// <summary>
        /// Name of the file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Last modified time (UTC)
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// MD5 hash of the file (if available)
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Provider-specific file ID
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Whether the file exists locally
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// Whether the file has been synchronized with the cloud
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Provider name
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Get human-readable file size
        /// </summary>
        /// <returns> Human-readable size </returns>
        public string GetReadableSize()
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = Size;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}