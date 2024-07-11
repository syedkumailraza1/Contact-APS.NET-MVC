using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using OfficeProject.Models;

namespace OfficeProject.Data
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetContactsAsync();
        Task<Contact> GetContactByIdAsync(int id);
        Task AddContactAsync(Contact contact);
        Task UpdateContactAsync(Contact contact);
        Task DeleteContactAsync(int id);
        Task<IEnumerable<Contact>> GetDeletedContactsAsync();
        Task RestoreContactAsync(int id);
    }


    public class ContactRepository : IContactRepository
    {
        private readonly IDbConnection _context;

        public ContactRepository(IDbConnection context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contact>> GetContactsAsync()
        {
            var contacts = await _context.QueryAsync<Contact>("SELECT * FROM Contacts WHERE isDeleted = 0");
            return contacts;
        }

        public async Task<Contact> GetContactByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            var contact = await _context.QueryFirstOrDefaultAsync<Contact>("SELECT * FROM Contacts WHERE Id = @Id AND isDeleted = 0", parameters);
            return contact;
        }


        public async Task AddContactAsync(Contact contact)
        {
            var query = "INSERT INTO Contacts (Name, Email, City, Skills) VALUES (@Name, @Email, @City, @Skills)";
            var parameters = new { contact.Name, contact.Email, contact.City, contact.Skills };
            await _context.ExecuteAsync(query, parameters);
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            var query = "UPDATE Contacts SET Name = @Name, Email = @Email, City = @City, Skills = @Skills WHERE Id = @Id";
            var parameters = new { contact.Name, contact.Email, contact.City, contact.Skills, contact.Id };
            await _context.ExecuteAsync(query, parameters);
        }

        public async Task DeleteContactAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            var query = "UPDATE Contacts SET isDeleted = 1 WHERE Id = @Id";
            await _context.ExecuteAsync(query, parameters);
        }

        public async Task<IEnumerable<Contact>> GetDeletedContactsAsync()
        {
            var deletedContacts = await _context.QueryAsync<Contact>("SELECT * FROM Contacts WHERE isDeleted = 1");
            return deletedContacts;
        }

        public async Task RestoreContactAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            var query = "UPDATE Contacts SET isDeleted = 0 WHERE Id = @Id";
            await _context.ExecuteAsync(query, parameters);
        }


    }
}
