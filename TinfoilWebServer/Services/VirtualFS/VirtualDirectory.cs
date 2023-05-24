﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace TinfoilWebServer.Services.VirtualFS;

public class VirtualDirectory : VirtualItem
{
    private readonly Dictionary<string, VirtualItem> _childItemsByUriSegment = new();


    public VirtualFile[] Files => _childItemsByUriSegment.Values.OfType<VirtualFile>().ToArray();

    public VirtualDirectory[] Directories => _childItemsByUriSegment.Values.OfType<VirtualDirectory>().ToArray();

    public VirtualDirectory(DirectoryUriSegment directoryUriSegment, string fullLocalPath) : base(fullLocalPath)
    {
        UriSegment = (directoryUriSegment ?? throw new ArgumentNullException(nameof(directoryUriSegment))).UriSegment;
    }

    public void AddDirectory(VirtualDirectory virtualDirectory)
    {
        if (virtualDirectory == null)
            throw new ArgumentNullException(nameof(virtualDirectory));

        _childItemsByUriSegment.Add(virtualDirectory.UriSegment, virtualDirectory);
        virtualDirectory.Parent = this;
    }

    public void AddFile(VirtualFile virtualFile)
    {
        if (virtualFile == null)
            throw new ArgumentNullException(nameof(virtualFile));

        _childItemsByUriSegment.Add(virtualFile.UriSegment, virtualFile);
        virtualFile.Parent = this;
    }

    [Pure]
    public VirtualItem? GetChild(string uriSegment)
    {
        _childItemsByUriSegment.TryGetValue(uriSegment, out var result);
        return result;
    }

    [Pure]
    public VirtualItem? ReachItem(string[]? uriSegments)
    {
        if (uriSegments is not { Length: > 0 })
            return this;

        var remainingSegments = uriSegments;
        var dirTemp = this;

        while (true)
        {
            var firstSegment = remainingSegments[0];
            remainingSegments = remainingSegments[1..];
            var childTemp = dirTemp.GetChild(firstSegment);

            if (remainingSegments.Length <= 0)
                return childTemp;

            if (childTemp is not VirtualDirectory childDirTemp)
                return null;

            dirTemp = childDirTemp;
        }

    }


    public bool ChildExists(string uriSegment)
    {
        return _childItemsByUriSegment.ContainsKey(uriSegment);
    }

    public override string UriSegment { get; }

    public override string ToString()
    {
        return $"DIRECTORY: {base.ToString()}";
    }

}

public class DirectoryUriSegment
{
    public DirectoryUriSegment(string fileName)
    {
        UriSegment = $"{Uri.EscapeDataString(fileName)}/";
    }

    public string UriSegment { get; }
}