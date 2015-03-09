﻿using System;

using Flurl.Http;

using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Draft.Responses;

using Flurl;

namespace Draft.Requests
{
    internal class CompareAndSwapRequest : BaseRequest, ICompareAndSwapRequest, ICompareAndSwapByIndexRequest, ICompareAndSwapByValueRequest
    {

        public CompareAndSwapRequest(IEtcdClient etcdClient, Url endpointUrl, string containerPath)
            : base(etcdClient, endpointUrl, containerPath) {}

        public long ExpectedIndex { get; private set; }

        public string ExpectedValue { get; private set; }

        public long? Ttl { get; private set; }

        public string Value { get; private set; }

        Task<IKeyEvent> ICompareAndSwapByIndexRequest.Execute()
        {
            return Execute(false);
        }

        TaskAwaiter<IKeyEvent> ICompareAndSwapByIndexRequest.GetAwaiter()
        {
            return GetAwaiter(false);
        }

        ICompareAndSwapByIndexRequest ICompareAndSwapByIndexRequest.WithNewValue(string value)
        {
            WithNewValue(value);
            return this;
        }

        ICompareAndSwapByIndexRequest ICompareAndSwapByIndexRequest.WithTimeToLive(long? seconds)
        {
            WithTimeToLive(seconds);
            return this;
        }

        Task<IKeyEvent> ICompareAndSwapByValueRequest.Execute()
        {
            return Execute(true);
        }

        TaskAwaiter<IKeyEvent> ICompareAndSwapByValueRequest.GetAwaiter()
        {
            return GetAwaiter(true);
        }

        ICompareAndSwapByValueRequest ICompareAndSwapByValueRequest.WithNewValue(string value)
        {
            WithNewValue(value);
            return this;
        }

        ICompareAndSwapByValueRequest ICompareAndSwapByValueRequest.WithTimeToLive(long? seconds)
        {
            WithTimeToLive(seconds);
            return this;
        }

        public ICompareAndSwapByIndexRequest WithExpectedIndex(long modifiedIndex)
        {
            ExpectedIndex = modifiedIndex;
            return this;
        }

        public ICompareAndSwapByValueRequest WithExpectedValue(string value)
        {
            ExpectedValue = value;
            return this;
        }

        private async Task<IKeyEvent> Execute(bool isByValue)
        {
            var values = new ListDictionary
            {
                {
                    // Key
                    Constants.Etcd.Parameter_Value,
                    // Value
                    Value
                }
            };

            if (Ttl.HasValue)
            {
                values.Add(Constants.Etcd.Parameter_Ttl, Ttl.Value);
            }

            try
            {
                return await TargetUrl
                    .Conditionally(isByValue, x => x.SetQueryParam(Constants.Etcd.Parameter_PrevValue, ExpectedValue))
                    .Conditionally(!isByValue, x => x.SetQueryParam(Constants.Etcd.Parameter_PrevIndex, ExpectedIndex))
                    .PutUrlEncodedAsync(values)
                    .ReceiveEtcdResponse<KeyEvent>(EtcdClient);
            }
            catch (FlurlHttpException e)
            {
                throw e.ProcessException();
            }
        }

        private TaskAwaiter<IKeyEvent> GetAwaiter(bool isByValue)
        {
            return Execute(isByValue).GetAwaiter();
        }

        private void WithNewValue(string value)
        {
            Value = value;
        }

        private void WithTimeToLive(long? seconds)
        {
            Ttl = seconds;
        }

    }
}
