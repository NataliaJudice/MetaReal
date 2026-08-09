using MetaReal.Application;
using MetaReal.Application.Interfaces;
using MetaReal.Application.Services;
using MetaReal.Infra.Data.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MetaReal.Infra.Ioc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureIoC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMetaRealDbContext>(provider => provider.GetRequiredService<MetaRealDbContext>());

            services.Configure<ConfiguracaoJwt>(configuration.GetSection(ConfiguracaoJwt.SecaoConfig));

            services.AddScoped<IVendedoresService, VendedoresService>();
            services.AddScoped<IVendasService, VendasService>();
            services.AddScoped<IAutenticacaoService, AutenticacaoService>();
            services.AddScoped<IAuditoriaService, AuditoriaService>();
            services.AddScoped<IMetaService, MetaService>();
            services.AddScoped<IRelatoriosService, RelatoriosService>();
            services.AddScoped<IUsuarioAtualService, UsuarioAtualService>();
            return services;
        }
    }
}
