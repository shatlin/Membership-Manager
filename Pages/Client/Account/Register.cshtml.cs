﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MM.ClientModels;
using MM.CoreModels;
using MM.Helper;
namespace MM.Pages.Client.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly ApplicationSignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private  UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly CoreDBContext _coreDbConext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RegisterModel(
            ApplicationSignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
            IEmailSender emailSender, CoreDBContext coreDbConex, IHttpContextAccessor httpContextAccessor)
        {
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _coreDbConext = coreDbConex;
            _roleManager = roleManager;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public ClientUser ClientUser { get; set; }

        [BindProperty]
        public ApplicationUser AppUser { get; set; }

        public class AppUserInputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        [ViewData]
        public SelectList ClientTitles { get; set; }

        [ViewData]
        public SelectList ClientDesignations { get; set; }

        [ViewData]
        public SelectList ClientGenders { get; set; }

        [ViewData]
        public SelectList ClientTypes { get; set; }

        [ViewData]
        public SelectList ClientReferralTypes { get; set; }

        [BindProperty]
        public ClientOrganization ClientOrganization { get; set; }

        [ViewData]
        public SelectList OrgDateSettings { get; set; }

        [ViewData]
        public SelectList OrgTimeFormat { get; set; }

        [ViewData]
        public SelectList OrgTimeZone { get; set; }

        [ViewData]
        public SelectList OrgCurrency { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            OrgDateSettings = new SelectList(_coreDbConext.CoreDateSetting, nameof(CoreDateSetting.Id), nameof(CoreDateSetting.Name));
            OrgTimeFormat = new SelectList(_coreDbConext.CoreTimeFormat, nameof(CoreTimeFormat.Id), nameof(CoreTimeFormat.Name));
            OrgTimeZone = new SelectList(_coreDbConext.CoreTimeZone, nameof(CoreTimeZone.Id), nameof(CoreTimeZone.Description));
            OrgCurrency = new SelectList(_coreDbConext.CoreCurrency, nameof(CoreCurrency.Id), nameof(CoreCurrency.Name));
            ClientTitles = new SelectList(_coreDbConext.CoreTitle, nameof(CoreTitle.Id), nameof(CoreTitle.Name));
            ClientDesignations = new SelectList(_coreDbConext.CoreDesignation, nameof(CoreDesignation.Id), nameof(CoreDesignation.Name));
            ClientGenders = new SelectList(_coreDbConext.CoreGender, nameof(CoreGender.Id), nameof(CoreGender.Name));
            ClientReferralTypes = new SelectList(_coreDbConext.CoreReferralType, nameof(CoreReferralType.Id), nameof(CoreReferralType.Name));
            ClientTypes = new SelectList(_coreDbConext.CoreClientType, nameof(CoreClientType.Id), nameof(CoreClientType.Name));
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        //public async Task<IActionResult> SeedRoles()
        //{
           
        //}

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                string ConnectionString = _coreDbConext.TenantConfig.FirstOrDefault().ConnectionString + ClientOrganization.Name.ToLower();
                Tenant tenant = _coreDbConext.Tenant.Where(x => x.ClientName.ToUpper() == ClientOrganization.Name.ToUpper()).FirstOrDefault();
                if (tenant == null)
                {
                    tenant = new Tenant();
                    tenant.ClientName = ClientOrganization.Name;
                    tenant.DbName = "mm_" + ClientOrganization.Name;
                    tenant.ConnectionString = ConnectionString;
                    _coreDbConext.Tenant.Add(tenant);
                    await _coreDbConext.SaveChangesAsync();

                    var tenantuserexists = _coreDbConext.TenantUserTenant.Any(x => x.Email == AppUser.Email);

                    if (!tenantuserexists)
                    {
                        TenantUserTenant tenantUserTenant = new TenantUserTenant();
                        tenantUserTenant.Email = AppUser.Email;
                        tenantUserTenant.TenantId = tenant.Id;
                        _coreDbConext.TenantUserTenant.Add(tenantUserTenant);
                        await _coreDbConext.SaveChangesAsync();
                    }
                }

                ClientDbContext clientDbContext = new ClientDbContext(tenant);
                clientDbContext.Database.EnsureCreated();
                AppUser.UserName = AppUser.Email;
                AppUser.UserTypeId = 1;
                var userManager = new IdentityHelper().GetUserManager(clientDbContext); // #TODO - This will fail in confirming the email sent. To be fixed later
             
                var result = await userManager.CreateAsync(AppUser, AppUser.Pwd);
                if (result.Succeeded)
                {
                    clientDbContext.ClientOrganization.Add(ClientOrganization);

                    ClientUser.ApplicaitonUserId = AppUser.Id;
                    clientDbContext.ClientUser.Add(ClientUser);
                    await clientDbContext.SaveChangesAsync();
                    var roleManager = new IdentityHelper().GetRoleManager(clientDbContext);

                    var adminfullAccessRole = new ApplicationRole("Admin-Full Access");
                    await roleManager.CreateAsync(adminfullAccessRole);
                    var adminreadAccessRole = new ApplicationRole("Admin-Read Access");
                    await roleManager.CreateAsync(adminreadAccessRole);
                    var noadminAccessRole = new ApplicationRole("No Admin Access");
                    await roleManager.CreateAsync(noadminAccessRole);
                    var limitedadminAccessRole = new ApplicationRole("Limited Admin");
                    await roleManager.CreateAsync(limitedadminAccessRole);
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Members", "Create"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Members", "Edit"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Members", "Delete"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Members", "View"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Event", "Create"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Event", "Edit"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Event", "Delete"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Event", "View"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Donations", "Create"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Donations", "Edit"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Donations", "Delete"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Donations", "View"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("NewLetter", "Create"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("NewLetter", "Edit"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("NewLetter", "Delete"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("NewLetter", "View"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Setup", "Create"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Setup", "Edit"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Setup", "Delete"));
                    await roleManager.AddClaimAsync(adminfullAccessRole, new Claim("Setup", "View"));
                    await roleManager.AddClaimAsync(adminreadAccessRole, new Claim("Members", "View"));
                    await roleManager.AddClaimAsync(adminreadAccessRole, new Claim("Event", "View"));
                    await roleManager.AddClaimAsync(adminreadAccessRole, new Claim("Donations", "View"));
                    await roleManager.AddClaimAsync(adminreadAccessRole, new Claim("NewLetter", "View"));
                    await roleManager.AddClaimAsync(adminreadAccessRole, new Claim("Setup", "View"));
                    await userManager.AddToRoleAsync(AppUser, "Admin-Full Access");
                    _logger.LogInformation("User created a new account with password.");
                    var host = _httpContextAccessor.HttpContext.Request.Host.Value;
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(AppUser);
                    code = HttpUtility.UrlEncode(code);
                    string callbackUrl = Url.Page(
                        "/Client/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = AppUser.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                  callbackUrl = callbackUrl.Replace(host, ClientOrganization.Name + "." + host);

                    await _emailSender.SendEmailAsync(AppUser.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = AppUser.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                       RedirectResult redirectResult = new RedirectResult("https://" + ClientOrganization.Name + "." + host + "/Client/Account/Login", true);
                       return redirectResult;
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}

