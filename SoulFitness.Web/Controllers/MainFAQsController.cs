using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Globalization;
using SoulFitness.DataObjects.Data;
using System.IO;
using SoulFitness.DataObjects;
using Microsoft.AspNetCore.Identity;
using SoulFitness.DataObjects.UserManagment.Identity;
using System.Security.Claims;
using SoulFitness.Abstractions;

namespace SoulFitness.Web.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MainFAQsController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public MainFAQsController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FAQ>>> GetFAQs()
        {
            var faqs = await applicationDbContext.FAQs.Where(e => e.Status == Status.Active).ToListAsync();
            return Ok(faqs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FAQ>> Details(long id)
        {
            var faq = await applicationDbContext.FAQs.FirstOrDefaultAsync(m => m.Id == id);
            if (faq == null) return NotFound();
            return Ok(faq);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FAQ faq)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

            faq.Status = Status.Active;
            faq.CreatedAt = DateTime.Now;
            faq.CreatedBy = loggedInUser?.Email ?? "System";

            applicationDbContext.Add(faq);
            await applicationDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(Details), new { id = faq.Id }, faq);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(long id, [FromBody] FAQ faq)
        {
            if (id != faq.Id) return BadRequest("ID mismatch.");

            if (ModelState.IsValid)
            {
                try
                {
                    var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

                    faq.UpdatedAt = DateTime.Now;
                    faq.UpdatedBy = loggedInUser?.Email;

                    applicationDbContext.Update(faq);
                    await applicationDbContext.SaveChangesAsync();
                    return Ok(new { Message = "FAQ updated successfully." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!applicationDbContext.FAQs.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var faq = await applicationDbContext.FAQs.FindAsync(id);
            if (faq == null) return NotFound();

            faq.Status = Status.Inactive;
            await applicationDbContext.SaveChangesAsync();
            return Ok(new { Message = "FAQ deactivated." });
        }
    }
}
