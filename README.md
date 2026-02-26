Uses two projects, one for mobile, one for desktop. The project is coded in C# with xaml.
The decision to use separate projects was due to MAUI's scaling limitations at the time, the code itself is cross-platform but when attempting to implement cross-compatability, the UI was ugly.
Application data is stored online using Firebase.
The code itself is tested and works, but new APIs must be generated for Firebase and Google Maps.
