using System;
using System.Collections.Generic;
using System.Data;
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
			if (metadataFile == null || !metadataFile.IsFile())
				throw new InvalidOperationException($"Pack {packDirectory.FileName} contains an invalid metadata JSON.");

			var metadata = metadataFile.ReadJson();
			if (!metadata.TryGet("pack_id", out var idObj) || !idObj.TryGet(out string packId))
				throw new InvalidOperationException($"Pack {packDirectory.FileName} metadata doesn't have a valid ID.");

			if (packNamesToIds.ContainsKey(packId))
				throw new DuplicateNameException($"Pack {packDirectory.FileName} contains a duplicate ID: {packId}");

			packNamesToIds[packId] = packDirectory.FileName;
		}

		// Start loading packs based on id load order
		var packsList = new FilesystemInstance(BasePath, $"{path}/loaded_packs.json").ReadJson();

		if (!packsList.IsArray())
			throw new InvalidOperationException("Data contains an invalid loaded_packs.json.");

		HashSet<PackData> loadedPacks = [];

		foreach (var packIdObj in packsList.GetArrayEnumerator())
		{
			if (!packIdObj.TryGet(out string packId) || !packNamesToIds.TryGetValue(packId, out var packFileName))
				throw new InvalidOperationException($"Loaded packs contains an invalid ID: {packId}");

			PackData packData = GetPackData(packId, $"{path}/packs/{packFileName}");

			loadedPacks.Add(packData);
		}

		return loadedPacks;
	}


	private PackData GetPackData(string packId, string packPath)
	{
		PackData packData = new(packId);


		var packDirectory = new FilesystemInstance(BasePath, packPath);
		var metadata = packDirectory.Get("metadata.json").ReadJson();

		ScanMetadata(packData, metadata);


		Queue<FilesystemInstance> queue = [];
		queue.Enqueue(packDirectory.Get("content"));

		while (queue.TryDequeue(out var parentFile))
		{
			if (parentFile.IsDirectory())
			{
				foreach (var child in parentFile)
					queue.Enqueue(child);

				continue;
			}

			if (parentFile.IsData())
				ScanDataFile(parentFile, packData);
			else if (parentFile.IsScript())
				ScanScriptFile(parentFile, packData);
		}


		return packData;
	}

	private void ScanMetadata(PackData packData, JsonObject metadata)
	{
		packData.Globals["pack_id"] = metadata.Get<string>("pack_id");
		
		if (metadata.TryGet("globals", out var globals))
		{
			if (!globals.IsDictionary())
				throw new InvalidOperationException($"Pack '{packData.Id}' contains invalid globals: not a dictionary");
			
			foreach (var obj in globals.GetDictionaryEnumerator())
				packData.Globals[obj.Key] = obj.Value.Get<string>();
		}
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

				"voxel" => GetVoxelData(id, dataPair.Value),

				"world" => GetWorldData(id, dataPair.Value),

				_ => throw new InvalidOperationException($"File path '{file.AbsolutePath}' contains invalid data type '{type}'.")
			};

			if (dataObj != null)
				packData.Data.Add(dataObj);
		}
	}

	private void ScanScriptFile(FilesystemInstance file, PackData packData)
	{
		// TODO: Throw errors when things go wrong

		packData.Scripts.Add(file);
	}


	private VoxelData GetVoxelData(string id, JsonObject jsonData)
	{
		VoxelData voxelData = new(id);

		if (jsonData.TryGet("color", out var colorObj) && colorObj.TryGet(out Color color))
			voxelData.ColorTemp = color;

		if (jsonData.TryGet("translucent", out var translucentObj) && translucentObj.TryGet(out bool translucent))
			voxelData.Translucent = translucent;

		return voxelData;
	}

	private WorldData GetWorldData(string id, JsonObject jsonData)
	{
		WorldData worldData = new(id);

		if (jsonData.TryGet("generator", out var generatorObj) && generatorObj.TryGet(out string generatorId))
			worldData.GeneratorId = generatorId;

		return worldData;
	}


	public void ClearPack(string packId)
		=> FilesystemInstance.Delete($"{BasePath}/data/packs/{packId}", true);


	public string GetPath(string localPath)
		=> $"{BasePath}{localPath}";

	public string GetResourcePath(string localPath)
		=> $"res://{localPath}";
}