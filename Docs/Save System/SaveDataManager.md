This class handles the logic to serealize and deserealize the desired data through file system operations organizating it using profile structures, it operates as a static class.

>[!note] SetPaths(string savePath, string profilesFileName)
>The data managet could be configured using SetPaths to set the names of the files

the manager uses type-safe on serialization and deserealization of the data.
## Profile management
The profile data is stored as structs named `UserProfile`, each profile creates their own folder using their ID as name and are stored in a list in a serialized file on save path.

the profiles are recovered and saved on static field `_profiles` as a `List` using `RecoverProfiles()` and exposed with `GetProfiles()` (that also recovers before return a value)

You can use `SaveProfile(UserProfile p)` to serialize the profile and save it on the profiles list, and use `CreateProfile(string name, int timePlayed)` to create and save one.
### Profile structure
The structure of an `UserProfile` contains:

| Field        | Type     | Purpose                                |
| ------------ | -------- | -------------------------------------- |
| `ID`         | `string` | Unique GUID identifier (never changes) |
| `Name`       | `string` | User-facing profile name               |
| `TimePlayed` | `float`  | Total gameplay time in seconds         |
it implements `IComparable` to compare between profiles by checking their IDs.
> `UserProfiles` are marked as `Serializable` with all fields also exposed to the Unity editor.

Example:
```json
[
  {
    "ID": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "Name": "Profile 1",
    "TimePlayed": 3625.5
  },
  {
    "ID": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "Name": "Profile 2",
    "TimePlayed": 1250.0
  }
]
```
## Data management
The data manager provides different methods to save data using type-safe operations and profiles to build the path where they will be saved.

```
Application.dataPath/
└── SaveData/                          (configurable via SavePath)
    ├── profiles.json                  (all profiles list)
    ├── last_used_profile.sp          (editor-only: last selected profile)
    │
    ├── {Profile-GUID-1}/              
    │   ├── custom_data.json          (custom profile data)
    │   └── Levels/                   (configurable via LevelsPath)
    │       ├── Scene1/
    │       │   └── Doors.json
    │       └── Scene2/
    │           └── Enemies.json
    │
    └── {Profile-GUID-2}/              
        └── ...
```

the manager receives the data that will be saved as generic T, then it's saved on simple struct `SavedData` that holds the data as an `object` and the original Type of the data saved as `string`

| Field        | Type     | Purpose                         |
| ------------ | -------- | ------------------------------- |
| `ObjectType` | `string` | The fullname of the object type |
| `Data`       | `Object` | The object to be serealized     |
Every time you use method to get or save data thought the manager, they are checked for its type with `IsSameDataType` which checks for the saved `ObjectType` on `SavedData`

## Examples of use:
Create a profile and save data to it
```cs
// CreateProfile creates and automatically saves the profile
UserProfile p = SaveDataManager.CreateProfile("My Profile");

PlayerStats stats = new PlayerStats { health: 100, damage: 5 }
SaveDataManager.SaveProfileData<PlayerStats>(p, "player_stats", stats);

//.. recover data at other place
PlayerStats stats = new();
var profile = SaveDataManager.GetProfiles()[0];
bool recovered = SaveDataManager.GetProfileData(profile, "player_stats", out stats);
if (recovered)
	Debug.Log("Stats loaded");
else
	Debug.Log("Stats data don't found");
```