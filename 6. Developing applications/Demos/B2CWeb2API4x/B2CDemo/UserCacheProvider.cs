using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace B2CDemo
{
    public class UserCacheProvider
    {
        public UserCacheProvider(ITokenCache cache, HttpSessionState session)
        {
            _session = session;
            cache.SetBeforeAccess(BeforeAccess);
            cache.SetAfterAccess(AfterAccess);
        }

        private static readonly object FileLock = new object();
        HttpSessionState _session;
        private void BeforeAccess(TokenCacheNotificationArgs args)
        {
            if (args.Account == null) return;
            lock (FileLock)
            {
                var key = $"token_{args.Account.HomeAccountId.ObjectId}";
                var data = (string)_session[key];
                if (!String.IsNullOrEmpty(data))
                    args.TokenCache.DeserializeMsalV3(Convert.FromBase64String(data));
            }
        }
        private void AfterAccess(TokenCacheNotificationArgs args)
        {
            if (args.HasStateChanged)
            {
                var key = $"token_{args.Account.HomeAccountId.ObjectId}";
                lock (FileLock)
                {
                    _session[key] = Convert.ToBase64String(args.TokenCache.SerializeMsalV3());
                }
            }
        }
    }
}