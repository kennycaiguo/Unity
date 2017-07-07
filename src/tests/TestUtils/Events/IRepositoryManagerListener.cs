using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GitHub.Unity;
using NSubstitute;

namespace TestUtils.Events
{
    interface IRepositoryManagerListener
    {
        void OnRepositoryChanged(GitStatus status);
        void OnActiveBranchChanged(string branch);
        void OnActiveRemoteChanged(ConfigRemote? remote);
        void OnHeadChanged(string head);
        void OnLocalBranchListChanged();
        void OnRemoteBranchListChanged();
        void OnRemoteOrTrackingChanged();
        void OnIsBusyChanged(bool obj);
        void OnLocksUpdated(IEnumerable<GitLock> locks);
    }

    class RepositoryManagerEvents
    {
        public EventWaitHandle OnIsBusyChanged { get; } = new ManualResetEvent(false);
        public EventWaitHandle OnRepositoryChanged { get; } = new ManualResetEvent(false);
        public EventWaitHandle OnActiveBranchChanged { get; } = new ManualResetEvent(false);
        public EventWaitHandle OnActiveRemoteChanged { get; } = new ManualResetEvent(false);
        public EventWaitHandle OnHeadChanged { get; } = new ManualResetEvent(false);
        public EventWaitHandle OnLocalBranchListChanged { get; } = new ManualResetEvent(false);
        public EventWaitHandle OnRemoteBranchListChanged { get; } = new ManualResetEvent(false);
        public EventWaitHandle OnRemoteOrTrackingChanged { get; } = new ManualResetEvent(false);
        public EventWaitHandle OnLocksUpdated { get; } = new ManualResetEvent(false);

        public void Reset()
        {
            OnIsBusyChanged.Reset();
            OnRepositoryChanged.Reset();
            OnActiveBranchChanged.Reset();
            OnActiveRemoteChanged.Reset();
            OnHeadChanged.Reset();
            OnLocalBranchListChanged.Reset();
            OnRemoteBranchListChanged.Reset();
            OnRemoteOrTrackingChanged.Reset();
            OnLocksUpdated.Reset();
        }
    }

    static class RepositoryManagerListenerExtensions
    {
        public static void AttachListener(this IRepositoryManagerListener listener, IRepositoryManager repositoryManager,
            RepositoryManagerEvents managerEvents = null, bool trace = true)
        {
            var logger = trace ? Logging.GetLogger<IRepositoryManagerListener>() : null;

            repositoryManager.OnIsBusyChanged += b => {
                logger?.Trace("OnIsBusyChanged: {0}", b);
                listener.OnIsBusyChanged(b);
                managerEvents?.OnIsBusyChanged.Set();
            };

            repositoryManager.OnRepositoryChanged += status => {
                logger?.Debug("OnRepositoryChanged: {0}", status);
                listener.OnRepositoryChanged(status);
                managerEvents?.OnRepositoryChanged.Set();
            };

            repositoryManager.OnActiveBranchChanged += branch => {
                logger?.Trace($"OnActiveBranchChanged {branch}");
                listener.OnActiveBranchChanged(branch);
                managerEvents?.OnActiveBranchChanged.Set();
            };

            repositoryManager.OnActiveRemoteChanged += remote => {
                logger?.Trace($"OnActiveRemoteChanged {(remote.HasValue ? remote.Value.Name : null)}");
                listener.OnActiveRemoteChanged(remote);
                managerEvents?.OnActiveRemoteChanged.Set();
            };

            repositoryManager.OnHeadChanged += head => {
                logger?.Trace($"OnHeadChanged {head}");
                listener.OnHeadChanged(head);
                managerEvents?.OnHeadChanged.Set();
            };

            repositoryManager.OnLocalBranchListChanged += () => {
                logger?.Trace("OnLocalBranchListChanged");
                listener.OnLocalBranchListChanged();
                managerEvents?.OnLocalBranchListChanged.Set();
            };

            repositoryManager.OnRemoteBranchListChanged += () => {
                logger?.Trace("OnRemoteBranchListChanged");
                listener.OnRemoteBranchListChanged();
                managerEvents?.OnRemoteBranchListChanged.Set();
            };

            repositoryManager.OnRemoteOrTrackingChanged += () => {
                logger?.Trace("OnRemoteOrTrackingChanged");
                listener.OnRemoteOrTrackingChanged();
                managerEvents?.OnRemoteOrTrackingChanged.Set();
            };

            repositoryManager.OnLocksUpdated += locks => {
                var lockArray = locks.ToArray();
                logger?.Trace("OnLocksUpdated Count:{0}", lockArray.Length);
                listener.OnLocksUpdated(lockArray);
                managerEvents?.OnLocksUpdated.Set();
            };
        }

        public static void AssertDidNotReceiveAnyCalls(this IRepositoryManagerListener repositoryManagerListener)
        {
            repositoryManagerListener.DidNotReceive().OnRepositoryChanged(Args.GitStatus);
            repositoryManagerListener.DidNotReceive().OnActiveBranchChanged(Args.String);
            repositoryManagerListener.DidNotReceive().OnActiveRemoteChanged(Arg.Any<ConfigRemote?>());
            repositoryManagerListener.DidNotReceive().OnHeadChanged(Args.String);
            repositoryManagerListener.DidNotReceive().OnLocalBranchListChanged();
            repositoryManagerListener.DidNotReceive().OnRemoteBranchListChanged();
            repositoryManagerListener.DidNotReceive().OnRemoteOrTrackingChanged();
            repositoryManagerListener.DidNotReceive().OnLocksUpdated(Args.EnumerableGitLock);
        }
    }
}