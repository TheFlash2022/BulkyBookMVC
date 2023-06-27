

using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

//namespace BulkyBookWeb.Areas.Admin.Controllers
//{
//	[Area("Admin")]
//	[Authorize(Roles = SD.Role_Admin)]
//	public class UserController : Controller
//	{
//		private readonly ApplicationDbContext _db;
//		private readonly UserManager<IdentityUser> _userManagment;

//		public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
//		{
//			_db = db;
//			_userManagment = userManager;
//		}
//		public IActionResult Index()
//		{
//			return View();
//		}

//		public IActionResult RoleManagment(string userId)
//		{
//			string RoleID = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

//			RoleManagmentVM RoleVM = new RoleManagmentVM()
//			{
//				ApplicationUser = _db.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId),
//				RoleList = _db.Roles.Select(i => new SelectListItem
//				{
//					Text = i.Name,
//					Value = i.Name
//				}),
//				CompanyList = _db.Companies.Select(i => new SelectListItem
//				{
//					Text = i.Name,
//					Value = i.Id.ToString()
//				}),
//			};

//			RoleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == RoleID).Name;

//			return View(RoleVM);
//		}

//		[HttpPost]
//		public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
//		{
//			string RoleID = _db.UserRoles.FirstOrDefault(u => u.UserId == roleManagmentVM.ApplicationUser.Id).RoleId;
//			string oldRole = _db.Roles.FirstOrDefault(u => u.Id == RoleID).Name;

//			if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
//			{
//				//a role was updated
//				ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVM.ApplicationUser.Id);

//				if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company)
//				{
//					applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
//				}
//				if (oldRole == SD.Role_Company)
//				{
//					applicationUser.CompanyId = null;
//				}
//				_db.SaveChanges();

//				_userManagment.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
//				_userManagment.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();
//			}


//			return RedirectToAction("Index");
//		}

//		#region API Calls
//		[HttpGet]
//		public IActionResult GetAll()
//		{
//			List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u => u.Company).ToList();

//			var userRoles = _db.UserRoles.ToList();
//			var roles = _db.Roles.ToList();

//			foreach (var user in objUserList)
//			{
//				var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
//				user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

//				if (user.Company == null)
//				{
//					user.Company = new() { Name = "" };
//				}
//			}
//			return Json(new { data = objUserList });
//		}

//		[HttpPost]
//		public IActionResult LockUnlock([FromBody] string id)
//		{
//			var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
//			if (objFromDb == null)
//			{
//				return Json(new { success = false, message = "Error while Locking/Unlocking" });
//			}

//			if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
//			{
//				//user is currenty locked and we need to unlock them
//				objFromDb.LockoutEnd = DateTime.Now;
//			}
//			else
//			{
//				objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
//			}
//			_db.SaveChanges();
//			return Json(new { success = true, message = "Operation successfullllll" });
//		}
//		#endregion

//	}
//}



namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class UserController : Controller
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IUnitOfWork _unitOfWork;
		public UserController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager)
		{
			_unitOfWork = unitOfWork;
			_roleManager = roleManager;
			_userManager = userManager;
		}
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult RoleManagment(string userId)
		{

			RoleManagmentVM RoleVM = new RoleManagmentVM()
			{
				ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company"),
				RoleList = _roleManager.Roles.Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.Name
				}),
				CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.Id.ToString()
				}),
			};

			RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == userId))
					.GetAwaiter().GetResult().FirstOrDefault();
			return View(RoleVM);
		}

		[HttpPost]
		public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
		{

			string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id))
					.GetAwaiter().GetResult().FirstOrDefault();

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);


			if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
			{
				//a role was updated
				if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company)
				{
					applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
				}
				if (oldRole == SD.Role_Company)
				{
					applicationUser.CompanyId = null;
				}
				_unitOfWork.ApplicationUser.Update(applicationUser);
				_unitOfWork.Save();

				_userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
				_userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();

			}
			else
			{
				if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId)
				{
					applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
					_unitOfWork.ApplicationUser.Update(applicationUser);
					_unitOfWork.Save();
				}
			}

			return RedirectToAction("Index");
		}


		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
		{
			List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();

			foreach (var user in objUserList)
			{

				user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

				if (user.Company == null)
				{
					user.Company = new Company()
					{
						Name = ""
					};
				}
			}

			return Json(new { data = objUserList });
		}


		[HttpPost]
		public IActionResult LockUnlock([FromBody] string id)
		{

			var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
			if (objFromDb == null)
			{
				return Json(new { success = false, message = "Error while Locking/Unlocking" });
			}

			if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
			{
				//user is currently locked and we need to unlock them
				objFromDb.LockoutEnd = DateTime.Now;
			}
			else
			{
				objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
			}
			_unitOfWork.ApplicationUser.Update(objFromDb);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Operation Successful" });
		}

		#endregion
	}
}