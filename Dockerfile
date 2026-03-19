FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 7120
ENV ASPNETCORE_URLS=http://+:7120
RUN mkdir -p /app/data

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["OTPer.API/OTPer.API.csproj", "OTPer.API/"]
COPY ["OTPer.Core/OTPer.Core.csproj", "OTPer.Core/"]
RUN dotnet restore "OTPer.API/OTPer.API.csproj"
COPY . .
WORKDIR "/src/OTPer.API"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
VOLUME ["/app/data"]
ENTRYPOINT ["dotnet", "OTPer.API.dll"]
