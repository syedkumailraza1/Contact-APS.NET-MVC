using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OfficeProject.Models;
using DocumentFormat.OpenXml.Spreadsheet;

namespace OfficeProject.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM Users WHERE Email = @Email";
                return await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
            }
        }

        public async Task<int> UpdateContactLoginStatusAsync(string email, bool isLogged)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "UPDATE Contacts SET IsLogged = @IsLogged WHERE Email = @Email";
                return await connection.ExecuteAsync(query, new { IsLogged = isLogged, Email = email });
            }
        }


        public async Task<int> AddUserAsync(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "INSERT INTO Users (Email, Password) VALUES (@Email, @Password)";
                return await connection.ExecuteAsync(query, user);
            }
        }
    }
}
