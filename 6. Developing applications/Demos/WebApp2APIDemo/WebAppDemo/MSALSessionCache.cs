using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppDemo
{
    public static class MSALSessionCacheHelper
    {
        public static IConfidentialClientApplication WithSessionCache(this IConfidentialClientApplication app, HttpContextBase ctx, string userId)
        {
            app.UserTokenCache.SetBeforeAccess(args => args.TokenCache.DeserializeMsalV3(ctx.Session[userId] as byte[]));
            app.UserTokenCache.SetAfterAccess(args => ctx.Session[userId] = args.TokenCache.SerializeMsalV3());
            return app;
        }
    }
}