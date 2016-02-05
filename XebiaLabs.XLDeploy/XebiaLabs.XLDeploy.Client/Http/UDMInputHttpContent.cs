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
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.Client.Http
{
	/// <summary>
	/// Generates an HTTPContent for an UDM object
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class UDMInputHttpContent<T> : IInputHttpContent where T : UdmBase
	{
		private readonly T _udmData;
		/// <summary>
		/// Initializes a new instance of the UDMInputContent class.
		/// </summary>
		/// <param name="udmData"></param>
		public UDMInputHttpContent(T udmData)
		{
			if (udmData == null)
			{
				throw new ArgumentNullException("udmData");
			}
			_udmData = udmData;
		}

		public HttpContent GetInputContent()
		{
		    var content = new StreamContent(UdmFactory<T>.Serialize(_udmData));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            return content;
		}
	}
}
