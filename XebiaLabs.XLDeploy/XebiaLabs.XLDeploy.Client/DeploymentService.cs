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
using XebiaLabs.Deployit.Client.Http;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.Client
{
    internal sealed class DeploymentService : ServiceBase, IDeploymentService
    {
        public DeploymentService(DeployitServer server)
            : base(server, "deployment")
        {
        }

        public Deployment PrepareInitial(string versionId, string environmentId)
        {
            return RetrieveDeployment(Get, "prepare/initial?version={0}&environment={1}", versionId, environmentId);
        }

        public Deployment PrepareUpdate(string versionId, string deploymentId)
        {
            return RetrieveDeployment(Get, "prepare/update?version={0}&deployedApplication={1}", versionId, deploymentId);
        }

        public Deployment AutoPrepareDeployeds(Deployment deployment)
        {
            if (deployment == null)
                throw new ArgumentNullException("deployment", "deployment is null.");

            return RetrieveDeployment(Post(deployment), "prepare/deployeds");
        }

        public Deployment Validate(Deployment deployment)
        {
            if (deployment == null)
                throw new ArgumentNullException("deployment", "deployment is null.");

            return RetrieveDeployment(Post(deployment), "validate");
        }

        public string GenerateDeploymentTask(Deployment deployment)
        {
            if (deployment == null)
                throw new ArgumentNullException("deployment", "deployment is null.");

            return ExecuteHttp<string, StringHttpContent, string, StringHttpContent>(Post(deployment), BuildCommand(""));
        }

        public string GenerateRollbackTask(string deploymentTaskId)
        {
            if (String.IsNullOrEmpty(deploymentTaskId))
                throw new ArgumentException("deploymentTaskId is null or empty.", "deploymentTaskId");

            var command = BuildCommand("rollback/{0}", deploymentTaskId);
            return ExecuteHttp<string, StringHttpContent, string, StringHttpContent>(
                new PostHttpResponseProvider(new EmptyPostInputContent()), command
            );
        }


        #region helper methods
        private static GetHttpResponseProvider Get
        {
            get { return new GetHttpResponseProvider(); }
        }

        private static PostHttpResponseProvider Post(Deployment deployment)
        {
            return new PostHttpResponseProvider(new UDMInputHttpContent<Deployment>(deployment));
        }

        private Deployment RetrieveDeployment(IHttpResponseProvider http, String urlTemplate, params Object[] args)
        {
            var command = BuildCommand(urlTemplate, args);
            return ExecuteHttp<Deployment, UDMHttpContent<Deployment>, string, StringHttpContent>(http, command);
        }

        #endregion
    }
}
