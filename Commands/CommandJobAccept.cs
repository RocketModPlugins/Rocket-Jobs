using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.API.User;
using Rocket.Core.Commands;
using System;
using System.Threading.Tasks;
using Rocket.API.Player;

namespace persiafighter.Plugins.Jobs.Commands
{
    public class CommandJobAccept : IChildCommand
    {
        private readonly RocketJobsPlugin _rocketJobsPlugin;
        
        public CommandJobAccept(IPlugin plugin)
        {
            _rocketJobsPlugin = (RocketJobsPlugin)plugin;
        }

        public bool SupportsUser(IUser user) => true;
        public string Name => "JobAccept";
        public string Summary => "Accepts a player requesting to join a job.";
        public string Description => "Accepts a player requesting to join a job.";
        public string Syntax => "<player>";
        public string[] Aliases => new[] { "AJ", "AJob", "AcceptJ" };

        public IChildCommand[] ChildCommands => null;

        public async Task ExecuteAsync(ICommandContext context)
        {
            if (context.Parameters.Length != 1)
                throw new CommandWrongUsageException();
            
            IPlayer toGiveRank = await context.Parameters.GetAsync<IPlayer>(0);

            await _rocketJobsPlugin.JobManager.AcceptApplication(toGiveRank, context.User);
        }
    }
}
