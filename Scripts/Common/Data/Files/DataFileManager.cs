using System;
using System.Collections.Generic;
using Godot;

public class DataFileManager(string basePath = "user:/")
{
	public readonly string BasePath = basePath;
	
	
	public void GenerateFolderFromResources(string resourcePath)
	{
		Queue<FilesystemInstance> queue = [];
		
		queue.Enqueue(new("res:/", resourcePath));
		
		while (queue.TryDequeue(out var parent))
		{
			new FilesystemInstance(BasePath, parent.Path, parent.TypeOfFile, parent.TypeOfFile == FilesystemInstance.FileType.File ? parent.ReadString() : null);
			
			if (parent.TypeOfFile == FilesystemInstance.FileType.Directory)
				foreach (var child in parent)
					queue.Enqueue(child);
		}
	}
	
	public void ClearPack(string packId)
		=> FilesystemInstance.Delete($"{basePath}/data/packs/{packId}", true);
	
	
	public string GetPath(string localPath)
		=> $"{BasePath}{localPath}";
	
	public string GetResourcePath(string localPath)
		=> $"res://{localPath}";
}