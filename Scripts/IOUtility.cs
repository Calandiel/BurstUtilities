using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Calandiel.Utilities
{
	public static class IOUtility
	{
		public static void CopyAll(string source, string target) => CopyAll(new DirectoryInfo(source), new DirectoryInfo(target));
		public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
		{
			Directory.CreateDirectory(target.FullName);
			foreach (FileInfo fi in source.GetFiles())
				if (fi.Extension != "meta")
					fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir =
				target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}
	}
}