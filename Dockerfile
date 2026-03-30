FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CommerceHub.sln ./
COPY src/CommerceHub.Api/CommerceHub.Api.csproj src/CommerceHub.Api/
COPY src/CommerceHub.Application/CommerceHub.Application.csproj src/CommerceHub.Application/
COPY src/CommerceHub.Domain/CommerceHub.Domain.csproj src/CommerceHub.Domain/
COPY src/CommerceHub.Infrastructure/CommerceHub.Infrastructure.csproj src/CommerceHub.Infrastructure/
COPY tests/CommerceHub.Tests/CommerceHub.Tests.csproj tests/CommerceHub.Tests/

RUN dotnet restore src/CommerceHub.Api/CommerceHub.Api.csproj

COPY . .
RUN dotnet publish src/CommerceHub.Api/CommerceHub.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CommerceHub.Api.dll"]