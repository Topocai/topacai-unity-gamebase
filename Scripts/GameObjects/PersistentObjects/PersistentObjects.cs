using System.Collections.Generic;
using Topacai.Utils.SaveSystem;
using UnityEngine;

namespace Topacai.Utils.GameObjects.Persistent
{
    public struct PersistentObjectsCategoryData
    {
        public string Category { get; set; }
        public Dictionary<string, IPersistentDataObject> ObjectList { get; set; }

        public PersistentObjectsCategoryData(string category, Dictionary<string, IPersistentDataObject> objectList)
        {
            Category = category;
            ObjectList = objectList;
        }
    }
    public interface IPersistentDataObject
    {
        public string UniqueID { get; set; }
        public SerializeableVector3 Position { get; set; }
        public SerializeableVector3 Rotation { get; set; }
        public SerializeableVector3 Scale { get; set; }
    }

    public struct PersistentObjectData : IPersistentDataObject
    {
        public string UniqueID { get; set; }
        public SerializeableVector3 Position { get; set; }
        public SerializeableVector3 Rotation { get; set; }
        public SerializeableVector3 Scale { get; set; }

        public override int GetHashCode() => UniqueID?.GetHashCode() ?? 0;

        public override bool Equals(object obj)
        {
            if (!(obj is PersistentObjectData other))
            {
                if (obj is string)
                {
                    return UniqueID == (string)obj;
                }
                return false;
            }
            return UniqueID == other.UniqueID;
        }
    }
}
