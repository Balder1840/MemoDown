# MemoDown
A simple knowledge management web application supports markdown, powered by blazor.

# Features
- [x] Full markdown support powered by [cherry-markdown](https://github.com/Tencent/cherry-markdown124)
- [x] Simple and Clean UI
- [x] Plain markdown files are supported
- [x] Images are supported, uploading or external
- [x] Saving all resources to local file system
- [x] Multiple level hierarchy sidebar 
- [x] Automatically saving
- [ ] Sync to Github

# Usage
## Build from source
- For Windows
```cmd
dotnet build -c Release -r win-x64 --self-contained true
```
- For Linux
```bash
dotnet build -c Release -r linux-x64 --self-contained true
```

## Using docker
- Create builder for cross-platform docker building (optional)
```bash
docker buildx create --driver-opt default-load=true --name=container --use
docker buildx inspect --bootstrap container
```

- Build docker image (optional)
  - Push to docker hub
```bash
docker buildx build --platform=linux/amd64,linux/arm64 --push --builder=container -t balder1840/memo-down:v1.0.1 .
```
  - Or load to local
```bash
docker buildx build --platform=linux/amd64 --load --builder=container -t balder1840/memo-down:v1.0.1 .
```

- Use the image
```bash
  docker run -d \
  --name memodown \
  -p 8080:8080 \
  -e MemoDown__Account__UserName=your_user_name \
  -e MemoDown__Account__Password=your_password_hash \
  -v ~/memo:/memo \
  balder1840/memo-down:tagname \
```

## Configuration
```json
"MemoDown": {
  "Account": {
    "UserName": "balder1840",
    "Password": "AQAAAAIAAYagAAAAEJXIr6HwTn5e4MD0a+kdmBi2J166MTfufoCThELvnOfDp0VrPrCbt9PBRW1if5YxlA=="
  },
  "MemoDir": "/path/to/markdown", // default to C:\Users\[UserName]\memo on Windows or /home/memo on Linux
  "AutoSavingIntervalSecond": 30,
  "UploadsDir": "/path/to/uploads", // default to [MemoDir]/uploads
  "UploadsVirtualPath": "request virtual path for uploads" // default to uploads
}
```

> you may need the [password-hasher](https://github.com/Balder1840/password-hasher) to hash your password