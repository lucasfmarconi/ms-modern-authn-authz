using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;

namespace TodoSPA.Controllers
{

    [Authorize]
    public class TodoListController : ApiController
    {
        private List<Todo> db = new List<Todo>();

        // GET: api/TodoList
        public IEnumerable<Todo> Get()
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            IEnumerable<Todo> currentUserToDos = db.Where(a => a.Owner == owner);
            return currentUserToDos;
        }

        // GET: api/TodoList/5
        public Todo Get(int id)
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            Todo todo = db.First(a => a.Owner == owner && a.ID == id);             
            return todo;
        }

        // POST: api/TodoList
        public void Post(Todo todo)
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;            
            todo.Owner = owner;
            db.Add(todo);
          
        }

        public void Put(Todo todo)
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            Todo xtodo = db.First(a => a.Owner == owner && a.ID == todo.ID);
            if (todo != null)
            {
                xtodo.Description = todo.Description;
            }
        }

        // DELETE: api/TodoList/5
        public void Delete(int id)
        {
            string owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            Todo todo = db.First(a => a.Owner == owner && a.ID == id);
            if (todo != null)
            {
                db.Remove(todo);
            }
        }        
    }
}
