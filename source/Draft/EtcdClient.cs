﻿using System;
using System.Linq;
using System.Threading;

using Draft.Configuration;
using Draft.Requests;
using Draft.Requests.Cluster;

using Flurl;

namespace Draft
{
    internal class EtcdClient : IEtcdClient, IAtomicEtcdClient, IClusterEtcdClient
    {

        private readonly Url _endpointUrl;

        private readonly object _gate = new object();

        private ClientConfig _clientConfig;

        public EtcdClient(Url endpointUrl, ClientConfig clientConfig)
        {
            _endpointUrl = endpointUrl;
            _clientConfig = clientConfig ?? new ClientConfig();
        }

        public IMutableEtcdClientConfig Config
        {
            get { return _clientConfig; }
        }

        public Url EndpointUrl
        {
            get { return new Url(_endpointUrl); }
        }

        public Url KeysUrl
        {
            get { return EndpointUrl.AppendPathSegment(Constants.Etcd.Path_Keys); }
        }

        IEtcdClientConfig IEtcdClient.Config
        {
            get { return Config; }
        }

        #region Client Config

        public void Configure(Action<IMutableEtcdClientConfig> configAction)
        {
            var clientConfig = _clientConfig.DeepCopy();
            lock (_gate)
            {
                configAction(clientConfig);
                Interlocked.Exchange(ref _clientConfig, clientConfig);
            }
        }

        #endregion

        #region IEtcd Client

        public ICreateDirectoryRequest CreateDirectory(string path)
        {
            return new UpsertQueueRequest(this, KeysUrl, path)
            {
                IsDirectory = true
            };
        }

        public IDeleteDirectoryRequest DeleteDirectory(string path)
        {
            return new DeleteRequest(this, KeysUrl, path, true);
        }

        public IDeleteKeyRequest DeleteKey(string key)
        {
            return new DeleteRequest(this, KeysUrl, key, false);
        }

        public IQueueRequest Enqueue(string key)
        {
            return new UpsertQueueRequest(this, KeysUrl, key)
            {
                IsQueue = true
            };
        }

        public IGetRequest GetKey(string key)
        {
            return new GetRequest(this, KeysUrl, key);
        }

        public IGetVersionRequest GetVersion()
        {
            return new GetVersionRequest(this, EndpointUrl, Constants.Etcd.Path_Version);
        }

        public IUpdateDirectoryRequest UpdateDirectory(string path)
        {
            var request = new UpsertQueueRequest(this, KeysUrl, path)
            {
                IsDirectory = true
            };

            request.WithExisting();

            return request;
        }

        public IUpsertKeyRequest UpsertKey(string key)
        {
            return new UpsertQueueRequest(this, KeysUrl, key);
        }

        public IWatchRequest Watch(string key)
        {
            return new WatchRequest(this, KeysUrl, key, false);
        }

        public IWatchRequest WatchOnce(string key)
        {
            return new WatchRequest(this, KeysUrl, key, true);
        }

        #endregion

        #region IAtomicEtcd Client

        public IAtomicEtcdClient Atomic
        {
            get { return this; }
        }

        public ICompareAndDeleteRequest CompareAndDelete(string key)
        {
            return new CompareAndDeleteRequest(this, KeysUrl, key);
        }

        public ICompareAndSwapRequest CompareAndSwap(string key)
        {
            return new CompareAndSwapRequest(this, KeysUrl, key);
        }

        #endregion

        #region IClusterEtcd Client

        public IClusterEtcdClient Cluster
        {
            get { return this; }
        }

        public ICreateMemberRequest CreateMember()
        {
            return new CreateMemberRequest(this, EndpointUrl, Constants.Etcd.Path_Members);
        }

        public IDeleteMemberRequest DeleteMember()
        {
            return new DeleteMemberRequest(this, EndpointUrl, Constants.Etcd.Path_Members);
        }

        public IGetLeaderRequest GetLeader()
        {
            return new GetLeaderRequest(this, EndpointUrl, Constants.Etcd.Path_Members_Leader);
        }

        public IGetMembersRequest GetMembers()
        {
            return new GetMembersRequest(this, EndpointUrl, Constants.Etcd.Path_Members);
        }

        public IUpdateMemberPeerUrlsRequest UpdateMemberPeerUrls()
        {
            return new UpdateMemberPeerUrlsRequest(this, EndpointUrl, Constants.Etcd.Path_Members);
        }

        #endregion
    }
}
