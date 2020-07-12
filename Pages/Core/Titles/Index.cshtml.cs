﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MM.CoreModels;

namespace MM.Pages.Core.Titles
{
    public class IndexModel : PageModel
    {
        private readonly CoreDBContext _context;

        public IndexModel(CoreDBContext context)
        {
            _context = context;
        }

        public IList<CoreTitle> TitleList { get;set; }

        [BindProperty]
        public CoreTitle Title { get; set; }
    
        public async Task<IActionResult>  OnGetSelectedRecordAsync(int id)
        {
            return new JsonResult(await _context.Title.Where(x=>x.Id==id).FirstOrDefaultAsync());
        }
    
        public async Task<IActionResult> OnGetListAsync()
        {
            return  new JsonResult(await _context.Title.ToListAsync());
        }

        public async Task<IActionResult> OnPostSaveAsync(CoreTitle title)
        {

            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Error. Please check values entered" });
            }

            if (title.Id > 0)
            {
                _context.Attach(title).State = EntityState.Modified;
            }
            else
            {
                _context.Title.Add(title);
            }
             await _context.SaveChangesAsync();
            return new JsonResult( new { success = true, message = "Saved successfully" });
        }

         public async Task<IActionResult> OnGetDeleteAsync(int? id)
        {

            if (id == null)
            {
                return new JsonResult(new { success = false, message = "No such reccord found to delete" });
            }

            Title = await _context.Title.FindAsync(id);

            if (Title != null)
            {
                _context.Title.Remove(Title);
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, message = "Deleted successfully" });
            }
            return new JsonResult(new { success = false, message = "No such reccord found to delete" });

        }
    }
}