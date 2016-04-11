using EnvDTE;
using EnvDTE80;
using GitPlugin.Git;

namespace GitPlugin.Commands
{
    public abstract class CommandBase
    {
        public abstract void OnCommand(DTE2 application, OutputWindowPane pane);

        public abstract bool IsEnabled(DTE2 application);

        protected static void RunGitEx(string command, string filename)
        {
            GitCommands.RunGitEx(command, filename);
        }

        public bool RunForSelection { get; set; }
    }
}