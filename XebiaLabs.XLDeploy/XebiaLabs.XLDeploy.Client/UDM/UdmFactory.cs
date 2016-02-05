//? Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
//?
//?
//? The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
//? <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
//? There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
//? this software, see the FLOSS License Exception
//? <https://github.com/jenkinsci/deployit-plugin/blob/master/LICENSE>.
//?
//? This program is free software; you can redistribute it and/or modify it under the terms
//? of the GNU General Public License as published by the Free Software Foundation; version 2
//? of the License.
//?
//? This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
//? without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//? See the GNU General Public License for more details.
//?
//? You should have received a copy of the GNU General Public License along with this
//? program; if not, write to the Free Software Foundation, Inc., 51 Franklin St, Fifth
//? Floor, Boston, MA 02110-1301  USA
//?
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace XebiaLabs.Deployit.Client.UDM
{
    public static class UdmFactory<T> where T : UdmBase
    {
		public static T Deserialize(XElement xml)
		{
			return Deserialize(xml.ToString());
		}

        public static T Deserialize(string xml)
        {
            return Deserialize(new StringReader(xml));
        }

        public static T Deserialize(TextReader textReader)
        {
            return new XmlSerializer(typeof(T)).Deserialize(textReader) as T;
        }

        public static MemoryStream Serialize(T udmObject)
        {
            var result = new MemoryStream();

            new XmlSerializer(typeof(T)).Serialize(result, udmObject);
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}
