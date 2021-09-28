﻿using System;
using System.Linq;
using System.Net;

using Flurl.Http.Testing;

namespace Draft.Tests
{
    public static partial class HttpTestExtensions
    {

        public static HttpTestSetup RespondWith(this HttpTest This, HttpStatusCode code, string body)
        {
            return This.RespondWith(body, (int) code);
        }

        public static HttpTestSetup RespondWithJson(this HttpTest This, HttpStatusCode code, object data)
        {
            return This.RespondWithJson(data, (int) code);
        }

    }
}
