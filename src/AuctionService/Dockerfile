FROM mcr.microsoft.com/dotnet/sdk:10.0 as build
WORKDIR /app

COPY AuctionLane.slnx /
COPY src/AuctionService/AuctionService.csproj ./src/AuctionService/
COPY src/Contracts/Contracts.csproj ./src/Contracts/
COPY tests/AuctionService.Tests/AuctionService.Tests.csproj ./tests/AuctionService.Tests/

RUN dotnet restore src/AuctionService/AuctionService.csproj

COPY . .

RUN dotnet publish src/AuctionService/AuctionService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]