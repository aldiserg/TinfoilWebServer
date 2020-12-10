﻿using System;
using System.IO;
using System.Web;
using TinfoilWebServer.Models;

namespace TinfoilWebServer.Services
{
    public class IndexBuilder : IIndexBuilder
    {
        private readonly IFileFilter _fileFilter;

        public IndexBuilder(IFileFilter fileFilter)
        {
            _fileFilter = fileFilter ?? throw new ArgumentNullException(nameof(fileFilter));
        }

        public TinfoilIndex Build(string directory, Uri correspondingUri, string? messageOfTheDay)
        {
            var rooDirUri = correspondingUri.OriginalString.EndsWith('/') ? correspondingUri : new Uri(correspondingUri.OriginalString + "/");

            var tinfoilIndex = new TinfoilIndex
            {
                Success = messageOfTheDay,
            };

            var dirs = Directory.GetDirectories(directory);
            foreach (var dir in dirs)
            {
                var dirName = HttpUtility.UrlPathEncode(Path.GetFileName(dir));

                var newUri = new Uri(rooDirUri, dirName);
                tinfoilIndex.Directories.Add(newUri.AbsoluteUri);
            }

            foreach (var file in Directory.GetFiles(directory))
            {
                if (!_fileFilter.IsFileAllowed(file))
                    continue;

                var fileName = Path.GetFileName(file);
                var encodedFileName = HttpUtility.UrlPathEncode(fileName);
                var newUri = new Uri(rooDirUri, new Uri(encodedFileName, UriKind.Relative));
                tinfoilIndex.Files.Add(new FileNfo
                {
                    Size = new FileInfo(file).Length,
                    Url = newUri.AbsoluteUri
                });
            }

            return tinfoilIndex;
        }
    }
}