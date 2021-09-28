using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Flurl;
using Flurl.Http;

namespace Draft
{
    internal static class FlurlExtensions
    {

        public static Url ToUrl(this Uri This)
        {
            return This.ToString();
        }

        public static Url Conditionally(this Url This, bool predicate, Func<Url, Url> action)
        {
            return predicate ? action(This) : This;
        }

        public static IFlurlRequest Conditionally(this Url This, bool predicate, Func<Url, IFlurlRequest> action)
        {
            return predicate ? action(This) : new FlurlRequest(This.ToString());
        }

        public static Task<IFlurlResponse> Conditionally(this Url This, bool predicate, object data, Func<Url, object, Task<IFlurlResponse>> ifTrue, Func<Url, object, Task<IFlurlResponse>> ifFalse)
        {
            return predicate ? ifTrue(This, data) : ifFalse(This, data);
        }

    }
}
