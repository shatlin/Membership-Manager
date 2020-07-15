﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MM.ClientModels;

namespace MM.Pages.Client
{
    public class PlanFrequencyModel : PageModel
    {
        private readonly ClientDbContext _context;

        public PlanFrequencyModel(ClientDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public IList<PlanFrequency> PlanFrequencyList { get;set; }

        [BindProperty]
        public PlanFrequency PlanFrequency { get; set; }

        public async Task<IActionResult> OnGetListAsync()
        {
            return new JsonResult(await _context.PlanFrequency.ToListAsync());
        }

        public async Task<IActionResult>  OnGetSelectedRecordAsync(int id)
        {
            return new JsonResult(await _context.PlanFrequency.Where(x=>x.Id==id).FirstOrDefaultAsync());
        }
    
 
        public async Task<IActionResult> OnPostSaveAsync(PlanFrequency PlanFrequency)
        {

            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Error. Please check values entered" });
            }

            if (PlanFrequency.Id > 0)
            {
                _context.Attach(PlanFrequency).State = EntityState.Modified;
            }
            else
            {
                _context.PlanFrequency.Add(PlanFrequency);
            }
             await _context.SaveChangesAsync();
            return new JsonResult( new { success = true, message = "Saved successfully" });
        }

         public async Task<IActionResult> OnGetDeleteAsync(int? id)
        {

            if (id == null)
            {
                return new JsonResult(new { success = false, message = "No such record found to delete" });
            }

            PlanFrequency = await _context.PlanFrequency.FindAsync(id);

            if (PlanFrequency != null)
            {
                _context.PlanFrequency.Remove(PlanFrequency);
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, message = "Deleted successfully" });
            }
            return new JsonResult(new { success = false, message = "No such record found to delete" });

        }
    }
}
