# PBB4PBLBackend

## Project Build

使用visual studio開起，組態選擇project-manage-system-backend 開起
API測試
```
GET http:\\localhost:5001\api\user
Response User Model
```

EF Core
```
dotnet tool install --global dotnet-ef //安裝EFCORE TOOL
dotnet ef migrations add InitialCreate //當有變動到DB MODEL時需執行
dotnet ef database update //更新資料庫
```
[參考資料](https://docs.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli)
