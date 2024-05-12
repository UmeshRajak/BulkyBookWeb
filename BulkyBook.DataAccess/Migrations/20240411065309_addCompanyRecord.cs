using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addCompanyRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "id", "City", "Name", "PhoneNumber", "PostalAddress", "State", "StreetAddress" },
                values: new object[,]
                {
                    { 1, "Tech City", "Tech Mahindra", "9584338981", "220686", "New York", "113, Billy Spark" },
                    { 2, "Tech City", "Tech Live", "9584118981", "229886", "Berlin", "113, Mayor Road" },
                    { 3, "Tech City", "Tech Instance", "9098979600", "229086", "Guawa", "113, Mount Barle" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "id",
                keyValue: 3);
        }
    }
}
