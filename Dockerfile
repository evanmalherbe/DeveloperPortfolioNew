# Stage 1: Build the Application
# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
# This step is often cached, speeding up builds if only code changes
COPY *.sln .
COPY DeveloperPortfolioNew/*.csproj DeveloperPortfolioNew/
RUN dotnet restore DeveloperPortfolioNew/DeveloperPortfolioNew.csproj

# Copy the rest of the source code
COPY DeveloperPortfolioNew/ DeveloperPortfolioNew/
WORKDIR /src/DeveloperPortfolioNew

# Publish the application to the 'app/publish' directory
RUN dotnet publish -c Release -o /app/publish --no-restore

# ----------------------------------------------------------------

# Stage 2: Run the Application (Minimal Image)
# Use the ASP.NET Runtime image for the final production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entry point to run the published application
# The host must bind to 0.0.0.0, and Render automatically sets the PORT environment variable to 10000.
# ASP.NET Core is configured to listen on all interfaces (0.0.0.0) by default when PORT is set.
ENTRYPOINT ["dotnet", "DeveloperPortfolioNew.dll"]

# Expose port 8080 (Render expects your app to listen on the port defined by the PORT env var, 
# but this EXPOSE is good practice and often set to 8080 in the runtime image)
EXPOSE 8080