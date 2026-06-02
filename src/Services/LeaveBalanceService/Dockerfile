# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Services/LeaveBalanceService/LeaveBalanceService.csproj", "src/Services/LeaveBalanceService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

# Restore dependencies
RUN dotnet restore "src/Services/LeaveBalanceService/LeaveBalanceService.csproj"

# Copy all source code
COPY . .

# Build
RUN dotnet build "src/Services/LeaveBalanceService/LeaveBalanceService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/Services/LeaveBalanceService/LeaveBalanceService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "LeaveBalanceService.dll"]
