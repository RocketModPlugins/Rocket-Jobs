using System;
using System.Threading.Tasks;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.API.User;
using Rocket.Core.Commands;

namespace persiafighter.Plugins.Jobs.Commands
{
    public class CommandJob : ICommand
    {
        private readonly RocketJobsPlugin _rocketJobsPlugin;

        public CommandJob(IPlugin plugin)
        {
            _rocketJobsPlugin = (RocketJobsPlugin)plugin;
        }

        public bool SupportsUser(IUser user) => true;

        public async Task ExecuteAsync(ICommandContext context) => throw new CommandWrongUsageException();

        public string Name => "Job";
        public string[] Aliases => null;
        public string Summary => "Provides job related commands.";
        public string Description => null;
        public string Syntax => "";
        public IChildCommand[] ChildCommands => new IChildCommand[]
        {
            new CommandJobAccept(_rocketJobsPlugin),
            new CommandJobAdmin(_rocketJobsPlugin),
            new CommandJobLeave(_rocketJobsPlugin),
        };
    }
}