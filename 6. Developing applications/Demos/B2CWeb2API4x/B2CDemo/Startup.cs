using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Builder;

namespace B2CDemo
{
    public partial class Startup
    {
        public void Configuration(IApplicationBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
