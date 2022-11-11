using ReaderBot.Clients;
using ReaderBot.Interfaces;

namespace ReaderBot
{
    public class Startup
    {

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<ILibraryClient, JavaLibreLibraryClient>();
            services.AddHostedService<BotClient>();
        }

        public void Configure()
        { }
    }
}
