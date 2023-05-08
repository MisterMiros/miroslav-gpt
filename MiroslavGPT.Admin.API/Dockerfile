FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MiroslavGPT.Admin.API/MiroslavGPT.Admin.API.csproj", "MiroslavGPT.Admin.API/"]
RUN dotnet restore "MiroslavGPT.Admin.API/MiroslavGPT.Admin.API.csproj"
COPY . .
WORKDIR "/src/MiroslavGPT.Admin.API"
RUN dotnet build "MiroslavGPT.Admin.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiroslavGPT.Admin.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiroslavGPT.Admin.API.dll"]
