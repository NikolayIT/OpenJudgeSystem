namespace OJS.Web.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ionic.Zip;

    public static class ZipFileExtensions
    {
        public static IEnumerable<ZipEntry> GetZipEntriesByExtensions(this ZipFile zipFile, string extensions) =>
            zipFile.EntriesSorted.Where(ze =>
                ze.FileName.Length > extensions.Length &&
                ze.FileName
                    .Substring(ze.FileName.Length - extensions.Length, extensions.Length)
                    .Equals(extensions, StringComparison.OrdinalIgnoreCase));
    }
}