using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

using EnvDTE;
using EnvDTE80;

using GitPlugin.Commands;

using Microsoft.VisualStudio.Shell;

using Constants = EnvDTE.Constants;

namespace GitExtensionsVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GitExtCommands
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private readonly Dictionary<string, CommandBase> _commandsByName = new Dictionary<string, CommandBase>();
        private readonly Dictionary<int, CommandBase> _commands = new Dictionary<int, CommandBase>();

        private readonly DTE2 _application;
        private OutputWindowPane _outputPane;
        private OleMenuCommandService _commandService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitExtCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private GitExtCommands(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            _package = package;
            _application = (DTE2)ServiceProvider.GetService(typeof(DTE));
            _commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            try
            {
                //RegisterCommand("Difftool_Selection", new ToolbarCommand<OpenWithDiftool>(runForSelection: true));
                RegisterCommand("Difftool", new ToolbarCommand<OpenWithDiftool>(), PackageIds.gitExtDiffCommand);
                //RegisterCommand("ShowFileHistory_Selection", new ToolbarCommand<FileHistory>(runForSelection: true));
                RegisterCommand("ShowFileHistory", new ToolbarCommand<FileHistory>(), PackageIds.gitExtHistoryCommand);
                //RegisterCommand("ResetChanges_Selection", new ToolbarCommand<Revert>(runForSelection: true));
                RegisterCommand("ResetChanges", new ToolbarCommand<Revert>(), PackageIds.gitExtResetFileCommand);
                RegisterCommand("Browse", new ToolbarCommand<Browse>(), PackageIds.gitExtBrowseCommand);
                RegisterCommand("Clone", new ToolbarCommand<Clone>(), PackageIds.gitExtCloneCommand);
                RegisterCommand("CreateNewRepository", new ToolbarCommand<Init>(), PackageIds.gitExtNewCommand);
                RegisterCommand("Commit", new ToolbarCommand<Commit>(), PackageIds.gitExtCommitCommand);
                RegisterCommand("Pull", new ToolbarCommand<Pull>(), PackageIds.gitExtPullCommand);
                RegisterCommand("Push", new ToolbarCommand<Push>(), PackageIds.gitExtPushCommand);
                RegisterCommand("Stash", new ToolbarCommand<Stash>(), PackageIds.gitExtStashCommand);
                RegisterCommand("Remotes", new ToolbarCommand<Remotes>(), PackageIds.gitExtRemotesCommand);
                RegisterCommand("GitIgnore", new ToolbarCommand<GitIgnore>(), PackageIds.gitExtGitIgnoreCommand);
                RegisterCommand("ApplyPatch", new ToolbarCommand<ApplyPatch>(), PackageIds.gitExtApplyPatchCommand);
                RegisterCommand("FormatPatch", new ToolbarCommand<FormatPatch>(), PackageIds.gitExtFormatPatchCommand);
                RegisterCommand("ViewChanges", new ToolbarCommand<ViewChanges>(), PackageIds.gitExtViewChangesCommand);
                RegisterCommand("FindFile", new ToolbarCommand<FindFile>(), PackageIds.gitExtFindFileCommand);
                RegisterCommand("SwitchBranch", new ToolbarCommand<SwitchBranch>(), PackageIds.gitExtCheckoutCommand);
                RegisterCommand("CreateBranch", new ToolbarCommand<CreateBranch>(), PackageIds.gitExtCreateBranchCommand);
                RegisterCommand("Merge", new ToolbarCommand<Merge>(), PackageIds.gitExtMergeCommand);
                RegisterCommand("Rebase", new ToolbarCommand<Rebase>(), PackageIds.gitExtRebaseCommand);
                RegisterCommand("SolveMergeConflicts", new ToolbarCommand<SolveMergeConflicts>(), PackageIds.gitExtSolveConflictsCommand);
                RegisterCommand("CherryPick", new ToolbarCommand<Cherry>(), PackageIds.gitExtCherryPickCommand);
                RegisterCommand("Bash", new ToolbarCommand<Bash>(), PackageIds.gitExtBashCommand);
                RegisterCommand("Settings", new ToolbarCommand<Settings>(), PackageIds.gitExtSettingsCommand);
                RegisterCommand("About", new ToolbarCommand<About>(), PackageIds.gitExtAboutCommand);
    }
            catch (Exception ex)
            {
                if (OutputPane != null)
                    OutputPane.OutputString("Error adding commands: " + ex);
            }
        }

        private void RegisterCommand(string commandName, CommandBase command, int id)
        {
            _commandsByName[commandName] = command;
            var commandId = new CommandID(PackageGuids.guidGitExtensionsPackageCmdSet, id);
            var menuCommand = new MenuCommand(MenuItemCallback, commandId);
            _commandService.AddCommand(menuCommand);
            _commands[id] = command;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GitExtCommands Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public OutputWindowPane OutputPane
        {
            get { return _outputPane ?? (_outputPane = AquireOutputPane(_application, "GitExtensions")); }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new GitExtCommands(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var guiCommand = (MenuCommand)sender;
            CommandBase command;
            if (!_commands.TryGetValue(guiCommand.CommandID.ID, out command))
                return;
            command.OnCommand(_application, OutputPane);
        }

        private static OutputWindowPane AquireOutputPane(DTE2 app, string name)
        {
            try
            {
                if (name == "")
                    return null;

                OutputWindowPane result = Plugin.FindOutputPane(app, name);
                if (result != null)
                    return result;

                var outputWindow = (OutputWindow)app.Windows.Item(Constants.vsWindowKindOutput).Object;
                OutputWindowPanes panes = outputWindow.OutputWindowPanes;
                return panes.Add(name);
            }
            catch (Exception)
            {
                //ignore!!
                return null;
            }
        }
    }
}
