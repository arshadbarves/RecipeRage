using System;
using System.IO;
using UnityEngine;

namespace Core.SaveSystem
{
    /// <summary>
    /// Real file system storage implementation
    /// </summary>
    public class FileSystemStorage : IFileStorage
    {
        private readonly string _basePath;

        public FileSystemStorage()
        {
            _basePath = Application.persistentDataPath;
        }

        public string Read(string filename)
        {
            string path = Path.Combine(_basePath, filename);
            
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileSystemStorage] Error reading {filename}: {e.Message}");
                return null;
            }
        }

        public void Write(string filename, string content)
        {
            string path = Path.Combine(_basePath, filename);
            
            try
            {
                File.WriteAllText(path, content);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileSystemStorage] Error writing {filename}: {e.Message}");
            }
        }

        public bool Exists(string filename)
        {
            string path = Path.Combine(_basePath, filename);
            return File.Exists(path);
        }

        public void Delete(string filename)
        {
            string path = Path.Combine(_basePath, filename);
            
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FileSystemStorage] Error deleting {filename}: {e.Message}");
                }
            }
        }
    }
}
