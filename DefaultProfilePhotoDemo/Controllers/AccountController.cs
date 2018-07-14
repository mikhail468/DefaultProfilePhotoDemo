using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DefaultProfilePhotoDemo.ViewModels;
using Microsoft.AspNetCore.Identity;
using DefaultProfilePhotoDemo.Models;
using Microsoft.AspNetCore.Hosting;
using DefaultProfilePhotoDemo.Services;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace DefaultProfilePhotoDemo.Controllers
{
    public class AccountController : Controller
    {
        #region - inject services
        private UserManager<IdentityUser> _userManagerService;
        private SignInManager<IdentityUser> _signInManagerService;
        private IDataService<Profile> _profileDataService;
        private IHostingEnvironment _hostingEnvironment;

        public AccountController(UserManager<IdentityUser> userManagerService,
                                SignInManager<IdentityUser> signInManagerService,
                                IDataService<Profile> profileDataService,
                                IHostingEnvironment hostingEnvironment)
        {
            _userManagerService = userManagerService;
            _signInManagerService = signInManagerService;
            _profileDataService = profileDataService;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region - register [Account/Register]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(AccountRegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = new IdentityUser(vm.UserName);
                IdentityResult result =await _userManagerService.CreateAsync(user, vm.Password);
                if (result.Succeeded)
                {
                    //get the user back to find the id
                    user = await _userManagerService.FindByNameAsync(vm.UserName);

                    //add empty profile - customer will update it later
                    Profile profile = new Profile
                    {
                        FullName = "",
                        UserId = user.Id
                    };

                    //upload server path
                    var serverPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                    //create file name [format MUST be userName.gif -->easy to show in navbar]
                    string fileName = vm.UserName + ".gif";
                    //stream the file to the server
                    using (var filestream = new FileStream(
                        Path.Combine(serverPath, "profilephoto", fileName)
                        , FileMode.Create)
                        )
                    {
                        //create stream for default photo
                        using (var defaultPhoto = new FileStream(Path.Combine(serverPath, "profilephoto", "Default.gif"), FileMode.Open))
                            //set default photo as user profile photo
                            await defaultPhoto.CopyToAsync(filestream);
                    }
                    //assign the picture URL to the category object
                    profile.PhotoUrl = "profilephoto/" + fileName;
                    //save profile in database
                    _profileDataService.Create(profile);
                    //return to home page
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                
            }

            return View(vm);

        }
        #endregion

        #region - update profile
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {

            //retrieve user information who is login
            IdentityUser user = await _userManagerService.FindByNameAsync(User.Identity.Name);
            Profile profile = await _profileDataService.GetSingle(p => p.UserId == user.Id);

            UpdateProfileViewModel vm = new UpdateProfileViewModel
            {
                FullName=profile.FullName,
                PhotoUrl = profile.PhotoUrl
            };
            return View(vm);


        }
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel vm, IFormFile file)
        {

                //retrieve user information who is login
                IdentityUser user = await _userManagerService.FindByNameAsync(User.Identity.Name);
                Profile profile = await _profileDataService.GetSingle(p => p.UserId == user.Id);


                if (ModelState.IsValid)
                {
                    profile.FullName = vm.FullName;

                    if (file != null)
                    {
                        //upload server path
                        var serverPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                        //create a folder

                        //Directory.CreateDirectory(Path.Combine(serverPath, User.Identity.Name));
                        //get the file name
                        string fileName = User.Identity.Name + ".gif";
                        //stream the file to the server
                        using (var filestream = new FileStream(
                            Path.Combine(serverPath, "profilephoto", fileName)
                            , FileMode.Create)
                            )
                        {
                            //copy upload picture to stream
                            await file.CopyToAsync(filestream);
                        }
                        //assign the picture URL to the category object
                        profile.PhotoUrl = "profilephoto/" + fileName;
                    }
                    //change data in database
                    _profileDataService.Update(profile);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(vm);
                }
        }

        #endregion

        #region - Login [Account/Login]
        [HttpGet]
        public IActionResult Login(string returnUrl = "/Home/Index")
        {
            AccountLoginViewModel vm = new AccountLoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManagerService.PasswordSignInAsync(vm.UserName, vm.Password, true, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(vm.ReturnUrl))
                    {
                        return Redirect(vm.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Username or password incorrect");
                }
            }
            return View(vm);
        }
        #endregion

        #region - Logout [Account/Logout]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManagerService.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion

    }
}