using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tvl.BusinessLayer.Interfaces.UserManagement;
using Tvl.Models.UserRegistration;
using Tvl.Entities;
using System.Linq;


namespace Tvl.Web.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    public class UserManagementController : Controller
    {

        private readonly IUserRegistration userRegistration;

        private readonly ILogger<UserManagementController> logger;

        private readonly IGetUserGroupList getUserGroupList;

        private readonly IGetUserGroupValues getUserGroupValues;

        private readonly ICheckEmailForValidDomains checkEmailForValidDomains;

        private readonly IGetAllUsers getAllUsers;

        private readonly AppDbContext appDbContext;


        public UserManagementController(ILogger<UserManagementController> logger, IUserRegistration userRegistration, IGetUserGroupList getUserGroupList,IGetUserGroupValues getUserGroupValues, ICheckEmailForValidDomains checkEmailForValidDomains,IGetAllUsers getAllUsers, AppDbContext appDbContext)
        {
            this.logger = logger;
            this.userRegistration = userRegistration;
            this.getUserGroupList = getUserGroupList;
            this.getUserGroupValues = getUserGroupValues;
            this.checkEmailForValidDomains = checkEmailForValidDomains;
            this.getAllUsers = getAllUsers;
            this.appDbContext = appDbContext;
        }

        public IActionResult Index()
        {

            return RedirectToAction("UserManagement");
        }

        public IActionResult CreateUser()
        {
            ViewBag.UserGroupList = getUserGroupList.GetUserGroup();
            return PartialView("~/Views/UserManagement/_CreateUser.cshtml");
        }
        

        public IActionResult UserManagement()
        { 
            ViewData["flag"] = TempData["flag"];
            TempData["flag"] = 0;
            UserListViewModel users = new UserListViewModel();
            users.UserList = getAllUsers.GetUserList();
            for (int i = 0; i < users.UserList.Count; i++)
            {
                users.UserList[i].UserGroup = new UserGroup
                {
                    UserGroupName = appDbContext.UserGroup.Where(x => x.UserGroupId == users.UserList[i].UserGroupId).Select(x => x.UserGroupName).Single()
                };
            }
            return View(users);
        }


        [HttpPost]
        [Route("registeruser")]
        public async Task<IActionResult> RegisterUser(UserRegistrationInputModel userRegistrationInputModel)
        {
            IActionResult actionResult = null;
            TempData["flag"] = 0;
          
            try
            {
                if (ModelState.IsValid)
                {
                    bool IsDomainValid = checkEmailForValidDomains.CheckValidity(userRegistrationInputModel.EmailAddress);

                    if(!IsDomainValid)
                    {
                        ViewBag.EmailMessage = "Enter Valid Email Domain";
                        ViewBag.UserGroupList = getUserGroupList.GetUserGroup();
                        return PartialView("~/Views/UserManagement/_CreateUser.cshtml");
                    }
                    userRegistrationInputModel = await getUserGroupValues.GetValues(userRegistrationInputModel);
                    string responseMessage = await userRegistration.RegisterUser(userRegistrationInputModel);

                    if (string.IsNullOrWhiteSpace(responseMessage))
                    {
                        actionResult = Ok("Successfully registered the user.");
                        TempData["flag"] = 1;
                    }
                    else
                    {
                        actionResult = BadRequest(responseMessage);
                        TempData["flag"] = -1;
                    }
                }
                else
                {
                    ViewBag.UserGroupList = getUserGroupList.GetUserGroup();
                    actionResult = BadRequest("Unable to register the user.");
                    return PartialView("~/Views/UserManagement/_CreateUser.cshtml");
                    
                }
            }
            catch (Exception e)
            {
                TempData["flag"] = -1;
                actionResult = BadRequest($"User Creation Failed: {e.Message}");
                logger.LogError($"User Creation Failed: {e.Message} {e.StackTrace}");
              
            }

            return RedirectToAction("UserManagement");
        }
    }
}
