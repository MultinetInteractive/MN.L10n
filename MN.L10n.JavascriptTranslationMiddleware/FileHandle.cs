using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public class FileHandle
    {
        private readonly IFileInfo _fileInfo;
        public bool Exists => _fileInfo.Exists;
        public string FileName => _fileInfo.Name;
        public string Path => _fileInfo.PhysicalPath;
        public string RelativePath { get; }
        
        public FileHandle(IFileInfo fileInfo, string relativePath)
        {
            _fileInfo = fileInfo;
            RelativePath = relativePath;
        }

        public async Task<string> GetFileContentsAsync()
        {
            if (!Exists)
            {
                throw new Exception("The file does not exist");
            }

            //TODO locking här?
            var contents = await File.ReadAllTextAsync(_fileInfo.PhysicalPath);
            return contents;
        }
    }
}
