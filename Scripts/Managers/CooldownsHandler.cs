using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Topacai.Managers.Cooldowns
{
    public class CooldownSettings
    {
        public string Name { get; private set; }
        public float CooldownTime { get; private set; }
        public float CooldownTimeLeft { get; private set; }

        public bool CooldownEnabled { get; private set; }
        public bool IgnoreTimeScale { get; private set; }

        public CooldownSettings(string name, float cooldownTime, bool cooldownEnabled = true, bool ignoreTimeScale = false)
        {
            Name = name;
            CooldownTime = cooldownTime;
            CooldownTimeLeft = cooldownTime;
            CooldownEnabled = cooldownEnabled;
            IgnoreTimeScale = ignoreTimeScale;
        }

        public void ResetCooldown()
        {
            CooldownTimeLeft = CooldownTime;
        }

        public void UpdateCooldown()
        {
            if (!CooldownEnabled || CooldownTimeLeft < 0) return;
            CooldownTimeLeft -= IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        public void UpdateCooldown(float time)
        {
            if (!CooldownEnabled || CooldownTimeLeft < 0) return;
            CooldownTimeLeft -= time;
        }

        public void SetEnable(bool enable) => CooldownEnabled = enable;

        public void SetIgnoreTimeScale(bool ignoreTimeScale) => IgnoreTimeScale = ignoreTimeScale;
    }
    public class CooldownsHandler : MonoBehaviour
    {
        public static List<CooldownSettings> Cooldowns { get; private set; } = new List<CooldownSettings>();

        private void FixedUpdate()
        {
            for (int i = 0; i < Cooldowns.Count; i++)
            {
                Cooldowns[i].UpdateCooldown();
            }
        }

        public static CooldownSettings AddCooldown(string name, float cooldownTime, bool cooldownEnabled = true, bool ignoreTimeScale = false)
        {
            CooldownSettings cd = new CooldownSettings(name, cooldownTime, cooldownEnabled, ignoreTimeScale);
            Cooldowns.Add(cd);
            return cd;
        }

        public static CooldownSettings GetCooldown(string name) => Cooldowns.Find(x => x.Name == name);

        public static void RemoveCooldown(string name) => Cooldowns.Remove(Cooldowns.Find(x => x.Name == name));

        public static void ResetCooldown(string name) => Cooldowns.Find(x => x.Name == name).ResetCooldown();

        public static void SetCooldownEnable(string name, bool enable) => Cooldowns.Find(x => x.Name == name).SetEnable(enable);

        public static void SetCooldownIgnoreTimeScale(string name, bool ignoreTimeScale) => Cooldowns.Find(x => x.Name == name).SetIgnoreTimeScale(ignoreTimeScale);
        
        public static bool IsOnCooldown(string name) => Cooldowns.Find(x => x.Name == name).CooldownTimeLeft > 0;


    }
}
