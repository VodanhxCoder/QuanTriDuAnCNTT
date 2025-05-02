using BELibrary.Core.Entity;
using BELibrary.Core.Utils;
using BELibrary.DbContext;
using HospitalManagement.Areas.Admin.Services;
using HospitalManagement.Core.Dto;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HospitalManagement.Controllers
{
    public class ChatController : BaseController
    {
        // GET: Admin/Chat
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SendMessage(Guid? groupId, MessageDto message)
        {
            try
            {
                var user = GetCurrentUser();

                ChatBoardService chatBoardService = new ChatBoardService(new HospitalManagementDbContext());
                var groupIdResult = chatBoardService.SendMessage(user.Id, groupId ?? Guid.Empty, message);

                return Json(new { status = true, groupId = groupIdResult });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, mess = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetChatHistories()
        {
            ResponseAPI responseApi = new ResponseAPI();
            try
            {
                var user = GetCurrentUser();
                ChatBoardService chatBoardService = new ChatBoardService(new HospitalManagementDbContext());
                responseApi.Data = chatBoardService.GetHistory(user.Id);
                return Json(responseApi, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                responseApi.Message = ex.Message;
                return Json(responseApi, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetInfo(Guid? groupId, Guid? contactId)
        {
            ResponseAPI responseApi = new ResponseAPI();
            try
            {
                ChatBoardService chatBoardService = new ChatBoardService(new HospitalManagementDbContext());
                var user = GetCurrentUser();
                responseApi.Data = chatBoardService.GetInfo(user.Id, groupId ?? Guid.Empty, contactId ?? Guid.Empty);
                return Json(responseApi, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                responseApi.Message = ex.Message;
                return Json(responseApi, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetMessageByGroup(Guid? groupId, Guid? contactId)
        {
            ResponseAPI responseApi = new ResponseAPI();
            try
            {
                ChatBoardService chatBoardService = new ChatBoardService(new HospitalManagementDbContext());
                var user = GetCurrentUser();
                if (groupId != null)
                {
                    responseApi.Data = chatBoardService.GetMessageByGroup(user.Id, groupId ?? Guid.Empty);
                    return Json(responseApi, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    responseApi.Data = chatBoardService.GetMessageByContact(user.Id, contactId ?? Guid.Empty);
                    return Json(responseApi, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                responseApi.Message = ex.Message;
                return Json(responseApi, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetContacts()
        {
            ResponseAPI responseApi = new ResponseAPI();
            try
            {
                using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
                { 
                    var users = workScope.Accounts.GetAll(); 
                    users = users.Where(x => x.Role == RoleKey.Doctor); 
                    responseApi.Data = users;
                    return Json(responseApi, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                responseApi.Message = ex.Message;
                return Json(responseApi, JsonRequestBehavior.AllowGet);
            }
        }
    }
}