using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyJD.Startup))]
namespace MyJD
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
