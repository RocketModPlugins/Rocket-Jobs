using persiafighter.Plugins.Jobs.Config;
using Rocket.API.Commands;
using Rocket.API.I18N;
using Rocket.API.Permissions;
using Rocket.API.User;
using Rocket.Core.I18N;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using persiafighter.Plugins.Jobs.Jobs;
using Rocket.API.Player;

namespace persiafighter.Plugins.Jobs
{
    public class JobManager
    {
        private readonly IPermissionProvider _permissionProvider;
        private readonly IPlayerManager _globalPlayerManager;
        private readonly ITranslationCollection _translations;

        private List<IJob> _availableJobs;
        private List<JobApplication> _applicants;

        public JobManager(JobsConfiguration config, ITranslationCollection translationCollection, IPermissionProvider permissionProvider, IPlayerManager playerManager)
        {
            if (_availableJobs == null)
                _availableJobs = new List<IJob>();

            _availableJobs.AddRange(config.PublicJobs.Cast<IJob>());
            _availableJobs.AddRange(config.PrivateJobs.Cast<IJob>());
            _availableJobs.RemoveAll(k =>
            {
                if (k is PrivateJob @private)
                    return permissionProvider.GetGroupAsync(@private.LeaderPermissionGroup) == null;

                return permissionProvider.GetGroupAsync(k.PermissionGroup) == null;
            });

            if (_applicants == null)
                _applicants = new List<JobApplication>();

            _permissionProvider = permissionProvider;
            _globalPlayerManager = playerManager;
            _translations = translationCollection;
        }

        public async Task RemovePlayerFromJob(IPlayer target, string job = null, IUser caller = null)
        {
            IPlayer giveRank = target.PlayerManager.GetPlayersAsync().Result.FirstOrDefault(c => string.Equals(c.User.Id, target.User.Id, StringComparison.OrdinalIgnoreCase));

            var tJob = job != null ? _availableJobs.FirstOrDefault(k => k.JobName.Equals(job, StringComparison.OrdinalIgnoreCase)) : await GetPlayerJob(giveRank.User);

            if (tJob != null)
            {
                IPermissionGroup targetGroup = await _permissionProvider.GetGroupAsync(tJob.PermissionGroup);

                if (await _permissionProvider.RemoveGroupAsync(giveRank.User, targetGroup))
                    await _globalPlayerManager.BroadcastLocalizedAsync(_translations, "left_job", target.User.UserName, tJob.JobName);
                else
                    caller?.SendLocalizedMessageAsync(_translations, "failed_remove_job");
            }
            else
            {
                if (target.User == caller)
                   await caller.SendLocalizedMessageAsync(_translations, "not_in_job", job);
                else
                    caller?.SendLocalizedMessageAsync(_translations, "not_in_job_admin", job, target.User.UserName);
            }
        }
        public void Reload(JobsConfiguration config)
        {
            if (_availableJobs == null)
                _availableJobs = new List<IJob>();

            _availableJobs.AddRange(config.PublicJobs.Cast<IJob>());
            _availableJobs.AddRange(config.PrivateJobs.Cast<IJob>());
            _availableJobs.RemoveAll(k =>
            {
                if (k is PrivateJob @private)
                    return _permissionProvider.GetGroupAsync(@private.LeaderPermissionGroup) == null;

                return _permissionProvider.GetGroupAsync(k.PermissionGroup) == null;
            });

            if (_applicants == null)
                _applicants = new List<JobApplication>();
        }
        public async Task AddPlayerToJob(IPlayer target, string job, IUser caller = null)
        {
            IPlayer giveRank = target.PlayerManager.GetPlayersAsync().Result.FirstOrDefault(c => string.Equals(c.User.Id, target.User.Id, StringComparison.OrdinalIgnoreCase));

            IJob tJob = _availableJobs.FirstOrDefault(k => k.JobName.Equals(job, StringComparison.OrdinalIgnoreCase));
            IJob playerJob = await GetPlayerJob(giveRank.User);

            if (playerJob != null)
            {
                if (target.User == caller)
                    await caller.SendLocalizedMessageAsync(_translations, "already_in_job");
                else
                    caller?.SendLocalizedMessageAsync(_translations, "already_in_job_admin", target.User.UserName);
                return;
            }

            if (tJob == null)
            {
                caller?.SendLocalizedMessageAsync(_translations, "job_destroyed", job);
                return;
            }

            if (caller == target.User && tJob is PrivateJob @private)
            {
                if (_applicants.Any(k => k.Id == target.User.Id))
                    await caller.SendLocalizedMessageAsync(_translations, "already_applying");
                else
                {
                    var players = GetOnlinePlayersInGroup(@private.LeaderPermissionGroup).ToList();
                    if (players.Count <= 0)
                    {
                        await caller.SendLocalizedMessageAsync(_translations, "no_leaders");
                        return;
                    }

                    _applicants.Add(new JobApplication() {Id = target.User.Id, Target = @private});
                    var player = _globalPlayerManager.GetPlayersAsync().Result.First(k => k.User.Id == players.First().User.Id);
                    await player.User.SendLocalizedMessageAsync(_translations, "player_applying",
                        target.User.UserName, @private.JobName);
                    await caller.SendLocalizedMessageAsync(_translations, "job_applied");
                }

                return;
            }

            IPermissionGroup targetGroup = await _permissionProvider.GetGroupAsync(tJob.PermissionGroup);
            ClearApplications(giveRank.User);

            if (await _permissionProvider.AddGroupAsync(giveRank.User, targetGroup))
                await _globalPlayerManager.BroadcastLocalizedAsync(_translations, "joined_job", target.User.UserName, tJob.JobName);
            else
                await caller.SendLocalizedMessageAsync(_translations, "failed_add_job");
        }
        public async Task AcceptApplication(IPlayer target, IUser caller)
        {
            IPlayer giveRank = target.PlayerManager.GetPlayersAsync().Result.First(c => string.Equals(c.User.Id, target.User.Id, StringComparison.OrdinalIgnoreCase));
            JobApplication jobApp = _applicants.FirstOrDefault(k => k.Id == giveRank.User.Id);

            if (jobApp == null)
            {
                await caller.SendLocalizedMessageAsync(_translations, "not_applying", target.User.UserName);
                return;
            }

            IJob job = _availableJobs.FirstOrDefault(k =>
                k.JobName.Equals(jobApp.Target.JobName, StringComparison.OrdinalIgnoreCase));
            if (job == null)
            {
                await caller.SendLocalizedMessageAsync(_translations, "job_destroyed", jobApp.Target);
                return;
            }

            if (!(job is PrivateJob @private))
            {
                await caller.SendLocalizedMessageAsync(_translations, "special_error");
                return;
            }

            IPermissionGroup leaderGroup = await _permissionProvider.GetGroupAsync(@private.LeaderPermissionGroup);
            IEnumerable<IPermissionGroup> permissionGroups = await _permissionProvider.GetGroupsAsync(caller);

            if (!permissionGroups.Contains(leaderGroup))
            {
                await caller.SendLocalizedMessageAsync(_translations, "not_leader_of_job",
                    job.JobName);
                return;
            }

            IPermissionGroup targetGroup = await _permissionProvider.GetGroupAsync(job.PermissionGroup);

            if (await _permissionProvider.AddGroupAsync(giveRank.User, targetGroup))
                await _globalPlayerManager.BroadcastLocalizedAsync(_translations, "joined_job", target.User.UserName,
                    job.JobName);
            else
                await caller.SendLocalizedMessageAsync(_translations, "failed_add_job");
        }
        public void ClearApplications(IUser caller = null)
        {
            _applicants.Clear();
            caller?.SendLocalizedMessageAsync(_translations, "applications_cleared");
        }
        public void HandlePlayerDisconnect(string id)
        {
            _applicants.RemoveAll(k => id.Equals(k.Id, StringComparison.OrdinalIgnoreCase));
        }
        public async Task ListJobs(IUser caller)
        {
            string allJobs = string.Join(", ", _availableJobs.Select(c => c.JobName).ToArray()) + ".";
            await caller.SendLocalizedMessageAsync(_translations, "list_jobs", allJobs);
        }
        public void ClearAll()
        {
            ClearApplications();
            _availableJobs.Clear();
        }

        public async Task<IJob> GetPlayerJob(IUser target)
        {
            IEnumerable<IPermissionGroup> groups = await _permissionProvider.GetGroupsAsync(target);
            return _availableJobs.FirstOrDefault(k => groups.Any(l => k.PermissionGroup.Equals(l.Id, StringComparison.OrdinalIgnoreCase)));
        }
        private IEnumerable<IPlayer> GetOnlinePlayersInGroup(string groupName)
        {
            return _globalPlayerManager.GetPlayersAsync()
                .Result.Where(k => !(k is IConsole))
                .Where(k => _permissionProvider.GetGroupsAsync(k.User).
                    Result.Any(l => l.Id.Equals(groupName, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
