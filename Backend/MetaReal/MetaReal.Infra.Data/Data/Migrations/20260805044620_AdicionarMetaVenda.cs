using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaReal.Infra.Data.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMetaVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetasVenda",
                columns: table => new
                {
                    IdMetaVenda = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdVendedor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    ValorMeta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NotificadoConclusao = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetasVenda", x => x.IdMetaVenda);
                    table.ForeignKey(
                        name: "FK_MetasVenda_Vendedores_IdVendedor",
                        column: x => x.IdVendedor,
                        principalTable: "Vendedores",
                        principalColumn: "IdVendedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetasVenda_IdVendedor_Mes_Ano",
                table: "MetasVenda",
                columns: new[] { "IdVendedor", "Mes", "Ano" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetasVenda");
        }
    }
}
