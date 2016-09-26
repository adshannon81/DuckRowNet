using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DuckRowNet.Startup))]
namespace DuckRowNet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
