using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Smart_Medical.EntityFrameworkCore;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Smart_Medical;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(Smart_MedicalApplicationModule),
    typeof(Smart_MedicalEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpCachingStackExchangeRedisModule)
)]
public class Smart_MedicalHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        /*  PreConfigure<OpenIddictBuilder>(builder =>
          {
              builder.AddValidation(options =>
              {
                  options.AddAudiences("Smart_Medical");
                  options.UseLocalServer();
                  options.UseAspNetCore();
              });
          });*/
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {

        var services = context.Services;
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        services.AddTransient<LMZTokenHelper>();
        services.AddSingleton<JwtSecurityTokenHandler>();

        //services.addsc

        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.TokenCookie.Expiration = TimeSpan.FromDays(365);
            options.AutoValidate = false;
        });
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "SmartMedical:"; // 推荐设置一个项目特有的前缀
                                                 // 可以设置默认的缓存过期时间等
            options.GlobalCacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
        });
       
        //Configure<AbpRedisCacheOptions>(options =>
        //{
        //    options.Configuration = "localhost:6379"; // 改成你自己的 Redis 地址
        //});

        ConfigureAuthentication(context);
        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureConventionalControllers();
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context, configuration);
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var secretKey = configuration["Jwt:SecretKey"];

        context.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(0), // 允许时间误差
                };


                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT认证失败：{context.Exception.Message}");
                        if (context.Exception.InnerException != null)
                        {
                            Console.WriteLine($"  Inner Exception: {context.Exception.InnerException.Message}");
                        }
                        return Task.CompletedTask;
                    },                    
                };
            });
    }


    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());

            /*    options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
                options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";*/
        });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<Smart_MedicalDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Smart_Medical.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<Smart_MedicalDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Smart_Medical.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<Smart_MedicalApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Smart_Medical.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<Smart_MedicalApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Smart_Medical.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(Smart_MedicalApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddSwaggerGen(

            options =>
            {
                options.SwaggerDoc("权限管理", new OpenApiInfo { Title = "权限管理", Version = "权限管理" });
                options.SwaggerDoc("角色管理", new OpenApiInfo { Title = "角色管理", Version = "角色管理" });
                options.SwaggerDoc("用户管理", new OpenApiInfo { Title = "用户管理", Version = "用户管理" });
                options.SwaggerDoc("用户角色关联管理", new OpenApiInfo { Title = "用户角色关联管理", Version = "用户角色关联管理" });
                options.SwaggerDoc("角色权限关联管理", new OpenApiInfo { Title = "角色权限关联管理", Version = "角色权限关联管理" });
                options.SwaggerDoc("药品管理", new OpenApiInfo { Title = "药品管理", Version = "药品管理" });
                options.SwaggerDoc("处方管理", new OpenApiInfo { Title = "处方管理", Version = "处方管理" });
                options.SwaggerDoc("制药公司管理", new OpenApiInfo { Title = "制药公司管理", Version = "制药公司管理" });
                options.SwaggerDoc("药品入库管理", new OpenApiInfo { Title = "药品入库管理", Version = "药品入库管理" });
                options.SwaggerDoc("患者管理", new OpenApiInfo { Title = "患者管理", Version = "患者管理" });
                options.SwaggerDoc("病种管理", new OpenApiInfo { Title = "病种管理", Version = "病种管理" }); 
                options.SwaggerDoc("科室管理", new OpenApiInfo { Title = "科室管理", Version = "科室管理" });
                options.SwaggerDoc("医生管理", new OpenApiInfo { Title = "医生管理", Version = "医生管理" });
                options.SwaggerDoc("收费发药管理", new OpenApiInfo { Title = "收费发药管理", Version = "收费发药管理" });
                options.SwaggerDoc("字典管理", new OpenApiInfo { Title = "字典管理", Version = "字典管理" });
                options.SwaggerDoc("医疗管理", new OpenApiInfo { Title = "医疗管理", Version = "医疗管理" });
                options.SwaggerDoc("用户登录", new OpenApiInfo { Title = "用户登录", Version = "用户登录" });

                options.DocInclusionPredicate((doc, desc) =>
                {
                    if (!desc.GroupName.IsNullOrWhiteSpace())
                    {
                        return doc == desc.GroupName;
                    }
                    return true;
                });

                //开启权限小锁
                options.OperationFilter<AddResponseHeadersFilter>();
                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.CustomSchemaIds(type => type.FullName);


                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "输入 Bearer + 空格 + JWT，例如：Bearer eyJhbGciOi...",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                //就是这里！！！！！！！！！
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "Smart_Medical.Application.xml");//这个就是刚刚配置的xml文件名
                options.IncludeXmlComments(xmlPath, true);//默认的第二个参数是false，这个是controller的注释，记得修改


                options.HideAbpEndpoints(); // 可选：隐藏 ABP 默认生成的接口
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        //app.UseAbpOpenIddictValidation();

        /*   if (MultiTenancyConsts.IsEnabled)
           {
               app.UseMultiTenancy();
           }*/
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/权限管理/swagger.json", "权限管理");
            c.SwaggerEndpoint("/swagger/角色管理/swagger.json", "角色管理");
            c.SwaggerEndpoint("/swagger/用户管理/swagger.json", "用户管理");
            c.SwaggerEndpoint("/swagger/用户角色关联管理/swagger.json", "用户角色管理");
            c.SwaggerEndpoint("/swagger/角色权限关联管理/swagger.json", "角色权限管理");
            c.SwaggerEndpoint("/swagger/药品管理/swagger.json", "药品管理");
            c.SwaggerEndpoint("/swagger/处方管理/swagger.json", "处方管理");
            c.SwaggerEndpoint("/swagger/制药公司管理/swagger.json", "制药公司管理");
            c.SwaggerEndpoint("/swagger/药品入库管理/swagger.json", "药品入库管理");
            c.SwaggerEndpoint("/swagger/患者管理/swagger.json", "患者管理");
            c.SwaggerEndpoint("/swagger/病种管理/swagger.json", "病种管理");
            c.SwaggerEndpoint("/swagger/科室管理/swagger.json", "科室管理");
            c.SwaggerEndpoint("/swagger/医生管理/swagger.json", "医生管理");
            c.SwaggerEndpoint("/swagger/收费发药管理/swagger.json", "收费发药管理");
            c.SwaggerEndpoint("/swagger/字典管理/swagger.json", "字典管理");
            c.SwaggerEndpoint("/swagger/医疗管理/swagger.json", "医疗管理");
            c.SwaggerEndpoint("/swagger/用户登录/swagger.json", "用户登录");

            // 模型的默认扩展深度，设置为 -1 完全隐藏模型
            c.DefaultModelsExpandDepth(1);
            // API文档仅展开标记
            c.DocExpansion(DocExpansion.List);
            c.DefaultModelRendering(ModelRendering.Example);
            c.DefaultModelExpandDepth(-1);
            // API前缀设置为空
            c.RoutePrefix = string.Empty;
            // API页面Title
            c.DocumentTitle = "Smart_Medical API";
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
