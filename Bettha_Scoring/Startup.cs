using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Bettha_Scoring.Startup))]
namespace Bettha_Scoring
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
