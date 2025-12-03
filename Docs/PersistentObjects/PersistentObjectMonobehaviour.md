# PersistentObjectMonobehaviour

> Documentation during development

Class extended from `UniqueIDAssigner` to always ensure that the gameobject has a unique ID, it holds an IPersistentDataObject to save and restore transform data.

All `PersistentObject` should belong to a category, by default, the monobehaviour would assign the scene name as the category but it could be changed on the inspector.

> To avoid repeat `PersistentObjectMonobehaviour` reference on this file, we are going to refer to it as `object` or `persistent object`

## General

The `PersistentObjectMonobehaviour` automatically register the object to the `PersistentObjectsSystem` and recovers the transform data when the game starts or a profile is changed even on the editor without running the game.

By listening to `OnDataRecoveredEvent` the `PersistentObjectMonobehaviour` applys the transform data and also saves the data on `PersistentData` field.

```cs
public IPersistentDataObject PersistentData { get; protected set; }
```

This field holds the data of the object and you could use any type that implements `IPersistentDataObject`, the default implementation is `PersistentObjectData`.
That interface forces you to save the transform data. When the data is recovered on the monobehaviour `OnUpdateData` is called and `OnApplyData` is called after that when the transform data recovered from the interface is applied to the object.

> Auxiliar / public methods used by `PersistentObjectsSystem`

| Scope    | Method                 | Purpose                                                                                             |
| -------- | ---------------------- | --------------------------------------------------------------------------------------------------- |
| `public` | `SaveObjectData`       | Auxiliar method used by `PersistentObjectsSystem` to ensure the data on `PersistentData` is updated |
| `public` | `UpdateData`           | Auxiliar method used by `PersistentObjectsSystem` to ensure the data on `PersistentData` is updated |
| `public` | `ResetTransform`       | Returns the transform to the saved original position, rotation and scale                            |
| `public` | `ApplyData`            | Applies the transform data from `PersistentData` to the object and calls to `OnApplyData`           |
| `public` | `RecoveryAndApplyData` | Searchs for data saved on the objects system and applies it with `ApplyData`                        |

> Custom implementation methods

| Scope               | Method                             | Purpose                                                                                                                                                                         |
| ------------------- | ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `protected virtual` | `OnDataRecovered(string category)` | Listens to `OnDataRecoveredEvent`, it defaults implementation calls `ResetTransform()` and `UpdateData()` applying data recovered on the category if it matches with the object |
| `protected virtual` | `OnApplyData`                      | Called after monobehaviour recovers new data and apply the transform data from it                                                                                               |
| `protected virtual` | `OnUpdateData`                     | This is called when any custom data should be stored into the `PersistentData`                                                                                                  |

### Example implementation use for custom data

```cs
public class PetObjectData : IPersistentDataObject {
    public string Name { get; set; }
    public int Age { get; set; }

    // IPersistentDataObject implementation...
}

public class DogPet : PersistentObjectMonobehaviour {
    private string _name = "MyDog";
    private int _age = 3;

    protected override void Awake() {
        // Sets before base awake to ensure when data is recovered the
        // custom data wouldn't be transformed into PersistentObjectData
        PersistentData = new MyPersistentObjectData();

        base.Awake();
    }

// Called when the system needs to scrap the data from the object
// use this to keep the data updated on PersistentData field
    protected override void OnUpdateData() {
        ((MyPersistentObjectData)PersistentData).Name = _name;
        ((MyPersistentObjectData)PersistentData).Age = _age;
    }

// Called when new data is recovered, for example at the start of the game
// use this to apply the recovered data into the object again
    protected override void OnApplyData() {
        _name = ((MyPersistentObjectData)PersistentData).Name;
        _age = ((MyPersistentObjectData)PersistentData).Age;
    }
}
```

## Editor Tools
