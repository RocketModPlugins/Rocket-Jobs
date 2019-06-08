using System.Threading.Tasks;
using Rocket.API.Eventing;
using Rocket.Core.Eventing;
using Rocket.Core.Player.Events;

namespace persiafighter.Plugins.Jobs.EventListeners
{
    public class PlayerListener : IEventListener<PlayerDisconnectedEvent>
    {
        private readonly RocketJobsPlugin _rocketJobsPlugin;

        [EventHandler]
        public async Task HandleEventAsync(IEventEmitter emitter, PlayerDisconnectedEvent @event) => 
            _rocketJobsPlugin.JobManager.HandlePlayerDisconnect(@event.Player.User.Id);

        public PlayerListener(RocketJobsPlugin plugin) => _rocketJobsPlugin = plugin;
    }
}
