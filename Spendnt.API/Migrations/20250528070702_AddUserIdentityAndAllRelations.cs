using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spendnt.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdentityAndAllRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Egresos_EgresosId",
                table: "Categorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Ingresos_IngresosId",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_EgresosId",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_IngresosId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "EgresosId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "IngresosId",
                table: "Categorias");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Saldo",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "RecordatoriosGasto",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notas",
                table: "RecordatoriosGasto",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RecordatoriosGasto",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MetasAhorro",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Historiales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Historiales",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaldoId",
                table: "Historiales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Saldo_UserId",
                table: "Saldo",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosGasto_UserId",
                table: "RecordatoriosGasto",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MetasAhorro_UserId",
                table: "MetasAhorro",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_CategoriaId",
                table: "Ingresos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Historiales_SaldoId",
                table: "Historiales",
                column: "SaldoId");

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_CategoriaId",
                table: "Egresos",
                column: "CategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Egresos_Categorias_CategoriaId",
                table: "Egresos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Historiales_Saldo_SaldoId",
                table: "Historiales",
                column: "SaldoId",
                principalTable: "Saldo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ingresos_Categorias_CategoriaId",
                table: "Ingresos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetasAhorro_User_UserId",
                table: "MetasAhorro",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordatoriosGasto_User_UserId",
                table: "RecordatoriosGasto",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Saldo_User_UserId",
                table: "Saldo",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_Categorias_CategoriaId",
                table: "Egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_Historiales_Saldo_SaldoId",
                table: "Historiales");

            migrationBuilder.DropForeignKey(
                name: "FK_Ingresos_Categorias_CategoriaId",
                table: "Ingresos");

            migrationBuilder.DropForeignKey(
                name: "FK_MetasAhorro_User_UserId",
                table: "MetasAhorro");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordatoriosGasto_User_UserId",
                table: "RecordatoriosGasto");

            migrationBuilder.DropForeignKey(
                name: "FK_Saldo_User_UserId",
                table: "Saldo");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_Saldo_UserId",
                table: "Saldo");

            migrationBuilder.DropIndex(
                name: "IX_RecordatoriosGasto_UserId",
                table: "RecordatoriosGasto");

            migrationBuilder.DropIndex(
                name: "IX_MetasAhorro_UserId",
                table: "MetasAhorro");

            migrationBuilder.DropIndex(
                name: "IX_Ingresos_CategoriaId",
                table: "Ingresos");

            migrationBuilder.DropIndex(
                name: "IX_Historiales_SaldoId",
                table: "Historiales");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_CategoriaId",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Saldo");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RecordatoriosGasto");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MetasAhorro");

            migrationBuilder.DropColumn(
                name: "SaldoId",
                table: "Historiales");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "RecordatoriosGasto",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Notas",
                table: "RecordatoriosGasto",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Historiales",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Historiales",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EgresosId",
                table: "Categorias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IngresosId",
                table: "Categorias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_EgresosId",
                table: "Categorias",
                column: "EgresosId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_IngresosId",
                table: "Categorias",
                column: "IngresosId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Egresos_EgresosId",
                table: "Categorias",
                column: "EgresosId",
                principalTable: "Egresos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Ingresos_IngresosId",
                table: "Categorias",
                column: "IngresosId",
                principalTable: "Ingresos",
                principalColumn: "Id");
        }
    }
}
