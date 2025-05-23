﻿using BELibrary.Core.Entity;
using BELibrary.Core.Utils;
using BELibrary.DbContext;
using BELibrary.Entity;
using HospitalManagement.Handler;
using HospitalManagement.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace HospitalManagement.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            if (!CookiesManage.Logined())
            {
                return RedirectToAction("Login", "Account");
            }

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var user = CookiesManage.GetUser();
                var patient = workScope.Patients.FirstOrDefault(x => x.Id == user.PatientId);
                ViewBag.Patient = patient;

                var detailRecord = workScope.PatientRecords
                    .Query(x => x.PatientId == user.PatientId)
                    .ToList();

                // Lấy danh sách lịch hẹn của bệnh nhân
                var bookings = workScope.DoctorSchedules
                    .Include(x => x.Doctor)
                    .Where(x => x.PatientId == user.PatientId)
                    .OrderByDescending(x => x.ScheduleBook)
                    .ToList();
                ViewBag.Bookings = bookings;

                return View(detailRecord);
            }
        }

        [HttpPost]
        public JsonResult CancelBooking(int bookingId)
        {
            if (!CookiesManage.Logined())
            {
                return Json(new { status = false, mess = "Vui lòng đăng nhập" });
            }

            try
            {
                var user = CookiesManage.GetUser();
                using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
                {
                    var booking = workScope.DoctorSchedules
                        .FirstOrDefault(x => x.Id == bookingId && x.PatientId == user.PatientId);
                    if (booking == null)
                    {
                        return Json(new { status = false, mess = "Lịch hẹn không tồn tại" });
                    }

                    if (booking.Status != BookingStatusKey.Pending)
                    {
                        return Json(new { status = false, mess = "Chỉ có thể hủy lịch hẹn đang chờ" });
                    }

                    booking.Status = BookingStatusKey.Reject;
                    workScope.DoctorSchedules.Put(booking, booking.Id);
                    workScope.Complete();

                    return Json(new { status = true, mess = "Hủy lịch hẹn thành công" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, mess = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public ActionResult PatientRecord(Guid id)
        {
            if (!CookiesManage.Logined())
            {
                return RedirectToAction("Login", "Account");
            }

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var user = CookiesManage.GetUser();
                var patient = workScope.Patients.FirstOrDefault(x => x.Id == user.PatientId);

                if (patient == null)
                    return RedirectToAction("E404", "Home");

                ViewBag.Patient = patient;

                var patientRecord = workScope.PatientRecords
                    .FirstOrDefault(x => x.Id == id);

                var detailRecords = workScope.DetailRecords.Query(x => x.RecordId == patientRecord.RecordId && !x.IsMainRecord).OrderByDescending(x => x.Process).ToList();

                ViewBag.DetailRecords = detailRecords;

                var record = workScope.Records.FirstOrDefault(x => x.Id == patientRecord.RecordId);

                var mainDetailRecord = workScope.DetailRecords.FirstOrDefault(x => x.RecordId == record.Id && x.IsMainRecord);

                var prescriptions = workScope.Prescriptions
                    .Include(x => x.DetailPrescription, x => x.DetailPrescription.Medicine)
                    .Where(x => x.DetailRecordId == mainDetailRecord.Id)
                    .ToList();

                ViewBag.Record = record;
                ViewBag.MainDetailRecord = mainDetailRecord;
                ViewBag.Prescriptions = prescriptions;

                return View(patientRecord);
            }
        }

        public ActionResult DetailRecord(Guid id)
        {
            if (!CookiesManage.Logined())
            {
                return RedirectToAction("Login", "Account");
            }

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var user = CookiesManage.GetUser();
                var patient = workScope.Patients.FirstOrDefault(x => x.Id == user.PatientId);

                if (patient == null)
                    return RedirectToAction("E404", "Home");

                ViewBag.Patient = patient;

                var detailRecord = workScope.DetailRecords
                    .FirstOrDefault(x => x.Id == id);

                return View(detailRecord);
            }
        }

        public ActionResult Edit()
        {
            if (!CookiesManage.Logined())
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var user = CookiesManage.GetUser();

                using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
                {
                    var account = workScope.Accounts.GetAll().Where(x => !x.IsDeleted && x.UserName.Trim().ToLower() == user.UserName.Trim().ToLower());
                    var patient = workScope.Patients.FirstOrDefault(x => x.Id == user.PatientId);
                    ViewBag.Patient = patient;
                    return View(account);
                }
            }
        }

        public ActionResult Login(string returnUrl = "")
        {
            if (CookiesManage.Logined())
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult Register(string returnUrl = "")
        {
            if (CookiesManage.Logined())
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Hàm xác minh reCAPTCHA
        private async Task<bool> VerifyRecaptcha(string recaptchaResponse)
        {
            var secretKey = "6LdovzArAAAAAFhhmcfG1ti-k7_l1nCWUHTxGah7"; //Secret Key 
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}",
                    null);
                var jsonString = await response.Content.ReadAsStringAsync();
                var captchaResult = JsonConvert.DeserializeObject<CaptchaResponse>(jsonString);
                return captchaResult.Success;
            }
        }

        private class CaptchaResponse
        {
            public bool Success { get; set; }
            public string[] ErrorCodes { get; set; }
        }

        [HttpPost]
        [ValidateInput(true)]
        public async Task<JsonResult> CheckLogin(LoginModel model, string gRecaptchaResponse)
        {
            // Xác minh reCAPTCHA
            if (string.IsNullOrEmpty(gRecaptchaResponse))
            {
                return Json(new { status = false, mess = "Vui lòng xác nhận bạn không phải là robot" });
            }

            var isCaptchaValid = await VerifyRecaptcha(gRecaptchaResponse);
            if (!isCaptchaValid)
            {
                return Json(new { status = false, mess = "Xác minh CAPTCHA thất bại" });
            }

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var account = workScope.Accounts.ValidFeAccount(model.Username, model.Password);

                if (HttpContext.Request.Url != null)
                {
                    var host = HttpContext.Request.Url.Authority;
                    if (account != null)
                    {
                        var cookieClient = account.UserName + "|" + host.ToLower() + "|" + account.Id;
                        var decodeCookieClient = CryptorEngine.Encrypt(cookieClient, true);
                        var userCookie = new HttpCookie(CookiesKey.Client)
                        {
                            Value = decodeCookieClient,
                            Expires = DateTime.Now.AddDays(30)
                        };
                        HttpContext.Response.Cookies.Add(userCookie);
                        return Json(new { status = true, mess = "Đăng nhập thành công" });
                    }
                    else
                    {
                        return Json(new { status = false, mess = "Tên và mật khẩu không chính xác" });
                    }
                }
                else
                {
                    return Json(new { status = false, mess = "Tên và mật khẩu không chính xác" });
                }
            }
        }

        [HttpPost]
        [ValidateInput(true)]
        public async Task<JsonResult> Register(Account us, string rePassword, string gRecaptchaResponse)
        {
            if (us.Password != rePassword)
            {
                return Json(new { status = false, mess = "Mật khẩu không khớp" });
            }

            // Xác minh reCAPTCHA
            if (string.IsNullOrEmpty(gRecaptchaResponse))
            {
                return Json(new { status = false, mess = "Vui lòng xác nhận bạn không phải là robot" });
            }

            var isCaptchaValid = await VerifyRecaptcha(gRecaptchaResponse);
            if (!isCaptchaValid)
            {
                return Json(new { status = false, mess = "Xác minh CAPTCHA thất bại" });
            }

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var account = workScope.Accounts.FirstOrDefault(x => !x.IsDeleted && x.UserName.ToLower() == us.UserName.ToLower());
                if (account == null)
                {
                    try
                    {
                        var passwordFactory = us.Password + VariableExtensions.KeyCryptorClient;
                        var passwordCrypto = CryptorEngine.Encrypt(passwordFactory, true);

                        us.Password = passwordCrypto;
                        us.Role = RoleKey.Patient;

                        us.LinkAvatar = us.Gender ? "/Content/images/team/2.png" : "/Content/images/team/3.png";
                        us.Id = Guid.NewGuid();

                        string code;

                        var patient = workScope.Patients.GetAll().OrderByDescending(x => x.JoinDate).FirstOrDefault();
                        if (patient != null)
                        {
                            var codeSplit = patient.PatientCode.Split('-');
                            code = Common.Prefix + (int.Parse(codeSplit[1]) + 1);
                        }
                        else
                        {
                            code = Common.Prefix + "1";
                        }

                        var patientId = Guid.NewGuid();
                        workScope.Patients.Add(new Patient
                        {
                            Id = patientId,
                            FullName = us.FullName,
                            Phone = us.Phone,
                            JoinDate = DateTime.Now,
                            Address = "Chưa xác định",
                            DateOfBirth = DateTime.Now,
                            IndentificationCardDate = DateTime.Now,
                            Gender = us.Gender,
                            Status = true,
                            ImageProfile = us.LinkAvatar,
                            PatientCode = code,
                            IsDeleted = false
                        });
                        workScope.Complete();

                        us.PatientId = patientId;
                        workScope.Accounts.Add(us);
                        workScope.Complete();

                        if (HttpContext.Request.Url == null)
                            return Json(new { status = false, mess = "Thêm không thành công" });

                        var host = HttpContext.Request.Url.Authority;

                        var cookieClient = us.UserName + "|" + host.ToLower() + "|" + us.Id;
                        var decodeCookieClient = CryptorEngine.Encrypt(cookieClient, true);
                        var userCookie = new HttpCookie(CookiesKey.Client)
                        {
                            Value = decodeCookieClient,
                            Expires = DateTime.Now.AddDays(30)
                        };
                        HttpContext.Response.Cookies.Add(userCookie);
                        return Json(new { status = true, mess = "Đăng ký thành công" });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { status = false, mess = "Thêm không thành công", ex });
                    }
                }
                else
                {
                    return Json(new { status = false, mess = "Username không khả dụng" });
                }
            }
        }

        [HttpPost]
        [ValidateInput(true)]
        public JsonResult Update(Account us, HttpPostedFileBase avataUpload)
        {
            if (!CookiesManage.Logined())
            {
                return Json(new { status = false, mess = "Chưa đăng nhập" });
            }
            var user = CookiesManage.GetUser();
            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var account = workScope.Accounts.FirstOrDefault(x => !x.IsDeleted && x.UserName.ToLower() == user.UserName.ToLower());
                if (account != null)
                {
                    try
                    {
                        if (avataUpload?.FileName != null)
                        {
                            if (avataUpload.ContentLength >= FileKey.MaxLength)
                            {
                                return Json(new { status = false, mess = L.T("FileMaxLength") });
                            }
                            var splitFilename = avataUpload.FileName.Split('.');
                            if (splitFilename.Length > 1)
                            {
                                var fileExt = splitFilename[splitFilename.Length - 1];
                                if (FileKey.FileExtensionApprove().Any(x => x == fileExt))
                                {
                                    var slugName = StringHelper.ConvertToAlias(account.FullName);
                                    var fileName = slugName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "." + fileExt;
                                    var path = Path.Combine(Server.MapPath("~/FileUploads/images/avatas/"), fileName);
                                    avataUpload.SaveAs(path);
                                    us.LinkAvatar = "/FileUploads/images/avatas/" + fileName;
                                }
                                else
                                {
                                    return Json(new { status = false, mess = L.T("FileExtensionReject") });
                                }
                            }
                            else
                            {
                                return Json(new { status = false, mess = L.T("FileExtensionReject") });
                            }
                        }

                        us.Password = account.Password;
                        us.UserName = account.UserName;
                        us.Role = RoleKey.Patient;
                        us.Id = account.Id;
                        us.PatientId = account.PatientId;

                        if (string.IsNullOrEmpty(us.LinkAvatar))
                        {
                            us.LinkAvatar = us.Gender ? "/Content/images/team/2.png" : "/Content/images/team/3.png";
                        }
                        else
                        {
                            var patient = workScope.Patients.FirstOrDefault(x => !x.IsDeleted && x.Id == user.PatientId);
                            if (patient != null)
                            {
                                patient.ImageProfile = us.LinkAvatar;
                                workScope.Patients.Put(patient, patient.Id);
                            }
                        }
                        account = us;
                        workScope.Accounts.Put(account, account.Id);
                        workScope.Complete();

                        var nameCookie = Request.Cookies[CookiesKey.Client];
                        if (nameCookie != null)
                        {
                            var myCookie = new HttpCookie(CookiesKey.Client)
                            {
                                Expires = DateTime.Now.AddDays(-1d)
                            };
                            Response.Cookies.Add(myCookie);
                        }

                        if (HttpContext.Request.Url != null)
                        {
                            var host = HttpContext.Request.Url.Authority;
                            var cookieClient = account.UserName + "|" + host.ToLower() + "|" + account.Id;
                            var decodeCookieClient = CryptorEngine.Encrypt(cookieClient, true);
                            var userCookie = new HttpCookie(CookiesKey.Client)
                            {
                                Value = decodeCookieClient,
                                Expires = DateTime.Now.AddDays(30)
                            };
                            HttpContext.Response.Cookies.Add(userCookie);
                            return Json(new { status = true, mess = "Cập nhật thành công" });
                        }
                        else
                        {
                            return Json(new { status = false, mess = "Cập nhật không thành công" });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { status = false, mess = "Cập nhật không thành công", ex });
                    }
                }
                else
                {
                    return Json(new { status = false, mess = "Tài khoản không khả dụng" });
                }
            }
        }

        [HttpPost]
        [ValidateInput(true)]
        public JsonResult UpdatePass(string oldPassword, string newPassword, string rePassword)
        {
            if (oldPassword == "" || newPassword == "" || rePassword == "")
            {
                return Json(new { status = false, mess = "Không được để trống" });
            }
            if (!CookiesManage.Logined())
            {
                return Json(new { status = false, mess = "Chưa đăng nhập" });
            }
            if (newPassword != rePassword)
            {
                return Json(new { status = false, mess = "Mật khẩu không khớp" });
            }
            var user = CookiesManage.GetUser();
            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var account = workScope.Accounts.FirstOrDefault(x => !x.IsDeleted && x.UserName.ToLower() == user.UserName.ToLower());
                if (account != null)
                {
                    try
                    {
                        var passwordFactory = oldPassword + VariableExtensions.KeyCryptorClient;
                        var passwordCryptor = CryptorEngine.Encrypt(passwordFactory, true);

                        if (passwordCryptor == account.Password)
                        {
                            passwordFactory = newPassword + VariableExtensions.KeyCryptorClient;
                            passwordCryptor = CryptorEngine.Encrypt(passwordFactory, true);

                            account.Password = passwordCryptor;
                            workScope.Accounts.Put(account, account.Id);
                            workScope.Complete();

                            var nameCookie = Request.Cookies[CookiesKey.Client];
                            if (nameCookie != null)
                            {
                                var myCookie = new HttpCookie(CookiesKey.Client)
                                {
                                    Expires = DateTime.Now.AddDays(-1d)
                                };
                                Response.Cookies.Add(myCookie);
                            }

                            if (HttpContext.Request.Url != null)
                            {
                                var host = HttpContext.Request.Url.Authority;
                                var cookieClient = account.UserName + "|" + host.ToLower() + "|" + account.Id;
                                var decodeCookieClient = CryptorEngine.Encrypt(cookieClient, true);
                                var userCookie = new HttpCookie(CookiesKey.Client)
                                {
                                    Value = decodeCookieClient,
                                    Expires = DateTime.Now.AddDays(30)
                                };
                                HttpContext.Response.Cookies.Add(userCookie);
                                return Json(new { status = true, mess = "Cập nhật thành công" });
                            }
                            else
                            {
                                return Json(new { status = false, mess = "Cập nhật không thành công" });
                            }
                        }
                        else
                        {
                            return Json(new { status = false, mess = "Mật khẩu cũ không đúng" });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { status = false, mess = "Cập nhật không thành công", ex });
                    }
                }
                else
                {
                    return Json(new { status = false, mess = "Tài khoản không khả dụng" });
                }
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            var nameCookie = Request.Cookies[CookiesKey.Client];
            if (nameCookie == null) return RedirectToAction("Index", "Home");
            var myCookie = new HttpCookie(CookiesKey.Client)
            {
                Expires = DateTime.Now.AddDays(-1d)
            };
            Response.Cookies.Add(myCookie);
            return RedirectToAction("Index", "Home");
        }
    }
}