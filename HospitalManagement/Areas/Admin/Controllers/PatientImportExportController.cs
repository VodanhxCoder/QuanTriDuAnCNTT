using BELibrary.Core.Entity;
using BELibrary.Core.Utils;
using BELibrary.DbContext;
using BELibrary.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NPOI.XSSF.UserModel; // Sử dụng NPOI cho định dạng .xlsx
using NPOI.SS.UserModel;
using System.IO;

namespace HospitalManagement.Areas.Admin.Controllers
{
    public class PatientImportExportController : BaseController
    {
        private readonly string KeyElement = "Bệnh nhân";

        // Xuất dữ liệu bệnh nhân ra Excel
        public ActionResult ExportToExcel()
        {
            try
            {
                using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
                {
                    // Lấy danh sách bệnh nhân
                    var patients = workScope.Patients.Query(x => x.Status).OrderByDescending(x => x.JoinDate).ToList();

                    var user = GetCurrentUser();

                    // Nếu là bác sĩ, chỉ lấy bệnh nhân liên quan
                    if (user.Role == RoleKey.Doctor)
                    {
                        var patientOfDoctors = workScope.PatientDoctors.Query(x => x.DoctorId == user.DoctorId) ?? new List<PatientDoctor>();
                        var patientOfDoctorIds = patientOfDoctors.Select(x => x.PatientId);
                        patients = patients.Where(x => patientOfDoctorIds.Contains(x.Id)).ToList();
                    }

                    if (!patients.Any())
                    {
                        return Json(new { status = false, mess = "Không có dữ liệu bệnh nhân để xuất" }, JsonRequestBehavior.AllowGet);
                    }

                    // Tạo file Excel
                    using (var workbook = new XSSFWorkbook())
                    {
                        var sheet = workbook.CreateSheet("Patients");

                        // Định dạng tiêu đề
                        var headerRow = sheet.CreateRow(0);
                        headerRow.CreateCell(0).SetCellValue("Mã bệnh nhân");
                        headerRow.CreateCell(1).SetCellValue("Họ tên");
                        headerRow.CreateCell(2).SetCellValue("Ngày sinh");
                        headerRow.CreateCell(3).SetCellValue("Giới tính");
                        headerRow.CreateCell(4).SetCellValue("Số điện thoại");
                        headerRow.CreateCell(5).SetCellValue("Email");
                        headerRow.CreateCell(6).SetCellValue("Địa chỉ");
                        headerRow.CreateCell(7).SetCellValue("Nghề nghiệp");
                        headerRow.CreateCell(8).SetCellValue("Nơi công tác");
                        headerRow.CreateCell(9).SetCellValue("Số căn cước");
                        headerRow.CreateCell(10).SetCellValue("Ngày cấp căn cước");
                        headerRow.CreateCell(11).SetCellValue("Tiền sử bệnh gia đình");
                        headerRow.CreateCell(12).SetCellValue("Tiền sử bệnh bản thân");

                        // Định dạng tiêu đề
                        var headerStyle = workbook.CreateCellStyle();
                        headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                        headerStyle.FillPattern = FillPattern.SolidForeground;
                        var font = workbook.CreateFont();
                        font.Boldweight = (short)FontBoldWeight.Bold;
                        headerStyle.SetFont(font);
                        for (int i = 0; i <= 12; i++)
                        {
                            headerRow.GetCell(i).CellStyle = headerStyle;
                        }

                        // Ghi dữ liệu
                        int rowIndex = 1;
                        foreach (var patient in patients)
                        {
                            var row = sheet.CreateRow(rowIndex++);
                            row.CreateCell(0).SetCellValue(patient.PatientCode);
                            row.CreateCell(1).SetCellValue(patient.FullName);
                            row.CreateCell(2).SetCellValue(patient.DateOfBirth.ToString("dd/MM/yyyy"));
                            row.CreateCell(3).SetCellValue(patient.Gender ? "Nam" : "Nữ");
                            row.CreateCell(4).SetCellValue(patient.Phone);
                            row.CreateCell(5).SetCellValue(patient.Email);
                            row.CreateCell(6).SetCellValue(patient.Address);
                            row.CreateCell(7).SetCellValue(patient.Job);
                            row.CreateCell(8).SetCellValue(patient.WorkPlace);
                            row.CreateCell(9).SetCellValue(patient.IndentificationCardId);
                            row.CreateCell(10).SetCellValue(patient.IndentificationCardDate.ToString("dd/MM/yyyy"));
                            row.CreateCell(11).SetCellValue(patient.HistoryOfIllnessFamily);
                            row.CreateCell(12).SetCellValue(patient.HistoryOfIllnessYourself);
                        }

                        // Tự động điều chỉnh độ rộng cột
                        for (int i = 0; i <= 12; i++)
                        {
                            sheet.AutoSizeColumn(i);
                        }

                        // Ghi dữ liệu vào Byte[]
                        using (var stream = new MemoryStream())
                        {
                            workbook.Write(stream);
                            var fileBytes = stream.ToArray();
                            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Patients.xlsx");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, mess = "Có lỗi xảy ra khi xuất file Excel: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Import dữ liệu bệnh nhân từ Excel
        [HttpPost]
        public JsonResult ImportFromExcel()
        {
            try
            {
                var user = GetCurrentUser();

                // Chỉ admin được phép import
                if (user.Role == RoleKey.Doctor)
                {
                    return Json(new { status = false, mess = "Bạn không có quyền import dữ liệu" });
                }

                var file = Request.Files["excelFile"];
                if (file == null || file.ContentLength == 0)
                {
                    return Json(new { status = false, mess = "Vui lòng chọn file Excel để import" });
                }

                using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
                {
                    using (var stream = file.InputStream)
                    {
                        var workbook = new XSSFWorkbook(stream);
                        var sheet = workbook.GetSheetAt(0);

                        if (sheet == null)
                        {
                            return Json(new { status = false, mess = "File Excel không có dữ liệu" });
                        }

                        int rowCount = sheet.LastRowNum + 1;
                        if (rowCount <= 1)
                        {
                            return Json(new { status = false, mess = "File Excel không có dữ liệu bệnh nhân" });
                        }

                        var patients = new List<Patient>();
                        for (int row = 1; row < rowCount; row++) // Bỏ qua dòng tiêu đề
                        {
                            var currentRow = sheet.GetRow(row);
                            if (currentRow == null || string.IsNullOrWhiteSpace(currentRow.GetCell(0)?.ToString()))
                                continue;

                            var patient = new Patient
                            {
                                Id = Guid.NewGuid(),
                                PatientCode = GeneratePatientCode(workScope),
                                FullName = currentRow.GetCell(0)?.ToString(),
                                DateOfBirth = DateTime.TryParse(currentRow.GetCell(1)?.ToString(), out var dob) ? dob : DateTime.Now.AddYears(-18),
                                Gender = currentRow.GetCell(2)?.ToString()?.ToLower() == "nam",
                                Phone = currentRow.GetCell(3)?.ToString(),
                                Email = currentRow.GetCell(4)?.ToString(),
                                Address = currentRow.GetCell(5)?.ToString(),
                                Job = currentRow.GetCell(6)?.ToString(),
                                WorkPlace = currentRow.GetCell(7)?.ToString(),
                                IndentificationCardId = currentRow.GetCell(8)?.ToString(),
                                IndentificationCardDate = DateTime.TryParse(currentRow.GetCell(9)?.ToString(), out var icd) ? icd : DateTime.Now,
                                HistoryOfIllnessFamily = currentRow.GetCell(10)?.ToString(),
                                HistoryOfIllnessYourself = currentRow.GetCell(11)?.ToString(),
                                JoinDate = DateTime.Now,
                                Status = true,
                                IsDeleted = false
                            };

                            patients.Add(patient);
                        }

                        if (!patients.Any())
                        {
                            return Json(new { status = false, mess = "Không có dữ liệu bệnh nhân hợp lệ để import" });
                        }

                        // Lưu vào database
                        foreach (var patient in patients)
                        {
                            workScope.Patients.Add(patient);
                        }
                        workScope.Complete();

                        return Json(new { status = true, mess = $"Đã import thành công {patients.Count} bệnh nhân" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, mess = "Có lỗi xảy ra khi import file Excel: " + ex.Message });
            }
        }

        // Tải file mẫu Excel để import
        public ActionResult DownloadImportTemplate()
        {
            try
            {
                // Tạo file Excel mẫu
                using (var workbook = new XSSFWorkbook())
                {
                    var sheet = workbook.CreateSheet("Patients Template");

                    // Định dạng tiêu đề (bỏ cột "Mã bệnh nhân")
                    var headerRow = sheet.CreateRow(0);
                    headerRow.CreateCell(0).SetCellValue("Họ tên");
                    headerRow.CreateCell(1).SetCellValue("Ngày sinh");
                    headerRow.CreateCell(2).SetCellValue("Giới tính");
                    headerRow.CreateCell(3).SetCellValue("Số điện thoại");
                    headerRow.CreateCell(4).SetCellValue("Email");
                    headerRow.CreateCell(5).SetCellValue("Địa chỉ");
                    headerRow.CreateCell(6).SetCellValue("Nghề nghiệp");
                    headerRow.CreateCell(7).SetCellValue("Nơi công tác");
                    headerRow.CreateCell(8).SetCellValue("Số căn cước");
                    headerRow.CreateCell(9).SetCellValue("Ngày cấp căn cước");
                    headerRow.CreateCell(10).SetCellValue("Tiền sử bệnh gia đình");
                    headerRow.CreateCell(11).SetCellValue("Tiền sử bệnh bản thân");

                    // Định dạng tiêu đề
                    var headerStyle = workbook.CreateCellStyle();
                    headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                    headerStyle.FillPattern = FillPattern.SolidForeground;
                    var font = workbook.CreateFont();
                    font.Boldweight = (short)FontBoldWeight.Bold;
                    headerStyle.SetFont(font);
                    for (int i = 0; i <= 11; i++)
                    {
                        headerRow.GetCell(i).CellStyle = headerStyle;
                    }

                    // Tự động điều chỉnh độ rộng cột
                    for (int i = 0; i <= 11; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }

                    // Ghi dữ liệu vào Byte[]
                    using (var stream = new MemoryStream())
                    {
                        workbook.Write(stream);
                        var fileBytes = stream.ToArray();
                        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportTemplate.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, mess = "Có lỗi xảy ra khi tạo file mẫu: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Hàm tạo mã bệnh nhân tự động
        private string GeneratePatientCode(UnitOfWork workScope)
        {
            var patient = workScope.Patients.GetAll().OrderByDescending(x => x.JoinDate).FirstOrDefault();
            if (patient != null)
            {
                var codeSplit = patient.PatientCode.Split('-');
                return Common.Prefix + (int.Parse(codeSplit[1]) + 1);
            }
            return Common.Prefix + "1";
        }
    }
}