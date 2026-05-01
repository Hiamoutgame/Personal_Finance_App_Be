# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["Personal_Finance_Management/Personal_Finance_Management.sln", "Personal_Finance_Management/"]
COPY ["Personal_Finance_Management/Personal_Finance_Management.Api/Personal_Finance_Management.Api.csproj", "Personal_Finance_Management/Personal_Finance_Management.Api/"]
COPY ["Personal_Finance_Management/Personal_Finance_Management.Service/Personal_Finance_Management.Service.csproj", "Personal_Finance_Management/Personal_Finance_Management.Service/"]
COPY ["Personal_Finance_Management/Personal_Finance_Management.Repository/Personal_Finance_Management.Repository.csproj", "Personal_Finance_Management/Personal_Finance_Management.Repository/"]

RUN dotnet restore "Personal_Finance_Management/Personal_Finance_Management.Api/Personal_Finance_Management.Api.csproj"

FROM restore AS build
COPY ["Personal_Finance_Management/", "Personal_Finance_Management/"]

RUN dotnet publish "Personal_Finance_Management/Personal_Finance_Management.Api/Personal_Finance_Management.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet Personal_Finance_Management.Api.dll"]
