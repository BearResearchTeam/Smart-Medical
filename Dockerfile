# 请参阅 https://aka.ms/customizecontainer 以了解如何自定义调试容器，以及 Visual Studio 如何使用此 Dockerfile 生成映像以更快地进行调试。

# 此阶段用于在快速模式(默认为调试配置)下从 VS 运行时
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# 此阶段用于生成服务项目
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Smart_Medical.HttpApi.Host/Smart_Medical.HttpApi.Host.csproj", "src/Smart_Medical.HttpApi.Host/"]
COPY ["src/Smart_Medical.Application/Smart_Medical.Application.csproj", "src/Smart_Medical.Application/"]
COPY ["src/Smart_Medical.Domain/Smart_Medical.Domain.csproj", "src/Smart_Medical.Domain/"]
COPY ["src/Smart_Medical.Domain.Shared/Smart_Medical.Domain.Shared.csproj", "src/Smart_Medical.Domain.Shared/"]
COPY ["src/Smart_Medical.Application.Contracts/Smart_Medical.Application.Contracts.csproj", "src/Smart_Medical.Application.Contracts/"]
COPY ["src/Smart_Medical.EntityFrameworkCore/Smart_Medical.EntityFrameworkCore.csproj", "src/Smart_Medical.EntityFrameworkCore/"]
RUN dotnet restore "./src/Smart_Medical.HttpApi.Host/Smart_Medical.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/src/Smart_Medical.HttpApi.Host"
RUN dotnet build "./Smart_Medical.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此阶段用于发布要复制到最终阶段的服务项目
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Smart_Medical.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此阶段在生产中使用，或在常规模式下从 VS 运行时使用(在不使用调试配置时为默认值)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Smart_Medical.HttpApi.Host.dll"]