using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dapper;
using OfficeProject.Models;
using Microsoft.AspNetCore.Http;

namespace OfficeProject.Controllers
{
    public class ContactController : Controller
    {
        private readonly IDbConnection _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IDbConnection context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(string searchTerm, string sortOrder, int pg = 1)
        {
            var contacts = _context.Query<Contact>("GetContacts", commandType: CommandType.StoredProcedure).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                contacts = contacts.Where(c => c.Name.Contains(searchTerm) ||
                                                c.Email.Contains(searchTerm) ||
                                                c.City.Contains(searchTerm) ||
                                                c.Skills.Contains(searchTerm)).ToList();
            }

            ViewData["NameOrder"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            switch (sortOrder)
            {
                case "name_desc":
                    contacts = contacts.OrderByDescending(a => a.Name).ToList();
                    break;
                default:
                    contacts = contacts.OrderBy(a => a.Name).ToList();
                    break;
            }

            const int pageSize = 5;
            if(pg < 1)
            {
                pg = 1;
            }
            int recsCount = contacts.Count;
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = contacts.Skip(recSkip).Take(pageSize).ToList();
            this.ViewBag.Pager = pager;

            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Received Contact: {@Contact}", contact);

                    // Process selected skills
                    var selectedSkills = contact.Skills; // Assuming Skills is a string of comma-separated values

                    // Insert into database
                    var query = "INSERT INTO Contacts (Name, Email, City, Skills) VALUES (@Name, @Email, @City, @Skills)";
                    var parameters = new { contact.Name, contact.Email, contact.City, Skills = selectedSkills };

                    _logger.LogInformation("Executing SQL query: {Query}", query);
                    await _context.ExecuteAsync(query, parameters);

                    _logger.LogInformation("Contact successfully created in the database.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while inserting contact into database.");
                    ModelState.AddModelError("", "An error occurred while saving the contact. Please try again.");
                }
            }

            // If ModelState is not valid, return to the view with validation errors
            _logger.LogWarning("Model state invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
            return View(contact);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            var contact = _context.QueryFirstOrDefault<Contact>("GetContactById", parameters, commandType: CommandType.StoredProcedure);

            if (contact == null)
            {
                return NotFound();
            }

            // Fetch all skills
            var allSkills = GetSkills();
            var contactViewModel = new Contact
            {
                Id = contact.Id,
                Name = contact.Name,
                Email = contact.Email,
                City = contact.City,
                Skills = contact.Skills
            };

            return View(contactViewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Contact contact, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var selectedSkills = form["Skills"].ToList();
                    var skillsString = string.Join(",", selectedSkills);

                    var query = "UPDATE Contacts SET Name = @Name, Email = @Email, City = @City, Skills = @Skills WHERE Id = @Id";
                    var parameters = new { contact.Name, contact.Email, contact.City, Skills = skillsString, contact.Id };
                    _context.Execute(query, parameters);

                    _logger.LogInformation("Contact successfully updated in the database.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating contact in database.");
                    ModelState.AddModelError("", "An error occurred while updating the contact. Please try again.");
                }
            }
            return View(contact);
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);
            _context.Execute("DeleteContact", parameters, commandType: CommandType.StoredProcedure);

            _logger.LogInformation("Contact successfully deleted from the database.");
            return RedirectToAction(nameof(Index));
        }

        private List<string> GetSkills()
        {
            return new List<string> { "C#", "JavaScript", "Java", "Python" };
        }
    }
}
