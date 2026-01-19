<details>
<summary>1.1.3 </summary>

* Gave artifact cachedName so it would save via LobbyRulebookDefSaver.
</details>
<details>
<summary>1.1.2 </summary>

* Fixed ProperSave compat for 3.0.
</details>
<details>
<summary>1.1.1 </summary>

* Added whitelist.
  * _Whitelist allows users to specify which items are always included in selection. These items respect rulebook settings, however they are not included in item counts, so it is best to adjust number of available items after filling out whitelist if you want for number of items to remain the same._
* Added config option to print selected items to log on the start of the run. Disabled by default.  
</details>
<details>
<summary>1.1.0 </summary>

* Fixed boss and lunar items basically being removed from drop tables when artifact is enabled.
* Fixed depending void items (as in void items that should be added if its original item is added) not respecting rulebook locks.
* Made ItemBlacklist incompatible.
  * _Basically, enabling ItemBlacklist and this artifact makes some drop tables broken, since ItemBlacklist does some stuff to drop tables to support its scrapper blacklist, even if it is disabled, the most egregious one is cubes not dropping after Solus Wing that you need to get inside the vault. I decided to just make this mod imcompabible with ItemBlacklist and made [alternative](https://thunderstore.io/package/viliger/LobbyRulebookDefSaver/) to it instead._
</details>
<details>
<summary>1.0.3 </summary>

* Fixed game hanging if you go to the next stage with Artifact disabled and ProperSave enabled.
</details>
<details>
<summary>1.0.2 </summary>

* Recompiled for AC.
* Fixed it so it actually works outside of Command.
</details>
<details>
<summary>1.0.0 </summary>

* Initial release
</details>