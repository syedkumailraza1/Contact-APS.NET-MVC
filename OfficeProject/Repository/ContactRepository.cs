using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using DocumentFormat.OpenXml.InkML;
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
            var contacts = await _context.QueryAsync<Contact>("GetContacts", commandType: CommandType.StoredProcedure);
            return contacts;
        }

        public async Task<Contact> GetContactByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            var contact = await _context.QueryFirstOrDefaultAsync<Contact>("GetContactById", parameters, commandType: CommandType.StoredProcedure);
            return contact;
        }

        public async Task AddContactAsync(Contact contact)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", contact.Name, DbType.String);
            parameters.Add("@Email", contact.Email, DbType.String);
            parameters.Add("@City", contact.City, DbType.String);
            parameters.Add("@Skills", contact.Skills, DbType.String);

            await _context.ExecuteAsync("InsertContact", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", contact.Id, DbType.Int32);
            parameters.Add("@Name", contact.Name, DbType.String);
            parameters.Add("@Email", contact.Email, DbType.String);
            parameters.Add("@City", contact.City, DbType.String);
            parameters.Add("@Skills", contact.Skills, DbType.String);

            await _context.ExecuteAsync("UpdateContact", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteContactAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            await _context.ExecuteAsync("DeleteContact", parameters, commandType: CommandType.StoredProcedure);
        }


        public async Task<IEnumerable<Contact>> GetDeletedContactsAsync()
        {
            var deletedContacts = await _context.QueryAsync<Contact>("GetDeletedContacts", commandType: CommandType.StoredProcedure);
            return deletedContacts;
        }

        public async Task RestoreContactAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            await _context.ExecuteAsync("RestoreContact", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
