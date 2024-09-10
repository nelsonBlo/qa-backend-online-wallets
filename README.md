# qa-backend-code-challenge

Code challenge for QA Backend Engineer candidates.

### Build Docker image

Run this command from the directory where there is the solution file.

```
docker build -f src/Betsson.OnlineWallets.Web/Dockerfile .
```

### Run Docker container

```
docker run -p <port>:8080 <image id>
```

### Open Swagger

```
http://localhost:<port>/swagger/index.html
```
### Run Unit Tests
Go to src/Betsson.OnlineWallets.Tests/ and run
```
dotnet test
```
9 unit test should run to test service layer

### Run API Tests
Expose microservice with previous steps and go to src/Betsson.OnlineWallets.ApiTests/ and run
```
dotnet test
```
14 API tests should run to test endpoints behaviour. These scripts should be self explanatory.