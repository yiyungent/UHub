#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Presentation/UHub.Web/UHub.Web.csproj", "src/Presentation/UHub.Web/"]
COPY ["src/Libraries/UHub.Core/UHub.Core.csproj", "src/Libraries/UHub.Core/"]
RUN dotnet restore "src/Presentation/UHub.Web/UHub.Web.csproj"
COPY . .
WORKDIR "/src/src/Presentation/UHub.Web"
RUN dotnet build "UHub.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UHub.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UHub.Web.dll"]