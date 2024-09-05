A simple Windows 10 (+/-?) system tray application. It watches for laptop lid open/close power events, and changes the default login method to match. Lid closed = PIN; Lid open = fingerprint. 

Requires admin permissions as it writes to HLKM in the registry, here:

HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\UserTile

Specifically, it creates (and *OVERWRITES* if it exists) a key based on the currently logged in user's SID. 

Lid closed, we go with pin, so we set the key to {D6886603-9D2F-4EB2-B667-1971041FA96B}". Lid open, we get "{BEC09223-B018-416D-A0AC-523971B639F5}" for fingerprint.

This currently works for my purposes, but if I get the itch, configuration may come. Password, face and smartcard technically could all be used.

Uses wpf-notifyicon from hardcodet: https://github.com/hardcodet/wpf-notifyicon
