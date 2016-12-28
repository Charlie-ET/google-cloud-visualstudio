﻿//------------------------------------------------------------------------------
// <copyright file="ErrorReportingToolWindowCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using GoogleCloudExtension.Accounts;
using GoogleCloudExtension.Utils;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ErrorReportingToolWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4231;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a7435138-27e2-410c-9d28-dffc5aa3fe80");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorReportingToolWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ErrorReportingToolWindowCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(this.ShowToolWindow, menuCommandID);
                commandService.AddCommand(menuItem);
                Action setEnabled = () =>
                {
                    menuItem.Enabled = (CredentialsStore.Default?.CurrentAccount != null &&
                        !string.IsNullOrWhiteSpace(CredentialsStore.Default?.CurrentProjectId));
                };

                setEnabled();
                menuItem.BeforeQueryStatus += (sender, e) => setEnabled();
                CredentialsStore.Default.Reset += (sender, e) => CloseWindow();
            };
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ErrorReportingToolWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new ErrorReportingToolWindowCommand(package);
        }

        /// <summary>
        /// Close the LogsViewerToolWindow when there is no account available.
        /// </summary>
        private static void CloseWindow()
        {
            ErrorHandlerUtils.HandleExceptions(() =>
            {
                // Get the instance number 0 of this tool window. This window is single instance so this instance
                // is actually the only one.
                // The last flag is set to true so that if the tool window does not exists it will be created.
                ToolWindowPane window = GoogleCloudExtensionPackage.Instance.FindToolWindow(
                    typeof(ErrorReportingToolWindow), 0, false);
                if (null == window?.Frame)
                {
                    Debug.WriteLine("Does not find LogsViewerToolWindow instance.");
                }
                else
                {
                    IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.CloseFrame(
                        (uint)__FRAMECLOSE.FRAMECLOSE_NoSave));
                }
            });
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(ErrorReportingToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}