using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeProject.Data;
using OfficeProject.Models;

namespace OfficeProject.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactRepository _repository;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IContactRepository repository, ILogger<ContactController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchTerm, string sortOrder, int pg = 1)
        {
            var contacts = (await _repository.GetContactsAsync()).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                contacts = contacts.Where(c => c.Name.Contains(searchTerm) ||
                                                c.Email.Contains(searchTerm) ||
                                                c.City.Contains(searchTerm) ||
                                                c.Skills.Contains(searchTerm)).ToList();
            }

            // Sorting logic
            switch (sortOrder)
            {
                case "name_desc":
                    contacts = contacts.OrderByDescending(a => a.Name).ToList();
                    break;
                case "email_asc":
                    contacts = contacts.OrderBy(a => a.Email).ToList();
                    break;
                case "email_desc":
                    contacts = contacts.OrderByDescending(a => a.Email).ToList();
                    break;
                case "city_asc":
                    contacts = contacts.OrderBy(a => a.City).ToList();
                    break;
                case "city_desc":
                    contacts = contacts.OrderByDescending(a => a.City).ToList();
                    break;
                default:
                    contacts = contacts.OrderBy(a => a.Name).ToList();
                    break;
            }


            const int pageSize = 5;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = contacts.Count;
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = contacts.Skip(recSkip).Take(pageSize).ToList();
            this.ViewBag.Pager = pager;

            ViewData["CurrentSort"] = sortOrder;

            return View(data);
        }

        public async Task<IActionResult> Excel()
        {
            var contacts = await _repository.GetContactsAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Contacts");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Id";
                worksheet.Cell(currentRow, 2).Value = "Name";
                worksheet.Cell(currentRow, 3).Value = "Email";
                worksheet.Cell(currentRow, 4).Value = "City";
                worksheet.Cell(currentRow, 5).Value = "Skills";

                foreach (var contact in contacts)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = contact.Id;
                    worksheet.Cell(currentRow, 2).Value = contact.Name;
                    worksheet.Cell(currentRow, 3).Value = contact.Email;
                    worksheet.Cell(currentRow, 4).Value = contact.City;
                    worksheet.Cell(currentRow, 5).Value = contact.Skills;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "contacts.xlsx");
                }
            }
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

                    await _repository.AddContactAsync(contact);

                    _logger.LogInformation("Contact successfully created in the database.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while inserting contact into database.");
                    ModelState.AddModelError("", "An error occurred while saving the contact. Please try again.");
                }
            }

            _logger.LogWarning("Model state invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
            return View(contact);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var contact = await _repository.GetContactByIdAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Edit(Contact contact, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var selectedSkills = form["Skills"].ToList();
                    contact.Skills = string.Join(",", selectedSkills);

                    await _repository.UpdateContactAsync(contact);

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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _repository.DeleteContactAsync(id);

                _logger.LogInformation("Contact successfully marked as deleted in the database.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting contact from database.");
                return BadRequest();
            }
        }

        public async Task<IActionResult> DeletedContacts()
        {
            var deletedContacts = (await _repository.GetDeletedContactsAsync()).ToList();
            return View(deletedContacts);
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                await _repository.RestoreContactAsync(id);

                _logger.LogInformation("Contact successfully restored.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while restoring contact.");
                return BadRequest();
            }
        }

        private List<string> GetSkills()
        {
            return new List<string> { "C#", "JavaScript", "Java", "Python" };
        }
    }
}
