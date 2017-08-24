namespace MN.L10n
{
	public static class FileHashHelper
	{
		public static bool HashesLoaded = false;
		public static System.Collections.Concurrent.ConcurrentDictionary<string, string> FileHashes = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
		public static string GetHash(string contents)
		{
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				var hash = System.Text.Encoding.Default.GetString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(contents)));
				return hash;
			}
		}

		public static void LoadFileHashes(string path)
		{
			if (!System.IO.File.Exists(path))
			{
				System.IO.File.WriteAllText(path, Jil.JSON.Serialize(FileHashes));
			}

			FileHashes = Jil.JSON.Deserialize<System.Collections.Concurrent.ConcurrentDictionary<string, string>>(System.IO.File.ReadAllText(path));
			HashesLoaded = true;
		}

		public static void SaveFileHashes(string path)
		{
			System.IO.File.WriteAllText(path, Jil.JSON.Serialize(FileHashes));
		}
	}
}
