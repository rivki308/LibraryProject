

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# העתקת כל תיקיית הפרויקט והפתרון
COPY . .
# ביצוע Restore לפיתרון כולו
RUN dotnet restore "Library.API/Library.API.csproj"

# מעבר לתיקיית ה-API ובניית ה-Publish
WORKDIR "/src/Library.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# הגדרת ה-DLL הנכון של ה-API
ENTRYPOINT ["dotnet", "Library.API.dll"]
