using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using Draft.Endpoints;
using Draft.Exceptions;

using FluentAssertions;

using Flurl.Http.Testing;

using Xunit;

namespace Draft.Tests.VerificationStrategies
{
    public class VerificationStrategyAllTests : BaseVerificationStrategyTests
    {

        protected override Uri[] Uris
        {
            get { return new[] {Uri1, Uri2, Uri3, Uri4, Uri5}; }
        }

        protected override EndpointVerificationStrategy VerificationStrategy
        {
            get { return new VerificationStrategyAll(); }
        }


        [Fact]
        public async Task ShouldThrowExceptionWhenSomeEndpointsAreOffline()
        {
            using (var http = InitializeInvalidHostHelper(
                (xh, xr) =>
                {
                    if (xr.RequestUri.ToString().StartsWith(Uri3.ToString()))
                    {
                        throw new SocketException();
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(string.Empty)
                        };
                }))
            {
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");

                Func<Task> action = async () =>
                {
                    try {
	                    await CreateSut(VerificationStrategy)
		                    .VerifyAndBuild(Uris);

                    }
                    catch (AggregateException ae)
                    {
                        ExceptionDispatchInfo.Capture(ae.Flatten().InnerExceptions.First()).Throw();
                    }
                };

                await action.Should().ThrowAsync<InvalidHostException>();
            }
        }

        [Fact]
        public async Task ShouldSucceedIfAllEndpointsAreOnline()
        {
            using (var http = new HttpTest())
            {
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");
                http.RespondWith("etc 1.2.3");

                var epool = await CreateSut(VerificationStrategy)
                    .VerifyAndBuild(Uris);

                epool.Should().NotBeNull();

                epool.OnlineEndpoints.Should().HaveSameCount(Uris);

                for (var i = 0; i < Uris.Length; i++)
                {
                    var endpoint = epool.OnlineEndpoints[i];
                    endpoint.Uri
                            .Should()
                            .BeSameAs(Uris[i]);
                }
            }
        }
    }
}
