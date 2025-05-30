# Sports Reservation Backend

## 运行环境要求
- .NET 7.0 SDK 或更高版本（请根据项目实际版本调整）

## 快速运行

1. 克隆仓库并进入后端目录：
   ```bash
   git clone <your-repo-url>
   cd sports-reservation-backend
   ```

2. 还原依赖：
   ```bash
   dotnet restore
   ```

3. （如果有数据库迁移）更新数据库：
   ```bash
   dotnet ef database update
   ```

4. 启动后端服务：
   ```bash
   dotnet run
   ```

5. 打开浏览器访问 API 地址。