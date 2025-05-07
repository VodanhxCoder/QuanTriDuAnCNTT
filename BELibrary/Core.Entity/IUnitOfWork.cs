using BELibrary.Core.Entity.Repositories;
using System;

namespace BELibrary.Core.Entity
{
    public interface IUnitOfWork : IDisposable //Tạo phiên làm việc duy nhất với cơ sở dữ liệu
                                               //Tạo các thuộc tính cho các repository
                                               //Các thuộc tính này sẽ được khởi tạo trong lớp UnitOfWork
                                               //Các lớp repository sẽ kế thừa từ lớp này
                                               //Các lớp repository sẽ được khởi tạo trong lớp UnitOfWork
                                               //Các lớp repository sẽ được sử dụng trong lớp Service
    {
        IAccountRepository Accounts { get; }
        ICategoryRepository Categories { get; }
        IDetailPrescriptionRepository DetailPrescriptions { get; }
        IItemRepository Items { get; }
        IDetailRecordRepository DetailRecords { get; }
        IMedicineRepository Medicines { get; }
        IPatientRepository Patients { get; }
        IMedicalSupplyRepository MedicalSupplies { get; }
        IPrescriptionRepository Prescriptions { get; }
        IRecordRepository Records { get; }
        IAttachmentAssignRepository AttachmentAssigns { get; }
        IAttachmentRepository Attachments { get; }
        IDoctorRepository Doctors { get; }
        IFacultyRepository Faculties { get; }
        IPatientRecordRepository PatientRecords { get; }
        IUserVerificationRepository UserVerifications { get; }
        IDoctorScheduleRepository DoctorSchedules { get; }
        IPatientDoctorRepository PatientDoctors { get; }
        IArticleRepository Articles { get; }

        int Complete();
    }
}