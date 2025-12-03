# Persistent Game Objects

> Documentation during development

## General

Persistent game objects are game objects that could conserve their transform and custom data between game sessions. They use `SaveSystem` to save and recover data at **level-scope**.

## Architecture

The `PersistentObjectsSystem` class conserves the data of the objects on the current scene by registering them per category.

Any `IPersistentDataObject` is considered a valid object to be saved, as long as they always have an unique identifier. The default implementation is `PersistentObjectData` as an struct.

### PersistentObjectsSystem

Is a static class that manages all -persistent objects by category and registration, it recover and save data by category with `SaveSystem`

```cs
//Data[category][uniqueID] => data as IPersistentDataObject
private static Dictionary<string, Dictionary<string, IPersistentDataObject>> PersistentDataByCategory = new();
    ├── "Level1" -> Dictionary<uniqueID, data>
    │   ├── "a1b2c3d4..." -> PersistentObjectData
    │   └── "e5f6g7h8..." -> PersistentObjectData
    └── "Collectibles" -> Dictionary<uniqueID, data>
        └── "i9j0k1l2..." -> CustomPersistentData
```

The system listens to two events from `SaveSystem` even when the game isn't running (editor).

- `OnProfileChanged`: call `RecoverAllObjects` after clear the current ones.
- `OnSaveGameEvent`: calls `SaveAllObjects`

> Recovery / Saving methods

| Method                                            | Purpose                                                                                      |
| ------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `RecoverAllObjects()`                             | Scans scene (including additive) for all persistent objects, recovers data for each category |
| `RecoverCategory(string category)`                | Loads data for specific category from disk                                                   |
| `SaveAllObjects()`                                | Saves all categories by scanning scene objects                                               |
| `SaveCategoryObjects(string category)`            | Saves specific category and calls `UpdateData()` on each object                              |
| `GetObjectData(string uniqueID, string category)` | Retrieves data for specific object                                                           |

`OnDataRecoveredEvent` is emitted when data from a category is recovered and passes the category name as parameter

The data is saved in one json file per category at level-scope:

```json
{
  "Category": "Level1",
  "ObjectList": {
    "a1b2c3d4-...": {
      "$type": "Topacai.Utils.GameObjects.Persistent.PersistentObjectData",
      "UniqueID": "a1b2c3d4-...",
      "Position": { "x": 10.5, "y": 2.0, "z": -5.3 },
      "Rotation": { "x": 0, "y": 45, "z": 0 },
      "Scale": { "x": 1, "y": 1, "z": 1 }
    }
  }
}
```

_saved on:_ `/{SaveSystemSavePath}/{profileID}/Levels/{sceneName}/PersistentObjectsData/{category}.json`

> Usage / Implementation methods to create your own persistent objec

| Method                                                                        | Purpose                                              |
| ----------------------------------------------------------------------------- | ---------------------------------------------------- |
| `AddDataToCategory(string c, Dictionary<string, IPersistentDataObject> data)` | Used to register a new category with data on it.     |
| `SetDataInCategory(string category, string id, IPersistentDataObject data)`   | overwrites or inserts the data for a specific object |
| `bool CategoryExists(string category)`                                        | Checks if a category exists                          |
