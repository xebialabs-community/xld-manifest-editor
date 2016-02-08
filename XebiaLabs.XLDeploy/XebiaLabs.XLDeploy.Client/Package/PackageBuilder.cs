// Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
//
//
// The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
// There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
// this software, see the FLOSS License Exception
// <https://github.com/xebialabs-community/xld-manifest-editor/blob/master/LICENSE>.
//
// This program is free software; you can redistribute it and/or modify it under the terms
// of the GNU General Public License as published by the Free Software Foundation; version 2
// of the License.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this
// program; if not, write to the Free Software Foundation, Inc., 51 Franklin St, Fifth
// Floor, Boston, MA 02110-1301  USA
//
using Ionic.Zip;
using System;
using System.IO;
using XebiaLabs.Deployit.Client.Manifest;
using System.Linq;

namespace XebiaLabs.Deployit.Client.Package
{
	public static class PackageBuilder
	{
		private static string FormatPath(string manifestPath)
		{
			if (String.IsNullOrEmpty(manifestPath))
				throw new ArgumentException("manifestPath is null or empty.", "manifestPath");

			return manifestPath.Replace('/', '\\');
		}

        private static bool EntryExists(ZipFile zip, string filePath)
        {
            return zip.EntryFileNames.Any(x => FormatPath(x).Trim('\\') == FormatPath(filePath).Trim('\\'));
        }

		public static void Build(DeployitManifest manifest, string packageRootPath,string packagePath)
		{
			using (var z = new ZipFile(System.Text.Encoding.UTF8))
			{
			    var stream = new MemoryStream();
				var writer = new StreamWriter(stream, DeployitManifest.Encoding);
				manifest.Save(writer);
				writer.Flush();
				stream.Seek(0L, SeekOrigin.Begin);
				z.AddEntry(@"deployit-manifest.xml", stream);

				foreach (var entry in manifest.Entries)
				{
				    if (string.IsNullOrEmpty(entry.Path)) {continue;}

                    var formattedPath = FormatPath(entry.Path);
				    var path = Path.Combine(packageRootPath, formattedPath);

                    if (EntryExists(z, formattedPath)) { continue; }

				    if (Directory.Exists(path))
				    {
                        z.AddDirectory(path, formattedPath);
				    }
				    else if (File.Exists(path))
				    {
				        z.AddFile(path, Path.GetDirectoryName(formattedPath));
				    }
				    else
				    {
				        throw new InvalidOperationException(string.Format("Cannot find data in '{0}' for entry '{1}'", path,entry.Name));
                    }
				}
				z.SortEntriesBeforeSaving = false;
				z.ParallelDeflateThreshold = -1;
				z.Save(packagePath);
			}
		}
	}
}
