using System.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeProject.Models;
using Dapper;

namespace OfficeProject.Controllers
{
    public class ContactController : Controller
    {
        private readonly IDbConnection _context;

        public ContactController(IDbConnection context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var contacts = _context.Query<Contact>("GetContact", null, commandType: CommandType.StoredProcedure);
            return View(contacts);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Contact contact)
        {
            DynamicParameters parameters = new();
            parameters.Add("@Name", contact.name, DbType.String);
            parameters.Add("@Email", contact.email, DbType.String);
            parameters.Add("@City", contact.city, DbType.String);
            parameters.Add("@Skill", contact.skills, DbType.String);
            _context.Execute("InsertContact", parameters, commandType: CommandType.StoredProcedure);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            DynamicParameters parameters = new();
            parameters.Add("@Id", id, DbType.Int32);
            var contact = _context.QueryFirstOrDefault<Contact>("GetById", parameters, commandType: CommandType.StoredProcedure);
            return View(contact);
        }

        [HttpPost]
        public IActionResult Edit(Contact contact)
        {
            DynamicParameters parameters = new();
            parameters.Add("@Id", contact.Id, DbType.Int32);
            parameters.Add("@Name", contact.name, DbType.String);
            parameters.Add("@Email", contact.email, DbType.String);
            parameters.Add("@City", contact.city, DbType.String);
            parameters.Add("@Skill", contact.skills, DbType.String);
            _context.QueryFirstOrDefault<Contact>("UpdateContact", parameters, commandType: CommandType.StoredProcedure);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            DynamicParameters parameters = new();
            parameters.Add("@Id", id, DbType.Int32);
            _context.Execute("DeleteContact", parameters, commandType: CommandType.StoredProcedure);
            return RedirectToAction("Index");
        }
    }
}
