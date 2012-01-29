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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SparkleLib {

    public class SparkleRepoScp : SparkleRepoBase {

        public SparkleRepoScp (string path, SparkleBackend backend) :
            base (path, backend) { }


        public override string Identifier {
            get {
                return "sparkles";
            }
        }

        public override List<string> ExcludePaths {
            get {
                List<string> rules = new List<string> ();
                //rules.Add (Path.DirectorySeparatorChar + ".git"); 

                return rules;
            }
        }

        public override double Size {
            get {
                return 0; // TODO: Implement some way of calculating repo size
            }
        }

        public override double HistorySize {
            get {
                return 0; // TODO: Implement some way of calculating history repo size. Not sure it makes sense in scp.
            }
        }

        public override string CurrentRevision {
            get {
                return "";
            }
        }


        public override bool CheckForRemoteChanges ()
        {
             return true;
        }


        public override bool SyncUp ()
        {
            return true;
        }


        public override bool SyncDown ()
        {
            return true;
        }


        public override bool AnyDifferences {
            get {
                return false;
            }
        }


        public override bool HasUnsyncedChanges {
            get {
                string unsynced_file_path = SparkleHelpers.CombineMore (LocalPath,
                    ".sparkleshare", "has_unsynced_changes");

                return File.Exists (unsynced_file_path);
            }

            set {
                string unsynced_file_path = SparkleHelpers.CombineMore (LocalPath,
                    ".sparkleshare", "has_unsynced_changes");

                if (value) {
                    if (!File.Exists (unsynced_file_path))
                        File.Create (unsynced_file_path);
                } else {
                    File.Delete (unsynced_file_path);
                }
            }
        }


        public override List <SparkleChangeSet> GetChangeSets (int count)
        {

	    List <SparkleChangeSet> change_sets = new List <SparkleChangeSet> ();
	    SparkleChangeSet change_set = new SparkleChangeSet ();
                    change_set.Revision   = "test";
                    change_set.User.Name  = "test";
                    change_set.User.Email = "test";
	    change_sets.Add (change_set);
	    return change_sets;
        }


        public override void CreateInitialChangeSet ()
        {
            base.CreateInitialChangeSet ();
        }

        // Recursively gets a folder's size in bytes
        public override double CalculateSize (DirectoryInfo parent)
        {
            if (!Directory.Exists (parent.ToString ()))
                return 0;

            double size = 0;

            // Ignore the temporary 'rebase-apply' and '.tmp' directories. This prevents potential
            // crashes when files are being queried whilst the files have already been deleted.
            if (parent.Name.Equals ("rebase-apply") || // TODO: Check if rebase-apply is GIT-specific and remove it.
                parent.Name.Equals (".tmp"))
                return 0;

            try {
                foreach (FileInfo file in parent.GetFiles()) {
                    if (!file.Exists)
                        return 0;

                    size += file.Length;
                }

                foreach (DirectoryInfo directory in parent.GetDirectories ())
                    size += CalculateSize (directory);

            } catch (Exception) {
                return 0;
            }

            return size;
        }

        public override bool UsesNotificationCenter
        {
            get {
                string file_path = SparkleHelpers.CombineMore (LocalPath, ".sparkleshare", "disable_notification_center");
                return !File.Exists (file_path);
            }
        }
    }
}
