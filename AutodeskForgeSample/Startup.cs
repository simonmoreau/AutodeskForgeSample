using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AutodeskForgeSample.Startup))]
namespace AutodeskForgeSample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
