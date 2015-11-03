//-------------------------------------------------------------------------------------------------
// <copyright file="UpdateViewModel.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
// 
// <summary>
// The model of the update view.
// </summary>
//-------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Tools.WindowsInstallerXml.UX
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Syndication;
    using System.Windows.Input;
    using System.Xml;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

    /// <summary>
    /// The states of the update view model.
    /// </summary>
    public enum UpdateState
    {
        Unknown,
        Initializing,
        Checking,
        Current,
        Available,
        Failed,
    }

    /// <summary>
    /// The model of the update view.
    /// </summary>
    public class UpdateViewModel : PropertyNotifyBase
    {
        private RootViewModel root;
        private UpdateState state;
        private ICommand updateCommand;
        

        public UpdateViewModel(RootViewModel root)
        {
            this.root = root;
            WixBA.Model.Bootstrapper.DetectUpdateBegin += this.DetectUpdateBegin;

            this.State = UpdateState.Initializing;

        }

        public bool CheckingEnabled
        {
            get { return this.State == UpdateState.Initializing || this.State == UpdateState.Checking; }
        }

        public bool IsUpToDate
        {
            get { return this.State == UpdateState.Current; }
        }

        public ICommand UpdateCommand
        {
            get
            {
                if (this.updateCommand == null)
                {
                    this.updateCommand = new RelayCommand(param => WixBA.Plan(LaunchAction.UpdateReplace), param => this.State == UpdateState.Available);
                }

                return this.updateCommand;
            }
        }

        public bool UpdateEnabled
        {
            get { return this.UpdateCommand.CanExecute(this); }
        }

        /// <summary>
        /// Gets and sets the state of the update view model.
        /// </summary>
        public UpdateState State
        {
            get
            {
                return this.state;
            }

            set
            {
                if (this.state != value)
                {
                    this.state = value;
                    base.OnPropertyChanged("State");
                    base.OnPropertyChanged("Title");
                    base.OnPropertyChanged("CheckingEnabled");
                    base.OnPropertyChanged("IsUpToDate");
                    base.OnPropertyChanged("UpdateEnabled");
                }
            }
        }

        /// <summary>
        /// Gets and sets the title of the update view model.
        /// </summary>
        public string Title
        {
            get
            {
                switch (this.state)
                {
                    case UpdateState.Initializing:
                        return "Initializing update detection...";

                    case UpdateState.Checking:
                        return "Checking for updates...";

                    case UpdateState.Current:
                        return "Up to date";

                    case UpdateState.Available:
                        return "Newer version available";

                    case UpdateState.Failed:
                        return "Failed to check for updates";

                    case UpdateState.Unknown:
                        return "Check for updates.";

                    default:
                        return "Unexpected state";
                }
            }
        }

        private void DetectUpdateBegin(object sender, Bootstrapper.DetectUpdateBeginEventArgs e)
        {
            // Don't check for updates if:
            //   the first check failed (no retry)
            //   if we are being run as an uninstall
            //   if we are not under a full UI.
            if ((UpdateState.Failed != this.State) && (LaunchAction.Uninstall != WixBA.Model.Command.Action) && (Display.Full == WixBA.Model.Command.Display))
            {
                State = UpdateState.Checking;

                var updateDirectory = Path.Combine(Path.GetTempPath(), e.UpdateLocation);

                if (Directory.Exists(updateDirectory))
                {
                    var latest =
                        Directory.GetDirectories(updateDirectory)
                            .Select(x => new {version = GetVersion(Path.GetFileName(x)), path = x}).ToList()
                            .OrderByDescending(x => x.version)
                            .FirstOrDefault();

                    if (latest?.version > WixBA.Model.Version)
                    {
                        WixBA.Model.Engine.SetUpdate(Path.Combine(latest.path, Process.GetCurrentProcess().MainModule.ModuleName), null, 0, UpdateHashType.None, null);
                        State = UpdateState.Available;
                    }
                    else
                    {
                        State = UpdateState.Current;
                    }
                }
                else
                {
                    State = UpdateState.Failed;
                }
            }
        }

        private static Version GetVersion(string str)
        {
            Version result;
            Version.TryParse(str, out result);
            return result;
        }
    }
}