using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Draft.Exceptions;

using FluentAssertions;

using Flurl;
using Flurl.Http.Testing;

using Xunit;

namespace Draft.Tests.Cluster
{
    public class DeleteMemberTests
    {

        [Fact]
        public async Task ShouldCallTheCorrectUrlByAwaitingImmediately()
        {
            using (var http = new HttpTest())
            {
                http.RespondWithJson(Fixtures.Cluster.CreateResponse(Fixtures.EtcdUrl.ToUri()));

                var memberId = StaticRandom.Instance.Next();

                await Etcd.ClientFor(Fixtures.EtcdUrl.ToUri())
                          .Cluster
                          .DeleteMember()
                          .WithMemberId(memberId.ToString());

                http.Should()
                    .HaveCalled(
                        Fixtures.EtcdUrl
                                .AppendPathSegment(Constants.Etcd.Path_Members)
                                .AppendPathSegment(memberId.ToString())
                    )
                    .WithVerb(HttpMethod.Delete)
                    .Times(1);
            }
        }

        [Fact]
        public async Task ShouldThrowBadRequestExceptionOn400ResponseCode()
        {
            using (var http = new HttpTest())
            {
                http.RespondWith(HttpStatusCode.BadRequest, string.Empty);

                Func<Task> action = async () =>
                {
                    await Etcd.ClientFor(Fixtures.EtcdUrl.ToUri())
                              .Cluster
                              .DeleteMember()
                              .WithMemberId(StaticRandom.Instance.Next().ToString());
                };

                (await action.Should().ThrowExactlyAsync<BadRequestException>())
                      .And
                      .IsBadRequest.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldThrowInvalidRequestExceptionOn404ResponseCode()
        {
            using (var http = new HttpTest())
            {
                http.RespondWith(HttpStatusCode.NotFound, string.Empty);

                Func<Task> action = async () =>
                {
                    await Etcd.ClientFor(Fixtures.EtcdUrl.ToUri())
                              .Cluster
                              .DeleteMember()
                              .WithMemberId(StaticRandom.Instance.Next().ToString());
                };

                (await action.Should().ThrowExactlyAsync<InvalidRequestException>())
                      .And
                      .IsInvalidRequest.Should().BeTrue();
            }
        }

    }
}
