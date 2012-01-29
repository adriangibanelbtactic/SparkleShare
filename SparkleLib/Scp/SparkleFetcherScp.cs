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


            if (!uri.Scheme.Equals ("ssh")) {
                uri = new Uri ("ssh://" + uri);
            }

	    if (string.IsNullOrEmpty (uri.UserInfo)) {
		uri = new Uri (uri.Scheme + "://git@" + uri.Host + ":" + uri.Port + uri.AbsolutePath);
		uri = new Uri (uri.ToString ().Replace (":-1", ""));
	    }

            base.target_folder = target_folder;
            base.remote_url    = uri.ToString ();
	    Uri uri2 = new Uri (SparkleConfig.DefaultConfig.GetUrlForFolder (base.remote_url));
	    SparkleScp scp = new SparkleScp("","")
	    { port = uri2.Port };

	}


        public override bool Fetch ()
        {
            SparkleScp scp = new SparkleScp (SparkleConfig.DefaultConfig.TmpPath,
                "-r \"" + base.remote_url + "\" " + "\"" + base.target_folder + "\"");

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
            string log_file_path = SparkleHelpers.CombineMore (base.target_folder, ".sparkleshare", "log");
            File.Create (log_file_path);

            string config_file_path = SparkleHelpers.CombineMore (base.target_folder, ".sparkleshare", "config");
            File.Create (config_file_path);

            string config = "";

            // Write the config to the file
            TextWriter writer = new StreamWriter (config_file_path);
            writer.WriteLine (config);
            writer.Close ();

            SparkleHelpers.DebugInfo ("Config", "Added configuration to '" + config_file_path + "'");
        }


        // Add a exclude file to the repo
        private void InstallExcludeRules ()
        {
            string exlude_rules_file_path = SparkleHelpers.CombineMore (base.target_folder, ".sparkleshare", "exclude");
            File.Create (exlude_rules_file_path);

            TextWriter writer = new StreamWriter (exlude_rules_file_path);

                // gedit and emacs
                writer.WriteLine ("*~");

                // vi(m)
                writer.WriteLine (".*.sw[a-z]");
                writer.WriteLine ("*.un~");
                writer.WriteLine ("*.swp");
                writer.WriteLine ("*.swo");
                
                // KDE
                writer.WriteLine (".directory");
    
                // Mac OSX
                writer.WriteLine (".DS_Store");
                writer.WriteLine ("Icon?");
                writer.WriteLine ("._*");
                writer.WriteLine (".Spotlight-V100");
                writer.WriteLine (".Trashes");

                // Mac OSX
                writer.WriteLine ("*(Autosaved).graffle");
            
                // Windows
                writer.WriteLine ("Thumbs.db");
                writer.WriteLine ("Desktop.ini");

                // CVS
                writer.WriteLine ("*/CVS/*");
                writer.WriteLine (".cvsignore");
                writer.WriteLine ("*/.cvsignore");
                
                // Subversion
                writer.WriteLine ("/.svn/*");
                writer.WriteLine ("*/.svn/*");

            writer.Close ();
        }
    }

}
