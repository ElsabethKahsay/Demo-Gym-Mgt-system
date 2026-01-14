using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoulFitness.DataObjects;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Type = SoulFitness.DataObjects.Types;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;
using System.IO.Pipes;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using Path = System.IO.Path;
using Microsoft.AspNetCore.Http;
using SoulFitness.DataObjects.ViewModels;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Policy;

namespace SoulFitness.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortalMgtController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> userManager;

        public PortalMgtController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
            this._webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortalManagement>>> Index()
        {
            var newsAndFeed = await applicationDbContext.PortalManagement.Where(e => e.Status == Status.Active).ToListAsync();
            return Ok(newsAndFeed);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PortalManagement>> Details(int id)
        {
            var portal = await applicationDbContext.PortalManagement.FirstOrDefaultAsync(m => m.Id == id);
            if (portal == null) return NotFound();
            return Ok(portal);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromForm] PortalManagementCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await applicationDbContext.PortalManagement.AnyAsync(c => c.Title == request.Title))
            {
                return BadRequest("A post with this title already exists.");
            }

            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

            string imageUrl = null;
            if (request.ImgLocation != null && request.ImgLocation.Count > 0)
            {
                var file = request.ImgLocation[0];
                var uploads = Path.Combine(_webHostEnvironment.ContentRootPath, "FileUploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                imageUrl = $"{Request.Scheme}://{Request.Host}/FileUploads/{fileName}";
            }

            var portalManagement = new PortalManagement
            {
                Title = request.Title,
                Text = request.Text,
                Type = request.Type,
                Priority = request.Priority,
                ExpirationDate = request.ExpirationDate,
                ImgLocation = imageUrl,
                Status = Status.Active,
                CreatedAt = DateTime.Now,
                CreatedBy = loggedInUser?.Email ?? "System"
            };

            applicationDbContext.PortalManagement.Add(portalManagement);
            await applicationDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Details), new { id = portalManagement.Id }, portalManagement);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] PortalManagement portal)
        {
            if (id != portal.Id) return BadRequest("ID mismatch.");

            if (ModelState.IsValid)
            {
                try
                {
                    var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

                    portal.UpdatedAt = DateTime.Now;
                    portal.UpdatedBy = loggedInUser?.Email;
                    applicationDbContext.Update(portal);
                    await applicationDbContext.SaveChangesAsync();
                    return Ok(new { Message = "Post updated successfully." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!applicationDbContext.PortalManagement.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var portal = await applicationDbContext.PortalManagement.FindAsync(id);
            if (portal == null) return NotFound();

            portal.Status = Status.Inactive;
            await applicationDbContext.SaveChangesAsync();
            return Ok(new { Message = "Post deactivated." });
        }
    }
}


