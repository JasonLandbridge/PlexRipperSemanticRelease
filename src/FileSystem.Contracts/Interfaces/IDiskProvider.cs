﻿using PlexRipper.Domain;

namespace FileSystem.Contracts;

public interface IDiskProvider
{
    string GetParent(string path);

    List<FileSystemModel> GetFiles(string path);

    List<FileSystemModel> GetDirectories(string path);

    string GetDirectoryPath(string path);

    List<DirectoryInfo> GetDirectoryInfos(string path);

    List<IMount> GetMounts();

    string GetVolumeName(IMount mountInfo);
}
