using System;
using System.Threading.Tasks;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.API.User;

namespace persiafighter.Plugins.Jobs.Commands
{
    public class CommandJobs : ICommand
    {
        private readonly RocketJobsPlugin _rocketJobsPlugin;

        public CommandJobs(IPlugin plugin)
        {
            _rocketJobsPlugin = (RocketJobsPlugin)plugin;
        }

        public bool SupportsUser(IUser user) => true;
        public string Name => "Jobs";
        public string Summary => "Lists the available jobs.";
        public string Description => "Lists the available jobs.";
        public string Permission => "Jobs";
        public string Syntax => null;
        public string[] Aliases => new[] { "LJ", "LJobs", "ListJobs" };

        public IChildCommand[] ChildCommands => null;

        public async Task ExecuteAsync(ICommandContext context)
        {
            await _rocketJobsPlugin.JobManager.ListJobs(context.User);
        }
    }
}
