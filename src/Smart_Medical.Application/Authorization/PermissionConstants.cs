namespace Smart_Medical.Authorization
{
    public static class PermissionConstants
    {
        // 药品管理
        public const string DrugView = "Pharmacy.Drug.View";
        public const string DrugCreate = "Pharmacy.Drug.Create";
        public const string DrugEdit = "Pharmacy.Drug.Edit";
        public const string DrugDelete = "Pharmacy.Drug.Delete";
        // 制药公司管理
        public const string PharmaceuticalCompanyView = "Pharmacy.PharmaceuticalCompany.View";
        public const string PharmaceuticalCompanyCreate = "Pharmacy.PharmaceuticalCompany.Create";
        public const string PharmaceuticalCompanyEdit = "Pharmacy.PharmaceuticalCompany.Edit";
        public const string PharmaceuticalCompanyDelete = "Pharmacy.PharmaceuticalCompany.Delete";
        // 药品出入库管理
        public const string DrugInStockView = "Pharmacy.DrugInStock.View";
        public const string DrugInStockCreate = "Pharmacy.DrugInStock.Create";
        public const string DrugInStockEdit = "Pharmacy.DrugInStock.Edit";
        public const string DrugInStockDelete = "Pharmacy.DrugInStock.Delete";
        // 医生管理
        public const string DoctorView = "Doctor.View";
        public const string DoctorCreate = "Doctor.Create";
        public const string DoctorEdit = "Doctor.Edit";
        public const string DoctorDelete = "Doctor.Delete";
        // 科室管理
        public const string DepartmentView = "Department.View";
        public const string DepartmentCreate = "Department.Create";
        public const string DepartmentEdit = "Department.Edit";
        public const string DepartmentDelete = "Department.Delete";
        // 患者管理
        public const string PatientView = "Patient.View";
        public const string PatientCreate = "Patient.Create";
        public const string PatientEdit = "Patient.Edit";
        public const string PatientDelete = "Patient.Delete";
        // 病历管理
        public const string MedicalHistoryView = "MedicalHistory.View";
        public const string MedicalHistoryCreate = "MedicalHistory.Create";
        public const string MedicalHistoryEdit = "MedicalHistory.Edit";
        public const string MedicalHistoryDelete = "MedicalHistory.Delete";
        // 发药管理
        public const string DispensingMedicineView = "DispensingMedicine.View";
        public const string DispensingMedicineCreate = "DispensingMedicine.Create";
        public const string DispensingMedicineEdit = "DispensingMedicine.Edit";
        public const string DispensingMedicineDelete = "DispensingMedicine.Delete";
        // 用户管理
        public const string UserView = "RBAC.User.View";
        public const string UserCreate = "RBAC.User.Create";
        public const string UserEdit = "RBAC.User.Edit";
        public const string UserDelete = "RBAC.User.Delete";
        public const string UserManage = "RBAC.User.Manage";
        // 角色管理
        public const string RoleView = "RBAC.Role.View";
        public const string RoleCreate = "RBAC.Role.Create";
        public const string RoleEdit = "RBAC.Role.Edit";
        public const string RoleDelete = "RBAC.Role.Delete";
        public const string RoleManage = "RBAC.Role.Manage";
        // 权限管理
        public const string PermissionView = "RBAC.Permission.View";
        public const string PermissionCreate = "RBAC.Permission.Create";
        public const string PermissionEdit = "RBAC.Permission.Edit";
        public const string PermissionDelete = "RBAC.Permission.Delete";
        // 挂号管理
        public const string RegistrationView = "Registration.View";
        public const string RegistrationCreate = "Registration.Create";
        public const string RegistrationEdit = "Registration.Edit";
        public const string RegistrationDelete = "Registration.Delete";
        // 处方管理
        public const string PrescriptionView = "Prescriptions.Prescription.View";
        public const string PrescriptionCreate = "Prescriptions.Prescription.Create";
        public const string PrescriptionEdit = "Prescriptions.Prescription.Edit";
        public const string PrescriptionDelete = "Prescriptions.Prescription.Delete";
        // 病种管理
        public const string SickView = "Medical.Sick.View";
        public const string SickCreate = "Medical.Sick.Create";
        public const string SickEdit = "Medical.Sick.Edit";
        public const string SickDelete = "Medical.Sick.Delete";

        public static string OutpatientClinicView { get; internal set; }
        public static string AppointmentView { get; internal set; }
        public static string AppointmentCreate { get; internal set; }
        public static string AppointmentEdit { get; internal set; }
        public static string AppointmentDelete { get; internal set; }
        public static string OutpatientClinicCreate { get; internal set; }
        public static string OutpatientClinicEdit { get; internal set; }
        public static string OutpatientClinicDelete { get; internal set; }
        public static string FeeView { get; internal set; }
        public static string FeeCreate { get; internal set; }
        public static string FeeEdit { get; internal set; }
        public static string FeeDelete { get; internal set; }
        // 其他模块可继续补充...
    }
} 