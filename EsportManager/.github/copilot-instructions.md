# Copilot Instructions

## Architecture

- Use the **vertical slice architecture**. Organize code by feature (e.g., `Features/Devices/`, `Features/Tournaments/`) rather than by technical layer. Each feature folder should contain its own models, UI components, and logic.
- Place models in a `Models` subfolder within each feature folder (e.g., `Features/Devices/Models/Device.cs`).
- Place sub-features in their own subfolder (e.g., `Features/Devices/UpdateFortnite/`).
- Shared layout and routing components go in `Components/Layout/` and `Components/`.

## Workflow

- **Always show a plan before executing.** Before making any code changes, present a clear step-by-step plan and wait for approval.

## Framework & Target

- This is a **.NET MAUI Blazor Hybrid** app. Do **not** use Xamarin.Forms APIs; always use .NET MAUI equivalents.
- Target **.NET 10** with **C# 14**.
- **Nullable reference types** are enabled (`<Nullable>enable</Nullable>`).
- **Implicit usings** are enabled (`<ImplicitUsings>enable</ImplicitUsings>`).

## UI

- Use **MudBlazor** for all UI components and styling.
- Inject MudBlazor services (e.g., `ISnackbar`) via `@inject` in Razor components.

## C# Conventions

- Use **file-scoped namespaces** (e.g., `namespace EsportManager.Features.Devices.Models;`).
- Use `_camelCase` for private backing fields and `PascalCase` for public properties.
- Nest enums inside their owning class when tightly coupled (e.g., `Device.StatusEnum`, `DeviceProcess.ProcessStatusEnum`).
- Prefer modern C# collection expressions (`[]`) over `new List<T>()`.
- Use `CliWrap` for wrapping external CLI process execution.
- Use **inheritance and virtual methods** for specializing behavior (e.g., `RobocopyProcess : DeviceProcess`).

## Code Style

- Do **not** add comments unless they match the style already present or are necessary to explain complex logic.
- Use existing libraries; only add new packages if absolutely necessary.
