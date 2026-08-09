using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaReal.Infra.Data.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialVendedoresVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    IdVendedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.IdVendedor);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosVenda",
                columns: table => new
                {
                    IdRegistroVenda = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PretasMistas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Garantia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CrediarioDujuca = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantAtendimento = table.Column<int>(type: "int", nullable: false),
                    NumVendas = table.Column<int>(type: "int", nullable: false),
                    ValorTotalVendas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IdVendedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosVenda", x => x.IdRegistroVenda);
                    table.ForeignKey(
                        name: "FK_RegistrosVenda_Vendedores_IdVendedor",
                        column: x => x.IdVendedor,
                        principalTable: "Vendedores",
                        principalColumn: "IdVendedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosVenda_IdVendedor_Data",
                table: "RegistrosVenda",
                columns: new[] { "IdVendedor", "Data" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosVenda");

            migrationBuilder.DropTable(
                name: "Vendedores");
        }
    }
}
