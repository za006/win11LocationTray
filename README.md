# 测试一个win11下的位置注册表托盘控制 代码基于Proxy Tray

2026制作 原程序是代理托盘控制程序https://github.com/Lingxi-Li/ProxyTray
A Windows tray app that monitors WinINet proxy settings for LAN (not VPN) and enables quick proxy on/off switch.

- A blue/yellow tray icon indicates proxy off/on status, respectively.
- A mouse hover-over tip shows proxy server info when proxy is on or "Direct" when off.
- Left click on the tray icon toggles proxy on/off status. Proxy server info must be pre-configured externally. If no configuration is found, automatically opens Windows proxy setting panel.
- Right-click opens context menu.
    - "Proxy Setting" opens Windows network proxy setting panel.
    - "Quit" exits the tray app.

Targets .NET Framework 4.8 that is included in Windows 11.
