FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:35792ea4ad1db051981f62b313f1be3b46b1f45cadbaa3c288cd0d3056eefb83 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef

# Add the installed tools to the PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Generate the migration bundle
RUN dotnet ef migrations bundle --project ./src/WebApiPlayground.Api/WebApiPlayground.Api.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:6c4df091e4e531bb93bdbfe7e7f0998e7ced344f54426b7e874116a3dc3233ff
WORKDIR /App
COPY --from=build-env /App/out .
COPY --from=build-env /App/efbundle .
ENTRYPOINT ["dotnet", "WebApiPlayground.Api.dll"]