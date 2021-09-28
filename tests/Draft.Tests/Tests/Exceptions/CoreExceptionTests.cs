using System;

using FluentAssertions;

using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Draft.Exceptions;
using Draft.Responses;

using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;

using Xunit;

namespace Draft.Tests.Exceptions
{
    public class CoreExceptionTests
    {
        private static readonly Func<Task<IEtcdVersion>> CallFixture = async () => await Etcd.ClientFor(Fixtures.EtcdUrl.ToUri()).GetVersion();

        private static HttpTestSetup NewErrorCodeFixture(int? etcdErrorCode = null, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            
            return new HttpTest()
                .RespondWithJson(status, Fixtures.CreateErrorMessage(etcdErrorCode));
        }

        [Fact]
        public async Task ShouldParseErrorCodeFromHttpStatusIfMissingFromBody()
        {
            using ((IDisposable)NewErrorCodeFixture(status : HttpStatusCode.Conflict))
            {
                await CallFixture.Should().ThrowAsync<ExistingPeerAddressException>();
            }
        }

        [Fact]
        public async Task ShouldParseErrorCodeFromMessageBody()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_Unknown))
            {
                await CallFixture.Should().ThrowAsync<UnknownErrorException>();
            }
        }

        [Fact]
        public async Task ShouldThrowEtcdTimeoutException()
        {
            using (var http = new HttpTest())
            {
                http.SimulateTimeout();

                (await CallFixture.Should().ThrowAsync<EtcdTimeoutException>())
                    .And
                    .IsTimeout.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowInvalidHostException()
        {
            using (HttpTest test = new HttpTest())
            {
                test.Configure(x=>{ x.HttpClientFactory = new TestingHttpClientFactory(); });
                (await CallFixture.Should().ThrowAsync<InvalidHostException>())
                    .And
                    .IsInvalidHost.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowInvalidRequestException()
        {
            using (var http = new HttpTest())
            {
                http.RespondWith(HttpStatusCode.NotFound, HttpStatusCode.NotFound.ToString());

                (await CallFixture.Should().ThrowAsync<InvalidRequestException>())
                    .And
                    .IsInvalidRequest.Should().BeTrue();
            }
        }

        #region Exception Type Tests

        [Fact]
        public async Task ShouldThrowClientInternalException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_ClientInternal))
            {
	            (await CallFixture.Should().ThrowAsync<ClientInternalException>())
                    .And
                    .IsClientInternal.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowDirectoryNotEmptyException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_DirectoryNotEmpty))
            {
	            (await CallFixture.Should().ThrowAsync<DirectoryNotEmptyException>())
                    .And
                    .IsDirectoryNotEmpty.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowEventIndexClearedException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_EventIndexCleared))
            {
	            (await CallFixture.Should().ThrowAsync<EventIndexClearedException>())
                    .And
                    .IsEventIndexCleared.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowExistingPeerAddressException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_ExistingPeerAddress))
            {
	            (await CallFixture.Should().ThrowAsync<ExistingPeerAddressException>())
                    .And
                    .IsExistingPeerAddress.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowHttpConnectionException() {

            using (HttpTest test = new HttpTest()) {
	            test.Configure(x => { x.HttpClientFactory = new TestingHttpClientFactory( /*new HttpTest(), */(ht, hrm) => { throw new WebException("The Message", WebExceptionStatus.ConnectFailure); }); });
                (await CallFixture.Should().ThrowAsync<HttpConnectionException>())
                    .And
                    .IsHttpConnection.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowIndexNotANumberException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_IndexNotANumber))
            {
	            (await CallFixture.Should().ThrowAsync<IndexNotANumberException>())
                    .And
                    .IsIndexNotANumber.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowIndexOrValueRequiredException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_IndexOrValueRequired))
            {
	            (await CallFixture.Should().ThrowAsync<IndexOrValueRequiredException>())
                    .And
                    .IsIndexOrValueRequired.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowIndexValueMutexException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_IndexValueMutex))
            {
	            (await CallFixture.Should().ThrowAsync<IndexValueMutexException>())
                    .And
                    .IsIndexValueMutex.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowInvalidActiveSizeException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_InvalidActiveSize))
            {
	            (await CallFixture.Should().ThrowAsync<InvalidActiveSizeException>())
                    .And
                    .IsInvalidActiveSize.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowInvalidFieldException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_InvalidField))
            {
	            (await CallFixture.Should().ThrowAsync<InvalidFieldException>())
                    .And
                    .IsInvalidField.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowInvalidFormException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_InvalidForm))
            {
	            (await CallFixture.Should().ThrowAsync<InvalidFormException>())
                    .And
                    .IsInvalidForm.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowInvalidRemoveDelayException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_InvalidRemoveDelay))
            {
	            (await CallFixture.Should().ThrowAsync<InvalidRemoveDelayException>())
                    .And
                    .IsInvalidRemoveDelay.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowKeyIsPreservedException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_KeyIsPreserved))
            {
	            (await CallFixture.Should().ThrowAsync<KeyIsPreservedException>())
                    .And
                    .IsKeyIsPreserved.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowKeyNotFoundException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_KeyNotFound))
            {
	            (await CallFixture.Should().ThrowAsync<KeyNotFoundException>())
                    .And
                    .IsKeyNotFound.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowLeaderElectException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_LeaderElect))
            {
	            (await CallFixture.Should().ThrowAsync<LeaderElectException>())
                    .And
                    .IsLeaderElect.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowNameRequiredException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_NameRequired))
            {
	            (await CallFixture.Should().ThrowAsync<NameRequiredException>())
                    .And
                    .IsNameRequired.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowNodeExistsException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_NodeExists))
            {
	            (await CallFixture.Should().ThrowAsync<NodeExistsException>())
                    .And
                    .IsNodeExists.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowNoMorePeerException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_NoMorePeer))
            {
	            (await CallFixture.Should().ThrowAsync<NoMorePeerException>())
                    .And
                    .IsNoMorePeer.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowNotADirectoryException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_NotDirectory))
            {
	            (await CallFixture.Should().ThrowAsync<NotADirectoryException>())
                    .And
                    .IsNotDirectory.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowNotAFileException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_NotFile))
            {
	            (await CallFixture.Should().ThrowAsync<NotAFileException>())
                    .And
                    .IsNotFile.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowPreviousValueRequiredException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_PreviousValueRequired))
            {
	            (await CallFixture.Should().ThrowAsync<PreviousValueRequiredException>())
                    .And
                    .IsPreviousValueRequired.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowRaftInternalException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_RaftInternal))
            {
	            (await CallFixture.Should().ThrowAsync<RaftInternalException>())
                    .And
                    .IsRaftInternal.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowRootIsReadOnlyException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_RootReadOnly))
            {
	            (await CallFixture.Should().ThrowAsync<RootIsReadOnlyException>())
                    .And
                    .IsRootReadOnly.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowStandbyInternalException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_StandbyInternal))
            {
	            (await CallFixture.Should().ThrowAsync<StandbyInternalException>())
                    .And
                    .IsStandbyInternal.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowTestFailedException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_TestFailed))
            {
	            (await CallFixture.Should().ThrowAsync<TestFailedException>())
                    .And
                    .IsTestFailed.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowTimeoutNotANumberException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_TimeoutNotANumber))
            {
	            (await CallFixture.Should().ThrowAsync<TimeoutNotANumberException>())
                    .And
                    .IsTimeoutNotANumber.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowTtlNotANumberException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_TtlNotANumber))
            {
	            (await CallFixture.Should().ThrowAsync<TtlNotANumberException>())
                    .And
                    .IsTtlNotANumber.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowUnknownErrorException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_Unknown))
            {
	            (await CallFixture.Should().ThrowAsync<UnknownErrorException>())
                    .And
                    .IsUnknown.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowValueOrTtlRequiredException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_ValueOrTtlRequired))
            {
	            (await CallFixture.Should().ThrowAsync<ValueOrTtlRequiredException>())
                    .And
                    .IsValueOrTtlRequired.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowValueRequiredException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_ValueRequired))
            {
	            (await CallFixture.Should().ThrowAsync<ValueRequiredException>())
                    .And
                    .IsValueRequired.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowWatcherClearedException()
        {
            using ((IDisposable)NewErrorCodeFixture(Constants.Etcd.ErrorCode_WatcherCleared))
            {
	            (await CallFixture.Should().ThrowAsync<WatcherClearedException>())
                    .And
                    .IsWatcherCleared.Should().BeTrue();
            }
        }

        #endregion
    }
}
