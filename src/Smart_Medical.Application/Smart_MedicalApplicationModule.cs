using Volo.Abp.AutoMapper;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;
using Microsoft.Extensions.DependencyInjection;
using Smart_Medical.Until.Redis;
using Smart_Medical.Redis;
using Smart_Medical.Dictionarys.DictionaryDatas;
using Smart_Medical.Dictionarys;
using System.Collections.Generic;
using Smart_Medical.RBAC;
using Smart_Medical.Patient;
using System;
using Volo.Abp.Domain.Repositories;
using System.Reflection;
using Volo.Abp.DependencyInjection;

namespace Smart_Medical;

[DependsOn(
    typeof(Smart_MedicalDomainModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(Smart_MedicalApplicationContractsModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAutoMapperModule)
)]
public class Smart_MedicalApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册泛型接口实现
        context.Services.AddScoped(typeof(IRedisHelper<>), typeof(RedisHelper<>));

        // 配置自动映射
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<Smart_MedicalApplicationModule>();
        });
       

        // 配置 ABP 分布式缓存选项
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "SmartMedical:";
        });
    }
}
