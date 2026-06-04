# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Services/EmployeeService/EmployeeService.csproj", "src/Services/EmployeeService/"]
COPY ["src/Shared/Shared/Shared.csproj", "src/Shared/"]

# Restore dependencies
RUN dotnet restore "src/Services/EmployeeService/EmployeeService.csproj"

# Copy all source code
COPY . .

# Build
RUN dotnet build "src/Services/EmployeeService/EmployeeService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/Services/EmployeeService/EmployeeService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EmployeeService.dll"]
