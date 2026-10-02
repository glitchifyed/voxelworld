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

	public HashSet<PackData> CompileDataPacks(string path)
	{
		// Get pack ids from directories
		Dictionary<string, string> packNamesToIds = [];

		foreach (var packDirectory in new FilesystemInstance(BasePath, $"{path}/packs"))
		{
			if (!packDirectory.IsDirectory())
				continue;

			var metadataFile = packDirectory.Get("metadata.json");
			if (metadataFile != null || !metadataFile.IsFile())
				throw new Exception($"Pack {packDirectory.FileName} contains an invalid metadata JSON.");

			var metadata = metadataFile.ReadJson();
			if (!metadata.TryGet("id", out var idObj) || !idObj.TryGet(out string packId))
				throw new Exception($"Pack {packDirectory.FileName} metadata doesn't have a valid ID.");

			if (packNamesToIds.ContainsKey(packId))
				throw new Exception($"Pack {packDirectory.FileName} contains a duplicate ID: {packId}");

			packNamesToIds[packId] = packDirectory.FileName;
		}

		// Start loading packs based on id load order
		var packsList = new FilesystemInstance(BasePath, $"{path}/loaded_packs.json").ReadJson();

		if (!packsList.IsArray())
			throw new Exception("Data contains an invalid loaded_packs.json.");

		HashSet<PackData> loadedPacks = [];

		foreach (var packIdObj in packsList.GetArrayEnumerator())
		{
			if (!packIdObj.TryGet(out string packId) || !packNamesToIds.TryGetValue(packId, out var packFileName))
				throw new Exception($"Loaded packs contains an invalid ID: {packId}");

			PackData packData = GetPackData(packId, $"{path}/packs/{packFileName}/data");

			loadedPacks.Add(packData);
		}

		return loadedPacks;
	}


	private PackData GetPackData(string packId, string packPath)
	{
		PackData packData = new(packId);


		Queue<FilesystemInstance> queue = [];
		queue.Enqueue(new FilesystemInstance(BasePath, packPath));

		while (queue.TryDequeue(out var parentFile))
		{
			if (parentFile.IsDirectory())
			{
				foreach (var child in parentFile)
					queue.Enqueue(child);

				continue;
			}

			ScanDataFile(parentFile, packData);
		}


		return packData;
	}

	private void ScanDataFile(FilesystemInstance file, PackData packData)
	{
		// TODO: Throw errors when things go wrong
		
		var jsonData = file.ReadJson();

		if (!jsonData.TryGet("type", out var typeObj) || !typeObj.TryGet(out string type) ||
			!jsonData.TryGet("namespace", out var namespaceObj) || !namespaceObj.TryGet(out string @namespace) ||
			!jsonData.TryGet("data", out var dataDictionaryObj) || !dataDictionaryObj.IsDictionary())
			return;
		
		foreach (var dataPair in dataDictionaryObj.GetDictionaryEnumerator())
		{
			if (!dataPair.Value.IsDictionary())
				continue;
			
			string id = $"{@namespace}.{dataPair.Key}";
			
			GenericData dataObj = type switch
			{
				"generic" => new(id),
				
				"voxel" => GetVoxelData(id, dataPair.Value)
				
				_ => throw new Exception($"'{type}' is not a valid data type.")
			};
		}
	}
	
	private VoxelData GetVoxelData(string id, JsonObject jsonData)
	{
		VoxelData voxelData = new(id);
		
		
	}


	public void ClearPack(string packId)
		=> FilesystemInstance.Delete($"{BasePath}/data/packs/{packId}", true);


	public string GetPath(string localPath)
		=> $"{BasePath}{localPath}";

	public string GetResourcePath(string localPath)
		=> $"res://{localPath}";
}