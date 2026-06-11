FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/LibraryManagement.API/LibraryManagement.API.csproj", "src/LibraryManagement.API/"]
COPY ["src/LibraryManagement.Services/LibraryManagement.Services.csproj", "src/LibraryManagement.Services/"]
COPY ["src/LibraryManagement.Data/LibraryManagement.Data.csproj", "src/LibraryManagement.Data/"]
RUN dotnet restore "src/LibraryManagement.API/LibraryManagement.API.csproj"
COPY . .
WORKDIR "/src/src/LibraryManagement.API"
RUN dotnet build "LibraryManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LibraryManagement.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LibraryManagement.API.dll"]
