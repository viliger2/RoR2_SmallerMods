# RegisterCommandChest
Registers unused Command Chest to catalogue. Also adds missing text strings and inspect info. This mod does not make it spawn on its own.

main purpose of this mod is for mod developers to reference it, so they can spawn the chest without needing to register it to the catalogue themselves, creating duplicates in networkObjects array, which can result in networking issues.

If you are a mod developer, you don't need to do anything in your code besides adding hard\soft dependency to this mod since the game already has spawn card for Command Chest that you can reference via Addressables.