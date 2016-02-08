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
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace XebiaLabs.Deployit.ManifestEditor
{
    internal static class FileDialogs
    {
        /// <summary>
        /// Opens a file dialog querying the user which manifest file to open
        /// </summary>
        /// <param name="title">The dialog title</param>
        /// <returns>The user-selected manifest file, or null</returns>
        public static string OpenManifestFileDialog(string title)
        {
            var openDialog = new OpenFileDialog
            {
                Title = title,
                DefaultExt = "*.xml",
                CheckFileExists = true,
                FileName = null,
                Filter = "XL Deploy manifest file|*.xml"
            };

            var ret = openDialog.ShowDialog();
            return (ret == true) ? openDialog.FileName : null;
        }


        public static string SaveManifestFileDialog(string title, string defaultFileName)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = title,
                DefaultExt = "*.xml",
                FileName = defaultFileName,
                AddExtension = true,
                Filter = "XL Deploy manifest file|*.xml"
            };

            var ret = saveDialog.ShowDialog();
            return ret == true ? saveDialog.FileName : null;
        }

        public static string GetFolder(string title)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = title;
                var result = dialog.ShowDialog();

                return result == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }
    }
}
