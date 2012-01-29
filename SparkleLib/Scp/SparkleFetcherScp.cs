//   SparkleShare, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace SparkleLib {

    // Sets up a fetcher that can get remote folders
    public class SparkleFetcherScp : SparkleFetcherBase {

        private SparkleScp scp;


        public SparkleFetcherScp (string server, string remote_folder, string target_folder) :
            base (server, remote_folder, target_folder)
	{
            if (server.EndsWith ("/"))
                server = server.Substring (0, server.Length - 1);

            if (!remote_folder.StartsWith ("/"))
                remote_folder = "/" + remote_folder;

            Uri uri;

            try {
                uri = new Uri (server + remote_folder);

            } catch (UriFormatException) {
                uri = new Uri ("ssh://" + server + remote_folder);
            }

	    if (string.IsNullOrEmpty (uri.UserInfo)) {
		uri = new Uri (uri.Scheme + "://git@" + uri.Host + ":" + uri.Port + uri.AbsolutePath);
		uri = new Uri (uri.ToString ().Replace (":-1", ""));
	    }

            base.target_folder = target_folder;
	    base.remote_url = uri.ToString();
	    string tmpPort = uri.Port.ToString();
            string tmpScpRemoteUrl = uri.UserInfo + "@" + uri.Host + ":" + uri.AbsolutePath;
	    this.scp = new SparkleScp(SparkleConfig.DefaultConfig.TmpPath,"--version")
	    { port = tmpPort, scp_remote_url = tmpScpRemoteUrl };

	}


        public override bool Fetch ()
        {
            SparkleScp scp = new SparkleScp (SparkleConfig.DefaultConfig.TmpPath,
                "-P " + this.scp.port + " " + "-r \"" + this.scp.scp_remote_url + "\" " + "\"" + base.target_folder + "\"");

            scp.Start ();
            scp.WaitForExit ();

            SparkleHelpers.DebugInfo ("Scp", "Exit code " + scp.ExitCode.ToString ());

            if (scp.ExitCode != 0) {
                return false;
            } else {
                InstallConfiguration ();
                InstallExcludeRules ();
                return true;
            }
        }

        public override string [] Warnings {
            get {
		return null; // TODO: Check if we should warn about anything
            }
        }

        // Install the user's name and email and some config into
        // the newly cloned repository
        private void InstallConfiguration ()
        {
            SparkleHelpers.DebugInfo ("Config", "Scp backend simulated to have added a configuration file");
        }


        // Add a exclude file to the repo
        private void InstallExcludeRules ()
        {
	  // No need to install Exclude Rules for the moment
        }
    }

}
