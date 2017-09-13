namespace MN.L10n
{
	public class FileResolver : IFileResolver
	{
		public bool FileExists(string file)
		{
			return System.IO.File.Exists(file);
		}
	}
}
