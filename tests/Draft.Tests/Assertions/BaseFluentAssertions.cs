using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

using Flurl.Http;

namespace Draft.Tests.Assertions
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseFluentAssertions
    {

        protected BaseFluentAssertions(IReadOnlyCollection<FlurlCall> calls)
        {
            Calls = new List<FlurlCall>(calls);
        }

        protected IList<FlurlCall> Calls { get; private set; }

        protected List<FlurlCall> FilterCalls(Func<FlurlCall, bool> filter)
        {
            return Calls.Where(filter).ToList();
        }

        protected static bool MatchesPattern(string toCheck, string pattern)
        {
            var regex = Regex.Escape(pattern).Replace("\\*", "(.*)");
            return Regex.IsMatch(toCheck, regex);
        }

        protected string FirstRequestAbsoluteUri
        {
            get { return Calls.Select(x => x.HttpRequestMessage.RequestUri.AbsoluteUri).FirstOrDefault(); }
        }

    }
}