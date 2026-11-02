# Ice Item Icons

A client-side BepInEx plugin that applies the supplied Ice Shoes and Ice Skates icons to Valheim's vanilla `IceShoes` and `IceSkates` item prefabs.

This plugin only changes `SharedData.m_icons`. It does not register recipes or trader stock, change item types, names or descriptions, or patch equipment visuals. It does not require Jotunn.

The PNG files are embedded in `IceItemIcons.dll`; they do not need to sit beside the DLL when installed. `manifest.json` provides Thunderstore-style package metadata.

## Install

1. Disable or remove the original DeepNorthUtilityItems plugin if you want none of its other behavior.
2. Copy `IceItemIcons.dll` into your Valheim profile's `BepInEx/plugins` folder (a subfolder such as `BepInEx/plugins/IceItemIcons` is fine).
3. Launch Valheim through that profile.

This package was compiled against the Valheim and BepInEx assemblies installed on the author's machine. It has not been launched in-game here.

## Source assets

`IceSpikes.png` is used for vanilla `IceShoes`; `IceSkates.png` is used for vanilla `IceSkates`. Both images are embedded from these PNG files at build time. The plugin source is in `src/Plugin.cs`.
