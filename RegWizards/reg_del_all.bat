Reg.exe delete "HKCU\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}" /f

Reg.exe delete "HKCU\SOFTWARE\Classes\ReadOnlyFileIconOverlayHandler.ReadOnlyFileIconOverlayHandler" /f

Reg.exe delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ShellIconOverlayIdentifiers\  ReadOnlyFileIconOverlayHandler" /f

Reg.exe delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" /v "{a259c04f-ffa8-310b-864c-fe602840399e}" /f

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe ReadOnlyFileIconOverlayHandler.dll /unregister

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe SharpShell.dll /unregister

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe NewApproach.exe /unregister

C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe NewApproach.exe /unregister

C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe ReadOnlyFileIconOverlayHandler.dll /unregister

C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe SharpShell.dll /unregister