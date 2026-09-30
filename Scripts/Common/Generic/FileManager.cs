using System;
using System.IO;
using Godot;

public class DataFileManager
{
	public readonly string FilePath;
	
	
	public DataFileManager(string filePath)
	{
		FilePath = filePath;
	}
	
	
	public bool DoesDirectoryExist(string localPath)
		=> Directory.Exists(GetFullPath(localPath));
	
	public void CopyFiles(string pathToCopyFrom, string toLocalPath)
	{
		File.Copy(pathToCopyFrom, GetFullPath(toLocalPath));
	}
	
	
	private string GetFullPath(string localPath)
		=> $"{FilePath}{localPath}";
}