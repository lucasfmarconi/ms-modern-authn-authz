using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;
using ToGoAPI.Models;
using System.Web.Http.Cors;

namespace ToGoAPI.Controllers
{
    [Authorize]
    [EnableCors (origins: "https://localhost:44326", headers: "*", methods: "*")]
    public class ToGoListController : ApiController
    {
        private List<ToGo> db = new List<ToGo>();

        // GET: api/ToGoList
        public IEnumerable<ToGo> Get()
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            IEnumerable<ToGo> currentUserToGos = db.Where(a => a.Owner == owner);
            return currentUserToGos;
        }

        // GET: api/ToGoList/5
        public ToGo Get(int id)
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            ToGo toGo = db.First(a => a.Owner == owner && a.ID == id);
            return toGo;
        }

        // POST: api/ToGoList
        public void Post(ToGo ToGo)
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            ToGo.Owner = owner;
            db.Add(ToGo);
        }

        public void Put(ToGo ToGo)
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            ToGo xToGo = db.First(a => a.Owner == owner && a.ID == ToGo.ID);
            if (ToGo != null)
            {
                xToGo.Description = ToGo.Description;
            }
        }

        // DELETE: api/ToGoList/5
        public void Delete(int id)
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            ToGo ToGo = db.First(a => a.Owner == owner && a.ID == id);
            if (ToGo != null)
            {
                db.Remove(ToGo);
            }
        }
    }
}
