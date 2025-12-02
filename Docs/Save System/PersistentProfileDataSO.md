Class that inherits from ScriptableObject and provides logic to save all the SO data into JSON when `OnSaveGameEvent` is emitted by [[SaveSystemClass]].

Any `ScriptableObject` that inherits from this class and are inside the `Resources` folder (and any subfolder) will conserve their data between game sessions.

> The data of the scriptable object are saved at profile scope `/SaveData/{profile-GUID}/{PersistentProfileDataSO}`

> You need to provide and set an **UNIQUE** name to the scriptable object to be saved.

## Example of use:

```cs
// Create this asset inside Resources folder
// any instance with an unique "fileName" will work
[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "ScriptableObjects/SpawnDataSO")]
public class PlayerData : PersistentProfileDataSO
{
	public int CurrentHealth;
}
```
