using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

public partial class FilesystemInstance
{
    public readonly string Root;
    
    public readonly string Path;
    public readonly string AbsolutePath;

    public readonly string FileName;

    public readonly string ParentPath;
    public readonly string ParentAbsolutePath;

    public readonly FileType TypeOfFile;

    
    public FilesystemInstance(string root, string path, FileType createIfNull = default, string storeString = null)
    {
        Root = root;
        
        Path = path;
        ParentPath = GetParentPath().Replace(Path, "");

        AbsolutePath = ProjectSettings.GlobalizePath($"{Root}/{Path}");
        ParentAbsolutePath = ProjectSettings.GlobalizePath($"{Root}/{ParentPath}");
        
        FileName = GetFileName().Match(Path).Value;

        if (DirAccess.DirExistsAbsolute(AbsolutePath))
            TypeOfFile = FileType.Directory;
        else if (FileAccess.FileExists(AbsolutePath))
            TypeOfFile = FileType.File;

        else if (createIfNull == FileType.Directory)
        {
            MakeDir(AbsolutePath);

            TypeOfFile = createIfNull;
        }
        else if (createIfNull == FileType.File)
        {
            MakeDir(ParentAbsolutePath);

            using FileAccess file = FileAccess.Open(AbsolutePath, FileAccess.ModeFlags.Write);
            CheckAccessError(file, "while being created");

            file.StoreString(storeString ?? "");
            CheckAccessError(file, "while storing a string");

            TypeOfFile = createIfNull;
        }

        if (TypeOfFile == default)
            throw new Exception($"Path '{AbsolutePath}' had an issue.");
    }
    
    public FilesystemInstance Get(string path, FileType createIfNull = default)
        => new(Root, $"{Path}/{path}", createIfNull);
    
    
    public static bool DoesPathExist(string absolutePath)
        => DirAccess.DirExistsAbsolute(absolutePath) || FileAccess.FileExists(absolutePath);
    
    public static FileType GetFileTypeAtPath(string absolutePath)
        => DirAccess.DirExistsAbsolute(absolutePath) ? FileType.Directory : FileAccess.FileExists(absolutePath) ? FileType.File : default;
    
    
    public static bool Delete(string absolutePath, bool safe = false)
    {
        FileType fileTypeAtPath = GetFileTypeAtPath(absolutePath);
        
        if (safe && fileTypeAtPath == default)
            return false;
        
        
        if (fileTypeAtPath == FileType.Directory)
        {
            foreach (var name in DirAccess.GetDirectoriesAt(absolutePath))
                Delete($"{absolutePath}/{name}");
        
            foreach (var name in DirAccess.GetFilesAt(absolutePath))
                DirAccess.RemoveAbsolute($"{absolutePath}/{name}");
        }
        
        DirAccess.RemoveAbsolute(absolutePath).CheckError($"Failed to delete path '{absolutePath}'");
        
        return true;
    }
    
    public void Delete()
        => Delete(AbsolutePath);
    
    
    private static void MakeDir(string absolutePath)
    {
        if (DirAccess.DirExistsAbsolute(absolutePath))
            return;

        DirAccess.MakeDirRecursiveAbsolute(absolutePath).CheckError();
    }


    public bool IsDirectory()
        => TypeOfFile == FileType.Directory;
    
    public bool IsFile()
        => TypeOfFile == FileType.File;


    // Directory
    private void ThrowIfNotDirectory()
    {
        if (!IsDirectory())
            throw new Exception($"Path '{AbsolutePath}' is not a directory.");
    }
    
    
    public IEnumerator<FilesystemInstance> GetEnumerator()
    {
        ThrowIfNotDirectory();

        using DirAccess directory = DirAccess.Open(AbsolutePath);
        CheckAccessError("while opening a directory");
        
        directory.ListDirBegin().CheckError($"Path '{AbsolutePath}'");
        
        string fileName = directory.GetNext();
        
        while (fileName != "")
        {
            yield return Get(fileName);
            
            fileName = directory.GetNext();
        }
    }
    
    
    // File
    private void ThrowIfNotFile()
    {
        if (!IsFile())
            throw new Exception($"Path '{AbsolutePath}' is not a file.");
    }
    
    
    public string ReadString()
    {
        ThrowIfNotFile();
        
        using FileAccess file = FileAccess.Open(AbsolutePath, FileAccess.ModeFlags.Read);
        CheckAccessError(file, "while reading a string");
        
        return file.GetAsText();
    }
    
    public void WriteString(string storeString)
    {
        ThrowIfNotFile();
        
        using FileAccess file = FileAccess.Open(AbsolutePath, FileAccess.ModeFlags.Write);
        CheckAccessError(file, "while storing a string");

        file.StoreString(storeString);
        CheckAccessError(file, "while storing a string");
    }
    
    
    public JsonObject ReadJson()
    {
        ThrowIfNotFile();
        
        using FileAccess file = FileAccess.Open(AbsolutePath, FileAccess.ModeFlags.Read);
        CheckAccessError(file, "while reading a string");
        
        return JsonUtil.Parse(file.GetAsText());
    }


    private void CheckAccessError(FileAccess access, string message)
    {
        Error? error = access?.GetError() ?? (access != null ? FileAccess.GetOpenError() : null);

        if (error == Error.Ok)
            return;
        
        if (error.HasValue)
            GD.PushWarning(error);

        throw new Exception($"File '{AbsolutePath}' had an error {message}.");
    }
    
    private void CheckAccessError(string message)
        => DirAccess.GetOpenError().CheckError($"Directory '{AbsolutePath}' had an error {message}");


    public enum FileType
    {
        None,
        Directory,
        File
    }

    [GeneratedRegex("([^/]+)/?$")] private static partial Regex GetFileName();
    [GeneratedRegex("/?[^/]+/?$")] private static partial Regex GetParentPath();
}