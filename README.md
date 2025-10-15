# MyLuxuryWatches

A cross-platform watch collection application built with .NET MAUI using the MVVM pattern.  
The repository contains the complete source code of the app.

## Overview
MyLuxuryWatches focuses on a clean and responsive UI with smooth navigation, data binding, and maintainable separation of concerns through MVVM.  
The app targets Android, iOS, Windows and Mac Catalyst via .NET MAUI.

## Features
- MVVM architecture with observable state and commands
- Navigation between list, detail, and favorites
- Search and filtering for watch models
- Persistent local storage (e.g., preferences or file-based storage)
- Theming and responsive layouts using MAUI styles and resources
- Cross-platform assets under `Resources/` and platform-specific tweaks under `Platforms/`

## Tech Stack
- .NET 8 (or 7) and .NET MAUI
- C#, XAML
- MVVM (INotifyPropertyChanged, Commands)
- Optional: CommunityToolkit.Mvvm for boilerplate reduction
