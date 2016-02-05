/// Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
///
///
/// The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
/// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
/// There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
/// this software, see the FLOSS License Exception
/// <https://github.com/jenkinsci/deployit-plugin/blob/master/LICENSE>.
///
/// This program is free software; you can redistribute it and/or modify it under the terms
/// of the GNU General Public License as published by the Free Software Foundation; version 2
/// of the License.
///
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
/// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
/// See the GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License along with this
/// program; if not, write to the Free Software Foundation, Inc., 51 Franklin St, Fifth
/// Floor, Boston, MA 02110-1301  USA
///
using System;
using System.Xml.Linq;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.Client
{
	public class CachedRepositoryService : IRepositoryService
	{
		private readonly IRepositoryService _repository;
		private readonly CachedValue<string, bool> _existsCache;
        private readonly CachedValue<CIQueryParameters, ConfigurationItemId[]> _queryCache;
        private readonly CachedValue<string, XElement> _getCache;

		public CachedRepositoryService(TimeSpan cacheDelay, IRepositoryService repository)
		{
			_repository = repository;
			_existsCache = new CachedValue<string, bool>(id => _repository.Exists(id), cacheDelay);
			_queryCache = new CachedValue<CIQueryParameters, ConfigurationItemId[]>(p => _repository.Query(p), cacheDelay);
			_getCache = new CachedValue<string, XElement>(id => _repository.Get(id), cacheDelay);
        }


		#region IRepositoryService Members

		public bool Exists(string id)
		{
			return _existsCache.GetValue(id);
		}

		public ConfigurationItemId[] Query(CIQueryParameters parameters)
		{
			return _queryCache.GetValue(parameters);
		}

		public XElement Get(string id)
		{
			return _getCache.GetValue(id);
		}

		#endregion
	}
}
