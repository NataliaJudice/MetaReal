using MetaReal.Domain.Models;
using MetaReal.Infra.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MetaReal.Infra.Data.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new MetaRealDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<MetaRealDbContext>>());

            await context.Database.MigrateAsync();

            // separado em dois porque antes era um if só no começo checando Vendedores. como o banco
            // já tinha vendedor de antes, ele saía fora e nunca criava os usuarios, aí ninguém logava
            var (luciana, georgete) = await SeedVendedoresEVendas(context);
            await SeedUsuarios(context, luciana, georgete);
        }

        private static async Task<(Vendedor Luciana, Vendedor Georgete)> SeedVendedoresEVendas(MetaRealDbContext context)
        {
            if (context.Vendedores.Any())
            {
                var luciana2 = await context.Vendedores.FirstAsync(v => v.Nome == "Luciana");
                var georgete2 = await context.Vendedores.FirstAsync(v => v.Nome == "Georgete");
                return (luciana2, georgete2);
            }

            var luciana = new Vendedor { IdVendedor = Guid.NewGuid(), Nome = "Luciana" };
            var georgete = new Vendedor { IdVendedor = Guid.NewGuid(), Nome = "Georgete" };

            await context.Vendedores.AddRangeAsync(luciana, georgete);
            await context.SaveChangesAsync();

            var registros = new List<RegistroVenda>();
            for (var dia = 1; dia <= 17; dia++)
            {
                registros.Add(new RegistroVenda
                {
                    IdRegistroVenda = Guid.NewGuid(),
                    Data = new DateTime(2026, 02, dia),
                    PretasMistas = 0m,
                    Garantia = 0m,
                    CrediarioDujuca = 0m,
                    QuantAtendimento = 0,
                    NumVendas = 0,
                    ValorTotalVendas = 0m,
                    IdVendedor = luciana.IdVendedor
                });
            }

            var dadosGeorgete = new (int Dia, decimal Garantia, int QuantAtendimento, int NumVendas, decimal ValorTotalVendas)[]
            {
                (1, 0m,      0,  0, 0m),
                (2, 11.75m,  15, 8, 7443.75m),
                (3, 446.75m, 13, 6, 6041.75m),
                (4, 99.72m,  18, 12, 8028m),
                (5, 298.32m, 10, 5, 3345m),
                (6, 0m,      9,  3, 3865m),
                (7, 0m,      8,  2, 964m),
            };

            foreach (var d in dadosGeorgete)
            {
                registros.Add(new RegistroVenda
                {
                    IdRegistroVenda = Guid.NewGuid(),
                    Data = new DateTime(2026, 02, d.Dia),
                    PretasMistas = 0m,
                    Garantia = d.Garantia,
                    CrediarioDujuca = 0m,
                    QuantAtendimento = d.QuantAtendimento,
                    NumVendas = d.NumVendas,
                    ValorTotalVendas = d.ValorTotalVendas,
                    IdVendedor = georgete.IdVendedor
                });
            }

            await context.RegistrosVenda.AddRangeAsync(registros);
            await context.SaveChangesAsync();

            return (luciana, georgete);
        }

        private static async Task SeedUsuarios(MetaRealDbContext context, Vendedor luciana, Vendedor georgete)
        {
            if (context.Usuarios.Any())
            {
                return;
            }

            var usuarios = new[]
            {
                new Usuario
                {
                    IdUsuario = Guid.NewGuid(),
                    Nome = "Gerente Dujuca",
                    Email = "gerente@gmail.com",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("Gerente@123", workFactor: 12),
                    Role = RoleUsuario.Gerente,
                    IdVendedor = null,
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                },
                new Usuario
                {
                    IdUsuario = Guid.NewGuid(),
                    Nome = luciana.Nome,
                    Email = "luciana@gmail.com",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("Vendedor@123", workFactor: 12),
                    Role = RoleUsuario.Vendedor,
                    IdVendedor = luciana.IdVendedor,
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                },
                new Usuario
                {
                    IdUsuario = Guid.NewGuid(),
                    Nome = georgete.Nome,
                    Email = "georgete@gmail.com",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("Vendedor@123", workFactor: 12),
                    Role = RoleUsuario.Vendedor,
                    IdVendedor = georgete.IdVendedor,
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                }
            };

            await context.Usuarios.AddRangeAsync(usuarios);
            await context.SaveChangesAsync();
        }
    }
}
