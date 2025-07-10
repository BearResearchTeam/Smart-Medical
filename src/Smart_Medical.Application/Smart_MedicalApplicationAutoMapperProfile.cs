using AutoMapper;
using Smart_Medical.Application.Contracts.RBAC.Users;
using Smart_Medical.Appointment;
using Smart_Medical.Dictionarys;
using Smart_Medical.Dictionarys.DictionaryDatas;
using Smart_Medical.Dictionarys.DictionaryTypes;
using Smart_Medical.DoctorvVsit;
using Smart_Medical.DoctorvVsit.DockerDepartments;
using Smart_Medical.DoctorvVsit.DoctorAccounts;
using Smart_Medical.Medical;
using Smart_Medical.OutpatientClinic.Dtos;
using Smart_Medical.Patient;
using Smart_Medical.PatientManagement;
using Smart_Medical.Pharmacy;
using Smart_Medical.Pharmacy.InAndOutWarehouse;
using Smart_Medical.Prescriptions;
using Smart_Medical.RBAC;
using Smart_Medical.RBAC.Permissions;
using Smart_Medical.RBAC.RolePermissions;
using Smart_Medical.RBAC.Roles;
using Smart_Medical.RBAC.UserRoles;
using Smart_Medical.RBAC.Users;
using Smart_Medical.UserLoginECC;
using System.Collections.Generic;

namespace Smart_Medical;

public class Smart_MedicalApplicationAutoMapperProfile : Profile
{
    public Smart_MedicalApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        //用户
        CreateMap<User, UserDto>();
        CreateMap<CreateUpdateUserDto, User>();
        CreateMap<RegisterUserDto, User>()
            .ForMember(dest => dest.UserPwd, opt => opt.MapFrom(src => src.UserPwd))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.UserEmail))
            .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.UserPhone))
            .ForMember(dest => dest.UserSex, opt => opt.MapFrom(src => src.UserSex));

        CreateMap<RegisterUserDto, BasicPatientInfo>()
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.UserSex.HasValue ? (src.UserSex.Value ? 1 : 2) : 1))
            .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.UserPhone))
            .ForMember(dest => dest.IdNumber, opt => opt.MapFrom(src => src.IdNumber ?? ""))
            .ForMember(dest => dest.VisitType, opt => opt.MapFrom(src => src.VisitType ?? "初诊"))
            .ForMember(dest => dest.IsInfectiousDisease, opt => opt.MapFrom(src => src.IsInfectiousDisease))
            .ForMember(dest => dest.DiseaseOnsetTime, opt => opt.MapFrom(src => src.DiseaseOnsetTime))
            .ForMember(dest => dest.EmergencyTime, opt => opt.MapFrom(src => src.EmergencyTime))
            .ForMember(dest => dest.VisitStatus, opt => opt.MapFrom(src => src.VisitStatus ?? "待就诊"))
            .ForMember(dest => dest.VisitDate, opt => opt.MapFrom(src => src.VisitDate));

        //角色
        CreateMap<Role, RoleDto>();
        CreateMap<CreateUpdateRoleDto, Role>();

        //权限
        CreateMap<Permission, PermissionDto>();
        CreateMap<CreateUpdatePermissionDto, Permission>();


        //用户角色关联
        CreateMap<UserRole, UserRoleDto>();
        CreateMap<CreateUpdateUserRoleDto, UserRole>();

        //角色权限关联
        CreateMap<RolePermission, RolePermissionDto>();
        CreateMap<CreateUpdateRolePermissionDto, RolePermission>();

        //处方
        CreateMap<PrescriptionDto, Prescription>().ReverseMap();
        CreateMap<PrescriptionListDto, Prescription>().ReverseMap();

        CreateMap<DrugsSelectDto, Drug>().ReverseMap();

        #region 

        CreateMap<DoctorClinic, InsertPatientDto>().ReverseMap();
        CreateMap<BasicPatientInfo, InsertPatientDto>().ReverseMap();
        CreateMap<Sick, InsertPatientDto>().ReverseMap();
        CreateMap<BasicPatientInfo, GetVisitingDto>().ReverseMap();
        CreateMap<BasicPatientInfo, BasicPatientInfoDto>().ReverseMap();
        CreateMap<User, ResultLoginDto>().ReverseMap();
        CreateMap<BasicPatientInfo, GetInsertPatientDto>().ReverseMap();
        CreateMap<BasicPatientInfo, UpdPatientDto>().ReverseMap();
        CreateMap<Sick, HospitalizedDto>().ReverseMap();
        CreateMap<Sick, CreateUpdateHospitalizedDto>().ReverseMap();
        #endregion
        //CreateMap<List<Medication>, List<MedicationDto>>().ReverseMap();
        //科室
        CreateMap<CreateUpdateDoctorDepartmentDto, DoctorDepartment>().ReverseMap();
        CreateMap<DoctorDepartment, GetDoctorDepartmentListDto>().ReverseMap();
        CreateMap<GetDoctorDepartmentSearchDto, DoctorDepartment>().ReverseMap();
        //医生
        CreateMap<CreateUpdateDoctorAccountDto, DoctorAccount>().ReverseMap();
        CreateMap<DoctorAccount, DoctorAccountListDto>().ReverseMap();
     
           
        CreateMap<MedicalHistory, PharmaceuticalCompanyDto>();
        CreateMap<CreateUpdatePharmaceuticalCompanyDto, MedicalHistory>();

        // 药品相关映射
        CreateMap<CreateUpdateDrugDto, Drug>();
        CreateMap<Drug, DrugDto>();
        //数据字典
        CreateMap<CreateUpdateDictionaryDataDto, DictionaryData>().ReverseMap();
        CreateMap<DictionaryData, GetDictionaryDataDto>().ReverseMap();
        CreateMap<CreateUpdateDictionaryTypeDto, DictionaryType>().ReverseMap();
        CreateMap<DictionaryType, GetDictionaryTypeDto>().ReverseMap();
        CreateMap<DrugInStock, DrugInStockDto>();
        CreateMap<CreateUpdateDrugInStockDto, DrugInStock>();

        CreateMap<Sick, SickDto>().ReverseMap();
        CreateMap<CreateUpdateSickDto, Sick>().ReverseMap();


        CreateMap<MakeAppointmentDto, BasicPatientInfo>().ReverseMap();
        CreateMap<AddPatientInfoDto, BasicPatientInfo>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.HasValue ? (src.Gender.Value ? 1 : 2) : 1))
            .ForMember(dest => dest.AgeUnit, opt => opt.MapFrom(src => src.AgeUnit ?? "年"))
            .ForMember(dest => dest.VisitType, opt => opt.MapFrom(src => src.VisitType ?? "初诊"))
            .ForMember(dest => dest.VisitStatus, opt => opt.MapFrom(src => src.VisitStatus ?? "待就诊"));

    }
}
