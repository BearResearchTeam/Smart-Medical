using Microsoft.EntityFrameworkCore;
using Smart_Medical.Dictionarys;
using Smart_Medical.DoctorvVsit;
using Smart_Medical.Medical;
using Smart_Medical.Patient;
using Smart_Medical.Pharmacy;
using Smart_Medical.Pharmacy.InAndOutWarehouse;
using Smart_Medical.Prescriptions;
using Smart_Medical.RBAC;
using Smart_Medical.Equipment;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace Smart_Medical.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class Smart_MedicalDbContext :
    AbpDbContext<Smart_MedicalDbContext>
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    /// <summary>
    /// 用户实体的 DbSet。
    /// </summary>
    public DbSet<User> Users { get; set; }
    /// <summary>
    /// 角色实体的 DbSet。
    /// </summary>
    public DbSet<Role> Roles { get; set; }
    /// <summary>
    /// 权限实体的 DbSet。（新增）
    /// </summary>
    public DbSet<Permission> Permissions { get; set; }
    /// <summary>
    /// 用户-角色关联实体的 DbSet。（新增）
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; }
    /// <summary>
    /// 角色-权限关联实体的 DbSet。（新增）
    /// </summary>
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserPatient> UserPatients { get; set; }

    public DbSet<Prescription> Prescriptions { get; set; }
    //public DbSet<Medication> Medications { get; set; }
    public DbSet<EquipmentManagement> EquipmentManagements { get; set; }    

    #region

    public DbSet<DoctorAccount> DoctorAccounts { get; set; }
    public DbSet<DoctorClinic> DoctorClinics { get; set; }
    public DbSet<DoctorDepartment> DoctorDepartments { get; set; }
    public DbSet<BasicPatientInfo> BasicPatientInfos { get; set; }
    public DbSet<PatientPrescription> PatientPrescriptions { get; set; }

    public DbSet<MedicalHistory> MedicalHistorys { get; set; }
    public DbSet<DrugInStock> DrugInStocks { get; set; }
    #endregion

    public DbSet<Drug> Drugs { get; set; }
    public DbSet<Sick> Sicks { get; set; }

    //数据字典表 
    public DbSet<DictionaryData> DictionaryDatas { get; set; }
    public DbSet<DictionaryType> DictionaryTypes { get; set; }


    public DbSet<Appointment> Appointments { get; set; }

    public DbSet<DoctorAudit> DoctorAudits{ get; set; }

    public Smart_MedicalDbContext(DbContextOptions<Smart_MedicalDbContext> options)
        : base(options)
    {

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.EnableSensitiveDataLogging(); // 添加此行
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(Smart_MedicalConsts.DbTablePrefix + "YourEntities", Smart_MedicalConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});


        builder.Entity<Prescription>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "Prescriptions",
                Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.PrescriptionName).IsRequired().HasMaxLength(128);

        });

        builder.Entity<User>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "Users",
                Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.UserName).IsRequired().HasMaxLength(128);

        });

        builder.Entity<Role>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "Roles",
                Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.RoleName).IsRequired().HasMaxLength(128);

        });
        builder.Entity<Permission>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "Permissions",
                Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.PermissionName).IsRequired().HasMaxLength(128);

        });


        /*  builder.Entity<Medication>(b =>
          {
              b.ToTable(Smart_MedicalConsts.DbTablePrefix + "Medications",
                  Smart_MedicalConsts.DbSchema);
              b.ConfigureByConvention(); //auto configure for the base class props
              b.Property(x => x.MedicationName).IsRequired().HasMaxLength(128);

          });*/

        // PatientPrescription 配置
        builder.Entity<PatientPrescription>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "PatientPrescriptions", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
                                       //长度限制、必填项等
            /*  b.Property(x => x.MedicationName).IsRequired().HasMaxLength(128);
              b.Property(x => x.Specification).HasMaxLength(128);
              b.Property(x => x.DosageUnit).IsRequired().HasMaxLength(20);
              b.Property(x => x.Dosage).IsRequired();
              b.Property(x => x.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");*/
            b.Property(x => x.PrescriptionTemplateNumber).IsRequired().HasDefaultValue(0);

        });

        // BasicPatientInfo 配置
        builder.Entity<BasicPatientInfo>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "BasicPatientInfos", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.VisitId).IsRequired().HasMaxLength(20);
            b.Property(x => x.PatientName).IsRequired().HasMaxLength(50);
            b.Property(x => x.AgeUnit).HasMaxLength(10);
            b.Property(x => x.ContactPhone).HasMaxLength(20);
            b.Property(x => x.IdNumber).HasMaxLength(18);
            b.Property(x => x.VisitType).IsRequired().HasMaxLength(20);
            b.Property(x => x.VisitStatus).HasMaxLength(20);
        });

        // DoctorAccount 配置
        builder.Entity<DoctorAccount>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "DoctorAccounts", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AccountId).IsRequired().HasMaxLength(20);
            b.Property(x => x.EmployeeId).IsRequired().HasMaxLength(10);
            b.Property(x => x.EmployeeName).IsRequired().HasMaxLength(20);
            b.Property(x => x.InstitutionName).IsRequired().HasMaxLength(50);
            b.Property(x => x.DepartmentName).HasMaxLength(30);
        });

        // DoctorClinic 配置
        builder.Entity<DoctorClinic>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "DoctorClinics", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.PatientId).IsRequired();
            b.Property(x => x.DoctorId).IsRequired();
            b.Property(x => x.VisitDateTime).IsRequired();
            b.Property(x => x.DepartmentName).IsRequired().HasMaxLength(50);
            b.Property(x => x.ChiefComplaint).HasMaxLength(500);
            b.Property(x => x.PreliminaryDiagnosis).HasMaxLength(1000);
            b.Property(x => x.VisitType).IsRequired().HasMaxLength(20);
            b.Property(x => x.Remarks).HasMaxLength(1000);
        });

        // DoctorDepartment 配置
        builder.Entity<DoctorDepartment>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "DoctorDepartments", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.DepartmentName).IsRequired().HasMaxLength(50);
            b.Property(x => x.DepartmentCategory).HasMaxLength(30);
            b.Property(x => x.Address).HasMaxLength(100);
            b.Property(x => x.DirectorName).HasMaxLength(20);
            b.Property(x => x.Type).HasMaxLength(20);
        });



        builder.Entity<Drug>(b =>
     {
         b.ToTable(Smart_MedicalConsts.DbTablePrefix + "Drugs", Smart_MedicalConsts.DbSchema);
         b.ConfigureByConvention();
       
         b.Property(x => x.DrugName).IsRequired().HasMaxLength(128);
         b.Property(x => x.DrugType).IsRequired().HasMaxLength(32);
         b.Property(x => x.FeeName).IsRequired().HasMaxLength(32);
         b.Property(x => x.DosageForm).IsRequired().HasMaxLength(32);
         b.Property(x => x.Specification).IsRequired().HasMaxLength(64);
         b.Property(x => x.Effect).IsRequired().HasMaxLength(256);
         b.Property(x => x.Category).IsRequired();
         b.Property(x => x.PurchasePrice).HasColumnType("decimal(18,2)");
         b.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");
     });

        builder.Entity<Sick>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "Sicks", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.InpatientNumber).IsRequired().HasMaxLength(32);
            /*            b.Property(x => x.Name).IsRequired().HasMaxLength(32);
                        b.Property(x => x.DischargeDepartment).IsRequired().HasMaxLength(64);
                        b.Property(x => x.Gender).IsRequired().HasMaxLength(8);*/
            b.Property(x => x.DischargeTime).IsRequired();
            b.Property(x => x.AdmissionDiagnosis).IsRequired().HasMaxLength(128);
            /*b.Property(x => x.DischargeDiagnosis).IsRequired().HasMaxLength(128);*/
        });

        builder.Entity<MedicalHistory>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "PharmaceuticalCompanies", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CompanyName).IsRequired().HasMaxLength(128);
            b.Property(x => x.ContactPerson).HasMaxLength(64);
            b.Property(x => x.ContactPhone).HasMaxLength(50);
            b.Property(x => x.Address).HasMaxLength(200);
        });

        builder.Entity<DrugInStock>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "DrugInStocks", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BatchNumber).IsRequired().HasMaxLength(64);
            /*
                        b.HasOne<Drug>().WithMany().HasForeignKey(x => x.Id).IsRequired();
                        b.HasOne<PharmaceuticalCompany>().WithMany().HasForeignKey(x => x.Id).IsRequired();*/
        });

        builder.Entity<DictionaryType>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "DictionaryTypes",
                Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
        });
       
        builder.Entity<DictionaryData>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "DictionaryDatas",
                Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Appointment>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "Appointments",
                Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Remarks).IsRequired().HasMaxLength(500);
        });

        /// <summary>
        /// 配置 UserRole 实体。
        /// </summary>
        builder.Entity<UserRole>(b =>
        {
            // 将 UserRole 实体映射到数据库表 Smart_MedicalConsts.DbTablePrefix + "UserRoles"。
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "UserRoles", Smart_MedicalConsts.DbSchema);
            // 应用ABP框架的约定配置，例如自动配置基类属性。
            b.ConfigureByConvention();
            // 定义复合主键：UserId 和 RoleId 的组合唯一标识一条 UserRole 记录。
            b.HasKey(ur => new { ur.UserId, ur.RoleId });

            // 配置 UserRole 与 User 之间的一对多关系。
            // 一个 UserRole 关联一个 User (HasOne(ur => ur.User))。
            // 一个 User 可以有多个 UserRole (WithMany(u => u.UserRoles))。
            // UserRole 的 UserId 是外键，关联到 User 的主键 (HasForeignKey(ur => ur.UserId))。
            b.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            // 配置 UserRole 与 Role 之间的一对多关系。
            // 一个 UserRole 关联一个 Role (HasOne(ur => ur.Role))。
            // 一个 Role 可以有多个 UserRole (WithMany(r => r.UserRoles))。
            // UserRole 的 RoleId 是外键，关联到 Role 的主键 (HasForeignKey(ur => ur.RoleId))。
            b.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });

        /// <summary>
        /// 配置 RolePermission 实体。
        /// </summary>
        builder.Entity<RolePermission>(b =>
        {
            // 将 RolePermission 实体映射到数据库表 Smart_MedicalConsts.DbTablePrefix + "RolePermissions"。
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "RolePermissions", Smart_MedicalConsts.DbSchema);
            // 应用ABP框架的约定配置。
            b.ConfigureByConvention();
            // 定义复合主键：RoleId 和 PermissionId 的组合唯一标识一条 RolePermission 记录。
            b.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // 配置 RolePermission 与 Role 之间的一对多关系。
            // 一个 RolePermission 关联一个 Role (HasOne(rp => rp.Role))。
            // 一个 Role 可以有多个 RolePermission (WithMany(r => r.RolePermissions))。
            // RolePermission 的 RoleId 是外键，关联到 Role 的主键 (HasForeignKey(rp => rp.RoleId))。
            b.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            // 配置 RolePermission 与 Permission 之间的一对多关系。
            // 一个 RolePermission 关联一个 Permission (HasOne(rp => rp.Permission))。
            // 一个 Permission 可以有多个 RolePermission (WithMany(p => p.RolePermissions))。
            // RolePermission 的 PermissionId 是外键，关联到 Permission 的主键 (HasForeignKey(rp => rp.PermissionId))。
            b.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        });

        //配置用户和患者的多对多关系
        builder.Entity<UserPatient>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "UserPatients",
                Smart_MedicalConsts.DbSchema);

            // 配置复合主键
            b.HasKey(up => new { up.UserId, up.PatientId });

            // 配置与 User 的关系
            b.HasOne(up => up.User)
                .WithMany(u => u.UserPatients)
                .HasForeignKey(up => up.UserId);

            // 配置与 BasicPatientInfo 的关系
            b.HasOne(up => up.Patient)
                .WithMany(p => p.UserPatients)
                .HasForeignKey(up => up.PatientId);

          /*  // 应用 ABP 框架的约定配置
            b.ConfigureByConvention();*/
        });

        builder.Entity<EquipmentManagement>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "EquipmentManagements", Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.Property(x => x.Model).HasMaxLength(64);
            b.Property(x => x.Manufacturer).HasMaxLength(64);
            b.Property(x => x.Department).HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Location).HasMaxLength(128);
            b.Property(x => x.Remark).HasMaxLength(256);
        });

        builder.Entity<DoctorAudit>(b =>
        {
            b.ToTable(Smart_MedicalConsts.DbTablePrefix + "DoctorAudits",
                Smart_MedicalConsts.DbSchema);
            b.ConfigureByConvention();
        });

    }
}
