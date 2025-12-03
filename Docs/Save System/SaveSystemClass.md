> Documentation during development

The `SaveSystemClass` is an high level API to handle profiles and data at different scope-levels using `SaveDataManager` below it, it works as an static class to access anywhere.

the current profile selected is exposed with `GetCurrentProfile()` but it returns a copy of the profile because the data is saved as an `struct`

you could change the current profile using `SetProfile()`

| Method                        | Purpose                                            | Return Type |
| ----------------------------- | -------------------------------------------------- | ----------- |
| `RecoverProfiles()`           | Initialize profiles from disk at application start | void        |
| `SetProfile(UserProfile)`     | Switch active profile and emit OnProfileChanged    | void        |
| `CallSaveGameEvent()`         | Trigger save operation and emit OnSaveGameEvent    | void        |
| `SaveDataToProfile<T>()`      | Save custom data to current profile root           | void        |
| `SaveLevelDataToProfile<T>()` | Save scene-specific data to Levels/{scene}/        | void        |
| `GetProfileData<T>()`         | Load custom data from current profile              | bool        |
| `GetLevelData<T>()`           | Load scene-specific data from Levels/{scene}/      | bool        |
| `GetCurrentProfile()`         | Retrieve active UserProfile                        | UserProfile |

---

this system handles a profile selection for the application and expose some events to allow other systems to interact between profiles changes and to save their own data when is desired

```cs
public static event EventHandler OnSaveGameEvent;
public static UnityEvent<UserProfile> OnProfileChanged = new ();
public static UnityEvent<List<UserProfile>> OnProfilesFetched = new ();
```

- `OnSaveGameEvent` called when the game receives the order of save data by custom logic with `CallSaveGameEvent()` or before the application is closed.
- `OnProfileChanged` is triggered when the current profile on use is changed.
- `OnProfilesFetched` is triggered when saved profiles list is recovered or updated.

---

you could configure path and files names using `SetPaths()` that receives 3 strings that corresponds to:

```cs
public static string SavePath { get; private set; } = "/SaveData";
public static string ProfilesFileName { get; private set; } = "profiles.json";
public static string LevelsPath { get; private set; } = "/Levels";
```

---

### SaveGame

The `SaveGame()` method is called along with `OnSaveGameEvent`, it serializes the current selected profile data and also emits and order to all the `PersistentProfileDataSO` to serialize their data using the profile the Save system itself.

```js
//SaveGame..
foreach (var data in Resources.FindObjectsOfTypeAll<PersistentProfileDataSO>())
       data.OnProfileSaved();
```
