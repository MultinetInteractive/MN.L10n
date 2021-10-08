using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public interface IFileHandle
    {
        bool Exists { get; }
        string FileName { get; }
        string Path { get; }
        string RelativeRequestPath { get; }
        Task<string> GetFileContentsAsync();
    }

    public class FileHandle : IFileHandle
    {
        private readonly IFileInfo _fileInfo;
        public bool Exists => _fileInfo.Exists;
        public string FileName => _fileInfo.Name;
        public string Path => _fileInfo.PhysicalPath;
        public string RelativeRequestPath { get; }
        
        public FileHandle(IFileInfo fileInfo, string relativeRequestPath)
        {
            _fileInfo = fileInfo;
            RelativeRequestPath = relativeRequestPath;
        }

        public async Task<string> GetFileContentsAsync()
        {
            if (!Exists)
            {
                throw new Exception("The file does not exist");
            }

            var contents = await File.ReadAllTextAsync(_fileInfo.PhysicalPath);
            return contents;
        }
    }
}
