using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smart_Medical.RBAC;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Authorization
{
    public class PermissionInitializationService
    {
        private readonly IRepository<Permission, Guid> _permissionRepository;
        public PermissionInitializationService(IRepository<Permission, Guid> permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }
        public async Task InitializeAsync()
        {
            var defaultPermissions = new List<Permission>
            {
                // 药品管理
                new Permission { PermissionName = "药品查看", PermissionCode = PermissionConstants.DrugView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "药品新增", PermissionCode = PermissionConstants.DrugCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "药品编辑", PermissionCode = PermissionConstants.DrugEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "药品删除", PermissionCode = PermissionConstants.DrugDelete, Type = Enums.PermissionType.Operation },
                // 制药公司管理
                new Permission { PermissionName = "制药公司查看", PermissionCode = PermissionConstants.PharmaceuticalCompanyView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "制药公司新增", PermissionCode = PermissionConstants.PharmaceuticalCompanyCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "制药公司编辑", PermissionCode = PermissionConstants.PharmaceuticalCompanyEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "制药公司删除", PermissionCode = PermissionConstants.PharmaceuticalCompanyDelete, Type = Enums.PermissionType.Operation },
                // 药品出入库管理
                new Permission { PermissionName = "药品出入库查看", PermissionCode = PermissionConstants.DrugInStockView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "药品出入库新增", PermissionCode = PermissionConstants.DrugInStockCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "药品出入库编辑", PermissionCode = PermissionConstants.DrugInStockEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "药品出入库删除", PermissionCode = PermissionConstants.DrugInStockDelete, Type = Enums.PermissionType.Operation },
                // 医生管理
                new Permission { PermissionName = "医生查看", PermissionCode = PermissionConstants.DoctorView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "医生新增", PermissionCode = PermissionConstants.DoctorCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "医生编辑", PermissionCode = PermissionConstants.DoctorEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "医生删除", PermissionCode = PermissionConstants.DoctorDelete, Type = Enums.PermissionType.Operation },
                // 科室管理
                new Permission { PermissionName = "科室查看", PermissionCode = PermissionConstants.DepartmentView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "科室新增", PermissionCode = PermissionConstants.DepartmentCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "科室编辑", PermissionCode = PermissionConstants.DepartmentEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "科室删除", PermissionCode = PermissionConstants.DepartmentDelete, Type = Enums.PermissionType.Operation },
                // 患者管理
                new Permission { PermissionName = "患者查看", PermissionCode = PermissionConstants.PatientView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "患者新增", PermissionCode = PermissionConstants.PatientCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "患者编辑", PermissionCode = PermissionConstants.PatientEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "患者删除", PermissionCode = PermissionConstants.PatientDelete, Type = Enums.PermissionType.Operation },
                // 病历管理
                new Permission { PermissionName = "病历查看", PermissionCode = PermissionConstants.MedicalHistoryView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "病历新增", PermissionCode = PermissionConstants.MedicalHistoryCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "病历编辑", PermissionCode = PermissionConstants.MedicalHistoryEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "病历删除", PermissionCode = PermissionConstants.MedicalHistoryDelete, Type = Enums.PermissionType.Operation },
                // 发药管理
                new Permission { PermissionName = "发药查看", PermissionCode = PermissionConstants.DispensingMedicineView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "发药新增", PermissionCode = PermissionConstants.DispensingMedicineCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "发药编辑", PermissionCode = PermissionConstants.DispensingMedicineEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "发药删除", PermissionCode = PermissionConstants.DispensingMedicineDelete, Type = Enums.PermissionType.Operation },
                // 预约管理
                new Permission { PermissionName = "预约查看", PermissionCode = PermissionConstants.AppointmentView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "预约新增", PermissionCode = PermissionConstants.AppointmentCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "预约编辑", PermissionCode = PermissionConstants.AppointmentEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "预约删除", PermissionCode = PermissionConstants.AppointmentDelete, Type = Enums.PermissionType.Operation },
                // 门诊管理
                new Permission { PermissionName = "门诊查看", PermissionCode = PermissionConstants.OutpatientClinicView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "门诊新增", PermissionCode = PermissionConstants.OutpatientClinicCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "门诊编辑", PermissionCode = PermissionConstants.OutpatientClinicEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "门诊删除", PermissionCode = PermissionConstants.OutpatientClinicDelete, Type = Enums.PermissionType.Operation },
                // 收费管理
                new Permission { PermissionName = "收费查看", PermissionCode = PermissionConstants.FeeView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "收费新增", PermissionCode = PermissionConstants.FeeCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "收费编辑", PermissionCode = PermissionConstants.FeeEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "收费删除", PermissionCode = PermissionConstants.FeeDelete, Type = Enums.PermissionType.Operation },
                // 用户管理
                new Permission { PermissionName = "用户查看", PermissionCode = PermissionConstants.UserView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "用户新增", PermissionCode = PermissionConstants.UserCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "用户编辑", PermissionCode = PermissionConstants.UserEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "用户删除", PermissionCode = PermissionConstants.UserDelete, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "用户管理", PermissionCode = PermissionConstants.UserManage, Type = Enums.PermissionType.Operation },
                // 角色管理
                new Permission { PermissionName = "角色查看", PermissionCode = PermissionConstants.RoleView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "角色新增", PermissionCode = PermissionConstants.RoleCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "角色编辑", PermissionCode = PermissionConstants.RoleEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "角色删除", PermissionCode = PermissionConstants.RoleDelete, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "角色管理", PermissionCode = PermissionConstants.RoleManage, Type = Enums.PermissionType.Operation },
                // 权限管理
                new Permission { PermissionName = "权限查看", PermissionCode = PermissionConstants.PermissionView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "权限新增", PermissionCode = PermissionConstants.PermissionCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "权限编辑", PermissionCode = PermissionConstants.PermissionEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "权限删除", PermissionCode = PermissionConstants.PermissionDelete, Type = Enums.PermissionType.Operation },
                // 挂号管理
                new Permission { PermissionName = "挂号查看", PermissionCode = PermissionConstants.RegistrationView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "挂号新增", PermissionCode = PermissionConstants.RegistrationCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "挂号编辑", PermissionCode = PermissionConstants.RegistrationEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "挂号删除", PermissionCode = PermissionConstants.RegistrationDelete, Type = Enums.PermissionType.Operation },
                // 处方管理
                new Permission { PermissionName = "处方查看", PermissionCode = PermissionConstants.PrescriptionView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "处方新增", PermissionCode = PermissionConstants.PrescriptionCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "处方编辑", PermissionCode = PermissionConstants.PrescriptionEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "处方删除", PermissionCode = PermissionConstants.PrescriptionDelete, Type = Enums.PermissionType.Operation },
                // 病种管理
                new Permission { PermissionName = "病种查看", PermissionCode = PermissionConstants.SickView, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "病种新增", PermissionCode = PermissionConstants.SickCreate, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "病种编辑", PermissionCode = PermissionConstants.SickEdit, Type = Enums.PermissionType.Operation },
                new Permission { PermissionName = "病种删除", PermissionCode = PermissionConstants.SickDelete, Type = Enums.PermissionType.Operation },
                // ... 其他基础权限可继续补充
            };
            foreach (var permission in defaultPermissions)
            {
                if (!await _permissionRepository.AnyAsync(x => x.PermissionCode == permission.PermissionCode))
                {
                    await _permissionRepository.InsertAsync(permission);
                }
            }
        }
    }
} 