using System;
using System.Threading.Tasks;
using Rocket.API.Commands;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.API.User;
using Rocket.Core.Commands;

namespace persiafighter.Plugins.Jobs.Commands
{
    public class CommandJoinJob : ICommand
    {
        private readonly RocketJobsPlugin _rocketJobsPlugin;

        public CommandJoinJob(IPlugin plugin)
        {
            _rocketJobsPlugin = (RocketJobsPlugin)plugin;
        }

        public bool SupportsUser(IUser user) => true;
        public string Name => "JoinJob";
        public string Summary => "Joins a public job or applies to a private job.";
        public string Description => "Joins a public job or applies to a private job.";
        public string Permission => "JoinJob";
        public string Syntax => "<job name>";
        public string[] Aliases => new[] { "JJ", "JJob" };

        public IChildCommand[] ChildCommands => null;

        public async Task ExecuteAsync(ICommandContext context)
        {
            if (context.Parameters.Length != 1)
                throw new CommandWrongUsageException();

            string job = await context.Parameters.GetAsync<string>(0);

            await _rocketJobsPlugin.JobManager.AddPlayerToJob((IPlayer)context.User, job, context.User);
        }
    }
}
