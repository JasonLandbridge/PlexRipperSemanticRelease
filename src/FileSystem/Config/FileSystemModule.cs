﻿using Autofac;
using System.IO.Abstractions;
using PlexRipper.Application.Common;

namespace PlexRipper.FileSystem.Config
{
    /// <summary>
    /// Used to register all dependancies in Autofac for the FileSystem project.
    /// </summary>
    public class FileSystemModule : Module
    {
        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystemCustom>().As<IFileSystemCustom>().SingleInstance();
            builder.RegisterType<System.IO.Abstractions.FileSystem>().As<IFileSystem>().SingleInstance();
            builder.RegisterType<FileMerger>().As<IFileMerger>().SingleInstance();
            builder.RegisterType<DiskProvider>().As<IDiskProvider>().SingleInstance();
        }
    }
}