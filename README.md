# MitsubishiPLC

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

三菱 PLC 通訊函式庫，適用於 .NET Standard 2.0 及以上版本。

## 專案說明

此專案將三菱 MX Component 5 封裝為簡單易用的 .NET 函式庫，讓您能更方便地與三菱 PLC 進行通訊。支援讀寫 M、X、Y 等裝置記憶體。

**建議使用方式：** 透過 Git Submodule 將此專案整合到您的解決方案中，以專案參考的方式使用。

**相容性說明：** 本專案基於 MX Component 5 開發與測試，其他版本的相容性未經驗證。

### 專案結構

- **MitsubishiPLC.Interop** - COM 互操作函式庫，封裝三菱原生 DLL
  - 包含 `Interop.ACTMULTILib.dll` 和 `Interop.ActUtlType64Lib.dll`
  - 已預先建置為本地 NuGet 套件 (`MitsubishiPLC.Interop.2025.11.5.nupkg`)
  - 僅供本專案內部使用，一般情況下不需要重新建置
  
- **MitsubishiPLC** - 主要函式庫，簡化 MX Component 的使用
  - 依賴 MitsubishiPLC.Interop 本地套件
  - 提供擴充方法和記憶體管理功能
  - 透過專案參考的方式整合到您的解決方案中

- **MitsubishiPLC.sln** - 方案檔，僅供需要修改程式碼時使用

## 功能特色

- ✅ 支援 .NET Standard 2.0，可用於 .NET Framework 4.6.1+ 和 .NET Core 2.0+
- ✅ 簡化 MX Component 的使用方式，提供易用的擴充方法
- ✅ 支援讀寫 M (內部繼電器)、X (輸入)、Y (輸出) 等裝置
- ✅ 內建緩衝區管理，優化效能
- ✅ 整合 Microsoft.Extensions.Logging 進行日誌記錄
- ✅ 支援區塊讀寫和隨機讀寫
- ✅ 執行緒安全的設計
- ✅ 透過 Git Submodule 整合，易於維護與更新

## 使用方式

### 建議方式：使用 Git Submodule

推薦使用 Git Submodule 將此專案整合到您的解決方案中，這樣可以：
- ✅ 保持程式碼同步與更新
- ✅ 透過專案參考自動建置
- ✅ 避免 DLL 版本管理問題
- ✅ 方便進行客製化修改

#### 1. 將此專案加入為 Submodule

```bash
cd your-project-root
git submodule add https://github.com/yushuhsiao/MitsubishiPLC.git
git submodule update --init --recursive
```

#### 2. 將 MitsubishiPLC.csproj 加入您的解決方案

使用 Visual Studio：
- 在方案總管中，對解決方案按右鍵
- 選擇「加入 > 現有專案」
- 瀏覽至 `MitsubishiPLC/MitsubishiPLC/MitsubishiPLC.csproj`

或使用命令列：
```bash
dotnet sln add MitsubishiPLC/MitsubishiPLC/MitsubishiPLC.csproj
```

#### 3. 在您的專案中加入參考

使用 Visual Studio：
- 在您的專案上按右鍵 > 「加入 > 專案參考」
- 勾選 `MitsubishiPLC`

或編輯 `.csproj` 檔案：
```xml
<ItemGroup>
  <ProjectReference Include="..\MitsubishiPLC\MitsubishiPLC\MitsubishiPLC.csproj" />
</ItemGroup>
```

#### 4. 建置您的解決方案

```bash
dotnet build
```

MitsubishiPLC 專案會自動建置，並包含必要的 Interop DLL 檔案。

### 更新 Submodule

當 MitsubishiPLC 有更新時，執行以下命令來更新：

```bash
git submodule update --remote MitsubishiPLC
```

或進入 submodule 目錄手動拉取：

```bash
cd MitsubishiPLC
git pull origin master
cd ..
git add MitsubishiPLC
git commit -m "Update MitsubishiPLC submodule"
```

### 開發與修改

如果您需要修改 MitsubishiPLC 的程式碼：

1. 開啟 `MitsubishiPLC/MitsubishiPLC.sln`（此方案檔專供開發修改使用）
2. 進行必要的修改
3. 若需更新 Interop 套件，執行 `MitsubishiPLC.Interop.nupkg.bat`

**注意：** 專案已包含預先建置的 Interop 套件 (`MitsubishiPLC.Interop.2025.11.5.nupkg`)，一般情況下不需要重新建置。

---

## 快速開始

以下是一個完整的整合與使用範例：

```bash
# 1. 在您的專案根目錄加入 submodule
git submodule add https://github.com/yushuhsiao/MitsubishiPLC.git

# 2. 將專案加入解決方案
dotnet sln add MitsubishiPLC/MitsubishiPLC/MitsubishiPLC.csproj

# 3. 在您的專案中加入參考（假設您的專案在 src/MyProject）
dotnet add src/MyProject/MyProject.csproj reference MitsubishiPLC/MitsubishiPLC/MitsubishiPLC.csproj

# 4. 建置
dotnet build
```

然後在您的程式碼中使用：

```csharp
using Mitsubishi.PLC;
using Microsoft.Extensions.Logging;

// 建立 Logger
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("PLC");

// 連線到 PLC
if (ActControl.Open(out var act, ActLogicalStationNumber: 1, logger))
{
    // 讀取 M0 的值
    var values = new Dictionary<int, M_Value>();
    act.ReadM(begin: 0, length: 100, values, out var time);
    
    // 使用值
    if (values.TryGetValue(0, out var m0))
    {
        Console.WriteLine($"M0 = {m0.Value}");
    }
    
    // 關閉連線
    act.Close(logger);
}
```

## 使用範例

### 基本連線

```csharp
using Mitsubishi.PLC;
using Microsoft.Extensions.Logging;

// 建立 Logger (選用)
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("PLC");

// 開啟 PLC 連線
if (ActControl.Open(out var act, ActLogicalStationNumber: 1, logger))
{
    // 使用 PLC...
    
    // 關閉連線
    act.Close(logger);
}
```

### 讀取 M 記憶體

```csharp
var values = new Dictionary<int, M_Value>();

// 讀取 M0 ~ M99
var error = act.ReadM(begin: 0, length: 100, values, out var time);

if (error.IsSuccess())
{
    Console.WriteLine($"讀取成功，耗時: {time.TotalMilliseconds} ms");
    
    // 取得特定位址的值
    if (values.TryGetValue(0, out var m0))
    {
        Console.WriteLine($"M0 = {m0.Value}");
    }
}
```

### 寫入 M 記憶體

```csharp
var values = new Dictionary<int, M_Value>
{
    [10] = new M_Value("M", 10, 1),  // M10 設為 1
    [11] = new M_Value("M", 11, 0)   // M11 設為 0
};

// 寫入單一區塊 (M0~M15)
var error = act.WriteM(begin: 10, values);

if (error.IsSuccess())
{
    Console.WriteLine("寫入成功");
}
```

### 讀取 X/Y 輸入輸出

```csharp
var inputValues = new Dictionary<int, M_Value>();
var outputValues = new Dictionary<int, M_Value>();

// 讀取所有 X 輸入 (預設 0-255)
act.ReadX(inputValues, out var xTime);

// 讀取所有 Y 輸出 (預設 0-255)
act.ReadY(outputValues, out var yTime);

// 讀取特定範圍
act.ReadX(begin: 0, length: 32, inputValues, out var time);
```

### 區塊讀寫

```csharp
// 讀取 D 暫存器
var data = new int[10];
var error = act.ReadDeviceBlock("D100", data, out var time);

if (error.IsSuccess())
{
    Console.WriteLine($"D100-D109: {string.Join(", ", data)}");
}

// 寫入 D 暫存器
var writeData = new int[] { 100, 200, 300 };
error = act.WriteDeviceBlock("D200", writeData, out var writeTime);
```

### 單點讀寫

```csharp
// 讀取單一裝置
act.GetDevice("M100", out var value);
Console.WriteLine($"M100 = {value}");

// 寫入單一裝置
act.SetDevice("M100", 1);

// 使用 M_Value 物件
var m100 = new M_Value("M", 100, 0);
act.SetDevice(m100, 1);  // 設為 1
act.SetDevice(m100);      // 切換值 (0->1 或 1->0)
```

## API 參考

### ActControl 類別

靜態類別，封裝 MX Component 的常用功能，提供擴充方法簡化 PLC 通訊操作。

#### 方法

- `Open(out IActControl act, int ActLogicalStationNumber, ILogger logger)` - 開啟 PLC 連線
- `Close(this IActControl act, ILogger logger)` - 關閉 PLC 連線
- `ReadM(...)` - 讀取 M 記憶體
- `WriteM(...)` - 寫入 M 記憶體
- `ReadX(...)` - 讀取 X 輸入
- `ReadY(...)` - 讀取 Y 輸出
- `ReadDeviceBlock(...)` - 區塊讀取
- `WriteDeviceBlock(...)` - 區塊寫入
- `GetDevice(...)` - 單點讀取
- `SetDevice(...)` - 單點寫入

### ErrorCode 列舉

包含所有可能的錯誤碼，例如：
- `0` - 成功
- `COM_port_handle_error` - COM 埠控制碼錯誤
- 等等...

使用 `IsSuccess()` 或 `IsNotSuccess()` 擴充方法來檢查執行結果。

## 系統需求

### 開發環境
- .NET SDK 6.0 或更高版本（支援 .NET Standard 2.0）
- Visual Studio 2019 或更高版本 / Visual Studio Code
- Windows 作業系統

### 執行環境
- .NET Framework 4.6.1+ 或 .NET Core 2.0+
- Windows 作業系統
- 已安裝三菱 MX Component 5（本專案基於此版本開發，其他版本未測試）

## 授權

本專案採用 [MIT License](LICENSE) 授權。

版權所有 © 2025 Yushu Hsiao

## 貢獻與支援

歡迎社群貢獻！

- **問題回報**：請開啟 [Issue](https://github.com/yushuhsiao/MitsubishiPLC/issues)
- **功能建議**：歡迎提出想法和討論
- **貢獻程式碼**：歡迎提交 [Pull Request](https://github.com/yushuhsiao/MitsubishiPLC/pulls)
- **版本資訊**：請參考 [Releases](https://github.com/yushuhsiao/MitsubishiPLC/releases)

### 貢獻指南

1. Fork 此專案
2. 建立您的功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交您的修改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 開啟 Pull Request

### 開發環境設定

如果您想要修改此專案：

1. Clone 此專案到本地
2. 開啟 `MitsubishiPLC.sln`
3. 進行修改並測試
4. 提交 Pull Request

### 建置 NuGet 套件（可選）

專案提供了兩個建置腳本：

#### MitsubishiPLC.Interop.nupkg.bat
建置 Interop 套件（通常不需要重新建置）：
```bash
MitsubishiPLC.Interop.nupkg.bat
```

#### MitsubishiPLC.nupkg.bat
建置 MitsubishiPLC 主要套件：

**不帶參數（自動使用當前日期作為版本號）：**
```bash
MitsubishiPLC.nupkg.bat
# 產生 MitsubishiPLC.2025.11.5.nupkg（版本號為當前日期 YYYY.M.D）
```

**帶自訂版本號參數：**
```bash
MitsubishiPLC.nupkg.bat 1.0.0
# 產生 MitsubishiPLC.1.0.0.nupkg
```

執行批次檔會在專案根目錄產生對應的 `.nupkg` 檔案。

## 注意事項

### 系統環境
1. 使用前請確保已正確安裝三菱 MX Component 5
2. 本專案僅在 MX Component 5 上測試，其他版本的相容性未經驗證
3. 確認 PLC 的邏輯站號 (ActLogicalStationNumber) 設定正確

### 整合與建置
4. **建議使用 Git Submodule** 整合此專案，而非直接複製程式碼或使用 DLL 參考
5. 使用專案參考 (ProjectReference) 可確保與您的專案一起建置，並自動處理依賴關係
6. 專案已包含預先建置的 Interop 套件，一般情況下不需要重新建置
7. 僅在需要自訂或更新時才開啟 `MitsubishiPLC.sln` 進行修改

### 記憶體操作
8. M 記憶體讀寫會自動對齊到 16 的倍數
9. X/Y 記憶體讀寫會自動對齊到 20 的倍數
10. 預設啟用執行緒安全鎖定 (Lock_DecodeM = true)

## 相關連結

- [三菱電機官方網站](https://www.mitsubishielectric.com/)
- [三菱 FA 產品與技術支援](https://www.mitsubishielectric.com/fa/)
- **MX Component 5** - 本專案使用此版本開發，請至三菱電機官方網站下載

