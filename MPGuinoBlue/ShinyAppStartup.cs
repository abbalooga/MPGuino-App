using Microsoft.Extensions.DependencyInjection;
using Shiny;

namespace MPGuinoBlue
{
    public class ShinyAppStartup : Shiny.ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.UseBleCentral();
            services.UseBlePeripherals();
        }
    }
}
